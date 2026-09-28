# 重命名后引用检测报告

- 仓库: `/home/lizhi/undead-slayer-xiahoudun-decompiled`（WSL / ext4）
- 远端: `git@github.com:pushName/undead-slayer-xiahoudun-decompiled.git`（`main`）
- 生成时间: 2026-09-28 08:59 UTC（Windows 本地 16:59）
- 扫描基线: commit `662a6fe`（即 36 个 `<>` 文件名重命名之后、本次大小写修正之前）
- 原始输出: `ref-scan-generic.txt`、`ref-scan-byfile.txt`、`ref-scan-types.txt`

## 0. 先说检测范围为什么比需求文档小

需求文档 B.1 列了 5 类可能的引用形式。按 `git ls-files` 实测，本仓库共 924 个跟踪文件：

| 类型 | 数量 | 说明 |
| :--- | ---: | :--- |
| `.cs` | 338 | 反编译源码 |
| `Assets/Resources/*` | 约 540 | Unity 序列化资源（`level0`~`level22`、`mainData`、32 位十六进制名、`*.assets`）+ 5 个 `Managed/*.dll` |
| `.yml` / `.xml` | 9 | 6 个 GitHub Actions + `AndroidManifest.xml` + `apktool.yml` |
| 其它 | 其余 | `.png`、`.so`、`.md`、`.tsv`、`.sh`、`.gitignore` |

实测结论：**仓库里不存在 `.csproj` / `.sln` / `.unity` / `.prefab` / `.meta` / `.asmdef`**（`git ls-files | grep -Ei "\.(csproj|sln|unity|prefab|meta|asmdef)$"` 无输出）。因此 B.1 的第 1 类（工程文件按文件名列举）、第 3 类（`.unity`/`.prefab` 的 guid 引用）、第 4 类（`.meta` 同步改名）在本项目**没有载体，无需处理**。实际只需要检测第 2 类（源码内容）和第 5 类（CI）。

## 1. 概览

| 类别 | 命中数 | 是否需要同步改名 |
| :--- | ---: | :--- |
| `<>` 残留（全部文本文件） | 696 行 | 否，见第 2.1 节 |
| 旧**文件名/路径**引用（36 个旧名逐个搜） | **0 处有效引用** | 否 |
| 编译器生成**类名**的跨文件引用 | **0 处** | 否 |
| CI（`.github/workflows/*.yml`）硬编码路径 | **0 处** | 否 |
| 大小写冲突（问题 A） | 1 组 | **是，已修** |

## 2. 明细

### 2.1 `<>` 残留（`ref-scan-generic.txt`，696 行）

分布：

| 来源 | 行数 | 性质 |
| :--- | ---: | :--- |
| `.cs` 内容（69 个文件，共 338 个 `.cs`） | 687 | C# 编译器生成标识符 + 反编译 IL 注释 |
| `RENAME-REPORT.md` | 4 | 改名映射表里的"原路径"列，属于记录本身 |
| `rename-log.tsv` | 4 | 同上 |
| `fix-names.sh` | 1 | `grep -zE '[<>:"\|?*\\]'` 的匹配模式，不是引用 |

`.cs` 里按标识符形态统计：

| 形态 | 出现次数 | 例子 |
| :--- | ---: | :--- |
| `<>f__this`（lambda/迭代器捕获的 this 字段） | 640 | `Assets/Scripts/_lt_Sk_Heal_gt_c__Iterator9.cs:8` |
| `<>f__switch$mapN` | 18 | `Assets/Scripts/Pet_horse.cs:463` |
| `<>m__N`（委托缓存方法） | 17 | `Assets/Scripts/DB_General.cs:133552` |
| `<外部方法名>c__IteratorXX`（类声明/构造器/`// Type:` 注释） | 21 | `internal sealed class <Sk_Heal>c__Iterator9 {{` |

**这些不能跟着文件名一起替换**。`<>c__Iterator13` 之类是 Mono 编译器写进 `Assembly-CSharp` 元数据的真实标识符，同名信息在 `Assets/Resources/Managed/Assembly-CSharp.dll` 里仍然存在（`grep -rla c__Iterator` 命中该 DLL）。把源码内容里的 `<>` 改成 `_lt__gt_` 会让反编译结果与 DLL 元数据不一致，属于制造第二个真源。所以文件名的 `_lt_/_gt_` 与内容的 `<>` **故意不对称**。

### 2.2 旧文件名引用（`ref-scan-byfile.txt`）

