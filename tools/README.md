# tools — 边缘武神系列工作流

一组 PowerShell 脚本，解决三件事：

1. **进游戏之前就能看出问题**（`validate`）
2. **不用在两个文件夹之间往返跑**（`snapshot` 把 Mods 里的内容回灌进仓库）
3. **出包不靠手**（`build` / `package`）

只依赖 Windows PowerShell 5.1（系统自带）和 VS 2022 的 MSBuild，不需要装 PowerShell 7。

---

## 快速开始

```powershell
cd D:\Desktop\RimWorldMods\RWrd
powershell -ExecutionPolicy Bypass -File tools\rw.ps1 validate
```

输出

```
================ 体检结果 ================
错误 0   警告 0   提示 4
扫描 139 个 Def / 49 个贴图引用 / 502 条语言键
报告：...\tools\reports\validate-20261020-230052.md
```

退出码：`0` 通过，`1` 有错误（加 `-Strict` 时有警告也算失败）。可以直接用来卡提交。

---

## 子命令

| 命令 | 作用 | 常用参数 |
|---|---|---|
| `validate` | 静态体检：About / LoadFolders / Def 图 / 贴图 / 语言注入 | `-Strict` `-Only <modId>` `-Refresh` |
| `snapshot` | 把游戏 Mods 目录里的 About·Defs·Languages·Patches 回灌进仓库 `Mod/`，可选直接 git 提交 | `-Message "..."` `-DryRun` `-IncludeTextures` |
| `build` | 用 MSBuild 编译 `RWrd/RWrd.csproj`，产物落在 `dist/` | `-Configuration Release` `-Deploy` |
| `package` | 打可上传工坊的 zip（自动排除 obj/bin/Source） | `-Only <modId>` `-DryRun` |
| `selftest` | 工具自检：坏样本必须报错、真配置必须通过 | — |
| `lang` | 对照原版中文 DefInjected 白名单：缺键 / 孤儿键 / 简繁不一致 | `-Mod <modId>` `-Todo` `-Refresh` |

`-Deploy` 会把 dll/pdb 复制进 `Mods\<本体>\1.6\Assemblies`，等价于你现在 F5 的行为，但是显式的。

---

## 为什么要 snapshot（解决“往返跑”）

代码在 `D:\Desktop\RimWorldMods\RWrd`，Def/语言在 `D:\Steam\...\Mods\RimWarlord`。你不想为了版本管理来回搬文件，所以：

