# AGENTS.md — 边缘武神（RimWarlord）

给在这个仓库里干活的 agent 看的操作约定。

## 两份东西在哪

| 内容 | 位置 |
|---|---|
| C# 源码、解决方案 | 本仓库 `RWrd/`（作者手写，不要代劳） |
| 工具脚本 | 本仓库 `tools/` |
| Def / 语言 / 贴图（游戏实际读取的） | `D:\Steam\steamapps\common\RimWorld\Mods\RimWarlord`、`...\Mods\baak-imperial-family` |
| 原版参考数据 | `D:\Steam\steamapps\common\RimWorld\Data\<Core,Royalty,Ideology,Biotech,Anomaly,Odyssey>\Defs` |
| 译名对照表 + 世界观（**不进 git**） | `D:\Desktop\RimWorldMods\Lore\`（仓库的上级目录） |

XML 不在仓库里是作者的明确选择：平时在 Mods 目录里改，游戏直接读。需要存档时才用 `tools\rw.ps1 snapshot` 回灌。**不要**出于“整洁”把 Def 搬进仓库。

仓库里未提交的改动是作者按版本号节奏提交的，**不要**替作者 commit / 清理工作区。

## 动笔之前先读 Lore

任何 Def、能力描述、语言条目、工坊文案，落笔前先读（用文件工具读，不要凭记忆）：

- `D:\Desktop\RimWorldMods\Lore\GLOSSARY.md` — 译名唯一基准（中文 ↔ 英文 ↔ defName）
- `D:\Desktop\RimWorldMods\Lore\WORLD.md` — 世界观骨架，空白处要问作者，不要编设定
- `D:\Desktop\RimWorldMods\Lore\TRANSLATION_FIELDS.md` — 哪些字段要翻译、怎么判定
- `D:\Desktop\RimWorldMods\Lore\SURNAMES.md` — 百家姓中英对照（中文 226 / 英文 173，粤拼，同音只留一个）
- `D:\Desktop\RimWorldMods\Lore\archive\` — 原典素材的离线副本（工坊页面 docx 提取文本、英文维基 `Warlord (manhua)` 提取文本）。**查原典先翻这里**；需要联网时只有 `browser_fetch` 可用（要自己给 URL），`browser_search` 返回无关结果，shell 无外网
- `D:\Desktop\RimWorldMods\Lore\WIKI-武神词条-定稿.md` — **查原典设定读这一份**（按作者修订稿整理、措辞已中性化，可安全读入对话）
- `D:\Desktop\RimWorldMods\Lore\WIKI-武神词条-中译修订稿.md` — 作者逐句修订的原典词条，**它比其它一切二手结论都优先**；`WIKI-武神词条-中译稿.md` 是被它取代的初稿，只留作对照

**原典正文不要整段读进对话**：模型侧的内容过滤会拦下含暴力／性描写的片段（`PI_AI_ERROR: The request contains sensitive content`），表现为整轮请求直接失败、看着像「工作流被截断」；一旦该文本进入历史，后续每一轮都会带着它，重试无效。正确做法：脚本落盘 → 生成脱敏副本 `Lore\archive\*.safe.txt` → 只读脱敏副本；原始文件只做 `grep` 计数。详见 `Lore\WORLD.md` 末尾的故障记录。

新术语：先查表；表里没有就给候选并说明依据，同时补进 `GLOSSARY.md` 与 `glossary.csv`。改写已有译名时先改表、再改 Def/语言文件。

## 写一个 Def 的标准流程

先查、再抄、后改、最后自查。全部通过 `tools\rw.ps1`：

```powershell
# 1) 找一个像样的现成 Def（原版或自己以前的）
powershell -File tools\rw.ps1 search -Type ThingDef -Like "*Cape*"

# 2) 看它写了什么、继承了谁
powershell -File tools\rw.ps1 show ThingDef/RWrd_Apparel_Cape

# 3) 不确定字段名时，查这类 Def 在原版+现有模组里用过的字段（带样例值）
powershell -File tools\rw.ps1 fields ThingDef -FieldMatch "comps|statBases"

