using UnityEngine.UI;

public sealed class CustomDialog : NavigationDialog
{
    public string Message { get; set; }

    private void Start()
    {
        VerticalLayoutGroup layout =
            DemoUi.BuildPageShell(this, "Custom Dialog");

        DemoUi.Label(
            layout.transform,
            Message ?? "Custom NavigationDialog");

        DemoUi.Button(
            layout.transform,
            "OK",
            delegate { Navigator.Pop("custom-ok"); });
    }
}
