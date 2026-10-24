#!/usr/bin/env powershell
#Requires -Version 5.1
<#
.SYNOPSIS
    边缘武神系列（RimWarlord 本体 + 附属）工作流入口。

.DESCRIPTION
    子命令：
      validate   静态体检：About / LoadFolders / Def 图 / 贴图 / 语言注入
      snapshot   把游戏 Mods 目录里的内容回灌进仓库（可选顺带 git 提交）
      build      编译 C#（-Deploy 同步进游戏目录）
      package    生成可上传工坊的 zip
      lang       对照原版中文 DefInjected 白名单：缺键 / 孤儿键 / 简繁不一致

    设计原则：
      * 只读游戏目录，不替游戏做决定；改动一律落在仓库或 dist。
      * 语言只做「对齐审计」：某个 Def 没有语言文件是正常的，但 CS 有、CT 没有就是问题。
      * 不假设 LoadFolders.xml 一定存在：没有就按 根目录 + 版本目录 计算。

.EXAMPLE
    ./rw.ps1 validate
    ./rw.ps1 validate -Strict
    ./rw.ps1 snapshot -Message "白家头衔与背景补充"
    ./rw.ps1 build -Deploy
    ./rw.ps1 package
    ./rw.ps1 lang
    ./rw.ps1 lang -Mod RimWarlord -Todo
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('validate', 'snapshot', 'build', 'package', 'selftest', 'search', 'show', 'fields', 'clone', 'check', 'lang', 'help')]
    [string]$Command = 'help',

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    # build：编译后把 dll/pdb 同步进游戏 Mods 目录
    [switch]$Deploy,

    # validate / 索引：强制重建缓存
    [switch]$Refresh,

    # validate：把 Warning 也当成失败
    [switch]$Strict,

    # snapshot：一并回灌贴图（体积大，默认不含）
    [switch]$IncludeTextures,

    # snapshot / package：只打印将发生什么
    [switch]$DryRun,

    # snapshot：git 提交信息
    [string]$Message,

    # 只处理配置里的某个 mod id
    [string]$Only,

    # 指定另一份配置文件（默认同目录 rw.config.json）
    [string]$ConfigPath,

    # ---- Def 写作 ----
    # search/show/clone 的目标：DefType/defName 或单独 defName
    [Parameter(Position = 1)][string]$Query,
    # fields/check 的 Def 类型
    [string]$Type,
    # search 的匹配模式（支持 *）
    [string]$Like,
    # fields 的字段过滤（正则）
    [string]$FieldMatch,
    # check 的目标文件或目录
    [string]$Path,
    # clone 的新 defName / label / description
    [string]$DefName,
    [string]$Label,
    [string]$Description,
    # clone 的输出文件，或追加到已有文件
    [string]$Out,
    [string]$AppendTo,
    [int]$Limit = 40,
    # lang：只查某个 mod id / packageId（默认配置里的全部）
    [string]$Mod,
    # lang：为缺键生成可粘贴的 TODO 骨架
    [switch]$Todo,
    # 索引/检查时覆盖 Mods 目录下的全部模组，而不只是配置里的两个
    [switch]$All,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Rw = @{
    Root     = $PSScriptRoot
    Config   = $null
    Issues   = [System.Collections.Generic.List[object]]::new()
    DefIndex = @{}
    Counters = @{}
    Stats    = @{ Defs = 0; TexRefs = 0; LangKeys = 0 }
    TypeDefNames = @{}
}

#region ---------------------------------------------------------------- 基础工具

function Write-RwStep {
    param([string]$Text)
    Write-Host "[rw] $Text" -ForegroundColor DarkCyan
}

function Add-Issue {
    param(
        [Parameter(Mandatory)][ValidateSet('Error', 'Warning', 'Info')][string]$Severity,
        [Parameter(Mandatory)][string]$Check,
        [Parameter(Mandatory)][string]$Message,
        [string]$Path,
        [string]$Detail
    )
    $script:Rw.Issues.Add([pscustomobject]@{
            Severity = $Severity
            Check    = $Check
            Message  = $Message
            Path     = $Path
            Detail   = $Detail
        })
}

function Read-RwXml {
    <# 读 XML，顺带把解析错误变成一条 Issue；失败返回 $null #>
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$Check = 'xml'
    )
    try {
        $text = [System.IO.File]::ReadAllText($Path)
    }
    catch {
        Add-Issue -Severity Error -Check $Check -Message '读取失败' -Path $Path -Detail $_.Exception.Message
        return $null
    }

    # 非法 UTF-8 字节：游戏能容忍 BOM，但不能容忍坏字节
    $strict = [System.Text.UTF8Encoding]::new($false, $true)
    try {
        [void]$strict.GetString([System.IO.File]::ReadAllBytes($Path))
    }
    catch {
        Add-Issue -Severity Error -Check 'encoding' -Message '不是合法 UTF-8（会被游戏读成乱码）' -Path $Path
    }

    try {
        $doc = [System.Xml.XmlDocument]::new()
        $doc.LoadXml($text)
        return $doc
    }
    catch {
        Add-Issue -Severity Error -Check $Check -Message 'XML 解析失败' -Path $Path -Detail $_.Exception.Message
        return $null
    }
}

function Get-RwConfig {
    $path = if ($ConfigPath) { $ConfigPath } else { Join-Path $script:Rw.Root 'rw.config.json' }
    if (-not (Test-Path $path)) {
        throw "缺少配置文件：$path"
    }
    $cfg = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json
    # 路径里的 {tools} 展开为脚本所在目录，便于配置随工具一起搬
    foreach ($key in @('gameDir', 'modsDir', 'repoRoot', 'distDir')) {
        if ($cfg.PSObject.Properties.Name -contains $key -and $cfg.$key) {
            $cfg.$key = ([string]$cfg.$key).Replace('{tools}', $script:Rw.Root)
        }
    }
    if (-not (Test-Path $cfg.gameDir)) { throw "配置的 gameDir 不存在：$($cfg.gameDir)" }
    if (-not (Test-Path $cfg.modsDir)) { throw "配置的 modsDir 不存在：$($cfg.modsDir)" }
    if (-not (Test-Path $cfg.repoRoot)) { throw "配置的 repoRoot 不存在：$($cfg.repoRoot)" }
    return $cfg
}

function Get-TargetMods {
    $mods = @($script:Rw.Config.mods)
    if ($Only) {
        $mods = @($mods | Where-Object { $_.id -eq $Only -or $_.packageId -eq $Only })
        if ($mods.Count -eq 0) { throw "配置里没有 mod：$Only" }
    }
    return $mods
}

function Get-DistRoot {
    $cfg = $script:Rw.Config
    if ([System.IO.Path]::IsPathRooted($cfg.distDir)) { return $cfg.distDir }
    return (Join-Path $cfg.repoRoot $cfg.distDir)
}

