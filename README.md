# 亡灵杀手：夏侯惇 - 反编译与重建工作区
> Undead Slayer: Xiahou Dun — decompiled source & reconstruction workspace

原始 APK：`com.nhncorp.skundeadck` v3.5.0 (versionCode 36)，Unity **4.5.4f1**，armeabi-v7a，minSdk 10。
本工作区的目标：把它重建成能在 **Unity 2022.3.62f3c1** 里打开、编译、出包的可运行工程。

## 目录结构（2026-09-28 改造后）

```
dump/                 原始 APK 转储，只读参照，别再让编辑器碰它
  assets/             ← 原 Assets/Resources：level0..22、mainData、sharedassets*(含 .splitN 分片)、
                        约 370 个按 GUID 命名的资源、unity default resources 等
    Managed/          ← 原始程序集：Assembly-CSharp.dll(962KB) 等 7 个，反编译的唯一真值来源
  il-dump/            ← 原 Assets/Scripts：338 个“伪源码”（自定义 Python+dnfile 的元数据转储，不可编译）
  textures/           ← 原 Assets/Textures（只有 1 个 splash.png）
  lib/                ← armeabi-v7a 的 9 个 .so（含 libunity/libmono 与运营商 SDK）
  AndroidManifest.xml, apktool.yml

unity/                ★ Unity 2022 工程本体，用编辑器打开这个目录
  Assets/Scripts/     292 个 .cs：由 ILSpy 从 dump/assets/Managed/Assembly-CSharp.dll 重生成的真 C#
                      （含 LitJson/、MiniJSON/、SimpleJSON/）
  ProjectSettings/    Unity 自己生成/重写过的真设置
  Packages/

tools/migration-probe/  无编辑器也能跑的编译探测（拿 2022 的 UnityEngine*.dll 编 Assets/Scripts）
REFERENCE-SCAN.md     文件名重命名后的引用检测报告
RENAME-REPORT.md      非法文件名 / 大小写冲突的改名记录
```

## 已经证实的事实

| 事项 | 结论 |
| :--- | :--- |
| `dump/il-dump` 那 338 个 `.cs` | **不是 C#**：338/338 含非法 `{{`，312 个是 IL 指令转写（约 150 万行），0 个文件出现 `UnityEngine`，真 C# 语句行 0，`csc` 全量编译 8032 个语法错误。调用点被写成 `call 0x0A00002A` 这类元数据 token，API 名不在其中 |
| 反编译真值来源 | `dump/assets/Managed/Assembly-CSharp.dll`（+ `-firstpass.dll` 里的 SimpleJSON）。ILSpy 产物 292 个 `.cs`，**285 个用户类型零丢失** |
| Unity 4.5 → 2022 托管代码迁移面 | 迁移前 **1430 处编译错误、158 个文件**（`dotnet build -t:Rebuild` 净编译实测，两次同值）。**现已归零**：292 个脚本在 C# 9 + 2022.3.62f3c1 托管程序集下 **0 error / 532 warning**，探测工程产出 951,296 字节程序集。根因高度集中，681 处是 `Component/GameObject` 旧版快捷访问器（`animation` 237、`renderer` 131、`collider` 99、`rigidbody` 98、`audio` 86、`camera` 17、`particleSystem` 7、`AddComponent(string)` 7）在 2022 变成 `CS0619`（过时即报错），另有 660 处是它们返回 `Component` 后引发的连锁 `CS1061/CS0021/CS0266`，84 处是已移除的旧粒子系统，5 处是已移除的 `GUIText` |
| 迁移是怎么做的 | 1341 处（94%）由 `tools/migration/migrate_legacy_accessors.py` 机械改写：`.animation` → `.GetComponent<Animation>()` 等；72 处 `X.emit = b;` 因为是给"属性返回的结构体"赋值（CS1612），统一提升成 `{ var emission = X.emission; emission.enabled = b; }`。跑之前先用 `tools/migration/audit_shims.py` 证明过安全性：全语料**没有任何脚本自己声明同名成员**、`.NAME` **没有一处出现在字符串或注释里**、只有 3 个同名局部变量且它们只从旧访问器**取值**（所以"必须带前导点"的正则碰不到它们）。剩余 15 处手工改：`FPS.cs` 的 `GUIText`→`UI.Text`、`Cha_Skill.cs` 的 `ParticleEmitter` 类型名、`Cha_Control_ride_horse_story.cs` 的 `minEnergy/maxEnergy`→`main.startLifetime`、7 处 `AddComponent(string)`→`typeof(T)`（字符串那 6 个类名已逐个核对确实存在，只有 `_scriptname` 是运行期变量才用 `Type.GetType`） |
| 两处非 1:1、需按视觉复核 | ① 旧 `ParticleEmitter.Emit()` 语义是"发一个粒子"，按字面译成 `Emit(1)`（`Bullet_Angel4`、`Splash_angel_bomb`）；② 旧 `rndVelocity`（每轴随机初速）在 2022 无同名 API，`Poisonball.cs` 用 `VelocityOverLifetime` 的"两常量间随机" ±1 近似 |
| `Application.LoadLevel` 等 | 2022.3.62f3c1 的 `UnityEngine.Application` **仍然提供** `LoadLevel/LoadLevelAdditive/LoadLevelAsync/loadedLevel/loadedLevelName/ExternalCall`（实测反编译 `UnityEngine.CoreModule.dll` 得到）→ 场景切换代码不需要改 |
| `WWW` | 仍在，只是 `CS0618` 过时告警，能编能跑 |
| `ICSharpCode.SharpZipLib` | **不是"多余的 using"，是真依赖**：SimpleJSON 的 `SaveToCompressedStream/File`、`LoadFromCompressedStream/File` 确实在用 `BZip2OutputStream`/`BZip2InputStream`。游戏自带的两份 SimpleJSON 也印证了这一点 —— `LitJson` 那份把 4 个方法写成 `throw`（没链 SharpZipLib），`SimpleJSON` 那份是真实现。**原版那颗珠子用不了**：它是"SharpZipLib for .NET Framework 2.0"，在工程当前的 `apiCompatibilityLevel: 6`（.NET Standard 2.1）下实测炸 12×`CS0012` + 4×`CS1503` + 2×`CS1674`，因为 `System.IO.Stream` 在两边解析成不同程序集。改放 NuGet `SharpZipLib` 1.4.2 的 **netstandard2.1** 珠子到 `unity/Assets/Plugins/`（MIT，仓库 `icsharpcode/SharpZipLib@33f64eb`）：命名空间与 `BZip2InputStream(Stream)`/`BZip2OutputStream(Stream)`/`IsStreamOwner` 签名与原版逐一对得上，**SimpleJSON 一行都不用改**，也保住了 .NET Standard 2.1 档位（也就是保住 IL2CPP/arm64 路线） |
| 分片资源 | 12 组 `sharedassetsN.assets.split0..N` 按序号拼接即还原：拼合后头部声明的文件长度与实际字节**完全相等**；序列化版本 9 = Unity 4.x |
| 资源可读性 | UnityPy 1.25.3 实测 **469 个文件全部解析成功、0 失败**：2774 `GameObject`、915 `Texture2D`、427 `AnimationClip`、355 `Material`、256 `Mesh`、82 `AudioClip`、270 `MonoScript` 等 |
| 数据分三处（#9 的范围） | `level0..22`（23 个场景，1207 `GameObject`）+ `sharedassets0..23`（24 个，1222 `GameObject`）+ **`dump/assets` 下 425 个哈希名 AssetBundle**（只有 345 `GameObject`，多数是单资源包）。旧粒子三件套 `ParticleAnimator`/`ParticleRenderer`/`EllipsoidParticleEmitter` = 121/121/120，分布是 **场景里 39 套、sharedassets 里 45 套、AssetBundle 里 37 套** —— 场景重建躲不开旧粒子，不是只在预制体里 |
| Unity 4 构建数据**没有 TypeTree** | 所以 UnityPy 按它自己的（Unity 5.5+ 的 12 字节 PPtr）假设去读 `MonoBehaviour`，394 个里只成功 98 个，其余报 `Expected to read N bytes, but only read 24 bytes`。实际布局是 **Unity 4 的 8 字节 PPtr（int32 FileID + int32 PathID）**，`MonoBehaviour` 头部固定 24 字节：`m_GameObject@0(FileID)/4(PathID)`、`m_Enabled@8`、`m_Script@12/16`、`m_Name.length@20`，**脚本自己的字段数据从偏移 24 开始**。按定长偏移读 → 394/394 全部解析、0 异常 |
| 脚本绑定链已打通 | 上面那条布局让每个场景组件都能走 `m_Script(FileID,PathID)` → 该文件 externals 表 → `sharedassetsN` 的 `MonoScript.m_Name`（Unity 4 没有独立类名字段，m_Name 就是类名）→ `unity/Assets/Scripts/<name>.cs` → `.cs.meta` 的 guid。实测 **394 个 MonoBehaviour 全部解析成功**，涉及 123 个脚本，**123 个全部有对应 .cs 且编辑器已给出 guid**，零缺口。表在 `tools/migration/monoscript-bindings.{tsv,json}`。布局正确性有独立验证：394/394 的 `m_GameObject` 回指都落在同文件真实存在的 GameObject 上 |
| 编辑器设置 | `mainData` 里带着 `BuildSettings`/`TagManager`/`InputManager`/`PhysicsManager`/`PlayerSettings` 等 17 个管理器，已逐个核实价值（见下面三行），并还原了 `TagManager.asset` + `EditorBuildSettings.asset` |
| 场景表（已还原） | `BuildSettings.levels` = **24 个场景原名**（`Assets/Scene/Ready.unity` … `Ending.unity`），已写进 `unity/ProjectSettings/EditorBuildSettings.asset`。脚本里 23 个 `LoadLevel("X")` 目标**全部**命中这 24 条，只有 `Ready`（index 0，启动场景）不靠名字加载。注意**磁盘上的 `levelN` 与这张表错位一格**（`levelN` = `levels[N+1]`，见缺口 5），别按文件名直接对位 |
| Tag/Layer（已还原） | **两者都是真依赖**。Layer：28/32 层有名字（`unit` `GUI` `obj` `loading1` `enemyattack` 和整套 `…attack` 攻击分层），数据侧 2774 个 `GameObject` 里 1073 个用 layer 8、188 个用 11、88 个用 10。Tag：Unity 4 的 `GameObject` 序列化的是 **`m_Tag`（int）**，不是 2022 的 `m_TagString`（字符串）—— 自定义 tag 存成 `20000 + 在 TagManager.tags 里的下标`。实测 65 个 `GameObject` 带非零 tag，且 **20000..20009 十个值全部出现**（`ui` 16 个最多），跟声明的 10 个自定义 tag 一一对上；另有内置 tag `5`（14 个，对象名就是 `Main Camera`）和 `6`（5 个，`cha1` 主角）。**所以 10 个 tag 必须还原**：2022 场景文件按**名字**存 tag，名字不在 TagManager 里会在导入时被重置成 Untagged。（更正：本仓库先前写的"2429 个 GameObject 的 m_TagString 全为空、tag 没人用"是**测量错误** —— 当时读的字段名 `m_TagString` 在 Unity 4 的 GameObject 上根本不存在，`getattr` 取了默认值 `""`，于是"全为空"是必然结果，不是发现。） |
| 输入设置（**故意不还原**） | `mainData` 的 17 条轴就是 Unity 4 出厂默认（逐字段比对 2022 默认表：14 条完全一致，差异只有 `Window Shake X/Y` 这两条早已被移除的轴、和 `Fire3` 绑的是 mac 的 `left cmd`）。而且脚本**一条轴都没读**：`GetAxis`/`GetButton` 全 0 次，输入全靠 `mousePosition`(56)/`GetTouch`(16)/`GetMouseButtonDown`(13)。覆盖反而会删掉 `Submit`/`Cancel`（#9 的 uGUI EventSystem 要用），所以留着 2022 的原样 |
| MonoBehaviour 规范 | 253 个 MonoBehaviour 类，文件名与类名**零不一致**，无一文件挂两个类 |

