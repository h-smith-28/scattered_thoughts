# 交互提示说明

两个 Shader Graph 和对应材质都在 `Assets/Resources/InteractionFeedback`。双击 `.shadergraph` 就能看节点，选中 `.mat` 可以直接调效果。Player 的 `s_PlayerCamera` 下，`Interaction Feedback` 分组引用这两个材质。

`InteractableOutline`：把模型从中心向外扩大一点，只画它的背面。原模型挡住中间，就只露出一圈蓝边，不会把整个物体染蓝。材质的 `Sky Blue Color` 是高亮颜色；`Outline Width` 是粗细，默认 `0.006`，单位跟场景一样，先小幅调整。

`StickyNotePlacement`：用 UV 找出四边，边框和中间分别设置透明度，再输出颜色。`Preview Opacity` 默认 `0.13`，控制中间；`Border Opacity` 默认 `0.95`，控制边框；`Border Width` 默认 `0.035`，控制边框宽度。数值越大通常越明显。

微波炉外壳使用独立的 `Assets/Materials/M_MicrowaveBody.mat`，原颜色和纹理不变，`Surface Type` 保持 `Opaque`，避免高亮透出来盖蓝整个外壳。电脑的 `Computer/sm_CRT` 补了一个 `BoxCollider`，这样看着外壳也能检测到电脑，不用只对准屏幕。

运行 `sc_SampleScene`，用屏幕中央对准便签、电脑或可互动的小游戏物体，会出现天蓝色轮廓；移开视线就消失。物体沿用已有的 `s_Grabbable`、`s_ComputerStation` 或 `s_MinigameObject` 组件判断，不用逐个添加高亮。左键拿起便签，对准墙面会显示放置框，再点左键贴上；位置太靠边、放不下整张便签时不显示。按 E 使用电脑或小游戏，Esc 退出。

溶解没有删除。要恢复，在 `RoomTest/Desktop` 和 `Computer/Monitor` 上重新勾选 `DissolveLoop`。桌子材质的 `DissolveAmount` 已归零，表示完整显示。

