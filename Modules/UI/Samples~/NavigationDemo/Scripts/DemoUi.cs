using System;
using UnityEngine;
using UnityEngine.UI;

public static class DemoUi
{
    public static Font Font
    {
        get
        {
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            catch { }

            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch { }
            }

            return font;
        }
    }

    public static VerticalLayoutGroup BuildPageShell(
        NavigationPage page,
        string title)
    {
        RectTransform root = page.transform as RectTransform;
        Stretch(root);

        GameObject panel = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup));

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(page.transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 460f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.08f, 0.09f, 0.12f, 0.97f);

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 26, 26);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Label(panel.transform, title, 24, FontStyle.Bold);
        return layout;
    }

    public static Text Label(
        Transform parent,
        string text,
        int size = 16,
        FontStyle style = FontStyle.Normal)
    {
        GameObject go = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(Text),
            typeof(LayoutElement));

        go.transform.SetParent(parent, false);

        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = Font;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.minHeight = Mathf.Max(30f, size * 1.7f);
        return label;
    }

    public static Button Button(
        Transform parent,
        string text,
        Action clicked)
    {
        GameObject go = new GameObject(
            text + "Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement));

        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.20f, 0.23f, 0.29f, 1f);

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.minHeight = 42f;

        GameObject textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(Text));

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(go.transform, false);
        Stretch(rect);

        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.font = Font;
        label.fontSize = 15;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        Button button = go.GetComponent<Button>();
        if (clicked != null)
            button.onClick.AddListener(delegate { clicked(); });

        return button;
    }

    public static void Stretch(RectTransform rect)
    {
        if (rect == null)
            return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