## 仍未解决 / 已知缺口

代码侧编译已经通了，剩下的全是**数据侧**和**可选清理**。

1. **旧粒子数据必须重做**：代码已经全部指向 `ParticleSystem`，但数据侧还有 121 `ParticleAnimator` + 121 `ParticleRenderer` + 120 `EllipsoidParticleEmitter`，2022 里这些组件不存在。**在它们被重建成 Shuriken 之前，那些 `GetComponent<ParticleSystem>()` 运行时会拿到 null 并抛 NRE** —— 这不是代码 bug，是资源还没到位。
2. **`FPS.cs` 需要一个 Canvas**：`GUIText` 被整体移除，已换成 `UnityEngine.UI.Text`，但 `UI.Text` 必须挂在 Canvas 下才渲染。原脚本的自检守卫保留了（拿不到组件就 `Debug.Log` + 自我禁用），所以缺 Canvas 时会**明确报错而不是静默失效**。已用 UnityPy 核实 `FPS` 确实是游戏自带的 MonoScript（在 `sharedassets0.assets` 里），不是可以随手删的死代码。
3. **532 条过时告警（不阻塞）**：`GameObject.active` 414、`Application.LoadLevel(string)` 77、`WWW` 21、`Application.loadedLevelName` 6、`Renderer.castShadows` 6、`Application.LoadLevelAdditiveAsync(string)` 3、`Application.loadedLevel` 2、`Transform.RotateAround(Vector3,float)` 2、`TouchScreenKeyboard.done` 1，2022.3 里全部仍可运行，但未来版本会移除。
   其中 `Transform.GetChildCount()` 那 4 处**已清掉**：它是全语料唯一带 Unity `(UnityUpgradable)` 标记的一档（`Cha_Costume.cs:44`、`Cha_Costume_shop.cs:39`、`UI_cashshop.cs:308/681`），编辑器会为此弹 "Script Updating Consent"。**点 No，别让它改** —— 那个对话框的默认按钮写的是 "these **and other files that might be found later**"，等于给整个会话挂上 updater，而 #9 导资源时会不断有新过时 API 冒出来，工作副本会被当着我们的面就地改写、事后无从核对。已改成 codemod 里的一条 `rename.GetChildCount` 规则（`.GetChildCount()` → `.childCount`，改前核对过：全语料没有任何类型自己声明过这两个名字、也没有一处出现在字符串或注释里），改完告警 536→532、幂等复跑 0 文件。
   另外原版有一批全小写类名（`weapon`/`stagestat`/`scenario`/`petset` 等），改名属于逻辑漂移，不动 —— 但**它们并不是告警**：早先记的"15 条 `CS8981`"是探测工程按 `net8.0` 编才产生的假信号，实测同一个 `class weapon{}` 在 `net8.0` 下出 CS8981、在 `netstandard2.1`（= Unity 真实档位）下一条不出，Unity 根本不会报它。
4. **着色器：已定论，是"改名表"，不是重写着色器**。权威口径是**顺着 355 个 `Material` 的 `m_Shader` 指针、经各文件的 externals 表解析出来的**：**355 个材质、0 个解析失败、实际只用到 15 个着色器名**。分布是 `Mobile-Particle-Add` 104、**`Mobile-VertexLit-OnlyDirectionalLights` 100**、`Unlit-Alpha` 43、`Unlit-Normal` 30、`Mobile-Particle-Alpha` 21、`Mobile-Particle-Multiply` 16、`Mobile Lightmap Only` 10、`Mobile Alpha Blend plus` 8、`Mobile Lightmap Only2` 8、`Font` 6、`Mobile-Diffuse` 4、`Particle Add` 2、`Mobile-BumpSpec` 1、`Mobile-VertexLit` 1、`Sprites-Default` 1。
   （数据里共有 **31 个 Shader 对象、31 个不同名字**，但另外 16 个是 `Internal-*` / `Camera-*` 这类引擎自带着色器，住在 `dump/assets/unity default resources` 里、不被任何材质引用，不用管。**我在这条上连错两次**：先说"31 个待重写"（没去重也没看谁在用），后来说"12 个"（扫描范围漏了 `unity default resources` 和 AssetBundle）。教训是：口径必须从**引用方**倒推，不能从对象清单正推。`ShaderProbe.cs` 已改成用这 15 个名字。）
   编辑器内 `Shader.Find` 实测（`Undead Slayer > Probe Shader Names (Read-only)`）：那 12 个 Unity 4 名字**一个都解析不出来（0/12）**，但每个都有现成的 2022 替代 —— 所以不需要写任何着色器代码。已定映射（剩 `Mobile Lightmap Only2` / `Font` / `Sprites-Default` 三个待同一次探针确认，探针已补进去）：

   | 数据里的名字（Unity 4） | 2022 目标 | 依据 |
   | :--- | :--- | :--- |
   | `Mobile-Diffuse` | `Mobile/Diffuse` | 只是分隔符变了，探针确认存在 |
   | `Mobile-BumpSpec` | `Mobile/Bumped Specular` | 同上 |
   | `Mobile-VertexLit` | `Legacy Shaders/VertexLit` | **`Mobile/Vertex Lit` 在 2022 已删**（探针实测 null） |
   | `Mobile-VertexLit-OnlyDirectionalLights` | `Legacy Shaders/VertexLit` | 没有"只算方向光"的变体，取最接近的；**这是一处行为差异** |
   | `Mobile Lightmap Only` | `Mobile/Diffuse` | 光贴图数据本来就没迁（LightmapSettings 未搬），差异不额外放大 |
   | `Mobile Alpha Blend plus` | `Legacy Shaders/Transparent/Diffuse` | 透明混合漫反射；这名字带空格和 "plus"，可能是当年打包进来的第三方着色器 |
   | `Mobile-Particle-Add` | `Legacy Shaders/Particles/Additive` | 选 Legacy 而不是 `Particles/Standard Unlit`，因为要的是固定管线那套行为 |
   | `Mobile-Particle-Alpha` | `Legacy Shaders/Particles/Alpha Blended` | 同上 |
   | `Mobile-Particle-Multiply` | `Legacy Shaders/Particles/Multiply` | 同上 |
   | `Particle Add` | `Legacy Shaders/Particles/Additive` | 同上 |
   | `Unlit-Alpha` | `Unlit/Transparent` | Unity 内部名 `Unlit-Alpha` 对应显示名 `Unlit/Transparent` |
   | `Unlit-Normal` | `Unlit/Texture` | 同上 |

   另外探针确认这些在 2022 存在、可作为兜底：`Standard`、`Unlit/Color|Texture|Transparent`、`Mobile/Diffuse`、`Mobile/Bumped Specular`、`Legacy Shaders/*` 全套、`Legacy Shaders/Particles/*`、`Particles/Standard Unlit`、`Sprites/Default`、`UI/Default`、`Diffuse`、`VertexLit`。这些**不存在**：`Mobile/Vertex Lit`、`Particles/Unlit`、`Particles/Added`、`Particles/Alpha Blended`、`Particles/Multiply`、`Particles/Additive`（即 `Particles/` 下只有 `Standard Unlit` 活着，旧的都要走 `Legacy Shaders/Particles/`）。
   材质对象本身解析是干净的：`m_Name`、`m_Shader`（PPtr）、`m_SavedProperties` 的 `_Color` / `_MainTex`（纹理 PPtr）都正确，而 2022 才有的字段（`m_ShaderKeywords`、`m_LightmapFlags`、`m_DoubleSidedGI`、`stringTagMap` 等）全为 None/空，说明 UnityPy 对 v9 的 Material 没有像 MonoBehaviour 那样读歪。
   **一个要提前认下来的代价**：单材质数最多的两档是 `Mobile-Particle-Add`(104) 和 `Mobile-VertexLit-OnlyDirectionalLights`(100)，前者有对等的 Legacy 着色器、问题不大；**后者在 2022 根本没有"只算方向光"的变体**，只能退到 `Legacy Shaders/VertexLit`，也就是说 **355 个材质里约 28% 拿不到等价的光照行为**（会额外接受点光源、光照算法也不同）。这不是工具没做好，是 Unity 5 删了这档着色器；真要一比一，得自己写一个只算方向光的着色器。