做法：读 `rename-log.tsv` 的 36 个旧路径，取 basename 后 `grep -rn --fixed-strings` 搜遍 `*.cs *.csproj *.sln *.unity *.prefab *.asset *.json *.yml *.yaml *.xml *.md *.sh`。

结果：36 个旧名**全部只命中 `RENAME-REPORT.md` 的那一行映射记录**，命中总数 36，其中非文档命中 **0**。也就是说没有任何一处代码/配置按文件名引用了这 36 个文件。

补充：`.cs` 文件之间也没有 `#line`/`// Assets/Scripts/...cs` 之类的路径注释（`fix-names.sh` 只改文件名，没动内容，若原来有路径注释就会变陈旧 —— 实测不存在）。

### 2.3 编译器生成类名引用（`ref-scan-types.txt`，109 行 / 36 个文件）

按 `c__Iterator|c__AnonStorey|c__DisplayClass|f__AnonymousType|PrivateImplementationDetails` 扫描：

- 形态计数：`c__Iterator` 204、`PrivateImplementationDetails` 8、`c__AnonStorey` 6。
- 逐个体检查：对 36 个被改名的文件，从各自第 2 行 `// Type: <Xxx>c__IteratorN` 取出类型名，再 `grep -rn --fixed-strings` 排除自身文件后搜索。
- **外部命中数 = 0**（脚本输出 `TOTAL_EXTERNAL_HITS=0`）。每个编译器生成类型只在自己的文件里出现（`// Type:` 注释、`internal sealed class` 声明、同名构造器），没有别的 `.cs` 通过 `new <>c__Iterator13()` / `typeof(...)` 引用它。

因此即便这些名字看起来"没人用"，也不会因为文件名变化而产生断链。

### 2.4 CI 配置

`grep -rn "Assets/Scripts" .github/` 的 7 处命中全是通配写法，没有硬编码单个脚本名：

```
.github/workflows/build-arm64.yml:21:        mkdir -p UnityProject/Assets/Scripts
.github/workflows/build-arm64.yml:29:        cp -r Assets/Scripts/* UnityProject/Assets/Scripts/ 2>/dev/null || true
.github/workflows/build-docker.yml:21 / :29   （同上）
.github/workflows/build.yml:20 / :28 / :29    （mkdir / -d 判断 / ls *.cs | wc -l）
```

改名对 CI 无影响，无需处理。

## 3. 大小写冲突（问题 A）

### 3.1 全量枚举

```bash
git ls-files | tr 'A-Z' 'a-z' | sort | uniq -d
```

输出**仅一行**：`assets/scripts/scenario.cs` —— 全仓库只有这一组冲突。

### 3.2 差异判定

| 路径 | 类型 | 行数 | 字节数 | blob |
| :--- | :--- | ---: | ---: | :--- |
| `Assets/Scripts/Scenario.cs` | `public class Scenario` | 847 | 18986 | `23d2621` |
| `Assets/Scripts/scenario.cs` | `public sealed class scenario` | 13 | 240 | `c22f96a` |

`diff` 显示两者字段集合、类型名、构造器实现全不相同（`Scenario` 有 `script_db`/`black`/`txtbox1` 等 33 个字段，`scenario` 只有 `_chaimg`/`_mirror`/`_pos`/`_showef`/`_txtidx`/`_extraimg` 6 个）。**是两个不同的类型，不是重复文件**，所以走"保留两份 + 重命名其一"，不删除。

### 3.3 处理方式

```bash
git mv Assets/Scripts/scenario.cs Assets/Scripts/scenario_lower.cs
```

- 改的是**小写**那个：`Scenario.cs` 与它的类型名 `Scenario` 保持自然一致，同时它是主体文件（847 行 vs 13 行），改动面更小。
- 后缀 `_lower` 表达"这一对里名字小写的那个"，不引入 `Scenario2` 这种会被误读成"第二个场景类"的编号。
- **文件内容一个字节都没改**：`git diff --cached --stat -M` 显示 `0 insertions(+), 0 deletions(-)`，blob 仍是 `c22f96aca754a95fcbd9f12761a821a62a7d6be1`。文件里声明的类型依然叫 `scenario`，只有文件名变了 —— 读代码时按 `// Type: scenario` 那行认它。
- 新名字与任何已有路径不冲突（`git ls-files | grep -ic '^Assets/Scripts/scenario_lower\.cs$'` 改名前为 0）。

### 3.4 复验（A-4）

改名后再跑 `git ls-files | tr 'A-Z' 'a-z' | sort | uniq -d`，输出为空。

## 4. 顺带做的 Windows 文件名安全体检