# 4) 克隆成新的，落到目标文件（或 -AppendTo 追加到已有文件）
powershell -File tools\rw.ps1 clone ThingDef/RWrd_Apparel_Cape -DefName RWrd_Apparel_Cape_II `
    -Out "D:\Steam\steamapps\common\RimWorld\Mods\RimWarlord\1.6\Defs\ThingDefs_Misc\Apparel_CapeII.xml"

# 5) 写完自查：字段名拼错、贴图找不到、类型不认识
powershell -File tools\rw.ps1 check -Path "D:\...\Apparel_CapeII.xml"

# 6) 全量回归
powershell -File tools\rw.ps1 validate
```

`-All` 会把 Mods 目录下所有模组一起索引，用于参考其它模组的写法：

```powershell
powershell -File tools\rw.ps1 search -Like "*PowerArmor*" -All
```

## 系列约定

- **继承优先**：能用 `ParentName` 复用原版模板，就不要整段复制字段。
  - `ParentName` 只能写父类标签里 `Name="..."` 定义的名字，**不能写 defName**（`<PawnKindDef Name="BaseBaakPawnKind">` 的父类名是 `BaseBaakPawnKind`，不是它的 defName `RWrd_Pawn_BaakSide`）。写成 defName 时 `validate` 会报 `[def-reference] ParentName 用了 defName`。
  - 没有 `Name` 的 Def 不能被继承，要复用它就先在定义处加 `Name`。
- **defName**：本体用 `RWrd_` 前缀，附属按现有习惯（`Empire_Baak_*`）；同一系列内不要与已有 defName 冲突（`validate` 会查同类型重名）。
- **默认语言是英文**：`<label>`/`<description>` 直接写英文即可，中文走 `Languages\ChineseSimplified\DefInjected\<DefType>\<文件>.xml`，键为 `<defName>.<字段>`（如 `RWrd_Apparel_Cape.label`）。
  - 不是每个 Def 都需要语言文件（Mote/Fleck/Sound/AbilityCategory 之类通常不需要）。
  - 加中文时同步 `ChineseTraditional`，`validate` 会报简繁不一致的键。
- **贴图**：路径相对 `Textures/`，不带扩展名；大小写必须与文件一致（Linux/Mac 敏感），`validate` 会查。
- **DLC 依赖**：只对 DLC 内容用 `MayRequire="Ludeon.RimWorld.Royalty"`；需要整块内容随 DLC 开关切换时，用 `LoadFolders.xml` + `IfModActive/IfModNotActive`（附属已有例子），不要用 try/catch 兜。
- **附属不得假设本体已加载**：跨模组引用只走 `packageId` 依赖 + `MayRequire`，并在 About 里写清楚依赖与 `loadAfter`。
- **Def 类型**：自定义 Def 类直接用完全限定名作元素名（如 `<Electromagnetic.Core.WarlordBackstoryDef>`），`check` 会核对类名是否在已编译的 dll 里。

## 不要做的事

- 不要为了让校验通过而删掉作者在研的 Def。
- 不要修改 `About.xml` 的 `modVersion`（版本号由作者在发布时统一处理）。
- 不要把 `.cache/`、`reports/`、`dist/`、`out/` 提交进仓库（`tools/.gitignore` 已忽略）。
- C# 代码由作者手写；需要改代码时先问，不要顺手重构。

## 会话交接

长会话会越来越慢，还会撞上工具/路由的硬限制，所以**常规操作是定期换会话**，靠文件而不是记忆衔接：

- 开会话**先读** `D:\Desktop\RimWorldMods\Lore\dev\交接单.md`（现状、基线数字、待办、环境坑、下一步）。
- 交接规程见 `D:\Desktop\RimWorldMods\Lore\dev\交接规范.md`；交接单只写**能复核的事实**（带路径/行号/实测数字），不写过程与情绪。
- **第 25 轮起**：出现新决定、新坑、新待办就随手追加进交接单。
- **第 30 轮**：无论任务是否完成，都在回复末尾提示「已到 30 轮，建议起新会话」，并把交接单整体刷新（基线的 validate/lang/git 三个数字当场重跑，不要抄旧的）。
