# Lunar Framework

![](https://img.shields.io/badge/Unity-2021.3-green.svg?style=flat-square)
<img src="https://komarev.com/ghpvc/?username=lunareclipse-lunarframework&color=green&style=flat-square&label=Views" width="1px">

Lunar Framework is a set of tools and patterns for Unity3D development. It's designed to make the development process easier and more efficient.

---

## Table of Contents

- [Installation](#installation)
- [UI](#UI)
  - [UI Navigation](#navigation)
- [Asset Management](#asset-management)
- [Event](#event)
- [Tools](#tools)
  - [Object Pool](#object-pool)
- [Scripting](#scripting)
  - [Attributes](#attributes)


## Installation

### Unity Package Manager

1. Open the Unity Package Manager by selecting `Window` > `Package Manager`.
2. Click the `+` button in the top-left corner and select `Add package from git URL...`.
3. Enter the git URL `https://github.com/949886/LunarFramework.git` and click `Add`.

<!--

## Gameplay

### Third Person 

-->


## UI

### Navigation

#### Navigator

To manage the navigation of the UI with the `Navigator` by following these steps:

1. Create a new game object and attach the `Navigator` component to it.
2. Set the root `Widget` you want to show first when the game starts (Root widget is a game object that has a component of the subclass of the `Widget` or `StatefulWidget`).
3. [Optional] Set the root canvas if you want to use the `Navigator` in a different canvas. If you don't set the root canvas, the `Navigator` will find it automatically.


#### Push a widget

```cs
public void OnSettingsButtonClicked()  
{  
    Navigator.Push<RouletteSettingsView>();  
}
```

`Navigator.Push` method will push a `Widget` to the top of the navigation stack and the widget will be instantiated and added to the root canvas.

This generic method takes a type of subclass of the `Widget` class as a parameter.

> [!NOTE]  
> **A widget type should only have one prefab that corresponds to it.**  
> Any prefab with a `Widget` component will automatically be registered in the Widgets prefab database
> (if addressables package is installed, the prefab will be registered as an addressable asset, otherwise it will be registered as a scriptable object resource).


#### Pop a widget

Pop current widget by calling the `Navigator.Pop` method. The top widget will be removed from the navigation stack and destroyed.

```cs
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape)) 
        Navigator.Pop();
}
```

Alternatively, you can pop a widget by calling the `Navigator.PopUntil<Target>` or `Navigator.PopToRoot` method to pop widgets until a specific widget or the root widget.


#### Pass data


**View -> Subview**

You can use a lambda callback to pass data to the widget. The callback will be executed after the widget is instantiated.

```cs
public void OnBlueButtonClick()
{
    Navigator.Push<RouletteGameView>(async (view) => {
        // Basically, you can pass data to the widget by setting corresponding properties directly.
        // You can also use UniTask to pass data at the next frame if you want to do something after the widget is enabled.
        /* await UniTask.NextFrame(); */
        view.RouletteData = Data;
    });
}
```

> [!NOTE]  
> The callback will be executed before the widget is enabled, so you should initialize the widget in Awake method.


**Subview -> View**

Basically, you can pass data back to the previous widget by using the `Navigator.Pop` method.

```cs
public void OnExitButtonClick()
{
    Navigator.Pop(data);
}
```

Then you can get the data from the previous widget by casting the return value of the `Navigator.Push` method.

```cs
public async void OnClickSettingsButton()  
{  
    var data = await Navigator.Push<RouletteSettingsView>() as string;
    
    // Following code will be executed after the SettingsView pop.
    HandleSettingsData(data);
    EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);  
}
```


**View <-> Subview**


```cs
public async void OnEditButtonClick()
{
    // Create a new roulette and let the user edit it.
    var roulette = new RouletteData();
    roulette.title = "New Roulette";
    roulette.sectors = new List<RouletteSector>();
    for (int i = 0; i < 8; i++)
        roulette.sectors.Add(new RouletteSector()
        {
            content = $"Roulette {i}",
            weight = 1,
            color = Color.HSVToRGB(1.0f / 8 * i, 0.5f, 1f),
        });
    
    // Open edit view with a new roulette.
    var result = await Navigator.Push<RouletteEditView>((view) => {
        view.Data = roulette;
    }) as RouletteData;
    
    // Add edited new roulette to front.
    if (result != null)
        roulettes.Insert(0, result);
}
```

#### Pop to specific widget

You can pop to a specific widget by calling the `Navigator.PopUntil` method. The top widget will be removed from the navigation stack until the target widget is found.

```cs
public void OnBackToHomeButtonClicked()  
{  
    Navigator.PopUntil<RouletteHomeView>();  
}
```

#### Pop to root widget

You can pop to the root widget by calling the `Navigator.PopToRoot` method. All widgets except the root widget will be removed from the navigation stack.

```cs
public void OnBackToHomeButtonClicked()  
{  
    Navigator.PopToRoot();  
}
```

#### Replace top widget

You can replace the top widget by calling the `Navigator.PushReplacement` method. The top widget will be removed from the navigation stack and destroyed, and the new widget will be pushed to the top of the navigation stack.

```cs
public void OnShowAgainButtonClicked()  
{  
    Navigator.PushReplacement<RouletteGameView>();
}
```

#### Compatibility

You can have a new navigator anywhere with a game object as root to adapt old code.

```cs
void Start()
{
    Navigator.Create(gameObject);
}
```


<!--

### Show Modal Dialog

You can show a modal dialog by calling the `Navigator.ShowModal` method.

-->


## Asset Management

### R.cs

Lunar framework provides a simple way to manage assets in Unity:  

1. It will automatically generate a script `R.cs` that contains the asset references when the editor starts or recompiles.  
2. You can also regenerate this file manually by selecting the menu item `Tools > LunarFramework > Generate R.cs`.

If you have some audio files like this file tree:
```
Assets
  └ Audios
      ├ bgm_lobby.mp3
      └ sf_click.mp3
```

The generated `R.cs` will look like this:

```csharp
namespace R
{
    public static class Audios
    {
        public static Asset<AudioClip> BgmLobby = new("Audios/bgm_lobby");
        public static Asset<AudioClip> SfxClick = new("Audios/sf_click");
    }
}
```

Then you can load and get the asset easily, for example:

```csharp
private void PlaySfxSync() => SFXManager.Play(R.Audios.SfxClick);

private void PlaySfxAsync1() => R.Audios.SfxClick.Load().Then(SFXManager.Play);
private async void PlaySfxAsync2()
{
    var clip = await R.Audios.SfxClick.Load();
    SFXManager.Play(clip);
}
```

Here is an example of preloading assets with a loading indicator:

```csharp
public class PreloadingExample : MonoBehaviour
{
    public GameObject prefab;
    
    private async void Start()
    {
        // Show loading indicator before necessary assets are loaded
        await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);
        Navigator.ShowModal<CircularLoadingIndicator>();
        
        // Load bgm
        var clip = await R.Audios.BgmLobby.Load();
        BgmManager.Play(clip);
        
        // Load audios with both "Audio" and "Games.Common" labels if you enabled addressables package
        await Assets.Load("Games.Common", "Audio");

        Navigator.PopToRoot();
    }
}
```

### Asset

The `Asset<T>` class is a generic class that represents an asset in Unity. It can be used to load assets from the Resources folder or from the Addressables system.

```csharp
Asset<SceneInstance> asset = new("Assets/Game/World/Teyvat/Mondstadt.unity");
asset.onProgress += (progress) => {
    Debug.Log($"[Assets] Loading {asset.Address}: {progress * 100}%");
};
asset.onDownload += status => {
    Debug.Log($"[Assets] Downloading {asset.Address}: {status.Percent * 100}%");
};
asset.onError += (error) => {
    Debug.LogError($"[Assets] Error loading {asset.Address}: {error}");
};
SceneInstance scene = await asset.Load(LoadSceneMode.Additive);
```

>[!NOTE] 
> If you want to load a scene, you should use the `SceneInstance` type as the generic type parameter.  
> If you use `Load()` method to load a scene asset, it will load the scene asynchronously with `LoadSceneMode.Single` mode by default.



## Event

Define some events in a class as static fields.

```cs
public static class MyEvents
{
    public static Event TestEvent = new();
    public static Event<(string a, string b)> TestArgumentEvent = new();
    public static Event<(string a, string b)> TestFilteredEvent = new() {
        filter = (sender, receiver, args) => false
    };
}
```

Subscribe and unsubscribe to the event in a class.

```csharp
public class EventSubscriber : MonoBehaviour
{
    private void OnEnable()
    {
        MyEvents.TestEvent += OnEvent;
        MyEvents.TestArgumentEvent += OnEvent;
        MyEvents.TestFilteredEvent += OnEvent;
    }
    
    private void OnDisable()
    {
        MyEvents.TestEvent -= OnEvent;
        MyEvents.TestArgumentEvent -= OnEvent;
        MyEvents.TestFilteredEvent -= OnEvent;
    }

    private void OnEvent(object sender) { }
    private void OnEvent(object sender, (string a, string b) args) { }
}
```

Trigger the event in another class.

```csharp
public class EventTrigger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MyEvents.TestEvent.Invoke(this);
            MyEvents.TestArgumentEvent.Invoke(this, ("Hello", "World"));
            MyEvents.TestFilteredEvent.Invoke(this, ("Hello", "Filtered World"));
        }
    }
}
```



## Tools

### Object Pool

#### Get an object from the pool

You can get an object from the pool by calling the `ObjectPool.Get` method easily.  
The method will return an object from the pool if there is an available **inactive** object in the pool.
Otherwise, it will instantiate a new object from the prefab.

```csharp
private async void Shoot(GameObject bulletPrefab)
{
  GameObject bulletObject = ObjectPool.Get(bulletPrefab);
  bulletObject.SetActive(true); // <- **Important** to activate the object
  
  // align to gun barrel/muzzle position
  bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);
  
  // move projectile forward
  var rigidbody = bulletObject.GetComponent<Rigidbody>();
  rigidbody.velocity = Vector3.zero;
  rigidbody.angularVelocity = Vector3.zero;
  rigidbody.AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);
  
  // turn off after a few seconds
  await UniTask.Delay(TimeSpan.FromSeconds(3));
  bulletObject.SetActive(false); // <- **Important** set inactive to return to the pool
}
```

## Scripting

### Attributes

**`OnChanged`**

This attribute is used to monitor the changes of a field in Unity Editor. The method will be invoked when the field is changed.
It will add a callback to the field. The callback will be invoked when the field value is changed.

```csharp
// Change the color of the mesh renderer when the intensity is changed.

[SerializeField, OnChanged(nameof(OnColorChange))]
public Color color;

public void OnColorChange() 
{
    if (meshRenderer == null)
        ChangeColor(GetComponent<MeshRenderer>(), color, intensity);    
    else ChangeColor(meshRenderer, color, intensity);
}

private void ChangeColor(MeshRenderer meshRenderer, Color color, float intensity)
{
    foreach (var material in meshRenderer.materials)
        if (material.IsKeywordEnabled("_EMISSION"))
            material.SetColor(PropertyName, color * intensity);
}
```