5. **缺的场景是 `Ready`（index 0），不是 `Ending` —— 本仓库先前搞反了**：`dump/assets` 里只有 `level0..level22` 共 23 个场景文件，而 BuildSettings 列 24 条。先前按"`levelN` = 第 N 条"直接对位，就推出"缺 index 23 = Ending"。**这个对位是错的**，实测关系是 **`levelN` = `BuildSettings.levels[N+1]`，整体错位一格**，所以没有源数据的是 index 0 的 `Assets/Scene/Ready.unity`，而 `Ending` 是有数据的（就是 `level22`）。
   证据不是靠场景内容猜的，而是**看每个场景挂了哪个控制脚本**：`level22` 唯一的 MonoBehaviour 绑的就是 `Ending`；`level6`→`UI_map`、`level7`→`UI_general`、`level8`→`UI_status`、`level9`→`UI_forge`、`level10`→`UI_skill`、`level11`→`UI_pet`、`level12`→`UI_archive`、`level13`→`UI_Extreme`、`level14`→`UI_Friends`、`level19`→`UI_result`、`level20`→`UI_result_ride`、`level21`→`UI_result_extreme`、`level0`→`UI_intro`+`Cam_intro`、`level1`→`Cha_Control`+`AI_StoryEnemy`+`Story_trans`。23 个全部一一对上，无一例外。另外 `UI_Ready` 这个脚本**没有挂在任何 level 上**，与"Ready 无场景文件"互相印证。
   实际影响：① `UI_map.cs:935` 的 `Application.LoadLevel("Ending")` 是**可以工作的**（先前记成"必然失败"，错了）；② 真正缺的是启动场景 —— `Login_again.cs:16` 会判 `Application.loadedLevelName != "Ready"`，而 `Ready` 无数据，重建时得自己补一个；③ 好在**全代码只用字符串名加载场景、没有一处用数字索引**（已核），所以错位一格不会直接弄坏跳转，但 `EditorBuildSettings` 的顺序仍要按原名表钉住。
   **④ 这条现在升级成了运行期的总闸门：`Ready` 里装的是存档解密的引导代码。** 由编辑器 Console 的真实堆栈 + 与当前源码逐行对齐的行号定死（不是推测）：`Crypto.KEY2` 只在 `Crypto.Init()` 里赋值，`Init()` 只被 `UI_Ready.cs:48`（`Ready` 场景）调用，而 `UI_Ready` 和 `AndroidScript` **在 23 个已恢复场景里一个都没挂上**、也没有任何 `AddComponent` 会补它 —— 它们就住在缺失的 `Ready` 里。于是 `KEY2` 恒为 `string.Empty`，`Load_int_key` 的 `catch` 兜底（`Crypto.cs:101`）拼出的密钥只有 8 字节（`_kind` 前 8 字符），`RijndaelManaged.Key` 只接受 128/192/256 位 → `CryptographicException: Specified key is not a valid size` 从 `Crypto.cs:150` 一路抛出到 `UI_general.Awake`(:256) / `UICampos.Start`(:7)。
   注意**存档本来读不到是不该炸的**：`DecryptRJ256` 对空密文返回 null，`Convert.ToInt32(null)` = 0（两条都已跑程序验过，不是查文档），所以真机上这条兜底就是"取默认值"。设备上不炸、桌面必炸，差别只在 `KEY2` 长度。而 `Init()` 里 `KEY2` 又依赖 `AndroidScript.i.getDeviceId()`（`new AndroidJavaClass("com.common.libs.CommonActivity")`，**原文件没有任何平台宏/`Application.platform` 保护**，`_i` 也只在它自己 Awake 里赋值）。
   **已按"Android 桥桌面降级 + 手补 Ready"修掉（2026-09-29）**，改动刻意只覆盖启动路径这三处：`AndroidScript.HasJavaBridge`（`Application.platform == RuntimePlatform.Android`，全项目唯一定义）→ `AndroidScript.Start()` 非 Android 不再构造 `jcBI`、`AndroidScript.getDeviceId()` 非 Android 返回 `SystemInfo.deviceUniqueIdentifier`、`ChannelMgr.init()` 非 Android 直接返回（`channelId` 保持默认 `"000000"`，`UI_Ready.OnGUI` 本来就有"未知渠道不画图"的分支）。**`pay`/`LogEvent`/`showNotice`/`QuitGame` 等用户触发的桥一律不加降级** —— 那些静默桩掉才是真正的逻辑漂移，缺 SDK 就该报错。**代价要说清**：桌面 KEY 和 KEY2 会算成同一个值（都取自 `deviceUniqueIdentifier`），所以这份存档与真机存档互不相通；`jcBI` 为 null 时 `LogEvent` 会 NRE，这是有意保留的现状。
   `Ready` 场景本身由 `unity/Assets/Editor/ReadySceneBuilder.cs` 的菜单 `Undead Slayer > Build Stand-in Ready Scene (NOT recovered data)` 生成（3 个 GameObject：`Main Camera`/tag MainCamera、`AndroidScript`、`UI_Ready`）—— **这是手工替身，不是还原数据**，`UI_Ready` 那些分渠道闪屏贴图和 `basicSkin` 无从知晓，保持 null。`AndroidScript` 必须和 `UI_Ready` 同场景，因为没有别处会 `AddComponent` 它。
   受影响的 Awake/Start/OnEnable 站点数（按场景静态数出来的）：`Stage` 13 处（含 `Cha_Control`、`Spawn`、`Cam_Move`、`Cha_Skill`、`AI_Asist`、`Icon_Skill`）、`Map` 4 处（`UI_map` OnEnable+Start、`CamScroll` Start、`LoopArchive` Awake）、`Extreme` 4 处、`General` 3 处。**抛出点之后的代码一律不执行**，所以"场景能打开、渲染器都在"和"界面按预期摆好"是两件事。