既然目标是"Windows 上干净 clone"，除大小写外把其它 NTFS 限制也扫了一遍：

| 检查项 | 命令要点 | 结果 |
| :--- | :--- | :--- |
| 保留设备名 `CON/PRN/AUX/NUL/COMn/LPTn` | `basename` 后正则匹配 | 无 |
| 结尾 `.` 或空格 | `git ls-files \| grep -E "[. ]$"` | 无 |
| 残留非法字符 `<>:"\|?*\\` | `git ls-files \| grep -E "[<>:\"'"\|'?*\\\\]"` | 无（36 个已全部处理） |
| 最长文件名 | — | 42 字符（`_lt_GetFaceBookFriends_gt_c__Iterator1C.cs`） |
| 最长相对路径 | — | 57 字符，远低于 260 的 MAX_PATH |

另有两组**同 basename 但大小写不同/相同、且位于不同目录**的情况，**不构成 NTFS 冲突**，不动：

- `Assets/Scripts/LitJson/JSON.cs` 与 `Assets/Scripts/MiniJSON/Json.cs`
- `Assets/Resources/splash.png` 与 `Assets/Textures/splash.png`

## 5. 结论

- [x] **无有效的旧文件名引用，不需要修复源码内容**（B-3-B 不执行）。36 个旧名的命中全在改名记录文档里；编译器生成类型零跨文件引用；CI 只用通配。
- [x] `.cs` 内容中的 687 行 `<>` 属于反编译产物固有属性（编译器生成标识符 + IL 注释），**保持原样，不改**。
- [x] 唯一实际需要修的是问题 A 的大小写冲突，已按 3.3 处理，1 组冲突 → 0 组。

## 6. Windows 工作区的落地方式

后续开发调试都在 Windows 工作区 `D:\wolf\self-game\...\code\undead-slayer-xiahoudun-decompiled` 进行，所以修正必须同时落到那里，而不只是留在 WSL。

### 6.1 落地前 Windows 侧的真实状态（已实测）

- 磁盘上存在的是 **`Assets/Scripts/scenario.cs`（253 字节）**，即小写那份 + CRLF（`core.autocrlf=true`，240 字节 LF → 253 字节）。
- `git hash-object` 得 `c22f96a`，**等于 `HEAD:Assets/Scripts/scenario.cs`** —— 所谓 `modified: Assets/Scripts/Scenario.cs` 完全是大小写冲突造成的假象（`core.ignorecase=true` 让 git 把磁盘上的 `scenario.cs` 认成索引里的 `Scenario.cs`），没有任何本地改动。**847 行的 `Scenario` 内容当时根本没写到磁盘上。**
- 该残留可从 git 对象库完整恢复，删它零丢失；`git stash list` 为空，`git reflog` 只有 `clone: from ...` 一条。

### 6.2 采用的方式：就地 fetch + reset（不必先 push）

Windows 侧 git 能直接把 WSL 仓库当 remote 读（已用 `git ls-remote` 验证）：

```bash
rm Assets/Scripts/scenario.cs      # 先清掉冲突残留，避免 ignorecase 把它认成新树的 Scenario.cs
git fetch '//wsl.localhost/Ubuntu/home/lizhi/undead-slayer-xiahoudun-decompiled' main
git reset --hard FETCH_HEAD
```

之所以要先 `rm`：新树里同时有 `Scenario.cs` 和 `scenario_lower.cs`，磁盘上残留的 `scenario.cs` 会被 `core.ignorecase` 匹配成 `Scenario.cs`，导致写错内容、文件名还是小写。删掉残留后 git 按新树干净地创建两个文件。

没有选"删目录重新 clone"：那样会连带清掉 `.git` 和本地配置；`git clone` 的干净性已经在上一次 clone 中验证过（`<>` 那批不再报 invalid path）。

### 6.3 落地后的验收点

| 检查 | 期望 |
| :--- | :--- |
| `ls Assets/Scripts \| grep -i scenario` | `Scenario.cs` + `scenario_lower.cs` + `DB_Scenario.cs` + `Language_Scenario.cs` 四份都在 |
| `Scenario.cs` | 847 行，内容对应 blob `23d2621` |
| `scenario_lower.cs` | 13 行，`// Type: scenario`，内容对应 blob `c22f96a` |
| `git status` | 干净，不再有 `modified: Scenario.cs` |
| checkout / clone 输出 | 无 `the following paths have collided` 警告 |
| `git ls-files \| tr 'A-Z' 'a-z' \| sort \| uniq -d` | 空 |

