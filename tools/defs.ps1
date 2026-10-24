# defs.ps1 — Def 写作工具：查原版、抄现成、写完自查
# 由 rw.ps1 点源加载，共用 $script:Rw（配置、缓存、统计）。
# 依赖 rw.ps1 里的：Read-RwXml / Get-CacheDir / Get-VanillaIndex / Get-TextureIndex / Get-AssemblyText

function Get-GameVersionShort {
    $verFile = Join-Path $script:Rw.Config.gameDir 'Version.txt'
    if (Test-Path $verFile) {
        $v = (Get-Content $verFile -Raw).Trim()
        if ($v -match '^(\d+\.\d+)') { return $Matches[1] }
    }
    return '1.6'
}

function Get-DefSources {
    <# 收集要索引的 Defs 目录：原版 + 自己的模组（可选全部模组） #>
    param([switch]$AllMods)

    $cfg = $script:Rw.Config
    $gameVer = Get-GameVersionShort
    $sources = [System.Collections.Generic.List[object]]::new()

    foreach ($pack in Get-ChildItem (Join-Path $cfg.gameDir 'Data') -Directory) {
        $dir = Join-Path $pack.FullName 'Defs'
        if (Test-Path $dir) {
            $sources.Add([pscustomobject]@{ Name = $pack.Name; Kind = 'vanilla'; Dir = $dir })
        }
    }

    $modDirs = @()
    if ($AllMods) {
        $modDirs = @(Get-ChildItem $cfg.modsDir -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'About/About.xml') })
    }
    else {
        foreach ($mod in $cfg.mods) {
            $p = Join-Path $cfg.modsDir $mod.id
            if (Test-Path $p) { $modDirs += Get-Item $p }
        }
    }

    foreach ($m in $modDirs) {
        foreach ($sub in @('', $gameVer)) {
            $dir = if ($sub) { Join-Path (Join-Path $m.FullName $sub) 'Defs' } else { Join-Path $m.FullName 'Defs' }
            if (Test-Path $dir) {
                $sources.Add([pscustomobject]@{ Name = $m.Name; Kind = 'mod'; Dir = $dir })
            }
        }
    }
    return $sources
}

function Get-DefCatalog {
    <# 索引所有 Def 的「类型 / defName / 模板名 / 父模板 / 文件」，用于搜索与克隆 #>
    param([switch]$AllMods)

    $key = if ($AllMods) { 'catalog-all' } else { 'catalog' }
    if ($script:Rw.Counters.ContainsKey($key)) { return $script:Rw.Counters[$key] }

    $sources = Get-DefSources -AllMods:$AllMods
    $stampParts = foreach ($s in $sources) { '{0}|{1}|{2}' -f $s.Name, $s.Dir, (Get-Item $s.Dir).LastWriteTimeUtc.Ticks }
    $stamp = (($stampParts -join ';') + "|v" + (Get-GameVersionShort)).GetHashCode().ToString()
    $cachePath = Join-Path (Get-CacheDir) ($key + '.json')

    $cached = $null
    if (-not $Refresh -and (Test-Path $cachePath)) {
        try {
            $raw = Get-Content $cachePath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($raw.stamp -eq $stamp) { $cached = $raw }
        }
        catch { }
    }
    if ($cached) {
        $entry = @{ Stamp = $stamp; Defs = $cached.defs; Templates = $cached.templates; Types = $cached.types }
        $script:Rw.Counters[$key] = $entry
        return $entry
    }

    Write-RwStep ("索引 Def 目录（{0} 个来源）…" -f $sources.Count)
    $defs = [System.Collections.Generic.List[object]]::new()
    $templates = [System.Collections.Generic.List[object]]::new()
    $types = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)

    foreach ($s in $sources) {
        foreach ($file in Get-ChildItem $s.Dir -Recurse -Filter *.xml -File) {
            $doc = Read-RwXml -Path $file.FullName -Check 'catalog'
            if (-not $doc -or -not $doc.DocumentElement) { continue }
            foreach ($node in $doc.DocumentElement.ChildNodes) {
                if ($node -isnot [System.Xml.XmlElement]) { continue }
                [void]$types.Add($node.LocalName)
                $dn = $node.SelectSingleNode('defName')
                $label = $node.SelectSingleNode('label')
                $rec = [pscustomobject]@{
                    Type     = $node.LocalName
                    DefName  = $(if ($dn) { $dn.InnerText.Trim() } else { $null })
                    Template = $node.GetAttribute('Name')
                    Parent   = $node.GetAttribute('ParentName')
                    Label    = $(if ($label) { $label.InnerText.Trim() } else { '' })
                    Source   = $s.Name
                    File     = $file.FullName
                }
                if ($rec.DefName) { $defs.Add($rec) }
                if ($rec.Template) { $templates.Add($rec) }
            }
        }
    }

    [System.IO.File]::WriteAllText($cachePath, (@{
                stamp     = $stamp
                defs      = @($defs)
                templates = @($templates)
                types     = @($types)
            } | ConvertTo-Json -Depth 3 -Compress), [System.Text.UTF8Encoding]::new($false))

    $entry = @{ Stamp = $stamp; Defs = $defs; Templates = $templates; Types = @($types) }
    $script:Rw.Counters[$key] = $entry
    Write-RwStep ("索引完成：{0} 个 Def / {1} 个模板" -f $defs.Count, $templates.Count)
    return $entry
}

function Resolve-DefRecord {
    <# 接受 "ThingDef/Apparel_Cape" 或 "Apparel_Cape" #>
    param($Catalog, [string]$Query)
    if (-not $Query) { return $null }

    if ($Query -match '^(.+?)/(.+)$') {
        $type = $Matches[1]; $name = $Matches[2]
        $hit = $Catalog.Defs | Where-Object { $_.DefName -eq $name -and $_.Type -eq $type } | Select-Object -First 1
        if ($hit) { return $hit }
    }
    return ($Catalog.Defs | Where-Object { $_.DefName -eq $Query } | Select-Object -First 1)
}

function Get-DefNode {
    <# 从文件里取出指定 defName 的节点 #>
    param([string]$File, [string]$DefName)
    $doc = Read-RwXml -Path $File -Check 'def-node'
    if (-not $doc) { return $null }
    foreach ($node in $doc.DocumentElement.ChildNodes) {
        if ($node -isnot [System.Xml.XmlElement]) { continue }
        $dn = $node.SelectSingleNode('defName')
        if ($dn -and $dn.InnerText.Trim() -eq $DefName) { return $node }
    }
    return $null
}