6. **层碰撞矩阵还没落地（`mainData` 里最有游戏性价值的一项）**：`PhysicsManager.m_LayerCollisionMatrix` 有 **27/32 条不是默认值** —— 这决定了哪个攻击层能打在哪个目标层上，也就是战斗手感本身。数据已完整取到并核对（528 对全对称），存在 `tools/migration/maindata-physics.tsv`，但**故意没有写进 `DynamicsManager.asset`**：2022 把它序列化成 256 个十六进制字符，每个 8 字符块究竟是 uint32 的**小端字节**还是**大端数字**我没能在离线环境确证（只有一篇 CSDN 断言小端）。两种写法 Unity 都照收不误、不报错，猜错就是把每个 mask 的位打乱、静默改掉命中关系。正解是**别让脚本猜**：在编辑器里用 `Physics.IgnoreLayerCollision(i, j, …)`（已核实这是唯一的公开写入 API，`Physics.SetLayerCollision` 并不存在）逐对设好再 `AssetDatabase.SaveAssets()`，由 Unity 自己按正确格式写文件。`m_SolverIterationCount 4→m_DefaultSolverIterations`（现为 6）同理待落，其余 Unity 4 物理字段要么已与 2022 相同、要么在 5.0 就换了模型无对应（`m_SleepVelocity`/`m_SleepAngularVelocity`/`m_MinPenetrationForPenalty`）。
7. **没有 arm64-v8a native 库**：`dump/lib` 只有 9 个 armeabi-v7a `.so`（`libunity`/`libmono` + `libcmcc_haze`/`libcmcc_rusteze`/`libcasdkjni`/`libad`/`libidentifyapp`/`libparkour`）。出 64 位包要么拿到厂商 64 位库，要么砍掉这些渠道 SDK。
8. **纹理已导出完毕（912 / 915）**：`tools/migration/export_textures.py` → `unity/Assets/`，26 MB，引用表在 `tools/migration/texture-index.json`（键是 `源文件!path_id`，材质重建要靠它对上）。3 颗没有像素可写，全部叫 `Font Texture`、尺寸 `0x0`、`.image` 抛 `No image data found`（`sharedassets0!27`、`sharedassets14!23`、`unity default resources!10103`）：它们是 Unity 4 给动态字体烘的图集，被 `Font.m_Texture` 和 `PreloadData.m_Assets` 引用，运行时按字符数据重建；2022 的动态字体同样不带烘好的图集，所以这 3 颗**不进引用表**，而是连同理由写进 `tools/migration/texture-skips.json`，让 912+3=915 这笔账在数据文件里也对得上。格式分布实测 `RGBA4444` 611（66.8%）/ `ETC_RGB4` 155（16.9%）/ `RGBA32` 120 / `RGB24` 12 / `ARGB32` 3 / `Alpha8` 7 / `RGB565` 7 —— **八成以上本来就未压缩**，"全是 ETC1 需自写解码器"的说法以偏概全；6 种格式各抽一颗走 UnityPy 的 `.image` 全部出图，含 ETC。（枚举值是按 Unity 2022 的 `UnityEngine.TextureFormat` 反编译对照的，不是凭记忆。）
   **文件名规则在 2026-10-10 改过一次**：旧规则无条件追加 `_<path_id>`，于是代码里 `Resources.Load("jewel01a")` 要的贴图在盘上叫 `jewel01a_91.png`，`check_resources_load_keys.py` 数出 63 个字面量 key 里有 23 个被这条路堵死（涉及 135 个文件）。新规则是**只用源名，名字被占用才追加 `_<path_id>`**（912 颗里 46 颗需要），并且**代码按名字加载、且要的就是贴图的那 161 颗放进 `Assets/Resources/`**，其余留在 `Assets/Imported/Textures/<bucket>/` 由 guid 触达。落位判据里**类型是必要条件**：第一版只问"有没有同名加载键"，于是 `bg_dun_cover`、`bg_worldmap`、`giftbox` 和 `wp_*` 共 **18 颗**因为一个 `as GameObject` 的键同名就被塞进 `Resources/`，等于让两个资产占同一个资源路径 —— `export_prefabs.py` 的命名冲突守卫把这条顶了出来，18 颗用同一套 guid 保序迁移挪了回去（`check_resources_load_keys.py` 早就按类型过滤，所以它的判决一直是对的，错的是导出器）。落地靠 `tools/migration/apply_texture_naming.py`：它做的是**带着 `.meta` 一起改名**，guid 不变所以引用不动；直接重跑导出器会得到新 guid 的新资产，等于把 349 个材质和 23 个场景的指向全部掐断。改完的核对：907 次移动逐个比对移动前后的 guid，`.mat`/`.unity`/`.prefab` 里 **1321 处贴图引用全部还能落到存在的文件**，`verify_scenes.py` 23/23 PASS，`materials.json` 里 344 个、23 份 `scenes/levelN.json` 里 527 个（按文件内去重）路径字符串重算后 0 处指向不存在的文件（场景 JSON 只改了 `asset` 这一个键，525 行换 525 行）。`check_resources_load_keys.py` 现在的判决是 **25 个 key OK + 38 个等 #17 的 prefab，没有任何 key 再因为命名被堵**。
   **出厂资源索引解到一半（2026-10-10，`tools/migration/dump_maindata_resource_index.py`，输出 `maindata-resource-index.json`）**：`mainData` 偏移 35,796–43,080 是 Unity 4 `ResourceManager` 的可加载资产表，共 **299 条**，记录形如 `(int32 A, int32 B, int32 名字长度, 名字补 NUL 到 4 对齐)`，名字按字典序。`(A=容器下标, B=path_id)` 这个读法是**从表内部钉住的**：凡"这个名字在全 build 只出现一次、且那个对象的 path_id 正好等于 B"的记录就强制 `files[A]` 必须是那个容器，结果 149 个 A 值钉出**零互相冲突、零与字节序容器列表不一致**，所以容器顺序与两列含义可以当已证。判决分布：**exact 163**、`points-at-other-object` 27（对象存在但 `m_Name` 不是键名，如 `angel_splash5`→GameObject `pt_ice`、`bullet_angel7`→`m2`）、`wrong-container` 17（如 `jewel01a`→`c170ce6b…!1` 那颗的 `m_Name` 是 `name_boss1`）、`name-exists-elsewhere` 92。**后两组共 109 条还没解释，别当结论用**：它们共享一个形状——某段连续记录里 B 等于**前一条**的真实 path_id（`petkind_angel3` 的 B=19，而 `petkind_angel2` 的对象真在 `!19`），这既可能是记录比"三个 int32 + 名字"更宽，也可能是另有一条并行的 path_id 数组被我读串了位。**这件事有两个后果**：① 原本要为 `angel_splash5` 这类同名根做启发式选择，现在不该做了——出厂表会直接给容器和 path_id；② 我按 `m_Name` 命名的纹理里，凡是被这 109 条指到的键（`jewel01a`、`ef_perfect`、`guide*`、`petkind_angel*`）**有可能是取错对象的那批**，等 109 条解释清楚再判要不要动 `67a2f9f` 那批名字，现在不回退也不追加改动。#17 的 prefab 导出同理，**没解完不动笔**。
   顺带一条硬证据：这张表把键写成**小写**（`cashshop`、`gameno`、`blockname`），而脚本里写的是混合大小写 `"CashShop"`、`"gameNo"`、`"blockName"`，包能跑，说明**原版查表不区分大小写**——`check_resources_load_keys.py` 文档里原先那句"player 里区分大小写"已按证据改掉（工具的比对本来就是折叠大小写后再单独报差异，行为不变）。
   **prefab 描述已导出 94 个（2026-10-10，`tools/migration/export_prefabs.py` → `tools/migration/prefabs/`，汇总 `prefabs-export.json`）**：要 GameObject 的键共 97 个，其中 **94 个只有唯一候选根**（按 `GameObject.m_Name` 在非 level 容器里匹配），3 个**只登记不猜**（`angel_splash5` 在 `cf478de1ef56c3444ac102712d1445a3` 里有 pid 2/3/4/5 四个同名根，`angel_splash8`、`wp_1` 各有两个不同容器）。共 334 个对象，节点结构与 `extract_scene.py` 的场景 JSON **同一套形状**，编辑器侧只有一套读法。prefab 相对场景多做的三件事都在这儿：只取根 Transform 的子树、把指向子树外的 `node` 字段和骨骼**删掉并计数**（资产不能引用兄弟对象，场景可以）、根 parent 强制 0。
   两个靠核对才露出来的问题：① **`FileCtx` 的 externals 原来只从 `level_externals` 取**，而那张表只覆盖 23 个 level，所以 bundle 里 `m_Script` 挂在外部文件的 `MonoBehaviour` 一律"脚本未解析"被静默丢掉（`bullet_angel3` 的 `Bullet_Angel2` 就是这么消失的，而 `prefab-contents.json` 的独立测量看得见它）——现在 level 用预计算、其他容器读自己的 externals 表；② 94 个根的分量直方图**逐个与那次独立测量比对**（把 MonoBehaviour 按类型计、并按 `Transform = 节点数` 对齐口径），不相等就整轮拒绝落盘。核对结论：23 份场景 JSON 二次生成逐字节稳定（externals 那处改动对 level 零影响），`verify_scenes.py` 23/23 PASS。
   **94 个 prefab 已在编辑器里建出（2026-10-10，菜单 `Undead Slayer/Build Prefabs From JSON (recovered roots)`，`unity/Assets/Editor/PrefabRebuilder.cs`）**：它**不搬 `SceneRebuilder` 那 500 行**，而是调 `SceneRebuilder.Rebuild(jsonPath)` 把描述建进一个临时场景（`Assets/Imported/_PrefabBuild/`，已 gitignore，跑完自己删），再把那个唯一的根 `SaveAsPrefabAsset` 成 `Assets/Resources/<键>.prefab` —— prefab 与场景共用同一条构建路径，不留第二套真源。每个 prefab 四道后置断言，任一不过就**拒绝这一份**：打开的场景必须正是刚存的临时场景（否则 Rebuild 提前 return 时会把上一个的根存成新名字）、根必须恰好 1 个、场景对象数必须等于描述的 `node_count`、临时场景里每个资产 guid 必须也出现在 `.prefab` 文本里。`SceneRebuilder` 只放宽了访问性（7 个嵌套类 `private→internal`、`FindJsonDir(sub="scenes")`、`EnsureFolderFor` 改 `internal`、`SceneFile` 加 `asset_path`），23 个 level 的调用一字未改。编译证据：先故意注入一个错确认 `EditorProbe.csproj` 真看得见新文件（报在 `PrefabRebuilder.cs(210)`），还原后 `--no-incremental` 全量 **0 错误**（533 条是既有 CS0618 积压，`AnimationClipBuilder.cs(571)` 那条也在其中，不是本批引入）。
   盘侧复核（全绿）：**94/94** 的 GameObject 名字集合与描述**完全相等**、对象数与 `node_count` 逐个相符；94 个 prefab 里 **416 处引用**只有 1 处不在 `Assets/` 的 guid 表里，而它是 `{fileID: 10100, guid: 0000...e000..., type: 0}` —— Unity 内建资源命名空间（默认粒子材质），不是丢档；**80 个不同的 `m_Script` guid 全部落到 `Assets/Scripts/*.cs`**（`AddComponent` 用当场解析的类型，所以 `.prefab` 里写的是 `.cs` 的真实 guid，与 `monoscript-bindings.json` 里那份 guid 不是同一个口径 —— 我第一版核对拿错了表，把 67 条读成"未知 guid"）。
   顺带修了核对工具自己的两处口径：`check_resources_load_keys.py` 的 `EXT_TYPE` **没有 `.prefab`**，而 `PENDING = {"GameObject", ...}` 是"还没有任何东西能满足 GameObject"的旧话，于是 94 个 prefab 建好后 38 个键仍报 `PREFAB PENDING #17` —— 那是对这个工具的判决而不是对工程的判决。补 `.prefab → GameObject`、把豁免收到只剩 `Transform` 之后，**63 个键全部 OK**。但"OK"只说名字与类型取得到，不说画面对：**3 个歧义根没有 prefab**（`angel_splash5`、`angel_splash8`、`wp_1`，登记在 `prefabs-export.json`），旧粒子组件与场景一样被标 `unsupported` 跳过（要等 #19 才有效果外观），所以按名字能取到 ≠ 演出来一样。
   **动画覆盖口径漏了 prefab（2026-10-10，`AnimationClipBuilder.Build`）**：那个菜单的"要建哪些 clip"只从 23 份场景描述里收集，prefab 描述完全不在视野内 —— 所以 427 份描述只有 140 个 `.anim` 落盘，建好的 prefab 里 6 处绑定报 `clip asset not found`（`general_fincanon` 的 creat/destroy/shoot、`Stage_dun2` 的 flower/flower_idle、`Stage_dun6` 的 snake_move）。把 `tools/migration/prefabs/*.json` 一起算进"被引用"后，`.anim` 到 **146**，prefab 的 11 处绑定 **0 处指向不存在的文件**，而那 3 个 prefab 的差异经 guid 差分确认是**只增不减**（各加上对应 clip 的引用）。
   同一轮 Editor 侧另外三处按证据修：① `Debug.LogWarning("A %s" + "B".Fmt(x))` 里 **`.Fmt` 只绑到最后那段字面量**（方法调用优先级高于 `+`），5 条动画日志因此常年打成字面 `%s`，既不报对象也不报 clip —— 现在改成括号包住整串；`StrFmt` 那句 `[Fmt: 0/2 arg(s) used]` 就是它自己在喊，别当装饰。② Unity **不允许关掉最后一个开着的场景**，`CloseScene` 被拒 94 次；改成先 `NewScene(EmptyScene, Single)` 再删临时文件，第二次跑该消息归 0。③ 临时场景与 `.prefab` 比 guid 时，`AssetDatabase.GUIDToAssetPath` 会把内建命名空间（`0000…e000…/f000…`，形状是 16 个 0 + 1 个 nibble + 15 个 0）解析成内部路径而非返回空，所以"非空即项目资产"筛不掉它们、94 个全误报；改判形状 + 路径必须在 `Assets/` 下。修完复核：**415 处项目资产引用 0 处不可解析**，也就是说这批警告从来不是引用丢失。
   口径教训两条，记下来免得再犯：**字节相同不是验收标准** —— Unity 每次保存都重新随机 fileID，比较要看规范化的文档集合/对象数/直方图（9 个变化的 prefab 里 6 个规范化后完全相同，剩下 3 个是上面那笔真实增量）；**"0 命中/全失败"要先怀疑自己的取键** —— 我这一轮写的两个临时 lint 都不可信（其中一个只认出 5 个 `.Fmt` 调用点，实际 14 个），最终结论全部改用逐处读源码 + 全量编译 + Editor 实跑日志三种实际证据支撑。
   还有一处诚实修正：`RankingManager.username` 是 `string[]`、50 个元素，描述里数据一直都在（50 个 `{kind:"str"}`，出厂值全为空串），缺的只是 C# 应用分支，所以每轮稳定报 `array of 50 applied nothing`。补了 `FieldItem.s` 与 `str` 分支 —— 因为字段初始化本来就是 `new string[50]`，**这次改动不改变任何可见值**，只是让那个计数器不再撒谎（它是否真的不再报，要下一次跑 `Build Prefabs From JSON` 才算证）。
   **一个坑值得记**：第一版把 `dump/assets` 下所有非 `level*`/`sharedassets*.assets` 的文件都当 AssetBundle 扫，结果**连 `sharedassetsN.assets.split0..N` 这些原始字节分片也扫了**，UnityPy 对半截文件解析出一堆幻影对象，报出"7466 张贴图"。分片必须先拼合，别直接 load。修好后 912+3=915，与独立统计吻合。
   **拼合是可重做的**：`<venv>\python.exe tools\migration\reassemble_splits.py`（`--verify-only` 可不写盘体检）。`dump/assets` 下 `sharedassetsN.assets` **只以 `.split0..K` 存在，仓库里没有任何完整文件**；副本位置由 `RECON_DIR` 决定，**现在固定用 `D:\wolf\self-game\recon-unity4`**，跑扫描前 `export RECON_DIR="D:\\wolf\\self-game\\recon-unity4"`（必须是 Windows 路径写法，那个 python.exe 是 Windows 可执行文件）。默认值仍是 `%TEMP%\recon`，而 temp 会被清：2026-10-10 那天 12 个只剩 1 个，`sources()` 于是只返回 461 个容器，一次音频清点因此数出"32 个 clip"（真数是 82）——**一次加载失败都没有，也没有任何警告**，世界只是变小了。为堵这条路，`unity4_files.sources()` 现在先比对 `split_families()`，哪个 split 家族没有可读整文件就直接抛 RuntimeError（要故意接受部分扫描才设 `ALLOW_INCOMPLETE_SOURCES=1`）。脚本本身按数值序拼接、要求下标从 0 连续（不连续就拒绝猜测），并做**两级后置断言**：字节总数与 md5、以及每个副本用 UnityPy 真加载出非 0 对象数。已验证：往全新目录重建 12 个，与现有副本 **md5 全一致**，且新副本单独通过加载断言。
