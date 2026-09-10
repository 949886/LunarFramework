# Cherry Navigation Demo

This sample now includes a real Unity scene and page prefabs.

After importing **Navigation Demo** through Unity Package Manager:

1. Wait for scripts and imported prefabs to finish compiling/importing.
2. Open:
   `Assets/Samples/Cherry Navigation/<package-version>/Navigation Demo/Scenes/CherryNavigationDemo.unity`
3. Press **Play**.

You can also use:

`Tools > Cherry Navigation > Open Navigation Demo Scene`

The sample contains these registered page prefabs:

- `HomePage` — `ui://home`
- `SettingsPage` — `ui://settings`
- `CharacterPickerPage` — `ui://character-picker`
- `ConfiguredPage` — `ui://configured`
- `ModalPage` — `ui://modal-demo`
- `OverlayPage` — `ui://overlay`
- `CustomDialog` — `ui://custom-dialog`

`PageRegistryGenerator` automatically sees the imported prefabs under `Assets`
and rebuilds `Assets/Resources/Cherry/PageRegistry.asset`.

The checked-in scene deliberately serializes only `DemoBootstrap`. On Play the
bootstrap creates the Canvas, Navigator, and EventSystem in code, then pushes
`HomePage`. This avoids hard-coding package-internal runtime script GUIDs into
the sample scene while still providing an actual scene asset to open directly.


## Input System compatibility

The demo bootstrap follows Unity's **Active Input Handling** setting:

- New Input System: `InputSystemUIInputModule`
- Old Input Manager: `StandaloneInputModule`
- Both: `InputSystemUIInputModule` is preferred

You do not need to switch Player Settings just to run this sample.
