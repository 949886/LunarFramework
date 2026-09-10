using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Starts the Cherry sample. The checked-in demo scene intentionally contains
/// only this bootstrap component; Canvas/Navigator/EventSystem are created at
/// runtime so the scene does not serialize package-internal script GUIDs.
/// </summary>
public sealed class DemoBootstrap : MonoBehaviour
{
    private void Start()
    {
        Navigator navigator = GetComponent<Navigator>();

        if (navigator == null)
            navigator = FindObjectOfType<Navigator>();

        if (navigator == null)
            navigator = CreateNavigationUi();

        EnsureEventSystem();

        if (navigator.ScheduledRouteCount == 0)
            navigator.Push<HomePage>();
    }

    private static Navigator CreateNavigationUi()
    {
        GameObject canvasObject = new GameObject(
            "Canvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900f, 560f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject navigatorObject = new GameObject(
            "Navigator",
            typeof(RectTransform),
            typeof(Navigator));

        RectTransform navigatorRect =
            navigatorObject.GetComponent<RectTransform>();
        navigatorRect.SetParent(canvasObject.transform, false);
        DemoUi.Stretch(navigatorRect);

        return navigatorObject.GetComponent<Navigator>();
    }

    /// <summary>
    /// Creates an EventSystem using the input backend enabled by Player Settings.
    ///
    /// Unity defines ENABLE_INPUT_SYSTEM / ENABLE_LEGACY_INPUT_MANAGER according
    /// to Active Input Handling. When both are enabled we intentionally prefer
    /// InputSystemUIInputModule so uGUI never falls back to UnityEngine.Input.
    /// </summary>
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystemObject = new GameObject(
            "EventSystem",
            typeof(EventSystem));

#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
        eventSystemObject.AddComponent<StandaloneInputModule>();
#else
        Debug.LogWarning(
            "Cherry Navigation demo: no supported Unity input backend is " +
            "enabled. UI buttons will not receive pointer/submit input.");
#endif
    }
}