function Get-DefFieldIndex {
    <# 某类 Def 在原版+现有模组里用过的所有字段路径（含继承来的），带出现次数与样例 #>
    param([Parameter(Mandatory)][string]$Type, [switch]$AllMods)

    $key = "fields:$Type"
    if ($script:Rw.Counters.ContainsKey($key)) { return $script:Rw.Counters[$key] }

    $catalog = Get-DefCatalog -AllMods:$AllMods
    $cacheName = 'fields-' + ($Type -replace '[^\w.-]', '_') + $(if ($AllMods) { '-all' } else { '' }) + '.json'
    $cachePath = Join-Path (Get-CacheDir) $cacheName

    $cached = $null
    if (-not $Refresh -and (Test-Path $cachePath)) {
        try {
            $raw = Get-Content $cachePath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($raw.stamp -eq $catalog.Stamp) { $cached = $raw }
        }
        catch { }
    }

    if ($cached) {
        $index = @{}
        foreach ($p in $cached.fields.PSObject.Properties) { $index[$p.Name] = @{ n = $p.Value.count; s = $p.Value.sample } }
        $script:Rw.Counters[$key] = $index
        return $index
    }

    Write-RwStep "统计 $Type 的字段用法…"
    $targets = @($catalog.Defs | Where-Object { $_.Type -eq $Type })
    $index = @{}

    foreach ($def in $targets) {
        $node = Get-DefNode -File $def.File -DefName $def.DefName
        if (-not $node) { continue }

        $stack = [System.Collections.Generic.Stack[object]]::new()
        $stack.Push(@($node, ''))
        while ($stack.Count -gt 0) {
            $item = $stack.Pop()
            $el = $item[0]; $prefix = [string]$item[1]
            foreach ($child in $el.ChildNodes) {
                if ($child -isnot [System.Xml.XmlElement]) { continue }
                $p = if ($prefix) { "$prefix/$($child.LocalName)" } else { $child.LocalName }
                if (-not $index.ContainsKey($p)) {
                    $sample = ''
                    if (-not $child.HasChildNodes) { $sample = $child.InnerText.Trim() }
                    if ($sample.Length -gt 60) { $sample = $sample.Substring(0, 60) + '…' }
                    $index[$p] = @{ n = 1; s = $sample }
                }
                else { $index[$p].n++ }

                foreach ($a in $child.Attributes) {
                    $ap = "$p/@$($a.LocalName)"
                    if (-not $index.ContainsKey($ap)) {
                        $index[$ap] = @{ n = 1; s = $a.Value.Trim() }
                    }
                    else { $index[$ap].n++ }
                }
                $stack.Push(@($child, $p))
            }
        }
    }

    $flat = [ordered]@{}
    foreach ($kv in $index.GetEnumerator()) { $flat[$kv.Key] = @{ count = $kv.Value.n; sample = $kv.Value.s } }
    [System.IO.File]::WriteAllText($cachePath, (@{ stamp = $catalog.Stamp; fields = $flat } | ConvertTo-Json -Depth 4 -Compress),
        [System.Text.UTF8Encoding]::new($false))

    $script:Rw.Counters[$key] = $index
    return $index
}

function Invoke-DefSearch {
    param([string]$Query, [string]$Type, [string]$Like, [int]$Limit = 40, [switch]$AllMods)

    $catalog = Get-DefCatalog -AllMods:$AllMods
    $pattern = if ($Like) { $Like } elseif ($Query) { "*$Query*" } else { $null }
    if (-not $pattern -and -not $Type) {
        Write-Host '至少给 -Like（模式，支持 *）或 -Type' -ForegroundColor Yellow
        return 1
    }

    $hits = $catalog.Defs
    if ($Type) { $hits = $hits | Where-Object { $_.Type -ieq $Type } }
    if ($pattern) {
        $hits = $hits | Where-Object { $_.DefName -like $pattern -or $_.Label -like $pattern }
    }

    $hits = @($hits)
    Write-Host ("命中 {0} 个{1}" -f $hits.Count, $(if ($hits.Count -gt $Limit) { "，显示前 $Limit 个" } else { '' })) -ForegroundColor Cyan
    $hits | Select-Object -First $Limit | ForEach-Object {
        Write-Host (('{0,-22} {1,-40} {2,-14} {3}' -f $_.Type, $_.DefName, $_.Source, [System.IO.Path]::GetFileName($_.File)))
    }
    return 0
}

function Invoke-DefShow {
    param([string]$Query)

    $catalog = Get-DefCatalog
    $rec = Resolve-DefRecord -Catalog $catalog -Query $Query
    if (-not $rec) {
        Write-Host "找不到 Def：$Query" -ForegroundColor Red
        return 1
    }

    Write-Host ("== {0}/{1}  ({2})" -f $rec.Type, $rec.DefName, $rec.Source) -ForegroundColor Cyan
    Write-Host ("   文件：{0}" -f $rec.File) -ForegroundColor DarkGray

    $node = Get-DefNode -File $rec.File -DefName $rec.DefName
    if (-not $node) {
        Write-Host '文件已变化，重新索引后再试' -ForegroundColor Red
        return 1
    }
    Write-Host $node.OuterXml

    # 父模板链：继承了什么、模板在哪
    $parent = $rec.Parent
    $depth = 0
    while ($parent -and $depth -lt 8) {
        $p = $catalog.Templates | Where-Object { $_.Template -eq $parent } | Select-Object -First 1
        if (-not $p) {
            Write-Host ("← ParentName '{0}' 未在索引中找到（原版内建模板或拼错）" -f $parent) -ForegroundColor DarkGray
            break
        }
        Write-Host ("← 继承自模板 '{0}'（{1}，{2}）" -f $p.Template, $p.Source, [System.IO.Path]::GetFileName($p.File)) -ForegroundColor DarkGray
        $parent = $p.Parent
        $depth++
    }
    return 0
}

function Invoke-DefFields {
    param([Parameter(Mandatory)][string]$Type, [string]$FieldMatch, [int]$Limit = 60, [switch]$AllMods)

    $index = Get-DefFieldIndex -Type $Type -AllMods:$AllMods
    if ($index.Count -eq 0) {
        Write-Host "没有找到类型为 $Type 的 Def" -ForegroundColor Yellow
        return 1
    }

    $rows = $index.GetEnumerator() | Sort-Object { $_.Value.n } -Descending
    if ($FieldMatch) { $rows = $rows | Where-Object { $_.Key -match $FieldMatch } }
    $rows = @($rows)

    Write-Host ("{0}：{1} 条字段路径{2}" -f $Type, $rows.Count, $(if ($FieldMatch) { "（匹配 $FieldMatch）" } else { '' })) -ForegroundColor Cyan
    $rows | Select-Object -First $Limit | ForEach-Object {
    Write-Host (('{0,-46} x{1,-5} {2}' -f $_.Key, $_.Value.n, $_.Value.s))
    }
    if ($rows.Count -gt $Limit) { Write-Host ("…还有 {0} 条，用 -FieldMatch 收窄" -f ($rows.Count - $Limit)) -ForegroundColor DarkGray }
    return 0
}

