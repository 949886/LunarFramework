using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Built-in lightweight dialog used by Navigator.ShowDialog.</summary>
/// <remarks>
/// DefaultDialog is a specialized Modal NavigationDialog, not an independent
/// dialog manager. Configure Title, Message, and actions before first activation;
/// closing and results use ordinary Navigator.Pop / NavigationRoute.Popped.
/// </remarks>
public sealed class DefaultDialog : NavigationDialog
{
    private sealed class DialogAction
    {
        public string Text;
        public Action Callback;
    }

    private readonly List<DialogAction> _actions = new List<DialogAction>();
    private bool _built;

    /// <summary>Dialog title configured before first activation.</summary>
    public string Title { get; set; } = "Dialog";
    /// <summary>Dialog body text configured before first activation.</summary>
    public string Message { get; set; } = "";

    /// <summary>Adds an action button with an optional synchronous callback.</summary>
    /// <param name="text">Button label.</param>
    /// <param name="callback">Optional click action; null creates an inert button.</param>
    /// <remarks>
    /// AddAction never closes the dialog implicitly. The callback decides whether
    /// to call Navigator.Pop(result), keep the dialog open, or do other work.
    /// </remarks>
    public void AddAction(string text, Action callback = null)
    {
        _actions.Add(new DialogAction
        {
            Text = text ?? "",
            Callback = callback,
        });
    }

    /// <summary>Adds a convenience action whose callback calls Navigator.Pop().</summary>
    /// <param name="text">Button label.</param>
    /// <remarks>The route completes with a null result.</remarks>
    public void AddCloseAction(string text)
    {
        AddAction(text, delegate { Navigator.Pop(); });
    }

    private void OnEnable()
    {
        if (!_built)
            BuildUi();
    }

    private void BuildUi()
    {
        _built = true;

        RectTransform root = transform as RectTransform;
        if (root != null)
            Stretch(root);

        if (_actions.Count == 0)
            AddCloseAction("Close");

        Font font = BuiltinFont();

        GameObject centerObject = new GameObject(
            "DialogCenter",
            typeof(RectTransform));
        RectTransform center = centerObject.GetComponent<RectTransform>();
        center.SetParent(transform, false);
        Stretch(center);

        GameObject panelObject = new GameObject(
            "Panel",
            typeof(RectTransform),
            typeof(Image),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(center, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(420f, 0f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.12f, 0.13f, 0.16f, 1f);

        VerticalLayoutGroup layout = panelObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 18, 18);
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Text title = CreateText(panel, "Title", Title, font, 22);
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;

        Text message = CreateText(panel, "Message", Message, font, 16);
        message.alignment = TextAnchor.MiddleCenter;

        GameObject actionsObject = new GameObject(
            "Actions",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup),
            typeof(ContentSizeFitter));
        RectTransform actions = actionsObject.GetComponent<RectTransform>();
        actions.SetParent(panel, false);

        HorizontalLayoutGroup actionLayout =
            actionsObject.GetComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 8f;
        actionLayout.childAlignment = TextAnchor.MiddleRight;
        actionLayout.childControlWidth = false;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = false;
        actionLayout.childForceExpandHeight = false;

        ContentSizeFitter actionFitter =
            actionsObject.GetComponent<ContentSizeFitter>();
        actionFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        actionFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < _actions.Count; i++)
        {
            DialogAction action = _actions[i];
            Button button = CreateButton(actions, action.Text, font);
            if (action.Callback != null)
            {
                Action callback = action.Callback;
                button.onClick.AddListener(delegate { callback(); });
            }
        }
    }

    private static Text CreateText(
        Transform parent,
        string name,
        string value,
        Font font,
        int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.text = value ?? "";
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        LayoutElement element = go.AddComponent<LayoutElement>();
        element.preferredHeight = size * 1.8f;
        return text;
    }

    private static Button CreateButton(Transform parent, string text, Font font)
    {
        GameObject go = new GameObject(
            text + "Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement));

        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = new Color(0.24f, 0.27f, 0.33f, 1f);

        LayoutElement element = go.GetComponent<LayoutElement>();
        element.minWidth = 92f;
        element.minHeight = 34f;

        GameObject labelObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(Text));
        RectTransform labelTransform = labelObject.GetComponent<RectTransform>();
        labelTransform.SetParent(go.transform, false);
        Stretch(labelTransform);

        Text label = labelObject.GetComponent<Text>();
        label.text = text ?? "";
        label.font = font;
        label.fontSize = 14;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;

        return go.GetComponent<Button>();
    }

    private static Font BuiltinFont()
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

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
