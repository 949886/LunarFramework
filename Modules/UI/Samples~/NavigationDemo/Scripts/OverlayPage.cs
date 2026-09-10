using UnityEngine;
using UnityEngine.UI;

public sealed class OverlayPage : NavigationPage
{
    private void Start()
    {
        RectTransform root = transform as RectTransform;
        DemoUi.Stretch(root);

        GameObject panel = new GameObject(
            "OverlayPanel",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup));

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(320f, 130f);

        panel.GetComponent<Image>().color =
            new Color(0.14f, 0.16f, 0.20f, 0.96f);

        VerticalLayoutGroup layout =
            panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.spacing = 8f;

        DemoUi.Label(panel.transform, "Overlay", 18, FontStyle.Bold);
        DemoUi.Button(
            panel.transform,
            "Close overlay",
            delegate { Navigator.Pop(); });
    }
}