function Invoke-DefClone {
    param([string]$Query, [string]$DefName, [string]$Out, [string]$AppendTo, [string]$Label, [string]$Description, [switch]$Force)

    if (-not $DefName) {
        Write-Host '需要 -DefName（新 def 名）' -ForegroundColor Yellow
        return 1
    }

    $catalog = Get-DefCatalog
    $rec = Resolve-DefRecord -Catalog $catalog -Query $Query
    if (-not $rec) {
        Write-Host "找不到要克隆的 Def：$Query" -ForegroundColor Red
        return 1
    }

    $node = Get-DefNode -File $rec.File -DefName $rec.DefName
    if (-not $node) {
        Write-Host '文件已变化，重新索引后再试' -ForegroundColor Red
        return 1
    }

    $clone = $node.CloneNode($true)
    $dn = $clone.SelectSingleNode('defName')
    if (-not $dn) {
        $dn = $clone.OwnerDocument.CreateElement('defName')
        [void]$clone.PrependChild($dn)
    }
    $dn.InnerText = $DefName

    if ($Label) {
        $n = $clone.SelectSingleNode('label')
        if (-not $n) { $n = $clone.OwnerDocument.CreateElement('label'); [void]$clone.PrependChild($n) }
        $n.InnerText = $Label
    }
    if ($Description) {
        $n = $clone.SelectSingleNode('description')
        if (-not $n) { $n = $clone.OwnerDocument.CreateElement('description'); [void]$clone.AppendChild($n) }
        $n.InnerText = $Description
    }

    # 独立成文，缩进 2 空格
    $doc = [System.Xml.XmlDocument]::new()
    $root = $doc.CreateElement('Defs')
    [void]$doc.AppendChild($root)
    [void]$root.AppendChild($doc.ImportNode($clone, $true))

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $settings.OmitXmlDeclaration = $false
    $settings.Encoding = [System.Text.UTF8Encoding]::new($false)

    $sb = [System.Text.StringBuilder]::new()
    $writer = [System.Xml.XmlWriter]::Create($sb, $settings)
    $doc.Save($writer)
    $writer.Close()
    $xml = $sb.ToString() -replace 'encoding="utf-16"', 'encoding="utf-8"'

    if ($AppendTo) {
        if (-not (Test-Path $AppendTo)) {
            Write-Host "-AppendTo 指定的文件不存在：$AppendTo" -ForegroundColor Red
            return 1
        }
        $text = [System.IO.File]::ReadAllText($AppendTo)
        $idx = $text.LastIndexOf('</Defs>')
        if ($idx -lt 0) {
            Write-Host "-AppendTo 文件里没有 </Defs>" -ForegroundColor Red
            return 1
        }
        $inner = $xml -replace '(?s)^.*?<Defs>\s*', '' -replace '(?s)\s*</Defs>\s*$', ''
        $updated = $text.Substring(0, $idx) + "`r`n" + $inner + "`r`n" + $text.Substring($idx)
        if ($DryRun) { Write-Host "[dry] 追加到 $AppendTo" -ForegroundColor DarkGray; return 0 }
        [System.IO.File]::WriteAllText($AppendTo, $updated, [System.Text.UTF8Encoding]::new($false))
        Write-RwStep "已追加到 $AppendTo"
    }
    else {
        if (-not $Out) { $Out = Join-Path $script:Rw.Root ("out\" + $DefName + '.xml') }
        if ((Test-Path $Out) -and -not $Force) {
            Write-Host "目标已存在：$Out（加 -Force 覆盖）" -ForegroundColor Yellow
            return 1
        }
        if ($DryRun) { Write-Host "[dry] 写入 $Out" -ForegroundColor DarkGray; return 0 }
        $dir = Split-Path $Out -Parent
        if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        [System.IO.File]::WriteAllText($Out, $xml, [System.Text.UTF8Encoding]::new($false))
        Write-RwStep "已写入 $Out"
    }

    Write-Host ("克隆自 {0}/{1}（{2}）" -f $rec.Type, $rec.DefName, $rec.Source) -ForegroundColor DarkGray
    if (-not $Label -or -not $Description) {
        Write-Host '提示：label/description 沿用了原 Def 的文本，记得改成自己的' -ForegroundColor DarkGray
    }
    return 0
}

function Invoke-DefCheck {
    <# 写作时自查单个文件（或目录）：类型、defName、字段名是否是原版/现有 Def 用过的 #>
    param([string]$Path, [switch]$AllMods)

    if (-not $Path) {
        Write-Host '需要 -Path（要检查的 xml 文件或目录）' -ForegroundColor Yellow
        return 1
    }
    if (-not (Test-Path $Path)) {
        Write-Host "路径不存在：$Path" -ForegroundColor Red
        return 1
    }

    $files = @(if (Test-Path $Path -PathType Container) { Get-ChildItem $Path -Recurse -Filter *.xml -File } else { Get-Item $Path })
    $catalog = Get-DefCatalog -AllMods:$AllMods
    $textures = Get-TextureIndex
    $vanilla = Get-VanillaIndex

    $bad = 0
    $defCount = 0
    $unknownFields = 0

    foreach ($file in $files) {
        $doc = Read-RwXml -Path $file.FullName -Check 'check'
        if (-not $doc -or -not $doc.DocumentElement) { $bad++; continue }

        foreach ($node in $doc.DocumentElement.ChildNodes) {
            if ($node -isnot [System.Xml.XmlElement]) { continue }
            $defCount++
            $type = $node.LocalName
            $dnNode = $node.SelectSingleNode('defName')
            $defName = if ($dnNode) { $dnNode.InnerText.Trim() } else { '' }

            if (-not $catalog.Types -contains $type) {
                Write-Host ("  [{0}] 未知 Def 类型 <{1}>" -f [System.IO.Path]::GetFileName($file.Name), $type) -ForegroundColor Red
                $bad++
            }
            if (-not $defName -and $node.GetAttribute('Abstract') -ne 'True') {
                Write-Host ("  [{0}] 缺少 defName（{1}）" -f [System.IO.Path]::GetFileName($file.Name), $type) -ForegroundColor Red
                $bad++
                continue
            }
            if (-not $defName) { continue }

            $fields = Get-DefFieldIndex -Type $type -AllMods:$AllMods
            foreach ($p in (Get-DefPathsFlat -Node $node)) {
                if (-not $fields.ContainsKey($p)) {
                    Write-Host ("  [{0}] {1}: 字段 {2} 在原版/现有 Def 里没出现过" -f [System.IO.Path]::GetFileName($file.Name), $defName, $p) -ForegroundColor Yellow
                    $unknownFields++
                }
            }

            # 贴图
            foreach ($p in @('texPath', 'iconPath', 'graphicPath', 'uiIconPath', 'factionIconPath', 'royalFavorIconPath')) {
                foreach ($pn in $node.SelectNodes(".//$p")) {
                    $rel = ($pn.InnerText.Trim() -replace '\\', '/').Trim('/')
                    if (-not $rel) { continue }
                    if ($rel -match '\.(png|jpg|psd)$') { $rel = $rel.Substring(0, $rel.LastIndexOf('.')) }
                    if ($textures.ContainsKey($rel) -or $vanilla.TexPaths.Contains($rel)) { continue }
                    $hit = $false
                    foreach ($k in $textures.Keys) {
                        if ($k.StartsWith("$rel/", [StringComparison]::OrdinalIgnoreCase) -or $k.StartsWith("$rel`_", [StringComparison]::OrdinalIgnoreCase)) { $hit = $true; break }
                    }
                    if (-not $hit) {
                        Write-Host ("  [{0}] {1}: 贴图找不到 {2}" -f [System.IO.Path]::GetFileName($file.Name), $defName, $rel) -ForegroundColor Red
                        $bad++
                    }
                }
            }
        }
    }

    Write-Host ''
    Write-Host ("检查 {0} 个文件 / {1} 个 Def：错误 {2}，字段疑点 {3}" -f $files.Count, $defCount, $bad, $unknownFields) -ForegroundColor Cyan
    if ($bad -gt 0) { return 1 }
    return 0
}

function Get-DefPathsFlat {
    <# 收集一个 Def 节点下的字段路径（含属性） #>
    param($Node)
    $out = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $stack = [System.Collections.Generic.Stack[object]]::new()
    $stack.Push(@($Node, ''))
    while ($stack.Count -gt 0) {
        $item = $stack.Pop()
        $el = $item[0]; $prefix = [string]$item[1]
        foreach ($child in $el.ChildNodes) {
            if ($child -isnot [System.Xml.XmlElement]) { continue }
            $p = if ($prefix) { "$prefix/$($child.LocalName)" } else { $child.LocalName }
            [void]$out.Add($p)
            foreach ($a in $child.Attributes) { [void]$out.Add("$p/@$($a.LocalName)") }
            $stack.Push(@($child, $p))
        }
    }
    return $out
}


#region ---------------------------------------------------------------- 语言（lang）

function ConvertTo-RwNormalizedHandle {
    <# 把 label / 类名 / Def 名归一化成 DefInjected 键里用的 handle。
       对齐 Verse.TranslationHandleUtility.NormalizedHandle：空格换行制表→_，去掉 . 和 -，
       去掉 {占位符}，只保留字母数字 _ -，折叠连续 _，纯数字前补 _。大小写保留。 #>
    param([string]$Text)

    if ($null -eq $Text -or $Text.Length -eq 0) { return $Text }
    $s = $Text.Trim()
    $s = $s.Replace("`r`n", '').Replace("`r", '_').Replace("`n", '_').Replace("`t", '_').Replace(' ', '_')
    $s = $s.Replace('.', '').Replace('-', '')
    $s = [regex]::Replace($s, '\{.*?\}', '')
    $s = [regex]::Replace($s, '[^A-Za-z0-9_-]', '')
    while ($s.Contains('__')) { $s = $s.Replace('__', '_') }
    $s = $s.Trim('_')
    if ($s.Length -gt 0 -and $s -match '^\d+$') { $s = '_' + $s }
    return $s
}

function Join-RwLangSegment {
    param([string]$Prefix, [string]$Segment)
    if ([string]::IsNullOrEmpty($Prefix)) { return $Segment }
    return $Prefix + '.' + $Segment
}

function ConvertTo-RwLangPathPattern {
    <# 把白名单里的具体键路径折叠成字段路径：列表元素段替换为 <n>（数字段一律折叠）。 #>
    param([string]$Path, $Containers)

    $segs = $Path.Split('.')
    for ($i = 0; $i -lt $segs.Length; $i++) {
        $s = $segs[$i]
        if ($s -match '^\d+$') { $segs[$i] = '<n>'; continue }
        # 命名 handle 只出现在列表中间位置；列表本身也可能是最后一段（字符串列表），但那一定是数字段
        if ($i -gt 0 -and $i -lt ($segs.Length - 1) -and $Containers.Contains($segs[$i - 1])) {
            $segs[$i] = '<n>'
        }
    }
    return ($segs -join '.')
}

function Get-RwLangTarFiles {
    <# 原版 + DLC 的简体中文 DefInjected tar（未解包） #>
    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'
    if (-not (Test-Path $dataDir)) { throw "找不到原版数据目录：$dataDir" }

    $list = [System.Collections.Generic.List[object]]::new()
    foreach ($pack in (Get-ChildItem $dataDir -Directory | Sort-Object Name)) {
        $langDir = Join-Path $pack.FullName 'Languages'
        if (-not (Test-Path $langDir)) { continue }
        foreach ($f in (Get-ChildItem $langDir -File -Filter 'ChineseSimplified*.tar')) {
            $list.Add([pscustomobject]@{
                    Pack   = $pack.Name
                    Path   = $f.FullName
                    Length = $f.Length
                    Ticks  = $f.LastWriteTimeUtc.Ticks
                })
        }
    }
    if ($list.Count -eq 0) {
        throw "没有找到原版简体中文 tar：$dataDir\*\Languages\ChineseSimplified*.tar"
    }
    return $list
}

function Read-RwLangTarEntries {
    <# 按 POSIX ustar 解析 tar（512 字节头，内容 512 对齐），只返回 DefInjected/*.xml 的文本。
       Windows PowerShell 5.1 / .NET Framework 4.8 没有现成 tar API，这里手写。 #>
    param([Parameter(Mandatory)][string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $entries = [System.Collections.Generic.List[object]]::new()
    $pos = 0
    $longName = $null

    while ($pos + 512 -le $bytes.Length) {
        $allZero = $true
        for ($i = 0; $i -lt 512; $i++) {
            if ($bytes[$pos + $i] -ne 0) { $allZero = $false; break }
        }
        if ($allZero) { break }

        $name = [System.Text.Encoding]::UTF8.GetString($bytes, $pos, 100).Trim([char]0)
        $prefix = [System.Text.Encoding]::UTF8.GetString($bytes, $pos + 345, 155).Trim([char]0)
        if ($prefix) { $name = $prefix + '/' + $name }

        $sizeText = [System.Text.Encoding]::ASCII.GetString($bytes, $pos + 124, 12).Trim([char[]]@(0, 32))
        $size = 0
        if ($sizeText) {
            try { $size = [Convert]::ToInt64($sizeText.Trim(), 8) }
            catch { throw "tar 头里的 size 不是八进制（$Path）：$sizeText" }
        }

        $flag = [int]$bytes[$pos + 156]
        $dataStart = $pos + 512
        $padded = [int64]([math]::Ceiling($size / 512.0) * 512)
        $next = $dataStart + $padded

        if ($flag -eq 76) { # 'L'：GNU long name，下一项使用它
            $longName = [System.Text.Encoding]::UTF8.GetString($bytes, $dataStart, $size).Trim([char]0)
            $pos = $next
            continue
        }
        if ($flag -eq 120 -or $flag -eq 103) { # 'x' / 'g'：pax 扩展头，跳过
            $pos = $next
            continue
        }
        if ($longName) { $name = $longName; $longName = $null }

        if (($flag -eq 0 -or $flag -eq 48) -and $name -match '^DefInjected/.+\.xml$') {
            $text = [System.Text.Encoding]::UTF8.GetString($bytes, $dataStart, $size)
            if ($text.Length -gt 0 -and [int][char]$text[0] -eq 0xFEFF) { $text = $text.Substring(1) }
            $entries.Add([pscustomobject]@{ Name = $name; Text = $text })
        }
        $pos = $next
    }
    return $entries
}

function Get-RwLangOfficialContainers {
    <# 原版 Def XML 里，所有「直接包含 <li>」的元素名。用来判断某个键段是不是列表元素。 #>
    $set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'

    foreach ($pack in (Get-ChildItem $dataDir -Directory)) {
        $defsDir = Join-Path $pack.FullName 'Defs'
        if (-not (Test-Path $defsDir)) { continue }
        foreach ($file in (Get-ChildItem $defsDir -Recurse -Filter *.xml -File)) {
            $doc = New-Object System.Xml.XmlDocument
            try { $doc.Load($file.FullName) } catch { continue }
            if (-not $doc.DocumentElement) { continue }

            $stack = [System.Collections.Generic.Stack[object]]::new()
            $stack.Push($doc.DocumentElement)
            while ($stack.Count -gt 0) {
                $el = $stack.Pop()
                $hasLi = $false
                foreach ($child in $el.ChildNodes) {
                    if ($child -is [System.Xml.XmlElement]) {
                        if ($child.LocalName -eq 'li') { $hasLi = $true }
                        $stack.Push($child)
                    }
                }
                if ($hasLi) { [void]$set.Add($el.LocalName) }
            }
        }
    }
    return $set
}

function Get-RwLangWhitelist {
    <# 扫描原版简体中文 tar，得到每个 DefType「原版实际翻译过的字段路径」白名单。
       缓存：.cache\lang-whitelist-<DefType>.json + lang-whitelist-manifest.json。 #>
    $cacheKey = 'lang-whitelist'
    if (-not $Refresh -and $script:Rw.Counters.ContainsKey($cacheKey)) { return $script:Rw.Counters[$cacheKey] }

    $tars = Get-RwLangTarFiles
    $dataDir = Join-Path $script:Rw.Config.gameDir 'Data'
    $stampParts = [System.Collections.Generic.List[string]]::new()
    foreach ($t in $tars) {
        $stampParts.Add(('tar|{0}|{1}|{2}|{3}' -f $t.Pack, $t.Path, $t.Length, $t.Ticks))
    }
    foreach ($pack in (Get-ChildItem $dataDir -Directory | Sort-Object Name)) {
        $defsDir = Join-Path $pack.FullName 'Defs'
        if (Test-Path $defsDir) {
            $stampParts.Add(('defs|{0}|{1}' -f $pack.Name, (Get-Item $defsDir).LastWriteTimeUtc.Ticks))
        }
    }
    $stamp = ($stampParts -join ';')

    $cacheDir = Get-CacheDir
    $manifestPath = Join-Path $cacheDir 'lang-whitelist-manifest.json'
    $manifest = $null
    if (-not $Refresh -and (Test-Path $manifestPath)) {
        try {
            $raw = Get-Content $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ($raw.version -eq 2 -and $raw.stamp -eq $stamp) { $manifest = $raw }
        }
        catch { $manifest = $null }
    }

    if ($manifest) {
        $types = @{}
        $stats = [System.Collections.Generic.List[object]]::new()
        $ok = $true
        foreach ($t in @($manifest.types)) {
            $fileName = 'lang-whitelist-' + ($t.type -replace '[^\w.-]', '_') + '.json'
            $file = Join-Path $cacheDir $fileName
            if (-not (Test-Path $file)) { $ok = $false; break }
            try {
                $tj = Get-Content $file -Raw -Encoding UTF8 | ConvertFrom-Json
                $set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
                foreach ($p in @($tj.patterns)) { [void]$set.Add([string]$p) }
                $types[[string]$t.type] = $set
                $stats.Add([pscustomobject]@{ Type = [string]$t.type; PatternCount = $set.Count; KeyCount = [int]$tj.keys; Numeric = [int]$tj.numeric; Named = [int]$tj.named; Plain = [int]$tj.plain })
            }
            catch { $ok = $false; break }
        }
        if ($ok) {
            $containers = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            foreach ($c in @($manifest.containers)) { [void]$containers.Add([string]$c) }
            $result = [pscustomobject]@{
                Types      = $types
                Containers = $containers
                Stats      = $stats
                TypeCount  = $types.Count
                PatternCount = [int]$manifest.patternTotal
                KeyCount   = [int]$manifest.keyTotal
                NumericKeyCount = [int]$manifest.numericTotal
                NamedKeyCount   = [int]$manifest.namedTotal
                PlainKeyCount   = [int]$manifest.plainTotal
                TarCount   = $tars.Count
                Stamp      = $stamp
            }
            $script:Rw.Counters[$cacheKey] = $result
            return $result
        }
    }

    Write-RwStep ("解析原版中文 tar（{0} 个）…" -f $tars.Count)
    $containers = Get-RwLangOfficialContainers
    $typeMap = @{}
    $rawMap = @{}
    $numMap = @{}
    $namedMap = @{}
    $plainMap = @{}
    $keyTotal = 0
    $numTotal = 0
    $namedTotal = 0
    $plainTotal = 0

    foreach ($t in $tars) {
        foreach ($entry in (Read-RwLangTarEntries -Path $t.Path)) {
            if ($entry.Name -notmatch '^DefInjected/([^/]+)/[^/]+\.xml$') { continue }
            $type = $Matches[1]
            $doc = New-Object System.Xml.XmlDocument
            try { $doc.LoadXml($entry.Text) } catch { continue }
            if (-not $doc.DocumentElement) { continue }

            if (-not $typeMap.ContainsKey($type)) {
                $typeMap[$type] = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
                $rawMap[$type] = 0
            }
            $set = $typeMap[$type]

            foreach ($keyNode in $doc.DocumentElement.ChildNodes) {
                if ($keyNode -isnot [System.Xml.XmlElement]) { continue }
                $full = $keyNode.LocalName
                $dot = $full.IndexOf('.')
                if ($dot -lt 1 -or $dot -ge ($full.Length - 1)) { continue }
                $path = $full.Substring($dot + 1)
                $pattern = ConvertTo-RwLangPathPattern -Path $path -Containers $containers
                [void]$set.Add($pattern)
                $rawMap[$type] = $rawMap[$type] + 1
                $keyTotal++

                # 区分三种键形态：数字索引 / 命名句柄（列表元素用 label、Class 等命名）/ 普通字段路径
                $segs = $path.Split('.')
                $hasNum = $false
                $hasNamed = $false
                for ($si = 0; $si -lt $segs.Count; $si++) {
                    if ($segs[$si] -match '^\d+$') { $hasNum = $true; continue }
                    if ($si -gt 0 -and $si -lt ($segs.Count - 1) -and $containers.Contains($segs[$si - 1])) { $hasNamed = $true }
                }
                if ($hasNum) {
                    $numMap[$type] = 1 + [int]$numMap[$type]; $numTotal++
                } elseif ($hasNamed) {
                    $namedMap[$type] = 1 + [int]$namedMap[$type]; $namedTotal++
                } else {
                    $plainMap[$type] = 1 + [int]$plainMap[$type]; $plainTotal++
                }
            }
        }
    }

    $stats = [System.Collections.Generic.List[object]]::new()
    $patternTotal = 0
    $manifestTypes = [System.Collections.Generic.List[object]]::new()
    foreach ($type in ($typeMap.Keys | Sort-Object)) {
        $set = $typeMap[$type]
        $patterns = @($set)
        $patternTotal += $patterns.Count
        $nNum = [int]$numMap[$type]; $nNamed = [int]$namedMap[$type]; $nPlain = [int]$plainMap[$type]
        $stats.Add([pscustomobject]@{ Type = $type; PatternCount = $patterns.Count; KeyCount = [int]$rawMap[$type]; Numeric = $nNum; Named = $nNamed; Plain = $nPlain })
        $manifestTypes.Add([pscustomobject]@{ type = $type; patterns = $patterns.Count; keys = [int]$rawMap[$type]; numeric = $nNum; named = $nNamed; plain = $nPlain })

        $fileName = 'lang-whitelist-' + ($type -replace '[^\w.-]', '_') + '.json'
        $payload = @{ type = $type; patterns = $patterns; keys = [int]$rawMap[$type]; numeric = $nNum; named = $nNamed; plain = $nPlain } | ConvertTo-Json -Depth 4 -Compress
        [System.IO.File]::WriteAllText((Join-Path $cacheDir $fileName), $payload,
            [System.Text.UTF8Encoding]::new($false))
    }

    $manifestPayload = @{
        version      = 2
        stamp        = $stamp
        builtUtc     = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
        containers   = @($containers)
        keyTotal     = $keyTotal
        numericTotal = $numTotal
        namedTotal   = $namedTotal
        plainTotal   = $plainTotal
        patternTotal = $patternTotal
        types        = @($manifestTypes)
    } | ConvertTo-Json -Depth 5 -Compress
    [System.IO.File]::WriteAllText($manifestPath, $manifestPayload, [System.Text.UTF8Encoding]::new($false))

    if ($keyTotal -eq 0) { throw "白名单为空：没有从原版中文 tar 里解析到 DefInjected 键，请检查游戏语言文件" }
    Write-RwStep ("白名单完成：{0} 个 DefType / {1} 条字段路径 / {2} 个原版键（数字索引 {3}，命名句柄 {4}，普通字段 {5}）" -f $typeMap.Count, $patternTotal, $keyTotal, $numTotal, $namedTotal, $plainTotal)

    $result = [pscustomobject]@{
        Types        = $typeMap
        Containers   = $containers
        Stats        = $stats
        TypeCount    = $typeMap.Count
        PatternCount = $patternTotal
        KeyCount     = $keyTotal
        NumericKeyCount = $numTotal
        NamedKeyCount   = $namedTotal
        PlainKeyCount   = $plainTotal
        TarCount     = $tars.Count
        Stamp        = $stamp
    }
    $script:Rw.Counters[$cacheKey] = $result
    return $result
}

function Get-RwLangLiSegments {
    <# 一个 <li> 在 DefInjected 键里可能用的段：<def> 值、Class、label、列表下标。
       与游戏 TranslationHandleUtility 的 handle 来源一致；下标永远保留作为备选。 #>
    param($Li, [int]$Index)

    $segs = [System.Collections.Generic.List[string]]::new()
    $defNode = $Li.SelectSingleNode('def')
    if ($defNode) {
        $v = $defNode.InnerText.Trim()
        if ($v) { $segs.Add($v) }
    }
    $class = $Li.GetAttribute('Class')
    if ($class) {
        $h = ConvertTo-RwNormalizedHandle -Text $class
        if ($h) { $segs.Add($h) }
    }
    $labelNode = $Li.SelectSingleNode('label')
    if ($labelNode) {
        $h = ConvertTo-RwNormalizedHandle -Text $labelNode.InnerText
        if ($h) { $segs.Add($h) }
    }
    $segs.Add([string]$Index)

    $uniq = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($s in $segs) {
        if ($seen.Add($s)) { $uniq.Add($s) }
    }
    return $uniq
}

function Add-RwLangDefPaths {
    <# 递归一个 Def 节点，产生字段记录：
       Pattern  = 归一化后的字段路径（列表段为 <n>），用于对照白名单；
       Variants = 所有可接受的完整字段路径（具体 handle / 下标），用于对照语言文件；
       Required = 叶子字符串字段（要翻译）；列表容器本身是非必需（整段翻译时用）。 #>
    param($Node, [string[]]$Prefixes, [string[]]$Patterns, $Records)

    foreach ($child in $Node.ChildNodes) {
        if ($child -isnot [System.Xml.XmlElement]) { continue }
        $name = $child.LocalName

        $newPre = [System.Collections.Generic.List[string]]::new()
        foreach ($p in $Prefixes) { $newPre.Add((Join-RwLangSegment -Prefix $p -Segment $name)) }
        $newPat = [System.Collections.Generic.List[string]]::new()
        foreach ($p in $Patterns) { $newPat.Add((Join-RwLangSegment -Prefix $p -Segment $name)) }

        $lis = @($child.ChildNodes | Where-Object { $_ -is [System.Xml.XmlElement] -and $_.LocalName -eq 'li' })
        if ($lis.Count -gt 0) {
            # 整段列表键（例如 <Def>.rulePack.rulesStrings 直接写整段 <li>）
            $Records.Add([pscustomobject]@{
                    Pattern  = $newPat[0]
                    Variants = @($newPre)
                    Required = $false
                    Kind     = 'list'
                })

            $segsPer = [System.Collections.Generic.List[object]]::new()
            $counts = @{}
            for ($i = 0; $i -lt $lis.Count; $i++) {
                $segs = Get-RwLangLiSegments -Li $lis[$i] -Index $i
                $segsPer.Add($segs)
                $canonical = $segs[0]
                if (-not $counts.ContainsKey($canonical)) { $counts[$canonical] = 1 } else { $counts[$canonical] = $counts[$canonical] + 1 }
            }

            $ord = @{}
            for ($i = 0; $i -lt $lis.Count; $i++) {
                $segs = $segsPer[$i]
                $canonical = $segs[0]
                $o = 0
                if ($ord.ContainsKey($canonical)) { $o = $ord[$canonical] }
                $ord[$canonical] = $o + 1
                $suffix = ''
                if ($counts[$canonical] -gt 1 -and $canonical -notmatch '^\d+$') { $suffix = '-' + $o }

                $itemPre = [System.Collections.Generic.List[string]]::new()
                foreach ($p in $newPre) {
                    foreach ($s in $segs) {
                        $itemPre.Add((Join-RwLangSegment -Prefix $p -Segment $(if ($s -match '^\d+$') { $s } else { $s + $suffix })))
                    }
                }
                $itemPat = [System.Collections.Generic.List[string]]::new()
                foreach ($p in $newPat) { $itemPat.Add((Join-RwLangSegment -Prefix $p -Segment '<n>')) }

                $hasElem = $false
                $hasText = $false
                foreach ($cc in $lis[$i].ChildNodes) {
                    if ($cc -is [System.Xml.XmlElement]) { $hasElem = $true; break }
                    if ($cc -is [System.Xml.XmlText] -and $cc.Value.Trim()) { $hasText = $true }
                }
                if ($hasElem) {
                    Add-RwLangDefPaths -Node $lis[$i] -Prefixes @($itemPre) -Patterns @($itemPat) -Records $Records
                }
                elseif ($hasText) {
                    $Records.Add([pscustomobject]@{
                            Pattern  = $itemPat[0]
                            Variants = @($itemPre)
                            Required = $true
                            Kind     = 'string-list-item'
                        })
                }
            }
        }
        else {
            $hasElem = $false
            foreach ($cc in $child.ChildNodes) { if ($cc -is [System.Xml.XmlElement]) { $hasElem = $true; break } }
            if ($hasElem) {
                Add-RwLangDefPaths -Node $child -Prefixes @($newPre) -Patterns @($newPat) -Records $Records
            }
            else {
                $Records.Add([pscustomobject]@{
                        Pattern  = $newPat[0]
                        Variants = @($newPre)
                        Required = $true
                        Kind     = 'field'
                    })
            }
        }
    }
}

function Get-RwLangDefRecords {
    param([System.Xml.XmlElement]$DefNode)

    $records = [System.Collections.Generic.List[object]]::new()
    Add-RwLangDefPaths -Node $DefNode -Prefixes @('') -Patterns @('') -Records $records
    return $records
}

function Get-RwLangFileKeys {
    <# 读一个模组的某语言 DefInjected 目录，返回 DefType -> 键集合。 #>
    param([string]$ModRoot, [string]$Lang)

    $injected = Join-Path (Join-Path $ModRoot 'Languages') $Lang
    $injected = Join-Path $injected 'DefInjected'

    $types = @{}
    $files = 0
    if (-not (Test-Path $injected)) {
        return [pscustomobject]@{ Types = $types; Files = 0; Count = 0 }
    }

    foreach ($file in (Get-ChildItem $injected -Recurse -Filter *.xml -File)) {
        $doc = Read-RwXml -Path $file.FullName -Check 'lang'
        if (-not $doc -or -not $doc.DocumentElement) { continue }
        $files++

        $rel = $file.FullName.Substring($injected.Length).TrimStart('\', '/')
        $slash = $rel.IndexOfAny([char[]]@('\', '/'))
        $type = if ($slash -gt 0) { $rel.Substring(0, $slash) } else { [System.IO.Path]::GetFileNameWithoutExtension($file.Name) }

        if (-not $types.ContainsKey($type)) {
            $types[$type] = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        }
        $set = $types[$type]
        foreach ($keyNode in $doc.DocumentElement.ChildNodes) {
            if ($keyNode -isnot [System.Xml.XmlElement]) { continue }
            [void]$set.Add($keyNode.LocalName)
        }
    }

    $count = 0
    foreach ($set in $types.Values) { $count += $set.Count }
    return [pscustomobject]@{ Types = $types; Files = $files; Count = $count }
}

function Write-RwLangTodo {
    <# 为缺键生成可直接粘贴的 XML 骨架（值写 TODO）。 #>
    param($Missing, [string]$ModId, [string]$OutRoot)

    $dir = Join-Path $OutRoot ('lang-todo-' + $ModId)
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

    $aggregate = Join-Path $OutRoot ('lang-todo-' + $ModId + '.xml')
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine('<LanguageData>')
    [void]$sb.AppendLine('  <!-- 由 rw.ps1 lang -Todo 生成：缺键占位，补完译文后删除本注释 -->')

    $perType = [System.Collections.Generic.List[string]]::new()
    foreach ($g in ($Missing | Group-Object Type | Sort-Object Name)) {
        [void]$sb.AppendLine('')
        [void]$sb.AppendLine('  <!-- ' + $g.Name + ' -->')
        foreach ($dg in @($g.Group | Group-Object DefName | Sort-Object Name)) {
            foreach ($r in @($dg.Group)) {
                $key = [string]$r.Key
                [void]$sb.AppendLine('  <' + $key + '>TODO</' + $key + '>')
            }
        }

        # 按 DefType 单独一份，直接拷进 DefInjected/<DefType>/
        $pt = New-Object System.Text.StringBuilder
        [void]$pt.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
        [void]$pt.AppendLine('<LanguageData>')
        foreach ($dg in @($g.Group | Group-Object DefName | Sort-Object Name)) {
            foreach ($r in @($dg.Group)) {
                $key = [string]$r.Key
                [void]$pt.AppendLine('  <' + $key + '>TODO</' + $key + '>')
            }
        }
        [void]$pt.AppendLine('</LanguageData>')
        $typeFile = Join-Path $dir (($g.Name -replace '[\\/:*?"<>|]', '_') + '.xml')
        [System.IO.File]::WriteAllText($typeFile, $pt.ToString(), (New-Object System.Text.UTF8Encoding($true)))
        $perType.Add($typeFile)
    }
    [void]$sb.AppendLine('</LanguageData>')

    [System.IO.File]::WriteAllText($aggregate, $sb.ToString(), (New-Object System.Text.UTF8Encoding($true)))
    Write-RwStep ("→ {0}（{1} 条缺键）" -f $aggregate, @($Missing).Count)
    if ($perType.Count -gt 0) {
        Write-RwStep ("→ {0}（按 DefType 拆分 {1} 个文件）" -f $dir, $perType.Count)
    }
    return $aggregate
}

function Invoke-RwLang {
    <# lang：对照原版中文 DefInjected 白名单，查缺键 / 孤儿键 / 简繁不一致，-Todo 生成骨架。 #>
    param([string]$Mod, [switch]$Todo)

    $cfg = $script:Rw.Config
    $mods = @($cfg.mods)
    if ($Mod) {
        $mods = @($mods | Where-Object { $_.id -ieq $Mod -or $_.packageId -ieq $Mod })
        if ($mods.Count -eq 0) { throw "配置里没有 mod：$Mod" }
    }

    Write-RwStep '语言检查（lang）：对照原版中文 DefInjected 白名单'
    $wl = Get-RwLangWhitelist
    Write-RwStep ("白名单：{0} 个 DefType / {1} 条字段路径（{2} 个原版键：数字索引 {3} / 命名句柄 {4} / 普通字段 {5}；{6} 个 tar）" -f `
            $wl.TypeCount, $wl.PatternCount, $wl.KeyCount, $wl.NumericKeyCount, $wl.NamedKeyCount, $wl.PlainKeyCount, $wl.TarCount)

    $outRoot = Join-Path $script:Rw.Root 'out'
    $totalMissing = 0
    $totalOrphan = 0

    foreach ($m in $mods) {
        $modRoot = Join-Path $cfg.modsDir $m.id
        if (-not (Test-Path $modRoot)) {
            Write-Host ("[rw] 跳过 {0}：目录不存在（{1}）" -f $m.id, $modRoot) -ForegroundColor Yellow
            continue
        }

        Write-Host ''
        Write-Host ("== {0} ==" -f $m.id) -ForegroundColor Cyan

        # ---- Def 索引（只读本模组的 1.6 / 根目录 Defs） ----
        $defs = [System.Collections.Generic.List[object]]::new()
        foreach ($sub in @('', $m.version)) {
            if (-not $sub) { $defsDir = Join-Path $modRoot 'Defs' }
            else { $defsDir = Join-Path (Join-Path $modRoot $sub) 'Defs' }
            if (-not (Test-Path $defsDir)) { continue }

            foreach ($file in (Get-ChildItem $defsDir -Recurse -Filter *.xml -File)) {
                $doc = Read-RwXml -Path $file.FullName -Check 'lang'
                if (-not $doc -or -not $doc.DocumentElement) { continue }
                foreach ($node in $doc.DocumentElement.ChildNodes) {
                    if ($node -isnot [System.Xml.XmlElement]) { continue }
                    $dnNode = $node.SelectSingleNode('defName')
                    if (-not $dnNode) { continue }
                    $defName = $dnNode.InnerText.Trim()
                    if (-not $defName) { continue }
                    $defs.Add([pscustomobject]@{
                            Type    = $node.LocalName
                            DefName = $defName
                            File    = $file.FullName
                            Records = (Get-RwLangDefRecords -DefNode $node)
                        })
                }
            }
        }

        if ($defs.Count -eq 0) {
            Write-Host '  （没有读到 Def）' -ForegroundColor DarkGray
            continue
        }

        # 同一 mod 内 (DefType, defName) -> Def 记录；不同 DefType 下允许 defName 重名
        $byKey = @{}
        $typesByName = @{}
        foreach ($d in $defs) {
            $tk = $d.Type + '|' + $d.DefName
            if (-not $byKey.ContainsKey($tk)) { $byKey[$tk] = $d }
            if (-not $typesByName.ContainsKey($d.DefName)) {
                $typesByName[$d.DefName] = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            }
            [void]$typesByName[$d.DefName].Add($d.Type)
        }

        # ---- 语言键（1.6 目录优先，兼容放在模组根目录的旧布局） ----
        $langRoot = Join-Path $modRoot $m.version
        if (-not (Test-Path (Join-Path $langRoot 'Languages'))) { $langRoot = $modRoot }
        $cs = Get-RwLangFileKeys -ModRoot $langRoot -Lang 'ChineseSimplified'
        $ct = Get-RwLangFileKeys -ModRoot $langRoot -Lang 'ChineseTraditional'

        # 每个 Def 可接受的完整键（具体 handle / 下标）
        $accepted = @{}
        foreach ($d in $defs) {
            $set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            foreach ($r in $d.Records) {
                foreach ($v in $r.Variants) { [void]$set.Add($d.DefName + '.' + $v) }
            }
            $accepted[$d.Type + '|' + $d.DefName] = $set
        }

        # ---- 缺键 ----
        $missing = [System.Collections.Generic.List[object]]::new()
        $seenMissing = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $skipTypes = @{}

        foreach ($d in $defs) {
            if (-not $wl.Types.ContainsKey($d.Type)) {
                if (-not $skipTypes.ContainsKey($d.Type)) { $skipTypes[$d.Type] = 0 }
                $skipTypes[$d.Type] = $skipTypes[$d.Type] + 1
                continue
            }
            $patterns = $wl.Types[$d.Type]
            $csKeys = $null
            if ($cs.Types.ContainsKey($d.Type)) { $csKeys = $cs.Types[$d.Type] }

            foreach ($r in $d.Records) {
                if (-not $r.Required) { continue }
                if (-not $patterns.Contains($r.Pattern)) { continue }

                $found = $false
                foreach ($v in $r.Variants) {
                    if ($csKeys -and $csKeys.Contains($d.DefName + '.' + $v)) { $found = $true; break }
                }
                if ($found) { continue }

                $key = $d.DefName + '.' + $r.Variants[0]
                if ($seenMissing.Add($d.Type + '|' + $key)) {
                    $missing.Add([pscustomobject]@{
                            Type    = $d.Type
                            DefName = $d.DefName
                            Key     = $key
                            Pattern = $r.Pattern
                        })
                }
            }
        }
        $missing = @($missing | Sort-Object Type, DefName, Key)

        # ---- 孤儿键 ----
        $orphan = [System.Collections.Generic.List[object]]::new()
        foreach ($type in ($cs.Types.Keys | Sort-Object)) {
            if (-not $wl.Types.ContainsKey($type)) { continue }
            $patterns = $wl.Types[$type]
            foreach ($key in ($cs.Types[$type] | Sort-Object)) {
                $dot = $key.IndexOf('.')
                if ($dot -lt 1) {
                    $orphan.Add([pscustomobject]@{ Type = $type; Key = $key; Reason = '缺少字段路径' })
                    continue
                }
                $dn = $key.Substring(0, $dot)
                $path = $key.Substring($dot + 1)

                $tk = $type + '|' + $dn
                if (-not $byKey.ContainsKey($tk)) {
                    if ($typesByName.ContainsKey($dn)) {
                        $other = @($typesByName[$dn]) -join ', '
                        $orphan.Add([pscustomobject]@{ Type = $type; Key = $key; Reason = "Def 类型是 $other，不是 $type" })
                    }
                    else {
                        $orphan.Add([pscustomobject]@{ Type = $type; Key = $key; Reason = 'Def 不存在' })
                    }
                    continue
                }

                if ($accepted[$tk] -and $accepted[$tk].Contains($path)) { continue }
                $pat = ConvertTo-RwLangPathPattern -Path $path -Containers $wl.Containers
                if ($patterns.Contains($pat)) { continue }
                $orphan.Add([pscustomobject]@{ Type = $type; Key = $key; Reason = '字段不在白名单（或自定义字段）' })
            }
        }
        $orphan = @($orphan | Sort-Object Type, Key)

        # ---- 简繁对齐 ----
        $onlyCs = [System.Collections.Generic.List[string]]::new()
        $onlyCt = [System.Collections.Generic.List[string]]::new()
        foreach ($type in ($cs.Types.Keys | Sort-Object)) {
            $ctSet = if ($ct.Types.ContainsKey($type)) { $ct.Types[$type] } else { $null }
            foreach ($key in ($cs.Types[$type] | Sort-Object)) {
                if (-not $ctSet -or -not $ctSet.Contains($key)) { $onlyCs.Add($type + '/' + $key) }
            }
        }
        foreach ($type in ($ct.Types.Keys | Sort-Object)) {
            $csSet = if ($cs.Types.ContainsKey($type)) { $cs.Types[$type] } else { $null }
            foreach ($key in ($ct.Types[$type] | Sort-Object)) {
                if (-not $csSet -or -not $csSet.Contains($key)) { $onlyCt.Add($type + '/' + $key) }
            }
        }

        # ---- 输出 ----
        Write-Host ("  Def {0} 个；简体 {1} 键 / {2} 文件；繁体 {3} 键 / {4} 文件" -f `
                $defs.Count, $cs.Count, $cs.Files, $ct.Count, $ct.Files)

        $color = if ($missing.Count -gt 0) { 'Yellow' } else { 'Green' }
        Write-Host ("  缺键 {0}" -f $missing.Count) -ForegroundColor $color
        if ($missing.Count -gt 0) {
            $missing | Select-Object -First $Limit | ForEach-Object {
                Write-Host ("    {0}/{1}  [{2}]" -f $_.Type, $_.Key, $_.Pattern) -ForegroundColor DarkYellow
            }
            if ($missing.Count -gt $Limit) { Write-Host ("    … 还有 {0} 条（-Limit 调整）" -f ($missing.Count - $Limit)) -ForegroundColor DarkGray }
        }

        $color = if ($orphan.Count -gt 0) { 'Red' } else { 'Green' }
        Write-Host ("  孤儿键 {0}" -f $orphan.Count) -ForegroundColor $color
        if ($orphan.Count -gt 0) {
            $orphan | Select-Object -First $Limit | ForEach-Object {
                Write-Host ("    {0}/{1}  [{2}]" -f $_.Type, $_.Key, $_.Reason) -ForegroundColor DarkRed
            }
            if ($orphan.Count -gt $Limit) { Write-Host ("    … 还有 {0} 条（-Limit 调整）" -f ($orphan.Count - $Limit)) -ForegroundColor DarkGray }
        }

        $color = if (($onlyCs.Count + $onlyCt.Count) -gt 0) { 'Yellow' } else { 'Green' }
        Write-Host ("  简繁不一致：仅简体 {0}，仅繁体 {1}" -f $onlyCs.Count, $onlyCt.Count) -ForegroundColor $color
        if (($onlyCs.Count + $onlyCt.Count) -gt 0) {
            foreach ($k in ($onlyCs | Select-Object -First $Limit)) { Write-Host ("    仅简 {0}" -f $k) -ForegroundColor DarkYellow }
            foreach ($k in ($onlyCt | Select-Object -First $Limit)) { Write-Host ("    仅繁 {0}" -f $k) -ForegroundColor DarkYellow }
            if (($onlyCs.Count + $onlyCt.Count) -gt (2 * $Limit)) { Write-Host '    …（-Limit 调整）' -ForegroundColor DarkGray }
        }

        if ($skipTypes.Count -gt 0) {
            $parts = @($skipTypes.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "{0}({1})" -f $_.Key, $_.Value })
            Write-Host ("  无原版参考（跳过字段判定）：{0}" -f ($parts -join '，')) -ForegroundColor DarkGray
        }

        if ($Todo) {
            if (@($missing).Count -eq 0) {
                Write-RwStep ("{0}：没有缺键，未生成 Todo" -f $m.id)
            }
            else {
                [void](Write-RwLangTodo -Missing $missing -ModId $m.id -OutRoot $outRoot)
            }
        }

        $totalMissing += $missing.Count
        $totalOrphan += $orphan.Count
    }

    Write-Host ''
    Write-Host ("合计：缺键 {0}，孤儿键 {1}" -f $totalMissing, $totalOrphan) -ForegroundColor Cyan
    return 0
}

#endregion
