using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class HomePage : NavigationPage
{
    private Text _result;

    private void Start()
    {
        VerticalLayoutGroup layout = DemoUi.BuildPageShell(
            this,
            "Cherry Navigation — Unity");

        DemoUi.Button(
            layout.transform,
            "Push<SettingsPage>()",
            delegate { Navigator.Push<SettingsPage>(); });

        DemoUi.Button(
            layout.transform,
            "PushAsync<CharacterPickerPage>()",
            async delegate
            {
                object result = await Navigator.PushAsync<CharacterPickerPage>(
                    new CharacterPickerParameters("Choose a character"));
                _result.text = "Picker result: " + (result ?? "(none)");
            });

        DemoUi.Button(
            layout.transform,
            "Push<ConfiguredPage>(configure)",
            delegate
            {
                Navigator.Push<ConfiguredPage>(
                    delegate(ConfiguredPage page)
                    {
                        page.Data = "Configured before first activation/Start.";
                        return Task.CompletedTask;
                    });
            });

        DemoUi.Button(
            layout.transform,
            "ShowModal<ModalPage>()",
            delegate { Navigator.ShowModal<ModalPage>(); });

        DemoUi.Button(
            layout.transform,
            "ShowOverlay<OverlayPage>()",
            delegate { Navigator.ShowOverlay<OverlayPage>(); });

        DemoUi.Button(
            layout.transform,
            "ShowDialog(default)",
            async delegate
            {
                NavigationRoute<DefaultDialog> route =
                    Navigator.ShowDialog(
                        delegate(DefaultDialog dialog)
                        {
                            dialog.Title = "Delete item?";
                            dialog.Message =
                                "DefaultDialog is a specialized Modal route.";
                            dialog.AddCloseAction("Cancel");
                            dialog.AddAction(
                                "Delete",
                                delegate { Navigator.Pop("delete"); });
                            return Task.CompletedTask;
                        });

                object result = await route.Popped;
                _result.text = "Dialog result: " + (result ?? "(none)");
            });

        _result = DemoUi.Label(
            layout.transform,
            "Result: (none)",
            14);
    }
}

public sealed class CharacterPickerParameters
{
    public string Prompt { get; private set; }

    public CharacterPickerParameters(string prompt)
    {
        Prompt = prompt;
    }
}