- **平时照旧在 Mods 目录里改**，游戏直接读，改完就能测；
- 想存档时跑一次 `snapshot`，它把内容镜像进仓库的 `Mod\`，顺便 `git commit`：
  ```powershell
  powershell -File tools\rw.ps1 snapshot -Message "白家头衔与背景补充"
  ```
- 幂等：没改动时第二次跑是 `新增 0 / 更新 0`。
- 贴图默认不搬（体积大），需要时加 `-IncludeTextures`。

`Mod\` 就成了 Def 的唯一版本化副本，而你不需要改变编辑习惯。

---

## 配置：`rw.config.json`

| 键 | 说明 |
|---|---|
| `gameDir` | 游戏根目录，用来读原版 `Data\` 做参照 |
| `modsDir` | 游戏 Mods 目录（内容事实源） |
| `repoRoot` | 代码仓库根 |
| `buildProject` | 相对 `repoRoot` 的 csproj |
| `distDir` | 产物目录；写绝对路径可以不落在仓库里 |
| `snapshotRoot` | `snapshot` 的目标子目录，默认 `Mod` |
| `basePackageId` | 本体 packageId，用于检查附属的依赖声明 |
| `defNamePrefix` | 命名前缀提示，只做 Info；留空则关闭 |
| `activeMods` | 判断 `LoadFolders.xml` 的 `IfModActive` / `IfModNotActive` 时假定启用的包 |
| `mods[]` | `id`（文件夹名）、`packageId`、`version`（校验的版本目录）、`languages`（对齐审计用） |

路径里可以写 `{tools}`，运行时展开成工具所在目录 —— `selftest/rw.config.json` 就是这么写的，所以整个 `tools` 能跟着仓库搬。

---

## 检查项

**错误（会挡住发布）**

- `def-xml` XML 解析失败 / 根节点不是 `<Defs>`
- `encoding` 不是合法 UTF-8（游戏会读成乱码）
- `def-duplicate` 同类型 defName 重复、模板 `Name` 重复
- `def-type` `<Defs>` 下的元素类型不认识（拼错，或 C# 类没编进 dll）
- `def-reference` `ParentName` 指向不存在的模板/Def（含原版 `Name` 模板、跨模组模板）
- `texture-missing` `texPath` / `iconPath` / `factionIconPath` 等指向的贴图不存在
- `lang-duplicate` 同一语言、同一 Def 类型下注入键重复
- `loadfolders` `LoadFolders.xml` 引用了不存在的目录
- `about` 缺 `packageId`/`name`、格式非法、附属没声明对本体依赖

**警告**

- `texture-case` 路径大小写与实际文件不一致（Linux/Mac 会加载失败）
- `class-missing` `needClass` / `hediffClass` 之类引用的类名在 dll 里找不到（启发式）
- `lang-orphan` 语言注入指向不存在的 Def（多半是改名后遗留）
- `about` `packageId` 与配置不符、缺 `modVersion`、`supportedVersions` 不含当前版本、附属缺 `loadAfter`
- `loadfolders` 有版本目录存在却没被任何 `LoadFolders` 引用

**提示（不挡）**

- `def-prefix` 不以约定前缀开头的 defName
- `lang-parity` 中文简/繁（或你配置的语言两两）之间的键差异

语言检查的原则是**只查一致性，不要求每个 Def 都有语言文件**：Mote/Fleck/Sound/AbilityCategory 这类没有翻译是正常的，但“简中有、繁中没有”会被抓出来。


---

## 写 Def：查原版、抄现成、写完自查

这三条是日常最常用的，目标是不用开游戏就知道字段名对不对。

```powershell
# 找现成的（-All 连 Mods 目录下的其它模组一起搜）
powershell -File tools\rw.ps1 search -Type ThingDef -Like "*Cape*"
powershell -File tools\rw.ps1 search -Like "*PowerArmor*" -All

# 看它长什么样、继承了谁（会把 ParentName 链一路打出来）
powershell -File tools\rw.ps1 show ThingDef/RWrd_Apparel_Cape

# 这类 Def 在原版+现有模组里到底有哪些字段名（带出现次数和样例值）
powershell -File tools\rw.ps1 fields ThingDef -FieldMatch "comps|statBases"
powershell -File tools\rw.ps1 fields HediffDef -Limit 80

# 克隆一个类似的，改成新 defName，输出到新文件（或 -AppendTo 追加到已有文件）
powershell -File tools\rw.ps1 clone ThingDef/RWrd_Apparel_Cape -DefName RWrd_Apparel_Cape_II `
    -Out "D:\Steam\steamapps\common\RimWorld\Mods\RimWarlord\1.6\Defs\ThingDefs_Misc\Apparel_CapeII.xml"
powershell -File tools\rw.ps1 clone ThingDef/RWrd_Apparel_Cape -DefName RWrd_Apparel_Cape_III `
    -AppendTo "D:\...\Apparel_Powerful.xml"