9. **网格已导出（256 / 256，0 跳过）**：`tools/migration/export_meshes.py` → `meshes.json`（8.3 MB，83,181 个顶点），编辑器菜单 `Undead Slayer > Build Meshes From JSON` 用 `Mesh.SetVertices/SetUVs/SetTriangles` 造真 Mesh 资产。UnityPy 1.25.3 的 `Mesh` **没有 `Export()`**，而 .obj 表达不了 UV1/切线/皮肤权重，所以走"Python 出描述 + 编辑器落盘"。
   **顶点通道布局是从数据里验出来的，不是查表查来的**（扫全部 256 个网格，0 解析失败）：`0=位置`(dim3，值域 -5..17.3、只有 1% 单位长)、`1=法线`(dim3，-1..1、**100% 单位长**)、`3=UV0`(dim2，-3.375..4.375，平铺超出 [0,1] 属正常)、`4=UV1`(dim2，0..0.9999 即光照贴图 UV)、`5=切线`(dim4，-1..1、xyz **100% 单位长**)。`2=顶点色` 只出现在 12 个网格里，且**采样的 642 个顶点全是 255**（= 纯白），所以按 no-op 丢掉，不是"看不懂就扔"。索引宽度数据里没有标志位，做法是 16/32 位都试一遍，要求"字节数刚好被各 submesh 的 indexCount 用尽且所有下标 < 顶点数"，两种都不满足就报名字而不是硬猜。造完还会拿源数据的 `m_LocalAABB` 跟 `RecalculateBounds()` 的结果比，误差超 1% 就告警 —— 解错顶点流会当场暴露。
   **蒙皮不是问题（本仓库先前判断错过一次）**：网格里的 `m_BoneNameHashes` 确实只有 32 位哈希、**没有骨骼名字**（74 个蒙皮网格 0 个带名字），一度被判断成"`SkinnedMeshRenderer` 接不回层级"。那个结论是错的：`SkinnedMeshRenderer.m_Bones` 本身就是**指向同一 level 文件内 Transform 的 PPtr 列表**，按 path_id 直接接回重建出来的层级即可，名字哈希只在网格跨 prefab 共享时才需要。实测场景里 **133 个 `SkinnedMeshRenderer` 全部带骨骼列表**。
   **编辑器侧建完 256 个网格：0 失败、0 bounds mismatch**（每个网格建完都拿源数据的 `m_LocalAABB` 对 `RecalculateBounds()` 的结果，误差阈值 1%），这等于对上面那套通道判断做了 256 次独立复核。过程中修掉一个真实缺陷：Unity 的 `Mesh.SetTriangles(tri, s)` **不会自动增长 `subMeshCount`**，导致 2 个多 submesh 网格（`level6`/`level13` 的 `Combined Mesh (root: scene)`，静态合批产物，各 3 个 submesh）只建出第一个四边形 —— 而当时汇总仍报 `failed 0`，因为这类失败 Unity 只走 `Debug.LogError` 不抛异常。现在补了后置校验（逐个 `GetIndexCount(s)` 对期望值），"built N"不会再包含半成品。
10. **`.github/workflows/` 三条 build 流水线尚未适配新布局**，且历史上用 `cp ... || true` 吞错、声称能出包 —— 现状是“未成立”，不要当入口用。
11. **`core.autocrlf=true` 曾会让一次全新 clone 破坏字节忠实性**（本机实测，不是推测）：仓库原先没有 `.gitattributes`，`git checkout-index` 把 `unity/ProjectSettings/TagManager.asset` 从 **721 字节变成 774 字节（53 处 CRLF）**、`EditorBuildSettings.asset` 从 2429 变成 2509。现已加 `.gitattributes` 把 Unity 序列化文件（`*.unity/*.asset/*.prefab/*.meta/*.mat/*.anim/*.controller/*.guiskin` 等）、图片音频 dll 以及 `dump/**`、`recon/**` 全部标成 `-text`；**故意不标 `*.cs`** —— 那 646 个 .cs 工作副本本来就是 CRLF，标了会为无意义的换行改动重写几百个 blob。复验：`TagManager.asset` / `Map.unity`(382235) / `boss01…mat`(1062) 检出后与磁盘**逐字节一致、CRLF=0**。
    **留给你一个待决问题**：加规则之后，`git add --renormalize dump` 显示 **343 个 dump 文件在索引里是 LF、工作副本是 CRLF**（`+97/-97` 这种完全对称的行数就是换行翻转的特征），也就是说**提交里那批"原始 APK 证据"文本文件存的是被 git 归一化过的字节**。要不要按工作副本原样重写回去，我**没有擅自决定**：源 APK 已不在本机（`D:/wolf/self-game` 下只有另一个项目的 apk），无法裁定工作副本到底是 apktool 的原始输出、还是早先 git 按 CRLF 物化出来的副本 —— 前者该提交（恢复忠实），后者提交了也只是把 CRLF 固化成"原始"。这批改动至今**保持未暂存、未回退**，等你判断。

## 工作流速查

重新反编译（**输出到临时目录做 diff，不要就地覆盖 `unity/Assets/Scripts`**，那里是含手工修改的工作副本）：

```powershell
# ilspycmd 9.1.0.7988（net8 版）；11.x 需要 .NET 10 运行时
dotnet tool install -g ilspycmd --version 9.1.0.7988
# -lv CSharp9_0 必须带：Unity 2022.3 用 C# 9 编译工程脚本，
# 而 ilspycmd 默认 Latest 会吐出 `namespace X;` 文件级命名空间，Unity 直接报 CS8773。
ilspycmd -p -lv CSharp9_0 -o "$env:TEMP\redecomp" -r dump\assets\Managed dump\assets\Managed\Assembly-CSharp.dll
ilspycmd -p -lv CSharp9_0 -o "$env:TEMP\redecomp" -r dump\assets\Managed dump\assets\Managed\Assembly-CSharp-firstpass.dll
```

三个坑，踩过一次就记住：

- ILSpy 在 Windows 上会把只有大小写不同的 `Scenario.cs` / `scenario.cs` **合并进同一个文件**（实测产物是 297 行、同时含 `struct scenario` 与 `class Scenario`）。合并后文件名是小写的 `scenario.cs`，但 Unity 要求 MonoBehaviour 的文件名等于类名，所以拷进工程后必须把它改名成 `Scenario.cs`。
- 探测工程**必须 `-t:Rebuild`**。不加的话 MSBuild 会跳过 CoreCompile，只回显上一次的少量错误 —— 本仓库就曾因此把 1430 处错误误报成 8 处。
- 探测工程的 `TargetFramework` **必须等于工程的 API 档位**（现在 `apiCompatibilityLevel: 6` ⇒ `netstandard2.1`，且 framework 引用直接指向编辑器自带的 `NetStandard/ref/2.1.0/netstandard.dll`，不是 SDK 的参考集）。之前它是 `net8.0`，而 net8 的 BCL 与 .NET Standard 2.1 的程序集身份不是同一套，于是**两个方向都会说谎**：往漏报方向，SimpleJSON 依赖的 .NET Framework 2.0 版 SharpZipLib 在 net8 下编得过、Unity 里实际炸 `CS0012/CS1503/CS1674`；往误报方向，net8 会报 15 条 Unity 根本不报的 `CS8981`。csproj 里加了个 `CheckApiProfile` 目标，档位对不上就直接报错停下，不给"看着很干净"的假结论。

