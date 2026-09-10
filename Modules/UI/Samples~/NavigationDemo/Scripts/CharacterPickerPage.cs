using UnityEngine.UI;

public sealed class CharacterPickerPage : NavigationPage
{
    private void Start()
    {
        VerticalLayoutGroup layout =
            DemoUi.BuildPageShell(this, "Character Picker");

        CharacterPickerParameters parameters =
            Parameters as CharacterPickerParameters;

        DemoUi.Label(
            layout.transform,
            parameters != null ? parameters.Prompt : "Choose");

        DemoUi.Button(
            layout.transform,
            "Cherry",
            delegate { Navigator.Pop("Cherry"); });

        DemoUi.Button(
            layout.transform,
            "Godot",
            delegate { Navigator.Pop("Godot"); });
    }
}
