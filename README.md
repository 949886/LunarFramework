# Lunar Framework

![](https://img.shields.io/badge/Unity-2021.3-green.svg?style=flat-square)

Lunar Framework is a set of tools and patterns for Unity3D development. It's designed to make the development process easier and more efficient.

## Table of Contents

- [UI](#UI)
  - [UI Navigation](#navigation)
- [Tools](#tools)
  - [Object Pool](#object-pool)
- [Scripting](#scripting)
  - [Attributes](#attributes)


<!--

## Gameplay

### Third Person 

-->


## UI

### Navigation

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
> Any prefab with a `Widget` component will automatically be registered in the Widgets prefab database.  
> A widget type should only have one prefab that corresponds to it.


#### Pop a widget

```cs
  private void Update()
  {
      if (Input.GetKeyDown(KeyCode.Escape)) 
          Navigator.Pop();
  }
```

#### Pass data

**View -> Subview**

```cs
public void OnBlueButtonClick()
{
      // Edit roulette
      Navigator.Push<RouletteGameView>(async (view) =>
      {
            // You can use UniTask to pass data at the next frame
            await UniTask.NextFrame();
            view.RouletteData = Data;
      });
}
```

You can use a lambda callback to pass data to the widget. The callback will be executed after the widget is instantiated.  

> [!NOTE]  
> The callback will be executed before the widget is enabled, so you should initialize the widget in Awake method.


**Subview -> View**

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
      roulettes.Insert(0, result);
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


## Tools

### Object Pool

#### Get an object from the pool

You can get an object from the pool by calling the `ObjectPool.Get` method easily.  
The method will return an object from the pool if there is an available **inactive** object in the pool.
Otherwise, it will instantiate a new object from the prefab.

```csharp
private void Shoot(GameObject bulletPrefab)
{
  GameObject bulletObject = ObjectPool.Get(bulletPrefab);
  bulletObject.SetActive(true);
  
  // align to gun barrel/muzzle position
  bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);
  
  // move projectile forward
  var rigidbody = bulletObject.GetComponent<Rigidbody>();
  rigidbody.velocity = Vector3.zero;
  rigidbody.angularVelocity = Vector3.zero;
  rigidbody.AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);
  
  // turn off after a few seconds
  ExampleProjectile p = bulletObject.GetComponent<ExampleProjectile>();
  p?.Deactivate();
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


<!--

## Event

Define an event in a class.
```cs
public static Event TestEvent = new();
public static Event TestFilteredEvent = new() {
    filter = (sender, receiver, args) => true
};
public static Event<(string a, string b)> TestArgumentEvent = new();
```

```csharp
void OnEvent(object sender, (string a, string b) args) { }

// Subscribe
TestArgumentEvent += OnEvent;

// Unsubscribe 
TestArgumentEvent -= OnEvent;
```



-->