# 写完自查：字段名拼错、贴图找不到、Def 类型不认识
powershell -File tools\rw.ps1 check -Path "D:\...\Apparel_CapeII.xml"
powershell -File tools\rw.ps1 check -Path "D:\...\ThingDefs_Misc"
```

`check` 的“字段疑点”是拿这一类 Def 在原版+现有模组里**实际用过的字段路径**做比对：`statBase` 写成单数、`compClass` 写成 `class` 这种会被点出来。它的判断依据是“从没人这么写过”，所以新版本新增的字段也会被标黄——看一眼就知道该不该理。

`clone` 会保留原 Def 的 label/description，除非你给 `-Label` / `-Description`，所以输出后记得改文案。

---

## 查语言：还缺哪些 DefInjected 键

`lang` 把模组里的 Def 字段和**原版中文实际翻译过的字段路径**对照，回答三个问题：缺键、孤儿键、简繁不一致。

```powershell
# 默认配置里的两个模组都查
powershell -File tools\rw.ps1 lang

# 只查本体，并为缺键生成 TODO 骨架
powershell -File tools\rw.ps1 lang -Mod RimWarlord -Todo
```

- **缺键**：该 DefType 在原版中文里翻译过这个字段（如 `HediffDef.label`、`ThingDef.comps.<n>...`），但模组 `Languages\ChineseSimplified\DefInjected\<DefType>\` 里没有对应的 `<defName>.<字段>` 键。
- **孤儿键**：语言文件里有键，但对应 Def 不存在、Def 类型和目录不一致，或字段路径不在白名单里。
- **繁简不一致**：简体有、繁体没有（以及反向）。
- 没有原版白名单的 DefType（自定义类型、Mote/Fleck/Sound 等）显示「无原版参考」并跳过，不会瞎报。

白名单直接从 `Data\*\Languages\ChineseSimplified (简体中文).tar` 按 tar 头解析（不解包），字段路径里的列表元素统一折叠成 `<n>`（原版的数字下标与命名 handle 都折），输出统计行会分别给出「数字索引 / 命名句柄 / 普通字段」的键数；缓存在 `tools\.cache\lang-whitelist-*.json`，游戏更新或想强制重建时加 `-Refresh`。`-Todo` 会生成 `tools\out\lang-todo-<mod>.xml`（汇总）和 `tools\out\lang-todo-<mod>\<DefType>.xml`（可直接粘贴进对应 DefType 目录），值写 `TODO`。

限制：只检查 Def XML 里**显式写出**的字段；靠 `ParentName` 继承来的 label/description 不要求新键（避免对 Mote/Fleck 这类继承模板误报）。列表项的键优先用 `Class` / `<def>` / `<label>` 归一化出的 handle，没有 handle 时退回数字下标；整段列表（如 RulePackDef 的 `rulesStrings`）也可以直接写整段键。

---

## 自检

```powershell
powershell -File tools\rw.ps1 selftest
```

`selftest/mods/BrokenMod` 是一个故意做坏的样本（重复 defName、未知类型、缺贴图、大小写不一致、坏 XML、孤儿语言键、非法 packageId、LoadFolders 指向不存在的目录）。`selftest` 会要求它**必须**报错，同时要求真实配置**必须**通过 —— 你改工具逻辑后先跑这个，避免把校验收成摆设。

---

## 已知限制

- **原版贴图不在磁盘上**（打包进 Unity 资源），所以“贴图存不存在”对原版路径是从原版 Def 里反推的合法路径集合，不是真实文件校验。
- **C# 类名检查是启发式的**：把游戏与模组的 dll 当文本扫类名，能抓拼错，但不能替代运行时验证。
- **`build` 的 `-Deploy` 默认部署本体**（`mods[0]`），附属目前没有 C# 工程。
- 组件类（`<li Class="...">`）、PatchOperation 的 xpath 尚未校验。

---

## 在 VS 里挂一个快捷键

`工具 → 外部工具 → 添加`：

- 标题：`RWrd Validate`
- 命令：`powershell.exe`
- 参数：`-NoProfile -ExecutionPolicy Bypass -File "$(ProjectDir)..\tools\rw.ps1" validate`
- 初始目录：`$(ProjectDir)..`
- 勾选「使用输出窗口」