function Get-CacheDir {
    $dir = Join-Path $script:Rw.Root '.cache'
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    return $dir
}

#endregion

#region ---------------------------------------------------------------- 索引

function Get-VanillaIndex {
    <# 原版 + DLC 的 defName / Def 类型集合，用于判断引用是否悬空 #>
    if ($script:Rw.Counters.ContainsKey('vanilla')) { return $script:Rw.Counters['vanilla'] }

    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'
    $packs = @(Get-ChildItem $dataDir -Directory | Sort-Object Name)
    $stamp = ($packs | ForEach-Object { '{0}:{1}' -f $_.Name, $_.LastWriteTimeUtc.Ticks }) -join '|'
    $cachePath = Join-Path (Get-CacheDir) 'vanilla.json'

    $cached = $null
    if (-not $Refresh -and (Test-Path $cachePath)) {
        try {
            $raw = Get-Content $cachePath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($raw.stamp -eq $stamp) { $cached = $raw }
        }
        catch { }
    }

    if ($cached) {
        $index = @{
            DefNames = [System.Collections.Generic.HashSet[string]]::new([string[]]@($cached.defNames), [StringComparer]::Ordinal)
            Types    = [System.Collections.Generic.HashSet[string]]::new([string[]]@($cached.types), [StringComparer]::Ordinal)
            TexPaths = [System.Collections.Generic.HashSet[string]]::new([string[]]@($cached.texPaths), [StringComparer]::OrdinalIgnoreCase)
            Templates = [System.Collections.Generic.HashSet[string]]::new([string[]]@($cached.templates), [StringComparer]::Ordinal)
        }
        $script:Rw.Counters['vanilla'] = $index
        return $index
    }

    Write-RwStep "建立原版 Def 索引（$($packs.Count) 个 Data 包）…"
    $defNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $types = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $texPaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $templates = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)

    foreach ($pack in $packs) {
        $defsDir = Join-Path $pack.FullName 'Defs'
        if (-not (Test-Path $defsDir)) { continue }
        foreach ($file in Get-ChildItem $defsDir -Recurse -Filter *.xml -File) {
            $doc = Read-RwXml -Path $file.FullName -Check 'vanilla-xml'
            if (-not $doc -or -not $doc.DocumentElement) { continue }
            foreach ($node in $doc.DocumentElement.ChildNodes) {
                if ($node -isnot [System.Xml.XmlElement]) { continue }
                [void]$types.Add($node.LocalName)
                $dn = $node.SelectSingleNode('defName')
                if ($dn) { [void]$defNames.Add($dn.InnerText.Trim()) }
                # 原版的 Abstract 模板只有 Name 没有 defName（如 BaseBullet）
                $tn = $node.GetAttribute('Name')
                if ($tn) { [void]$templates.Add($tn) }
                # 原版贴图打包在 Unity 资源里、磁盘上没有 png，只能从原版 Def 反推合法路径
                foreach ($p in $node.SelectNodes('.//texPath | .//iconPath | .//graphicPath | .//uiIconPath')) {
                    $v = $p.InnerText.Trim()
                    if (-not $v) { continue }
                    $v = ($v -replace '\\', '/').Trim('/')
                    if ($v -match '\.(png|jpg|psd)$') { $v = $v.Substring(0, $v.LastIndexOf('.')) }
                    [void]$texPaths.Add($v)
                }
            }
        }
    }

    [System.IO.File]::WriteAllText($cachePath, (@{
                stamp     = $stamp
                defNames  = @($defNames)
                types     = @($types)
                texPaths  = @($texPaths)
                templates = @($templates)
            } | ConvertTo-Json -Depth 3 -Compress), [System.Text.UTF8Encoding]::new($false))

    $index = @{ DefNames = $defNames; Types = $types; TexPaths = $texPaths; Templates = $templates }
    $script:Rw.Counters['vanilla'] = $index
    Write-RwStep ("原版索引：{0} 个 defName / {1} 个模板名 / {2} 种 Def 类型 / {3} 条贴图路径" -f `
            $defNames.Count, $templates.Count, $types.Count, $texPaths.Count)
    return $index
}

function Get-AssemblyText {
    <# 把游戏与模组的 dll 当成 Latin1 文本读进来，用于「这个类名存不存在」的启发式检查 #>
    if ($script:Rw.Counters.ContainsKey('dlltext')) { return $script:Rw.Counters['dlltext'] }

    $sb = [System.Text.StringBuilder]::new()
    $paths = @()
    $managed = Join-Path $script:Rw.Config.gameDir 'RimWorldWin64_Data/Managed'
    if (Test-Path $managed) {
        $paths += @(Get-ChildItem $managed -Filter *.dll -File | Select-Object -ExpandProperty FullName)
    }
    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'
    $paths += @(Get-ChildItem $dataDir -Recurse -Filter *.dll -File -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName)
    foreach ($mod in $script:Rw.Config.mods) {
        $modRoot = Join-Path $script:Rw.Config.modsDir $mod.id
        if (-not (Test-Path $modRoot)) { continue }
        $paths += @(Get-ChildItem $modRoot -Recurse -Filter *.dll -File -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName)
    }

    $enc = [System.Text.Encoding]::GetEncoding(28591)  # Latin1：1 字节 = 1 字符，便于子串查找
    foreach ($p in $paths) {
        try { [void]$sb.Append($enc.GetString([System.IO.File]::ReadAllBytes($p))) }
        catch { }
    }

    $text = $sb.ToString()
    $script:Rw.Counters['dlltext'] = $text
    return $text
}

function Get-TextureIndex {
    <# texPath -> 实际文件；key 为去掉扩展名的相对路径（小写），value 为原始大小写 #>
    if ($script:Rw.Counters.ContainsKey('textures')) { return $script:Rw.Counters['textures'] }

    $cachePath = Join-Path (Get-CacheDir) 'textures.json'
    $index = [System.Collections.Generic.Dictionary[string, string]]::new([StringComparer]::OrdinalIgnoreCase)

    $roots = @()
    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'
    $roots += @(Get-ChildItem $dataDir -Directory | ForEach-Object { Join-Path $_.FullName 'Textures' } | Where-Object { Test-Path $_ })
    foreach ($mod in $script:Rw.Config.mods) {
        $modRoot = Join-Path $script:Rw.Config.modsDir $mod.id
        if (-not (Test-Path $modRoot)) { continue }
        $roots += @(Get-ChildItem $modRoot -Recurse -Directory -Filter 'Textures' | Select-Object -ExpandProperty FullName)
    }

    $stampParts = foreach ($r in $roots) {
        # 用「根目录下最新文件的写入时间 + 文件数」判断，别只看根的目录时间戳：
        # 往已有子目录里加图不会改根目录的 mtime，会导致缓存过期仍被当成有效（曾因此误报贴图缺失）
        $files = @(Get-ChildItem $r -Recurse -File -Filter *.png -ErrorAction SilentlyContinue)
        $maxTicks = 0
        foreach ($f in $files) { if ($f.LastWriteTimeUtc.Ticks -gt $maxTicks) { $maxTicks = $f.LastWriteTimeUtc.Ticks } }
        '{0}:{1}:{2}' -f $r, $maxTicks, $files.Count
    }
    $stamp = ($stampParts -join '|').GetHashCode().ToString()

    $cached = $null
    if (-not $Refresh -and (Test-Path $cachePath)) {
        try {
            $raw = Get-Content $cachePath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($raw.stamp -eq $stamp -and $raw.entries) { $cached = $raw }
        }
        catch { }
    }

    if ($cached) {
        foreach ($entry in $cached.entries.PSObject.Properties) {
            $index[$entry.Name] = $entry.Value
        }
        $script:Rw.Counters['textures'] = $index
        return $index
    }

    Write-RwStep "建立贴图索引（$($roots.Count) 个 Textures 根）…"
    foreach ($root in $roots) {
        foreach ($file in Get-ChildItem $root -Recurse -File -Filter *.png -ErrorAction SilentlyContinue) {
            $rel = $file.FullName.Substring($root.Length).TrimStart('\', '/') -replace '\\', '/'
            $key = $rel.Substring(0, $rel.Length - 4)
            $index[$key] = $rel
        }
    }

    $entries = [ordered]@{}
    foreach ($kv in $index.GetEnumerator()) { $entries[$kv.Key] = $kv.Value }
    [System.IO.File]::WriteAllText($cachePath, (@{ stamp = $stamp; entries = $entries } | ConvertTo-Json -Depth 3 -Compress),
        [System.Text.UTF8Encoding]::new($false))

    $script:Rw.Counters['textures'] = $index
    Write-RwStep ("贴图索引：{0} 张" -f $index.Count)
    return $index
}

function Get-ModLayout {
    <# 计算某个 mod 实际会被游戏加载的目录列表 #>
    param([Parameter(Mandatory)]$Mod)

    $root = Join-Path $script:Rw.Config.modsDir $Mod.id
    $layout = @{ Root = $root; Folders = @(); Missing = $false }
    if (-not (Test-Path $root)) {
        $layout.Missing = $true
        Add-Issue -Severity Error -Check 'layout' -Message '模组目录不存在' -Path $root
        return $layout
    }

    $folders = [System.Collections.Generic.List[string]]::new()
    $folders.Add('')   # 根目录永远参与加载

    $lfPath = Join-Path $root 'LoadFolders.xml'
    if (Test-Path $lfPath) {
        $doc = Read-RwXml -Path $lfPath -Check 'loadfolders'
        if ($doc -and $doc.DocumentElement) {
            $versionNode = $null
            foreach ($child in $doc.DocumentElement.ChildNodes) {
                if ($child -is [System.Xml.XmlElement] -and $child.LocalName -eq ('v' + $Mod.version)) { $versionNode = $child; break }
            }
            if (-not $versionNode) {
                Add-Issue -Severity Warning -Check 'loadfolders' -Message "LoadFolders.xml 没有 v$($Mod.version) 节点，游戏会退回其它版本" -Path $lfPath
            }
            else {
                $active = @($script:Rw.Config.activeMods)
                foreach ($li in $versionNode.ChildNodes) {
                    if ($li -isnot [System.Xml.XmlElement]) { continue }
                    $path = $li.InnerText.Trim()
                    $ifActive = $li.GetAttribute('IfModActive')
                    $ifNotActive = $li.GetAttribute('IfModNotActive')

                    $include = $true
                    if ($ifActive) {
                        $ids = @($ifActive -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
                        $include = @($ids | Where-Object { $active -contains $_ }).Count -eq $ids.Count
                    }
                    if ($include -and $ifNotActive) {
                        $ids = @($ifNotActive -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
                        $include = @($ids | Where-Object { $active -contains $_ }).Count -eq 0
                    }
                    if (-not $include) { continue }

                    $clean = $path.Trim('/', '\')
                    if ($clean -and $clean -ne '.') {
                        if (-not (Test-Path (Join-Path $root $clean))) {
                            Add-Issue -Severity Error -Check 'loadfolders' -Message "LoadFolders.xml 引用了不存在的目录：$clean" -Path $lfPath
                        }
                        elseif (-not ($folders -contains $clean)) { $folders.Add($clean) }
                    }
                }
            }
        }
    }
    else {
        # 没有 LoadFolders.xml：根目录 + 版本目录（存在才加）
        if (Test-Path (Join-Path $root $Mod.version)) { $folders.Add($Mod.version) }
        if (Test-Path (Join-Path $root 'Common')) { $folders.Add('Common') }
    }

    # 有内容却永远加载不到的版本目录（只在显式 LoadFolders 时才有意义）
    if (Test-Path $lfPath) {
    foreach ($dir in Get-ChildItem $root -Directory) {
        if ($dir.Name -notmatch '^\d+\.\d+$') { continue }
        if ($folders -contains $dir.Name) { continue }
        Add-Issue -Severity Warning -Check 'loadfolders' -Message "目录 $($dir.Name) 不会被加载（不在当前版本的 LoadFolders 里）" -Path $dir.FullName
    }
    }

    $layout.Folders = @($folders)
    return $layout
}

function Get-ModDefIndex {
    <# 解析某个 mod 全部 Def，返回结构化索引 #>
    param(
        [Parameter(Mandatory)]$Mod,
        [Parameter(Mandatory)]$Layout
    )

    $index = @{
        Defs       = [System.Collections.Generic.List[object]]::new()
        ByKey      = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
        DefNames   = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        Templates  = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
        Folders    = $Layout.Folders
        SourceDirs = [System.Collections.Generic.List[string]]::new()
    }
    if ($Layout.Missing) { return $index }

    foreach ($folder in $Layout.Folders) {
        $defsDir = if ($folder) { Join-Path (Join-Path $Layout.Root $folder) 'Defs' } else { Join-Path $Layout.Root 'Defs' }
        if (-not (Test-Path $defsDir)) { continue }
        $index.SourceDirs.Add($defsDir)

        foreach ($file in Get-ChildItem $defsDir -Recurse -Filter *.xml -File) {
            $doc = Read-RwXml -Path $file.FullName -Check 'def-xml'
            if (-not $doc -or -not $doc.DocumentElement) { continue }
            if ($doc.DocumentElement.LocalName -ne 'Defs') {
                Add-Issue -Severity Warning -Check 'def-xml' -Message "根节点应为 <Defs>，实际是 <$($doc.DocumentElement.LocalName)>" -Path $file.FullName
            }

            foreach ($node in $doc.DocumentElement.ChildNodes) {
                if ($node -isnot [System.Xml.XmlElement]) { continue }
                $dn = $node.SelectSingleNode('defName')
                $record = [pscustomobject]@{
                    DefName    = $(if ($dn) { $dn.InnerText.Trim() } else { $null })
                    Type       = $node.LocalName
                    ParentName = $node.GetAttribute('ParentName')
                    Template   = $node.GetAttribute('Name')
                    Abstract   = ($node.GetAttribute('Abstract') -eq 'True')
                    File       = $file.FullName
                    Node       = $node
                    Mod        = $Mod.id
                }
                $index.Defs.Add($record)

                if ($record.DefName) {
                    [void]$index.DefNames.Add($record.DefName)
                    if (-not $script:Rw.TypeDefNames.ContainsKey($record.Type)) {
                        $script:Rw.TypeDefNames[$record.Type] = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
                    }
                    [void]$script:Rw.TypeDefNames[$record.Type].Add($record.DefName)
                    # rim 的 defName 唯一性按 Def 类型算：NeedDef 与 JoyKindDef 可以同名
                    $key = "$($record.Type)/$($record.DefName)"
                    if ($index.ByKey.ContainsKey($key)) {
                        $prev = $index.ByKey[$key]
                        Add-Issue -Severity Error -Check 'def-duplicate' `
                            -Message "同类型 defName 重复：$($record.DefName)（$($record.Type)，另一处在 $(Split-Path $prev.File -Leaf)）" `
                            -Path $file.FullName -Detail $prev.File
                    }
                    else {
                        $index.ByKey[$key] = $record
                    }
                }
                if ($record.Template) {
                    if ($index.Templates.ContainsKey($record.Template)) {
                        Add-Issue -Severity Error -Check 'def-duplicate' -Message "模板 Name 重复：$($record.Template)" -Path $file.FullName
                    }
                    else {
                        $index.Templates[$record.Template] = $record
                    }
                }
            }
        }
    }

    $script:Rw.DefIndex[$Mod.id] = $index
    return $index
}

#endregion

#region ---------------------------------------------------------------- 校验

function Test-ModMetadata {
    param([Parameter(Mandatory)]$Mod)

    $root = Join-Path $script:Rw.Config.modsDir $Mod.id
    $aboutPath = Join-Path $root 'About/About.xml'
    if (-not (Test-Path $aboutPath)) {
        Add-Issue -Severity Error -Check 'about' -Message '缺 About/About.xml' -Path $root
        return
    }

    $doc = Read-RwXml -Path $aboutPath -Check 'about'
    if (-not $doc -or -not $doc.DocumentElement) { return }

    $meta = $doc.DocumentElement
    foreach ($required in @('name', 'packageId')) {
        if (-not $meta.SelectSingleNode($required)) {
            Add-Issue -Severity Error -Check 'about' -Message "About.xml 缺 <$required>" -Path $aboutPath
        }
    }

    $pkg = $meta.SelectSingleNode('packageId')
    if ($pkg) {
        if ($pkg.InnerText.Trim() -ne $Mod.packageId) {
            Add-Issue -Severity Warning -Check 'about' `
                -Message "packageId 与配置不一致：实际 $($pkg.InnerText.Trim())，配置 $($Mod.packageId)" -Path $aboutPath
        }
        if ($pkg.InnerText.Trim() -notmatch '^[A-Za-z0-9]+(\.[A-Za-z0-9]+)+$') {
            Add-Issue -Severity Error -Check 'about' -Message 'packageId 格式非法（只允许字母数字与点，且至少一个点）' -Path $aboutPath
        }
    }

    if (-not $meta.SelectSingleNode('modVersion')) {
        Add-Issue -Severity Warning -Check 'about' -Message 'About.xml 缺 <modVersion>' -Path $aboutPath
    }

    $versions = @($meta.SelectNodes('supportedVersions/li') | ForEach-Object { $_.InnerText.Trim() })
    if ($versions.Count -gt 0 -and ($versions -notcontains $Mod.version)) {
        Add-Issue -Severity Warning -Check 'about' `
            -Message "supportedVersions 不含当前校验版本 $($Mod.version)（列的是 $($versions -join ', ')）" -Path $aboutPath
    }

    $deps = @($meta.SelectNodes('modDependencies/li/packageId') | ForEach-Object { $_.InnerText.Trim() })
    $baseId = $script:Rw.Config.basePackageId
    if ($Mod.packageId -ne $baseId) {
        if ($deps -notcontains $baseId) {
            Add-Issue -Severity Error -Check 'about' -Message "附属模组没有声明对本体 $baseId 的依赖" -Path $aboutPath
        }
        $loadAfter = @($meta.SelectNodes('loadAfter/li') | ForEach-Object { $_.InnerText.Trim() })
        if ($loadAfter -notcontains $baseId) {
            Add-Issue -Severity Warning -Check 'about' -Message "附属模组没有 loadAfter $baseId" -Path $aboutPath
        }
    }
    else {
        if ($deps -contains $baseId) {
            Add-Issue -Severity Error -Check 'about' -Message '本体把自己写进了依赖' -Path $aboutPath
        }
    }
}

function Test-ModDefs {
    param(
        [Parameter(Mandatory)]$Mod,
        [Parameter(Mandatory)]$Index,
        [Parameter(Mandatory)]$Vanilla,
        [Parameter(Mandatory)]$Textures,
        [Parameter(Mandatory)]$KnownNames,
        [Parameter(Mandatory)]$KnownTemplates
    )

    $extraTypes = @()
    $typesFile = Join-Path $script:Rw.Root 'def-types.extra.txt'
    if (Test-Path $typesFile) {
        $extraTypes = @(Get-Content $typesFile -Encoding UTF8 |
            ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') })
    }

    $dllText = Get-AssemblyText
    $pathFields = @('texPath', 'iconPath', 'graphicPath', 'uiIconPath', 'factionIconPath', 'royalFavorIconPath')

    foreach ($def in $Index.Defs) {
        $script:Rw.Stats.Defs++
        if (-not $def.DefName -and -not $def.Abstract) {
            Add-Issue -Severity Error -Check 'def-model' -Message "缺少 defName（类型 $($def.Type)）" -Path $def.File
            continue
        }
        if ($def.Abstract -and -not $def.Template) {
            Add-Issue -Severity Warning -Check 'def-model' -Message 'Abstract 模板缺少 Name' -Path $def.File
        }

        $typeKnown = $Vanilla.Types.Contains($def.Type) -or ($extraTypes -contains $def.Type)
        if (-not $typeKnown -and $def.Type -match '\.') {
            # 自定义 Def 类型写成完全限定名，例如 Electromagnetic.Core.WarlordBackstoryDef
            $short = $def.Type.Split('.')[-1]
            $typeKnown = ($extraTypes -contains $short) -or ($dllText -and $dllText.Contains($short))
        }
        elseif (-not $typeKnown -and $dllText) {
            $typeKnown = $dllText.Contains($def.Type)
        }
        if (-not $typeKnown) {
            Add-Issue -Severity Error -Check 'def-type' `
                -Message "未知的 Def 元素类型 <$($def.Type)>（拼错，或对应 C# 类不在已编译的 dll 里）" -Path $def.File -Detail $def.DefName
        }

        if ($def.ParentName) {
            $target = $def.ParentName
            if (-not $KnownNames.Contains($target)) {
                Add-Issue -Severity Error -Check 'def-reference' `
                    -Message "ParentName 指向不存在的模板/Def：$target" -Path $def.File -Detail $def.DefName
            }
            elseif (-not $KnownTemplates.Contains($target)) {
                # 常见错误：ParentName 写成 defName。父类只能引用标签里 Name= 定义的模板名
                Add-Issue -Severity Warning -Check 'def-reference' `
                    -Message "ParentName 用了 defName（$target）：这里要写父类标签里 Name= 的名字，defName 只能用来定位 Def" -Path $def.File -Detail $def.DefName
            }
        }

        # 类名引用（needClass / hediffClass / abilityClass…）只做启发式提示，避免误报
        if ($dllText) {
            foreach ($node in $def.Node.SelectNodes('.//*[substring(name(), string-length(name()) - 4) = "Class"]')) {
                $cls = $node.InnerText.Trim()
                if (-not $cls -or $cls -match '\s' -or $cls -notmatch '^[A-Za-z_][A-Za-z0-9_.]*$') { continue }
                if ($node.LocalName -eq 'graphicClass') { continue }
                if (-not $dllText.Contains($cls.Split('.')[-1])) {
                    Add-Issue -Severity Warning -Check 'class-missing' `
                        -Message "类名疑似不存在：$($node.LocalName) = $cls" -Path $def.File -Detail $def.DefName
                }
            }
        }

        foreach ($field in $pathFields) {
            $nodes = $def.Node.SelectNodes(".//$field")
            foreach ($node in $nodes) {
                $raw = $node.InnerText.Trim()
                if (-not $raw -or $raw -match '^\$') { continue }
                $script:Rw.Stats.TexRefs++
                $rel = ($raw -replace '\\', '/').Trim('/')
                if ($rel -match '\.(png|jpg|psd)$') { $rel = $rel.Substring(0, $rel.LastIndexOf('.')) }

                $hit = $null
                if ($Textures.ContainsKey($rel)) { $hit = $Textures[$rel] }

                if ($hit) {
                    $expected = "$rel.png"
                    if ($hit -cne $expected) {
                        Add-Issue -Severity Warning -Check 'texture-case' `
                            -Message "贴图大小写与路径不一致（Linux/Mac 会加载失败）：路径写 $expected，实际 $hit" `
                            -Path $def.File -Detail $def.DefName
                    }
                    continue
                }

                # Graphic_Random / Graphic_Collection 用目录或前缀，不是单个文件
                $prefixHit = $false
                foreach ($key in $Textures.Keys) {
                    if ($key.StartsWith("$rel/", [StringComparison]::OrdinalIgnoreCase) -or
                        $key.StartsWith("$rel`_", [StringComparison]::OrdinalIgnoreCase)) { $prefixHit = $true; break }
                }
                if ($prefixHit) { continue }

                # 原版贴图不在磁盘上，用原版 Def 里出现过的路径兜底
                if ($Vanilla.TexPaths.Contains($rel)) { continue }

                Add-Issue -Severity Error -Check 'texture-missing' `
                    -Message "贴图找不到：$rel" -Path $def.File -Detail "$($def.DefName).$field"
            }
        }
    }

    # defName 前缀提醒：系列内统一，便于跨模组引用与排查
    $prefix = $script:Rw.Config.defNamePrefix
    if ($prefix) {
        $bad = @($Index.Defs | Where-Object { $_.DefName -and -not $_.DefName.StartsWith($prefix) -and -not $_.DefName.StartsWith('Baak') })
        if ($bad.Count -gt 0) {
            Add-Issue -Severity Info -Check 'def-prefix' `
                -Message "有 $($bad.Count) 个 defName 不以 $prefix 开头" `
                -Detail (($bad | Select-Object -First 12 | ForEach-Object { $_.DefName }) -join ', ')
        }
    }
}

function Test-DefNameKnown {
    param([string]$Name, [string]$TypeHint, $KnownNames)
    if (-not $Name) { return $false }
    if ($TypeHint -and $script:Rw.TypeDefNames.ContainsKey($TypeHint)) {
        return $script:Rw.TypeDefNames[$TypeHint].Contains($Name)
    }
    return $KnownNames.Contains($Name)
}

function Test-ModLanguages {
    param(
        [Parameter(Mandatory)]$Mod,
        [Parameter(Mandatory)]$Layout,
        [Parameter(Mandatory)]$Index,
        [Parameter(Mandatory)]$Vanilla,
        [Parameter(Mandatory)]$KnownNames
    )

    $perLang = @{}
    foreach ($folder in $Layout.Folders) {
        $langRoot = if ($folder) { Join-Path (Join-Path $Layout.Root $folder) 'Languages' } else { Join-Path $Layout.Root 'Languages' }
        if (-not (Test-Path $langRoot)) { continue }

        foreach ($langDir in Get-ChildItem $langRoot -Directory) {
            $lang = $langDir.Name
            if (-not $perLang.ContainsKey($lang)) { $perLang[$lang] = @{ Keys = @{} } }

            foreach ($file in Get-ChildItem $langDir.FullName -Recurse -Filter *.xml -File) {
                $rel = $file.FullName.Substring($langDir.FullName.Length).TrimStart('\', '/')
                $seg = $rel.Split([char[]]@('\', '/'))
                # 只处理 <lang>/DefInjected/<DefType>/<file>.xml 这一层
                if ($seg.Count -lt 3 -or $seg[0] -ne 'DefInjected') { continue }
                $typeHint = $seg[1]

                $doc = Read-RwXml -Path $file.FullName -Check 'lang-xml'
                if (-not $doc -or -not $doc.DocumentElement) { continue }

                foreach ($node in $doc.DocumentElement.ChildNodes) {
                    if ($node -isnot [System.Xml.XmlElement]) { continue }
                    $full = $node.LocalName
                    $parts = $full.Split('.')
                    if ($parts.Count -lt 2) {
                        Add-Issue -Severity Warning -Check 'lang-key' -Message "注入键缺少 .字段 后缀：$full" -Path $file.FullName
                        continue
                    }

                    # defName 可能是第一段，也可能被类型名包了一层
                    $defName = $null
                    $candidates = @($parts[0])
                    if ($parts.Count -gt 2) { $candidates += $parts[1] }
                    foreach ($cand in $candidates) {
                        if (Test-DefNameKnown -Name $cand -TypeHint $typeHint -KnownNames $KnownNames) { $defName = $cand; break }
                    }
                    if (-not $defName) {
                        Add-Issue -Severity Warning -Check 'lang-orphan' `
                            -Message "语言注入指向不存在的 Def：$($parts[0])（改名后遗留？）" -Path $file.FullName -Detail $full
                        continue
                    }

                    $key = "$typeHint/$full"
                    if ($perLang[$lang].Keys.ContainsKey($key)) {
                        Add-Issue -Severity Error -Check 'lang-duplicate' -Message "同一语言里重复的键：$key" -Path $file.FullName `
                            -Detail ("另一处在 " + $perLang[$lang].Keys[$key])
                    }
                    else {
                        $perLang[$lang].Keys[$key] = $file.FullName
                        $script:Rw.Stats.LangKeys++
                    }
                }
            }
        }
    }

    # 对齐审计：只报告语言之间的差异，不要求每个 Def 都有语言文件
    $langs = @($Mod.languages)
    $script:Rw.Counters["lang:$($Mod.id)"] = $perLang

    # 只比较「键数量同级」的语言：有的语言故意只靠 Def 内联默认值，不该拿来对齐
    $counts = @{}
    foreach ($l in $langs) { if ($perLang.ContainsKey($l)) { $counts[$l] = $perLang[$l].Keys.Count } }
    $maxCount = 0
    foreach ($c in $counts.Values) { if ($c -gt $maxCount) { $maxCount = $c } }

    for ($i = 0; $i -lt $langs.Count - 1; $i++) {
        $a = $langs[$i]; $b = $langs[$i + 1]
        if (-not $counts.ContainsKey($a) -or -not $counts.ContainsKey($b)) { continue }
        # 某一侧完全没有注入（例如英文靠 Def 内联默认值）时不做对齐比较
        if ($counts[$a] -eq 0 -or $counts[$b] -eq 0) { continue }
        if ($counts[$a] -lt 0.5 * $maxCount -or $counts[$b] -lt 0.5 * $maxCount) {
            Add-Issue -Severity Info -Check 'lang-parity' `
                -Message "$a ($($counts[$a]) 条) 与 $b ($($counts[$b]) 条) 条数差太多，跳过对齐比较"
            continue
        }
        $onlyA = @($perLang[$a].Keys.Keys | Where-Object { -not $perLang[$b].Keys.ContainsKey($_) })
        $onlyB = @($perLang[$b].Keys.Keys | Where-Object { -not $perLang[$a].Keys.ContainsKey($_) })
        if ($onlyA.Count -gt 0) {
            Add-Issue -Severity Info -Check 'lang-parity' `
                -Message "$a 有 $($onlyA.Count) 条键在 $b 里没有" `
                -Detail (($onlyA | Select-Object -First 15) -join ', ')
        }
        if ($onlyB.Count -gt 0) {
            Add-Issue -Severity Info -Check 'lang-parity' `
                -Message "$b 有 $($onlyB.Count) 条键在 $a 里没有" `
                -Detail (($onlyB | Select-Object -First 15) -join ', ')
        }
    }
}
function Test-CrossMod {
    param([Parameter(Mandatory)]$Mods)

    $seen = @{}
    foreach ($mod in $Mods) {
        if (-not $script:Rw.DefIndex.ContainsKey($mod.id)) { continue }
        foreach ($key in $script:Rw.DefIndex[$mod.id].ByKey.Keys) {
            if ($seen.ContainsKey($key)) {
                Add-Issue -Severity Error -Check 'def-cross-mod' `
                    -Message "defName 与 $($seen[$key].Mod) 撞名：$key" `
                    -Path $script:Rw.DefIndex[$mod.id].ByKey[$key].File
            }
            else {
                $seen[$key] = $script:Rw.DefIndex[$mod.id].ByKey[$key]
            }
        }
    }
}

