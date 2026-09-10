using UnityEngine.UI;

public sealed class SettingsPage : NavigationPage
{
    private void Start()
    {
        VerticalLayoutGroup layout = DemoUi.BuildPageShell(this, "Settings");
        DemoUi.Label(layout.transform, "Ordinary PAGE presentation.");
        DemoUi.Button(layout.transform, "Back", delegate { Navigator.Pop(); });
    }
}
