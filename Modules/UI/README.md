# Navigation 转场

Unity uGUI 的 Navigation 模块提供与 Cherry Godot addon 对应的内置转场：

| 资源 | Push / Pop | 默认值 |
| --- | --- | --- |
| `FadeNavigationTransition` | 淡入 / 淡出 | 0.18 秒，线性 |
| `SlideNavigationTransition` | 从指定边缘滑入 / 向同一边缘滑出 | Right，1 倍页面宽度，0.28 秒 |
| `SlideFadeNavigationTransition` | 短距离滑动并淡入 / 淡出 | Bottom，0.08 倍页面高度，0.22 秒 |
| `ScaleNavigationTransition` | 缩放进入 / 缩放退出，可同时淡入淡出 | 0.9 倍原始缩放，页面中心，0.24 秒 |

新增效果默认使用 Cubic Out 缓动。动画作用于进场或退场页面，底层页面保持原位；遮罩和页面覆盖策略仍由 Navigator 管理。

## 使用

1. 在 Project 窗口中选择 **Create > Cherry Navigation**，创建 Slide Transition、Slide Fade Transition 或 Scale Transition 资源。
2. 将资源赋给 Navigator 的 **Default Transition**，或者页面 Prefab 的 **Transition**。
3. 在资源 Inspector 中调整参数；同一个资源可供多个 Navigator 使用。

页面的 Transition 在配置回调前写入 Route；回调中需要为本次导航覆盖效果时，设置 `page.Route.Transition`。

所有内置效果继承 `TweenNavigationTransition`：

- `Duration`：秒数，零或负值立即完成且不修改页面。
- `Easing`：Unity AnimationCurve，横轴为归一化时间，纵轴为动画进度。空曲线引用使用线性进度，推荐曲线从 (0, 0) 到 (1, 1)。移动和缩放支持曲线超调。
- Slide / Slide Fade 的 `FromEdge` 支持四个方向；`DistanceRatio` 按页面 RectTransform 的宽度或高度计算。页面某一维为零时使用父 RectTransform 对应尺寸。
- Scale 的 `HiddenScale` 是相对原有 X/Y 缩放的倍率，Z 缩放保持原值；`PivotRatio` 遵循 Unity RectTransform 坐标，(0, 0) 为左下，(0.5, 0.5) 为中心；`Fade` 可关闭透明度动画。

Slide 和 Scale 要求页面根节点使用 RectTransform。Fade 仍支持普通 Transform。需要淡入淡出时会在页面根节点自动补充 CanvasGroup；转场不修改其 interactable、blocksRaycasts 或 ignoreParentGroups。

动画使用 `Time.unscaledDeltaTime`，在 `Time.timeScale = 0` 时仍能完成。结束时恢复原有位置、缩放、pivot 和 CanvasGroup alpha；页面或 CanvasGroup 在等待期间被销毁时，也会结束等待。改变缩放中心会补偿页面位置，支持非默认缩放、旋转与拉伸锚点。

尺寸在每次转场开始时计算。转场期间避免让其他动画或布局组件同时驱动页面根 RectTransform；页面内的布局和动画可放在子节点中。嵌套导航区域需要裁剪滑动内容时，可在导航区域上使用 RectMask2D。

## 代码示例

```csharp
var slide = ScriptableObject.CreateInstance<SlideNavigationTransition>();
slide.FromEdge = SlideNavigationTransition.Edge.Right;
slide.DistanceRatio = 1f;
slide.Duration = 0.3f;
navigator.DefaultTransition = slide;

var zoom = ScriptableObject.CreateInstance<ScaleNavigationTransition>();
zoom.HiddenScale = 0.85f;
zoom.PivotRatio = new Vector2(0.5f, 0.5f);
zoom.Fade = true;

// 例如在页面配置回调中，为当前 Route 指定转场。
page.Route.Transition = zoom;
```

运行时创建的 ScriptableObject 由创建者管理，在不再被页面或 Navigator 使用后调用 `Destroy`。Inspector 中引用的项目资源由 Unity 管理。

原有 Fade 资源的脚本 GUID、`_duration` 字段名称和默认线性效果保持兼容；Pop 完成后也会恢复原始 alpha。

## 验证

```text
python Modules/UI/Tests~/run_tests.py --unity <Unity.exe路径>
```

运行器从该模块创建独立临时 Unity 项目，使用指定 Editor 自带的 uGUI 包执行编译、资源序列化检查和 Play Mode 测试。测试目录位于 `Tests~`，不会被宿主项目导入。旧版或自定义安装没有内置 uGUI 时可通过 `--ugui <本地包目录>` 指定。

覆盖四方向进退场、缩放中心补偿、原有变换与透明度恢复、非正时长、游戏暂停、共享资源、对象销毁、旧 Fade 资产兼容，以及真实 Navigator 队列和返回操作。运行器输出临时项目路径，并保留 `Editor.log` 与 `results.json`。

2026-09-10 验证：Unity 2021.3.45f1 与 Unity 6000.6.0f1 均通过编译、编辑器资源检查及各 362 项 Play Mode 检查。
