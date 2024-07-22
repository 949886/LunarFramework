# LunarFramework

![](https://img.shields.io/badge/Unity-2021.3-green.svg?style=flat-square)



- [UI](#UI)
  - [UI Navigation](#navigation)
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

This method is a generic method that takes a type of subclass of `Widget` class as a parameter.

> [!NOTE]  
> Any prefab with Widget component will be registered in the Widgets prefab database automatically.  
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
            // You can use UniTask to pass data at next frame
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
    
    // Following code will be executed after SettingsView pop.
    HandleSettingsData(data);
    EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);  
}
```


**View <-> Subview**


```cs
public async void OnEditButtonClick()
{
      // Create new roulette and let user to edit it.
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


#### Compability

You can have a new navigator anywhere with a game object as root to adapt old code.

```cs
void Start()
{
      Navigator.Create(gameObject);
}
```