不带编辑器验证脚本能否编译：

```powershell
dotnet build tools\migration-probe\Probe.csproj -c Release -t:Rebuild
```

探测工程已锁 `<LangVersion>9.0</LangVersion>`，与 Unity 的语言级一致，别改回 `latest`。它还会引用 `unity/Library/ScriptAssemblies/UnityEngine.UI.dll`（`FPS.cs` 用 `UI.Text`），带 `Exists` 守卫，所以没打开过编辑器时会跳过而不是硬失败 —— 但那样 `FPS.cs` 就会误报 CS0246，**先用编辑器打开一次工程再跑探测**。同理 `unity/Assets/Plugins/ICSharpCode.SharpZipLib.dll` 是 `SimpleJSON` 的硬依赖，缺了必报 CS0246；要重新取：

```powershell
# NuGet 包 id 是 SharpZipLib（不是 ICSharpCode.SharpZipLib，那个只有 0.86 的 .NET FW 老珠子）
Invoke-WebRequest https://api.nuget.org/v3-flatcontainer/sharpziplib/1.4.2/sharpziplib.1.4.2.nupkg -o szl.zip
Expand-Archive szl.zip -DestinationPath szl
Copy-Item szl\lib\netstandard2.1\ICSharpCode.SharpZipLib.dll unity\Assets\Plugins\
```

重跑 Unity 4 → 2022 的机械迁移（幂等：已经是 `GetComponent<T>()` 的不会再被改）：

```powershell
python tools\migration\audit_shims.py                 # 先证明安全性：有无同名成员/字符串里的误伤点
python tools\migration\migrate_legacy_accessors.py unity\Assets\Scripts --dry-run
python tools\migration\migrate_legacy_accessors.py unity\Assets\Scripts
```

从 `mainData` 还原 ProjectSettings（幂等：内容一致时打印 `unchanged`，覆盖前自动备份到 `%TEMP%\projectsettings-backup-<时间戳>`）：

```powershell
# 只读：先把 mainData 里的管理器对象打出来看
python tools\migration\dump_maindata_settings.py
python tools\migration\dump_maindata_physics.py
# 只读：证明 Tag/Layer 到底有没有人用（结论见上面"Tag/Layer"那两行）
python tools\migration\scan_tags_layers.py
# 落地：写 TagManager.asset + EditorBuildSettings.asset + 两张表
python tools\migration\restore_projectsettings.py --dry-run
python tools\migration\restore_projectsettings.py
```

需要 UnityPy 1.25.3 的虚拟环境。系统 Python（`D:\Program\Python\Python312`）和 WSL 里都没有装，隔离环境建在**仓库外但不进 temp**：**`D:\wolf\self-game\unitypy-venv`**（`D:\wolf\self-game\unitypy-venv\Scripts\python.exe`，实测 `UnityPy 1.25.3`，依赖含 `brotli/lz4/pillow/texture2ddecoder/fmod_toolkit` 等）。之所以强调位置：它原先就在 `%TEMP%\unitypy-venv`，2026-10-10 被 Windows 清成半个空壳 —— `Lib\site-packages` 里所有 `.py` 都没了，只剩目录和 `.pyd`，于是 `import UnityPy` 依然"成功"（变成命名空间包：`__file__ is None`、`dir()` 空、没有 `load`）而不是报 ImportError，结果 449 个容器全部加载失败、脚本照旧打印出一行看着像结论的 `matched 0 / 61`。凡碰 `mainData`/`sharedassets` 的脚本都用它跑，只读 `tools/migration/scenes/*.json` 的那批用系统 python 即可。

`scene-guids.tsv` 里的 guid 是 `md5(path)` 生成的，**但这只是张台账，不是要写进 `.meta` 的规格** —— `.unity.meta` 的 guid 由 Unity 导入时自己分配（实测 `General.unity.meta = aaa0b646…`，而 `EditorBuildSettings.asset` 里当初写的是 `ba29674c…`），拿 md5 去覆盖等于和导入管线抢所有权。
   **在编辑器里改这条路是走不通的，已经证伪**：`EditorBuildSettings.scenes = 新数组` 加 `AssetDatabase.SaveAssets()` 之后，`ProjectSettings/EditorBuildSettings.asset` **字节没变、mtime 没动、24 条 guid 一条都没换成真值** —— `SaveAssets()` 只覆盖 `Assets/` 下的资产，不写 `ProjectSettings`。（这条是靠给同步逻辑加后置断言才发现的：日志明说 `0 guid(s) verified, 24 not found in it`。没有那句断言，这就是又一个"点了没报错但什么都没发生"。）那个菜单已从 `SceneRebuilder.cs` 删掉，不留假入口。
   **能落地的路是离线的**：`restore_projectsettings.py` 的 `scene_guid()` 改成**先读该 `.unity.meta` 里 Unity 真分配的 guid**、没有才退回 `md5(path)`，再重新生成 `EditorBuildSettings.asset`。实测 `python tools\migration\restore_projectsettings.py` 后 **24/24 与 `.meta` 一致、0 stale、0 缺文件**，复跑报 `unchanged`（幂等）。这个断言已固化进 `verify_scenes.py` 的 `build_settings()`，并用 md5 时代的旧值反向标定过（会被判 stale）。

#9 摸底与脚本绑定（两者都只读，输出可重复）：

```powershell
python tools\migration\survey_scene_data.py           # 组件/类型/纹理格式直方图 -> %TEMP%\survey9.json
python tools\migration\build_monoscript_table.py      # 394 个 MonoBehaviour 的脚本绑定表
```

`build_monoscript_table.py` 是**按定长偏移读原始字节**的，不要用 UnityPy 的 `obj.read()` 去读场景里的 `MonoBehaviour` —— 它会静默失败三分之二（原因见上面"Unity 4 构建数据没有 TypeTree"那行）。后续任何要还原组件字段值的工具，都必须沿用同一套偏移约定，字段数据从 24 开始。

场景重建（**故意不手写 .unity YAML**，改成"Python 出描述 + 编辑器落盘"，让 Unity 自己序列化，格式就不会错）：

```powershell
python tools\migration\extract_scene.py level7        # -> tools/migration/scenes/level7.json
# 编辑器菜单：Undead Slayer > Rebuild Scene From Level JSON...   （单个，弹文件选择框）
#            Undead Slayer > Rebuild ALL Extracted Levels        （批量）
```

23 个 level 已全部抽好，合计 **1207 个节点 / 394 个 MonoBehaviour**（与上面两张独立统计对得上），只有 4 条告警（`m_Tag` 1 和 2 我没敢猜名字，见 `extract_scene.py` 的 `BUILTIN_TAGS`）。重建时会丢 39 套旧粒子组件 + 22 `GUILayer` + 16 `FlareLayer`（后两个是 Unity 4 给相机自动挂的伴生组件，2022 已删，丢了不影响）。

顺带一个自己踩到的坑，它会误导验收：`SceneRebuilder` 的汇总日志走 `StrFmt.Fmt`，而它内部包的是 `string.Format`；可 7 个调用点写的是 printf 风格的 `%s`/`%d`，`string.Format` 根本不认，于是整条汇总**原样打出占位符**（`GameObjects : %d`），看着像重建没跑。这不是数据错，是格式化函数和占位符风格不匹配 —— `Fmt` 已改成按顺序替换 `%s`/`%d`（`{0}` 这类原样保留，参数不够时剩余占位符也保留成字面量）。识别信号：**日志里的数字没填上，先怀疑打印，别怀疑产物**。

**v1 只还原结构，不还原状态**：层级 / 名字 / layer / tag / active / 局部 TRS / 组件种类 / 脚本绑定都在，但 ① MonoBehaviour 的**字段值全是默认**（要按类从 Assembly-CSharp 解偏移 24 起的字节，还没做）；② 网格和材质引用**已经能对上**（`#14` 的纹理/材质/网格三步做完后实测：`level7` 重建出的 `General.unity` 里 8 个 `MeshFilter.m_Mesh` + 8 个 `m_Materials` 全部解析到 `Assets/Imported/` 下的真资产，场景内除引擎内置的 `e000…`/`f000…` 之外 0 个未解析引用），只剩 AudioClip 没导；③ 场景级设置（RenderSettings 雾效、LightmapSettings 等）用的是 Unity 新建场景的默认值，没从 `levelN` 里搬。所以"重建出来的场景能打开、层级对、脚本不丢"是这一步的验收标准，**不等于能玩**。

**想看画面就选 `Map`（dump `level6`），别选 `General`** —— 这是逐个 JSON 静态数出来的（"可见"= 同节点有 MeshFilter+MeshRenderer、两侧资产都在盘上、自身与所有祖先都 active）：

| dump 文件 | 场景 | 节点 | 可见渲染器 | 源头就是 inactive | 资产没导齐 | 挂脚本 |
|---|---|---|---|---|---|---|
| `level6` | Map | 116 | **103** | 0 | 0 | 112 |
| `level13` | Extreme | 80 | **69** | 0 | 0 | 72 |
| `level17` | Stage | 237 | 17 | 23 | 0 | 73 |
| `level1` | Story | 219 | 16 | 8 | 1 | 42 |
| `level7` | General | 15 | **1** | 7 | 0 | 10 |

