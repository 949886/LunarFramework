using UnityEngine.UI;

public sealed class ModalPage : NavigationPage
{
    private void Start()
    {
        VerticalLayoutGroup layout =
            DemoUi.BuildPageShell(this, "Modal");

        DemoUi.Label(
            layout.transform,
            "The underlying route remains visually composed.");

        DemoUi.Button(
            layout.transform,
            "Close",
            delegate { Navigator.Pop("modal-closed"); });
    }
}