#endregion

#region ---------------------------------------------------------------- 报告

function Write-ValidationReport {
    $issues = @($script:Rw.Issues)
    $errors = @($issues | Where-Object Severity -eq 'Error')
    $warnings = @($issues | Where-Object Severity -eq 'Warning')
    $infos = @($issues | Where-Object Severity -eq 'Info')

    Write-Host ''
    Write-Host '================ 体检结果 ================' -ForegroundColor Cyan
    Write-Host ("错误 {0}   警告 {1}   提示 {2}" -f $errors.Count, $warnings.Count, $infos.Count)
    Write-Host ("扫描 {0} 个 Def / {1} 个贴图引用 / {2} 条语言键" -f `
            $script:Rw.Stats.Defs, $script:Rw.Stats.TexRefs, $script:Rw.Stats.LangKeys) -ForegroundColor DarkGray

    foreach ($group in @($errors, $warnings) | Where-Object { $_.Count -gt 0 }) {
        Write-Host ''
        foreach ($issue in $group) {
            $color = if ($issue.Severity -eq 'Error') { 'Red' } else { 'Yellow' }
            $file = if ($issue.Path) { Split-Path $issue.Path -Leaf } else { '' }
            $line = "  [{0}] {1}" -f $issue.Check, $issue.Message
            if ($file) { $line += "  <$file>" }
            Write-Host $line -ForegroundColor $color
            if ($issue.Detail) { Write-Host "        $($issue.Detail)" -ForegroundColor DarkGray }
        }
    }

    if ($infos.Count -gt 0) {
        Write-Host ''
        foreach ($issue in ($infos | Group-Object Check)) {
            Write-Host ("  [info] {0}: {1} 条" -f $issue.Name, $issue.Count) -ForegroundColor DarkGray
        }
    }

    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $reportDir = Join-Path $script:Rw.Root 'reports'
    if (-not (Test-Path $reportDir)) { New-Item -ItemType Directory -Path $reportDir -Force | Out-Null }
    $reportPath = Join-Path $reportDir "validate-$stamp.md"

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("# 边缘武神 静态体检 $stamp")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("错误 $($errors.Count) / 警告 $($warnings.Count) / 提示 $($infos.Count)")
    [void]$sb.AppendLine()
    foreach ($issue in ($issues | Where-Object { $_.Severity -ne 'Info' } | Sort-Object Severity, Check)) {
        [void]$sb.AppendLine(('- **{0}** `{1}` {2}' -f $issue.Severity, $issue.Check, $issue.Message))
        if ($issue.Path) { [void]$sb.AppendLine(('  - 文件：`{0}`' -f $issue.Path)) }
        if ($issue.Detail) { [void]$sb.AppendLine(('  - 详情：{0}' -f $issue.Detail)) }
    }
    if ($infos.Count -gt 0) {
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('## 提示')
        foreach ($issue in $infos) {
            [void]$sb.AppendLine(('- `{0}` {1}' -f $issue.Check, $issue.Message))
            if ($issue.Detail) { [void]$sb.AppendLine(('  - {0}' -f $issue.Detail)) }
        }
    }

    [System.IO.File]::WriteAllText($reportPath, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
    Write-Host ''
    Write-Host "报告：$reportPath" -ForegroundColor DarkCyan

    if ($errors.Count -gt 0) { return 1 }
    if ($Strict -and $warnings.Count -gt 0) { return 1 }
    return 0
}

#endregion

#region ---------------------------------------------------------------- 子命令

function Invoke-Validate {
    Write-RwStep "校验开始（版本目录 $($script:Rw.Config.mods.Count) 个 mod）"
    $mods = Get-TargetMods
    $vanilla = Get-VanillaIndex
    $textures = Get-TextureIndex

    # 先建全部索引：附属的 ParentName 可能指向本体里的模板
    $knownNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $knownTemplates = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($n in $vanilla.DefNames) { [void]$knownNames.Add($n) }
    foreach ($n in $vanilla.Templates) { [void]$knownNames.Add($n); [void]$knownTemplates.Add($n) }

    $layouts = @{}
    foreach ($mod in $mods) {
        Write-RwStep "→ $($mod.id)"
        $layout = Get-ModLayout -Mod $mod
        $layouts[$mod.id] = $layout
        $index = Get-ModDefIndex -Mod $mod -Layout $layout
        foreach ($n in $index.DefNames) { [void]$knownNames.Add($n) }
        foreach ($n in $index.Templates.Keys) { [void]$knownNames.Add($n); [void]$knownTemplates.Add($n) }
    }

    foreach ($mod in $mods) {
        Test-ModMetadata -Mod $mod
        Test-ModDefs -Mod $mod -Index $script:Rw.DefIndex[$mod.id] -Vanilla $vanilla -Textures $textures -KnownNames $knownNames -KnownTemplates $knownTemplates
        Test-ModLanguages -Mod $mod -Layout $layouts[$mod.id] -Index $script:Rw.DefIndex[$mod.id] -Vanilla $vanilla -KnownNames $knownNames
    }

    Test-CrossMod -Mods $mods
    return (Write-ValidationReport)
}

function Invoke-Snapshot {
    $cfg = $script:Rw.Config
    $mods = Get-TargetMods
    $repoSnapRoot = Join-Path $cfg.repoRoot $cfg.snapshotRoot
    $changed = 0
    $added = 0

    foreach ($mod in $mods) {
        $srcRoot = Join-Path $cfg.modsDir $mod.id
        if (-not (Test-Path $srcRoot)) {
            Add-Issue -Severity Error -Check 'snapshot' -Message '源目录不存在' -Path $srcRoot
            continue
        }
        $destRoot = Join-Path $repoSnapRoot $mod.id

        $targets = @(
            'About',
            'LoadFolders.xml',
            "$($mod.version)/Defs",
            "$($mod.version)/Languages",
            "$($mod.version)/Patches",
            'Patches'
        )
        if ($IncludeTextures) {
            $targets += 'Textures'
            $targets += "$($mod.version)/Textures"
        }

        foreach ($rel in $targets) {
            $src = Join-Path $srcRoot $rel
            if (-not (Test-Path $src)) { continue }

            $files = if (Test-Path $src -PathType Container) {
                Get-ChildItem $src -Recurse -File
            }
            else {
                Get-Item $src
            }

            foreach ($file in $files) {
                $relPath = $file.FullName.Substring($srcRoot.Length).TrimStart('\', '/')
                $dest = Join-Path $destRoot $relPath
                $isNew = -not (Test-Path $dest)
                $needsCopy = $true
                if (-not $isNew) {
                    $existing = Get-Item $dest
                    $needsCopy = ($existing.Length -ne $file.Length) -or
                    ($existing.LastWriteTimeUtc -ne $file.LastWriteTimeUtc)
                }
                if (-not $needsCopy) { continue }
                if ($isNew) { $added++ } else { $changed++ }
                if ($DryRun) {
                    Write-Host ("  [dry] {0}/{1}" -f $mod.id, ($relPath -replace '\\', '/')) -ForegroundColor DarkGray
                    continue
                }
                $destDir = Split-Path $dest -Parent
                if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
                Copy-Item $file.FullName $dest -Force
            }
        }
    }

    Write-RwStep "回灌完成：新增 $added / 更新 $changed → $repoSnapRoot"
    if ($DryRun) { return 0 }

    if (-not (Test-Path (Join-Path $cfg.repoRoot '.git'))) {
        Write-RwStep '仓库不是 git 仓库，跳过提交'
        return 0
    }

    $relSnap = $cfg.snapshotRoot
    git -C $cfg.repoRoot add -- $relSnap 2>&1 | Out-Null
    $status = @(git -C $cfg.repoRoot status --porcelain -- $relSnap 2>&1)
    if ($status.Count -eq 0) {
        Write-RwStep '仓库里的快照已是最新，没有可提交的改动'
        return 0
    }

    if (-not $Message) { $Message = "sync mod content $(Get-Date -Format 'yyyy-MM-dd HH:mm')" }
    git -C $cfg.repoRoot commit -m $Message -- $relSnap 2>&1 | ForEach-Object { Write-Host "  $_" -ForegroundColor DarkGray }
    Write-RwStep "已提交：$Message"
    return 0
}

function Invoke-SelfTest {
    <# 自检：坏样本必须报错，真实配置必须通过（工具本身的回归测试） #>
    $hostExe = (Get-Process -Id $PID).Path
    $brokenCfg = Join-Path $script:Rw.Root 'selftest/rw.config.json'
    if (-not (Test-Path $brokenCfg)) {
        Write-Host '缺少 selftest/rw.config.json' -ForegroundColor Red
        return 2
    }

    Write-RwStep '自检 1/2：坏样本应当报错'
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath validate -ConfigPath $brokenCfg 2>&1 | Out-Null
    $codeBroken = $LASTEXITCODE

    Write-RwStep '自检 2/2：真实配置应当通过'
    & $hostExe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath validate 2>&1 | Out-Null
    $codeReal = $LASTEXITCODE

    if ($codeBroken -eq 1 -and $codeReal -eq 0) {
        Write-Host 'SELFTEST PASS：坏样本 exit=1，真实配置 exit=0' -ForegroundColor Green
        return 0
    }
    Write-Host ("SELFTEST FAIL：坏样本 exit=$codeBroken（期望 1），真实配置 exit=$codeReal（期望 0）") -ForegroundColor Red
    return 1
}

function Find-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $inst = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath 2>$null
        if ($inst) {
            $candidate = Join-Path $inst 'MSBuild\Current\Bin\MSBuild.exe'
            if (Test-Path $candidate) { return $candidate }
        }
    }
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { return 'dotnet' }
    return $null
}

function Invoke-Build {
    $cfg = $script:Rw.Config
    $project = Join-Path $cfg.repoRoot $cfg.buildProject
    if (-not (Test-Path $project)) {
        Add-Issue -Severity Error -Check 'build' -Message '工程文件不存在' -Path $project
        return 1
    }

    $msbuild = Find-MSBuild
    if (-not $msbuild) { throw '找不到 MSBuild，也没有 dotnet' }

    $distRoot = Get-DistRoot
    $outDir = Join-Path $distRoot "$Configuration\Assemblies"
    $objDir = Join-Path $distRoot "obj\$Configuration\"
    if (-not $DryRun) {
        foreach ($d in @($outDir, $objDir)) {
            if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
        }
    }

    $msbuildArgs = @(
        $project,
        '/nologo',
        '/v:minimal',
        "/p:Configuration=$Configuration",
        "/p:OutputPath=$outDir\",
        "/p:BaseIntermediateOutputPath=$objDir"
    )

    if ($msbuild -eq 'dotnet') { $msbuildArgs = @('msbuild') + $msbuildArgs }
    Write-RwStep ("编译 {0} [{1}]" -f (Split-Path $project -Leaf), $Configuration)
    if ($DryRun) {
        Write-Host ("  [dry] {0} {1}" -f $msbuild, ($msbuildArgs -join ' ')) -ForegroundColor DarkGray
        return 0
    }

    & $msbuild @msbuildArgs
    if ($LASTEXITCODE -ne 0) {
        Add-Issue -Severity Error -Check 'build' -Message "编译失败（exit $LASTEXITCODE）"
        return 1
    }

    $dll = Join-Path $outDir 'RWrd.dll'
    if (-not (Test-Path $dll)) {
        Add-Issue -Severity Error -Check 'build' -Message "编译结束但没找到 $dll"
        return 1
    }
    Write-RwStep ("产物：{0} ({1:N0} bytes)" -f $dll, (Get-Item $dll).Length)

    if ($Deploy) {
        $target = Join-Path $cfg.modsDir $cfg.mods[0].id
        $target = Join-Path $target (Join-Path $cfg.mods[0].version 'Assemblies')
        if (-not (Test-Path $target)) { New-Item -ItemType Directory -Path $target -Force | Out-Null }
        foreach ($ext in @('dll', 'pdb')) {
            $src = Join-Path $outDir "RWrd.$ext"
            if (Test-Path $src) {
                Copy-Item $src $target -Force
                Write-RwStep "已部署：$(Join-Path $target "RWrd.$ext")"
            }
        }
    }
    return 0
}

function Invoke-Package {
    $cfg = $script:Rw.Config
    $mods = Get-TargetMods
    $distRoot = Get-DistRoot
    if (-not $DryRun -and -not (Test-Path $distRoot)) { New-Item -ItemType Directory -Path $distRoot -Force | Out-Null }

    $code = 0
    foreach ($mod in $mods) {
        $srcRoot = Join-Path $cfg.modsDir $mod.id
        if (-not (Test-Path $srcRoot)) {
            Add-Issue -Severity Error -Check 'package' -Message '源目录不存在' -Path $srcRoot
            $code = 1
            continue
        }

        $AboutPath = Join-Path $srcRoot 'About/About.xml'
        $version = '0.0.0'
        if (Test-Path $AboutPath) {
            $doc = Read-RwXml -Path $AboutPath -Check 'package'
            $node = if ($doc) { $doc.SelectSingleNode('/ModMetaData/modVersion') } else { $null }
            if ($node) { $version = $node.InnerText.Trim() }
        }
        else {
            Add-Issue -Severity Error -Check 'package' -Message '缺少 About/About.xml' -Path $srcRoot
            $code = 1
        }

        $zipName = "$($mod.id)-$version.zip"
        $zipPath = Join-Path $distRoot $zipName
        $stage = Join-Path $distRoot ("stage\" + $mod.id)

        Write-RwStep ("打包 {0} ({1})" -f $mod.id, $version)
        if ($DryRun) {
            Write-Host "  [dry] $srcRoot → $zipPath" -ForegroundColor DarkGray
            continue
        }

        if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
        New-Item -ItemType Directory -Path $stage -Force | Out-Null

        $exclude = @('bin', 'obj', '.vs', '.git', 'Source', 'source')
        foreach ($item in Get-ChildItem $srcRoot -Force) {
            if ($exclude -contains $item.Name) { continue }
            $dest = Join-Path $stage $item.Name
            if ($item.PSIsContainer) { Copy-Item $item.FullName $dest -Recurse -Force }
            else { Copy-Item $item.FullName $dest -Force }
        }

        if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
        Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath -CompressionLevel Optimal
        Write-RwStep ("→ {0} ({1:N1} MB)" -f $zipPath, ((Get-Item $zipPath).Length / 1MB))
        Remove-Item $stage -Recurse -Force
    }
    return $code
}

#endregion

#region ---------------------------------------------------------------- 入口

try {
    $script:Rw.Config = Get-RwConfig
}
catch {
    Write-Host "配置读取失败：$($_.Exception.Message)" -ForegroundColor Red
    exit 2

}

. (Join-Path $script:Rw.Root 'defs.ps1')

switch ($Command) {
    'validate' {
        $code = Invoke-Validate
        exit $code
    }
    'snapshot' {
        $code = Invoke-Snapshot
        exit $code
    }
    'build' {
        $code = Invoke-Build
        exit $code
    }
    'package' {
        $code = Invoke-Package
        exit $code
    }
    'selftest' {
        $code = Invoke-SelfTest
        exit $code
    }
    'search' {
        $code = Invoke-DefSearch -Query $Query -Type $Type -Like $Like -Limit $Limit -AllMods:$All
        exit $code
    }
    'show' {
        $code = Invoke-DefShow -Query $Query
        exit $code
    }
    'fields' {
        $code = Invoke-DefFields -Type $Type -FieldMatch $FieldMatch -Limit $Limit -AllMods:$All
        exit $code
    }
    'clone' {
        $code = Invoke-DefClone -Query $Query -DefName $DefName -Out $Out -AppendTo $AppendTo -Label $Label -Description $Description -Force:$Force
        exit $code
    }
    'check' {
        $code = Invoke-DefCheck -Path $Path -AllMods:$All
        exit $code
    }
    'lang' {
        $code = Invoke-RwLang -Mod $Mod -Todo:$Todo
        exit $code
    }
    default {
        Get-Help $PSCommandPath -Detailed
        exit 0
    }
}

#endregion