决定性的一点是着色器：`Map` 那 103 个材质的原着色器**全是 `Unlit-Alpha`**（→ `Unlit/Transparent`），不吃光；`Stage` 的 29 个 `Mobile-VertexLit-OnlyDirectionalLights`（→ `Legacy Shaders/VertexLit`）则要看有没有光。
   **这里我犯过一次全称错误，记下来当反面教材**：先前写的"23 个场景一个 `Light` 组件都没有"是拿 `level6`/`level17`/`level7` 三个样本外推的，而这三个恰好都没有。穷举重测的结果是 **11 个 level 带 `Light`**（`level0/1/2/3/4/15/16/18/19/20/21`，每个 1 颗，都挂在名为 `Directional light` 的 GameObject 上）。`Map`(level6) 与 `Stage`(level17) 确实没有，所以"Stage 那批 VertexLit 在编辑器里偏暗"仍然成立，但**"全工程无光"这句不成立**。教训同上一条：否定结论必须穷举，三个样本不是证据。`General` 只有 1 个渲染器是激活的，其余 7 个在原始数据里就是 `m_IsActive: 0`（卡片结算类 UI，靠脚本运行期点亮）—— 之前那张"只有地面和天空"的截图是**忠实还原**，不是重建出错。`Map` 上那 97 个 `Icon_Stage` 也没有 `Awake`/`Start`，只提供一个 `IconDown()`，不会在启动时把渲染器关掉。

`Map` 已实测重建出来并对过文件（`unity/Assets/Scene/Map.unity`，382 KB）：**116 GameObject / 116 Transform / 103 MeshFilter + 103 MeshRenderer / 97 SphereCollider / 2 Camera / 111 MonoBehaviour**，`m_Mesh` 103 条、材质槽 103 条，**除引擎内置 `e000…`/`f000…` 外 0 个未解析引用** —— 与上面表格对 `level6.json` 的静态预测逐项吻合。
   少的那 1 个脚本不是重建漏掉了：`PayMgr` 上挂的 MonoBehaviour 绑的是 `ChannelMgr`，而 **`ChannelMgr` 根本没有基类**（不是 MonoBehaviour）。这是**拿 APK 里原始的 `dump/assets/Managed/Assembly-CSharp.dll` 直接反编译对证的**（`ilspycmd -t ChannelMgr`，输出 `public class ChannelMgr` 无 `: ...`），所以不是 ILSpy 把基类弄丢了；旁证还有两条：全语料**没有任何一处把 `ChannelMgr` 当组件用**（`GetComponent<ChannelMgr>`/`AddComponent<ChannelMgr>` 零命中），文件里也**没用任何 MonoBehaviour 专属 API**（`StartCoroutine`/`Invoke`/`transform`/`gameObject` 零命中），单例是 `new ChannelMgr()`。`SceneRebuilder.FindType` 要求 `typeof(MonoBehaviour).IsAssignableFrom(t)`，于是它被跳过并打 **`Debug.LogError`（不是 warning）**。**只报现象，不猜原意**。穷举核对过全部 23 个场景：**394 个绑定里只有这 1 个绑到非 MonoBehaviour 类，且没有任何被绑类名在 `unity/Assets/Scripts` 里找不到**。
   顺手修了这条诊断本身的措辞问题：原来两种完全不同的失败都写成 `not found`，会把人指向"文件丢了"。现在 `FindAnyType`（不带 MonoBehaviour 过滤）先判断类型到底存不存在，存在但不是 MonoBehaviour 就明说 `EXISTS but is not a MonoBehaviour`。另外 `StrFmt.Fmt` 加了自检：**参数一个都没被消费、或占位符没吃完，就在末尾打 `[Fmt: 0/3 arg(s) used]`**。起因是我自己写同步日志时踩了 `"a %s " + "b".Fmt(x)` —— 成员调用优先级高于 `+`，`.Fmt` 只作用于最后一段字面量，前半的 `%s` 原样漏出（和上一条 `%s` 事故同源）。已用只读 harness 跑过：正常替换/多行模板/无占位符无参数这三种都不加噪音，只有畸形调用会标出来。

验收用 `python tools\migration\verify_scenes.py`（只读，退出码 0 才算过）：它读**Unity 真正写出来的 .unity**，按 `--- !u!<classID>` 数对象，和 `levelN.json` 的声明逐项对（GameObject / MeshFilter / 渲染器 / MonoBehaviour / 未解析 guid / 是否残留 2022 已删除的旧组件），并把空 PPtr 槽**按所属字段拆开数**（`noMesh`、`noMat`、`nullRef`）。**当前 23/23 全 PASS，exit 0。**
   这个后置断言是必需的：第一版就抓到 **`Loading.unity` 是过期的** —— 它 11:19 重建，那时网格/材质还没导出，**20 个 `m_Mesh` 和 20 个材质槽全是 `{fileID: 0}`**，而"0 个未解析引用"完全看不出来（空槽不带 guid，天然通过）。重跑 Rebuild ALL 后已归零。所以顺序很重要：**先 Build Meshes / Build Materials，再 Rebuild ALL**。
   但**"数空槽"同样是错的判据**，我在这上面连错两轮、把两个本来正常的场景报成 FAIL：① 一个 `MeshFilter` 若 `extract_scene.py` 当初解不出网格，JSON 里**根本没有 `mesh` 这个键**（不是 `mesh: null`），到文件里却会序列化成 `m_Mesh: {fileID: 0}` —— 于是"期望 4、实际 4 条 m_Mesh、却有 1 条空"这种数法自己就不自洽；② **`ParticleSystemRenderer`（class 199）自己也带 `m_Mesh`**，和网格导出毫无关系。正解是**包含关系**而不是计数：`每个"现在磁盘上存在"的期望资产路径，都必须在对应字段里以非空 guid 出现`。并用注入一个假路径的方式标定过它会报错（missing 0→1），不给"永远绿"的假结论。
   剩下的空槽都能归因，不是 bug：`Story` 2 个 + `Stage` 1 个空材质槽 = JSON 里就是 null（`export_materials.py` 主动推迟的 `Font` 那档）；`Story_2` 1 个空 `m_Mesh` = 当初解不出该网格；`Stage` 3 个空 `m_Mesh`+3 个空材质槽 = 3 个 `ParticleSystemRenderer`（旧粒子还没迁成 Shuriken，本来就是空壳）。`nullRef` 那列则是"MonoBehaviour 字段没解"的直接体量（`Stage` 204、`Story` 137、`Forge` 66、`Archive` 63、`Map` 23…，字段名如 `icon_mission`/`general_weapon`/`card_general`），要等按类解偏移 24 起的字节才降得下来。

