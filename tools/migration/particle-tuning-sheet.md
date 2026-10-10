# 旧粒子特效 对表清单

这一份把 APK 里解出来的数字，换成只能在原版画面上确认的东西。

已经证到的：四类组件的记录长度在全 build 内完全一致（116 / 90 / 104 / 120 字节），
363 条记录在「每条字节必须用尽」的约束下全部解出；发射器里有 4 对相邻 float，
每条记录都满足 前<=后 且至少一条严格小于——这就是 (最小, 最大) 参数的样子。

还缺的：这四对分别管什么。离线渠道已经走尽——Unity 4 这批文件不带 TypeTree，
venv 里也没有 typetree 签名库；出厂 Assembly-CSharp.dll 的字符串堆里只出现 8 个粒子成员名
（ParticleEmitter / emit / inside / radius / force / rndVelocity / Particles / ClearParticles），
maxParticles、min|maxSize、min|maxEmitPower、colorKey、damping 一个都没有。
也就是说外观是当年在编辑器里设的，APK 里没有任何程序提到过它们的名字。

所以每条给了两样可对表的东西：这个特效用的是哪张贴图（靠已导出的材质反查），
以及四个只能靠眼睛回答的量。填得上就填，看不出就写「看不出来」，
我按未定名处理，不会把猜的值写进转换器。

## 1. pt_thrust（`level1!446`）

- 出处：`level1!446`（关卡里的对象名：pt_thrust）
- 材质：`Assets/Imported/Materials/pt_thrust_sharedassets2_assets_45.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_thrust_99.png`
- 打包标志：[8..11]=[1, 0, 0, 0]，[96..99]=[0, 0, 1, 0]
- 四对 min/max 候选：`12/16=0.15..0.15`、`24/28=0.5..1.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[255, 255, 73, 16, 0]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 2. bullet_explode1（`sharedassets18.assets!2864`）

- 出处：`sharedassets18.assets!2864`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 bullet_explode1）
- 材质：`Assets/Imported/Materials/pt_fire_sharedassets18_assets_109.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_ki_red_302.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 1, 1, 0]
- 四对 min/max 候选：`12/16=0.1..0.2`、`24/28=1.0..20.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 3. pt_lvup（`level7!36`）

- 出处：`level7!36`（关卡里的对象名：pt_lvup）
- 材质：`Assets/Imported/Materials/pt_thrust_sharedassets2_assets_45.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_thrust_99.png`
- 打包标志：[8..11]=[1, 0, 0, 0]，[96..99]=[1, 0, 1, 0]
- 四对 min/max 候选：`12/16=0.2..0.2`、`24/28=1.5..40.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 4. general_meteo（`9892c31ba4790444981a9c82b86460dc!10`）

- 出处：`9892c31ba4790444981a9c82b86460dc!10`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 general_meteo）
- 材质：`Assets/Imported/Materials/pt_fire_sharedassets18_assets_109.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_ki_red_302.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 0, 0, 0]
- 四对 min/max 候选：`12/16=0.1..0.15`、`24/28=0.7..50.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[255, 255, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 5. general_arrowrain（`cf08e3d0aa897104cbdd6e7cd696f2bf!4`）

- 出处：`cf08e3d0aa897104cbdd6e7cd696f2bf!4`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 general_arrowrain）
- 材质：`Assets/Imported/Materials/pt_arrowrain_sharedassets2_assets_40.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/ln_arrow_76.png`
- 打包标志：[8..11]=[1, 0, 0, 0]，[96..99]=[0, 1, 0, 0]
- 四对 min/max 候选：`12/16=0.05..0.05`、`24/28=0.4..200.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[255, 255, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 6. pt_thrust（`level17!488`）

- 出处：`level17!488`（关卡里的对象名：pt_thrust）
- 材质：`Assets/Imported/Materials/pt_thrust_sharedassets2_assets_45.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_thrust_99.png`
- 打包标志：[8..11]=[1, 0, 0, 0]，[96..99]=[0, 0, 1, 0]
- 四对 min/max 候选：`12/16=0.15..0.15`、`24/28=0.5..1.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[255, 255, 73, 16, 0]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 7. Boss05（`sharedassets18.assets!2853`）

- 出处：`sharedassets18.assets!2853`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 Boss05）
- 材质：`Assets/Imported/Materials/pt_thrust_sharedassets2_assets_45.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_thrust_99.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 1, 1, 0]
- 四对 min/max 候选：`12/16=0.5..0.5`、`24/28=0.2..1.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 8. Stage_dun3（`69f958559f37c4d4bbfbace1100f80d9!37`）

- 出处：`69f958559f37c4d4bbfbace1100f80d9!37`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 Stage_dun3）
- 材质：`Assets/Imported/Materials/pt_volcano_5b468a17455826e4ea75565ec9970270_1.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/bundles/pt_volcano_1.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 0, 0, 0]
- 四对 min/max 候选：`12/16=0.5..0.7`、`24/28=2.0..2.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[9, 150, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 9. Stage_dun7（`e20a04d6f18f1dc46a1531f21b46e5e2!30`）

- 出处：`e20a04d6f18f1dc46a1531f21b46e5e2!30`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 Stage_dun7）
- 材质：`Assets/Imported/Materials/pt_waterdrop_dark_f0f22dc82e3a7b243ab2721d92079037_1.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/bundles/pt_waterdragon_dark_1.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 0, 0, 0]
- 四对 min/max 候选：`12/16=0.5..0.8`、`24/28=2.0..2.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[9, 150, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 10. pt_envir_2（`level13!174`）

- 出处：`level13!174`（关卡里的对象名：pt_envir_2）
- 材质：`Assets/Imported/Materials/pt_leaf_g_sharedassets14_assets_4.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_leaf_g_43.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 0, 0, 0]
- 四对 min/max 候选：`12/16=0.02..0.08`、`24/28=2.5..3.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 11. wp_general5（`c377ababfc2b3a64b8ff90f905598948!4`）

- 出处：`c377ababfc2b3a64b8ff90f905598948!4`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 wp_general5）
- 材质：`Assets/Imported/Materials/pt_shining_b_sharedassets10_assets_19.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_shiningstar_b_107.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[1, 0, 0, 0]
- 四对 min/max 候选：`12/16=0.08..0.1`、`24/28=1.0..4.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |

## 12. tank（`sharedassets18.assets!2885`）

- 出处：`sharedassets18.assets!2885`（关卡里的对象名：不在 23 个关卡里，多半是 prefab 内部对象，属于 tank）
- 材质：`Assets/Imported/Materials/pt_skillgather_sharedassets18_assets_115.mat`
- 纹理（认画面用这个）：`Assets/Imported/Textures/sharedassets/pt_skillgather_279.png`
- 打包标志：[8..11]=[1, 1, 0, 0]，[96..99]=[0, 1, 1, 0]
- 四对 min/max 候选：`12/16=0.03..0.05`、`24/28=0.6..5.0`、`80/84=0.0..0.0`、`88/92=0.0..0.0`
- 透明度轨（5 帧 alpha）：[10, 180, 255, 180, 10]

| 观察项 | 你在原版里看到的 |
|---|---|
| 单个粒子寿命（秒） |  |
| 初速（米/秒，估） |  |
| 粒子大小（相对角色身高） |  |
| 同屏峰值粒子数 |  |
