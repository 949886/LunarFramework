using UnityEngine.UI;

public sealed class ConfiguredPage : NavigationPage
{
    public string Data { get; set; }

    private void Start()
    {
        VerticalLayoutGroup layout =
            DemoUi.BuildPageShell(this, "Configured Page");

        DemoUi.Label(
            layout.transform,
            Data ?? "(not configured)");

        DemoUi.Button(
            layout.transform,
            "Back",
            delegate { Navigator.Pop(); });
    }
}