四个编辑器脚本（`SceneRebuilder.cs` / `MaterialRebuilder.cs` / `MeshRebuilder.cs` / `ShaderProbe.cs` / `ReadySceneBuilder.cs`）在探测工程里是**逐个 `<Compile Include>` 列出来的**，跟 `Assets\Scripts\**\*.cs` 一起编 —— 不是"排除在外"。它们能用纯 `*.dll` 通配拿到 UnityEditor，是因为 `Editor\Data\Managed\UnityEngine\` 下就有 `UnityEditor.CoreModule.dll`（`EditorSceneManager`、`MenuItem`、`EditorUtility` 都在里面）。这么编验过 0 error —— 第一次就是这么查出一个 `SetParent(GameObject)` 应该是 `SetParent(Transform)` 的错。改完 `Assets/Editor/*.cs` 记得重跑一次探测工程，别只靠编辑器热重编译。

材质同一条路子（Python 出描述 + 编辑器落盘）：

```powershell
python tools\migration\export_materials.py     # -> tools/migration/materials.json
# 编辑器菜单：Undead Slayer > Build Materials From JSON
```

改名表是数据文件 `tools/migration/shader-remap.json`，导出器会校验覆盖度：**没映射到的着色器一律跳过并报名字，绝不瞎猜**。当前实测 355 个材质 → **349 个可建、6 个主动推迟**（`Font` 那档，理由见 `shader-remap.json` 的 `deferred`），**357 个纹理引用全部命中、0 缺失**（349+6=355 对得上），编辑器侧 `Shader.Find` 失败 0。

一个值得注意的发现：材质的 `_MainTex` 普遍带 **非 1 的 `scale` 和不为 0 的 `offset`**（例：scale `[0.5,1]` + offset `[0.51,…]`），说明这游戏**把图打包成了图集**、靠纹理 ST 取子区域。所以 ST 必须一起迁，只导单图不够。

`.mat` 会**持久化存 `m_Name`**，而 Unity 要求它等于文件名，否则每个材质报一条 `Main Object Name 'X' does not match filename 'X_...'`。所以 `MaterialRebuilder` 必须把 `mat.name` 主动写成文件名词干（文件名带来源后缀 `<源文件>_assets_<path_id>`，比裸原名更有用，重名也靠它区分）。**只"不再赋值"修不好** —— 错名字已经写进文件了，得覆写。这个坑的识别信号是"警告条数 == 材质条数（349）"：量级和产物一样的警告要先当 bug 查，别当噪音。

`tools/migration/unity4_files.py` 是共用入口，把"绝不加载 `.splitN` 分片"和"PPtr 的 m_FileID 要靠各文件自己的 externals 表解析"两条规则收在一处 —— 前者曾让纹理数从 915 虚报成 7466。新工具都从它取文件清单。**它的 `sources()` 返回的是 `(basename, abspath)` 元组列表**，直接 `UnityPy.load(pair)` 会得到 `TypeError: Unsupported type for EndianBinaryReader: <class 'tuple'>`，而 `except Exception: continue` 会把这个变成"472 个文件全读不了、0 个 AnimationClip"的**假阴性**（我自己踩了才报出这个数，逐文件计数后才发现是解包漏了）。教训：探针要把"加载失败"和"没有该类型"分开计数，否则自己的用法错误会被读成数据的缺失。

**`Resources/` 是第三类缺口，和"字段没解"不是一回事。** 2022 工程里 `unity/Assets/Resources/` 原先**只有 1 个文件**（`BillingMode.json`，Unity 自己生成的），而脚本里有 `Resources.Load` 调用。**已修掉第一层**：`python tools\migration\export_textassets.py` 把包里 **6 个 `TextAsset`（5 个不同名字）** 原样落到 `Assets/Resources/<裸名>.bytes`（名字就是 `Resources.Load` 的键，所以**不能**像 `Imported/` 那样加来源后缀）。`UI_intro.cs:191` 要的 `hsp_version` 就是其中之一（内容 `1.0.0.SG`），192 行那句 NRE 到此解决。
   取字节的方式是**按 Unity 4 的 `TextAsset` 布局从原始字节解**（`{ string m_Name; byte[] m_Script; }`，字段之间 **4 字节对齐** —— `hsp_version` 名字 11 字节，所以长度前缀在偏移 16 而不是 15），不直接用 UnityPy 解码出的 `m_Script` 字符串：`blockName` 那个字符串是 39902 个字符而真实负载是 91547 字节，拿它再编码等于赌一次编解码往返（脚本里仍把"往返无损"当第二道断言）。两份同尺寸的分片副本已确认**逐字节相同**（md5 `da90828ff1`）所以去重安全；同名不同内容时脚本**拒绝挑选**而不是随便选一个。已知残留：`purchaselog` 负载是 **0 字节**，Unity 导入器可能跳过空文件，但全语料没有任何字面量加载它，暂不影响。
   **覆盖面重新量过（先前那个"16 个键找不到"是我正则算错的，已撤回）**：区分"完整字面量"与"前缀拼接"之后是 —— 完整字面量 **44 种 / 67 处，44 个全部能在包里按名字命中，0 缺失**；另有 **19 种前缀族**（`wp_`、`sk_`、`prt_general`、`treasure`、`guide`、`bgm_stage`…），展开后共 **271 个对象名**，其中 `wp_` 43 个、`wp_general` 9 个、`Stage_dun`/`bullet_angel` 各 8 个是 GameObject 根，其余多是 Texture2D/AudioClip。加上 44 个字面量里的 **33 个 GameObject**，**#17 剩下的真实工作量 ≈ 315 个名字、其中约 100 个是 prefab 根**，不是"数据缺失"，是还没做 prefab 导出。

**`nullRef` 那列先前是低估的下界，已修正**：`verify_scenes.py` 原来只数数组形态的 `- {fileID: 0}`，而 `Cam_intro` 这种 `public Transform cha/gate/mon1/mon2` 序列化成**标量行** `cha: {fileID: 0}`，于是一个都没算进去（`Intro` 报 2，实际 36，`cha/gate/UI_intro/mon1/mon2` 全空）。现在按 `!u!114` 文档逐个走、标量与数组两种形态都数、并跳过 Unity 自己写的 PPtr 字段（`m_GameObject`/`m_PrefabInstance`/…）。修正后的真值：**全工程 23 个场景共 1319 个空引用字段**（`Stage` 339、`Story` 259、`Forge` 107、`Map` 82、`Archive`/`Result` 各 84、`Extreme` 74、`Intro` 36）。**偏移 24 起按类解字段这条路已经打通（首例）**：`level0` 里 `Cam_intro` 那个 MonoBehaviour `raw_len=64`，去掉 24 字节头正好 **40 字节 = 5 个 8 字节 PPtr**，与它 5 个 `public Transform` 字段的声明序（`UI_intro/gate/cha/mon1/mon2`）一一对上，解出来是 —— `UI_intro→GameObject 'UI_intro'`、`gate→'bg_castle'`、`cha→'cha'`、`mon1→'mon1'`、`mon2→'mon2'`。**5 个里 4 个对象名与字段名完全一致**，剩下 `gate` 指向城堡图（语义上也讲得通）。所以那 1319 个空引用**是可恢复的**，不是必须放弃的东西。
   两个实现要点：① `m_Script` 的 **`FileID` 是 externals 列表的 1-based 下标**（`externals[FileID-1]`），`level0` 的 externals 是 `['sharedassets1.assets','sharedassets0.assets']`，`Cam_intro` 的脚本就在 `sharedassets0.assets!84` —— 直接按 0-based 取或用 UnityPy 对象的属性名去猜都会错（我这么翻车过一次）；② 这套解析 `monoscript-bindings.json` 里已经有（`level_externals`/`monoscripts`/`script_to_guid`），解码器要复用它，别另起一套。注意 Unity 只序列化 `public`/`[SerializeField]` 字段，`private` 的 `dx`/`startui` 不占字节 —— 上面的 40 字节正好印证了这点。

**第一刀（只做全定长类）已经跑通并落到场景文件里**：`tools/migration/monobehaviour_fields.py` 负责按类走字段布局，`extract_scene.py` 把解出的值写进 `levelN.json` 的 `components[].fields`，`SceneRebuilder.cs` 用 `SerializedObject` 应用。实测 **23 个场景共 64 个对象引用落地**（`Intro` 的 nullRef 36→31，正是 `Cam_intro` 那 5 个；`Stage` 339→318；`Map` 保持 82 是对的，它那些 `Icon_Stage` 字段是 int 不是引用），值类型字段也一并生效（`Map.unity` 里 `Icon_Stage._index` 出现 0..5）。
   **一次真实的假绿**：第一版应用引用时拿 `SerializedProperty.type` 去和字符串 `"Transform"`/`"GameObject"` 比，不匹配就静默走 `GetComponent(prop.type)`；结果 `int/float/bool` 全应用成功、5 个引用全是 `{fileID: 0}`，而汇总却报了 "refs applied"，因为那个计数是**照着赋值动作算的**。改成按字段的**声明 CLR 类型**（`FieldInfo.FieldType`，并手工沿继承链找 —— 反射默认不向上找实例字段）决定塞 `target.transform` 还是 `target`，并且 **apply 完 `so.Update()` 再读回来数**。现在汇总行是 `N refs assigned + M values applied, K refused, L landed in the file, D did not survive`，`landed` 只认读回来的结果。教训：赋值动作不等于值进了文件，序列化类操作的后置断言必须读最终产物。
   **`Rebuild ALL` 不是字节幂等的**：每跑一次，Unity 会给场景里每个对象重新随机 fileID，所以即使内容完全一样，23 个 `.unity` 也会出现整文件 diff（实测一次重跑后 `Map.unity` 3360 行全变）。核对两次产物是否等价，别看 diff 行数，要看**类直方图 + 对象数 + 空引用数 + 已接引用数**这四项（已验证重跑前后 `Map` 655 对象 / 2762 空引用、`Intro` 43 对象 / 5 个已接引用完全不变）。所以提交节奏上最好是"跑完就提"，别攒着。
   两个仍未解的：① **数组/string 字段（45 个实例）是第二刀**，需要一个真正按类型走的 walker（数组 = `int32 元素数 + 元素`，string = `int32 长度 + 字节`），`Map` 的 `icon_mission`/`general_weapon` 这类大 nullRef 全在这里；② 有一处**我没对上账**：`extract_scene` 报 `dangling ref dropped: 63`，而用相同 `obj2node` 构造写的独立复核脚本报 0，两者必有一错、尚未定位（可疑点是两条路径解析脚本名的方式不同：`script_name()` vs `monoscripts["file!pid"]`，覆盖的实例集合可能不一样）。所以 "63" 这个数**不作为结论使用**。另外 `General_Stat` 有 4 个实例字段区只有 4 字节、而按 DLL 声明该有 20 字节，严格规则把它们拒了（占 394 的 1%）—— 只记现象，MonoScript 记录本身已核对无误（`sharedassets0.assets!78`，类名/程序名都对）。

**动画是可导的（和 MonoBehaviour 不一样）**：修正解包后实测 **472/472 个序列化文件加载成功、0 失败**，其中 **427 个 `AnimationClip` 全部 `read()` 成功**，曲线内容自洽 —— 骨骼路径是真的（`path='Bip001'`/`Bip01`…）、每 clip 约 30 条位置/旋转/缩放曲线（对应一具骨骼）、关键帧时间落在正常区间、**没有一个 clip 是"所有曲线都为空"**，`m_Name` 就是动画状态名（`costume`/`open`/`fail`/`fail_loop`…，`CostumeAniSpeed.cs:7` 要的 `["costume"]` 确实在数据里）。所以 `AnimationClip` **不需要**像 MonoBehaviour 那样按定长偏移手解，走"Python 出描述 + 编辑器落盘"这条正路就行。两个还没证的前置：① **Legacy 标志**，UnityPy 读出的 `m_Legacy` 是 `None`，而 2022 的 `Animation` 组件只认 legacy clip（`AnimationClip.legacy` 没有公开 setter，得走 `SerializedObject` 的 `m_Legacy`）；② 关键帧 in/outSlope 在 Unity 4 压缩布局下是否逐字节准确（`m_CompressedRotationCurves` 全为 0、旋转走的是 `m_RotationCurves` 的显式 Quaternion，看着是未压缩那条路径，但没做往返验证）。

用 Unity 打开：`unity/` 目录（编辑器版本 2022.3.62f3c1）。**探测工程实测 0 error / 532 warning**（按 Unity 真实档位 netstandard2.1 编），即脚本按 C# 9 + 2022.3 自带的程序集是编得过的。
   **已经成立的部分（2026-09-29 实测，不再是推断）**：从 `Ready` 进 playmode 后 `Application.LoadLevel("Intro")` **成功跳转**，`Intro` 场景**在 Game 视图里渲染出了主角立绘与背景**（网格 + 材质 + 纹理全链路出图）。证据有两条且互相独立：① Hierarchy 里出现 `DontDestroyOnLoad` 分组，里面就是 `Ready` 的 `AndroidScript`（它 `Start()` 调了 `DontDestroyOnLoad`）；② 注册表 `HKCU\Software\Unity\UnityEditor\NHN Corp\Undead Slayer Xiahou Dun` 多了 `isFirst="YES"`，而全工程唯一写这个键的是 `AndroidScript.Awake()`。
   **还不成立的部分**：数组/string 那 45 个实例的字段还没解（第二刀，`Map` 的 `icon_mission`/`general_weapon` 在这里）；`Animation` 组件里没有 clip（427 个 clip 已确认可读，还没导）；`Resources/` 的 prefab 整类没导（`wp_*` 武器、`bg_worldmap` 等）；旧粒子仍是空壳。也就是说**"能看到画面"和"启动链能跳"已经成立，"能玩"还没有**。

## 免责声明

本项目仅用于学习和研究目的。所有原始代码和资源的版权归原始开发者 NHN Corp 所有，请勿用于商业用途。
