// // Created by LunarEclipse on 2024-10-02 11:10.
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor.AddComponent;
// using UnityEditor.IMGUI.Controls;
// using UnityEngine;
//
// namespace UnityEditor.UI.Extensions
// {
//     public class PickComponentWindow
//     {
//         public static bool Show(Rect rect = default, Action<MonoScript> onComponentPicked = null)
//         {
//             var propertyEditor = PropertyEditor.FocusedPropertyEditor;
//             var editors = propertyEditor?.tracker.activeEditors;
//             Editor editor = InspectorWindowUtils.GetFirstNonImportInspectorEditor(editors);
//             return _PickComponentWindow.Show(rect, editor.targets.Cast<GameObject>().Where(o => o).ToArray(), onComponentPicked);
//         }
//         
//         public static bool Show(Rect rect, GameObject[] gos, Action<MonoScript> onSelected = null)
//         {
//             return _PickComponentWindow.Show(rect, gos, onSelected);
//         }
//     }
//     
//     
//     [InitializeOnLoad]
//     internal class _PickComponentWindow : AdvancedDropdownWindow
//     {
//         internal const string OpenAddComponentDropdown = "OpenAddComponentDropdown";
//         [Serializable]
//         internal class AnalyticsEventData
//         {
//             public string name;
//             public string filter;
//             public bool isNewScript;
//         }
//
//         private GameObject[] m_GameObjects;
//         private DateTime m_ComponentOpenTime;
//         private const string kComponentSearch = "ComponentSearchString";
//         private const int kMaxWindowHeight = 395 - 80;
//         private static AdvancedDropdownState s_State = new AdvancedDropdownState();
//         
//         private static Action<MonoScript> onSelected;
//
//         protected override bool setInitialSelectionPosition { get; } = false;
//
//         protected override bool isSearchFieldDisabled
//         {
//             get
//             {
//                 var child = state.GetSelectedChild(renderedTreeItem);
//                 if (child != null)
//                     return child is NewScriptDropdownItem;
//                 return false;
//             }
//         }
//
//         internal static bool Show(Rect rect, GameObject[] gos)
//         {
//             
//             CloseAllOpenWindows<_PickComponentWindow>();
//             var window = CreateInstance<_PickComponentWindow>();
//             window.dataSource = new AddComponentDataSource(s_State, gos);
//             window.gui = new AddComponentGUI(window.dataSource, window.OnCreateNewScript);
//             window.state = s_State;
//             window.m_GameObjects = gos;
//             window.m_ComponentOpenTime = DateTime.UtcNow;
//             window.Init(rect);
//             window.searchString = EditorPrefs.GetString(kComponentSearch, "");
//             return true;
//         }
//         
//         internal static bool Show(Rect rect, GameObject[] gos, Action<MonoScript> onComponentSelected)
//         {
//             onSelected = onComponentSelected;
//             return Show(rect, gos);
//         }
//
//         protected override void OnEnable()
//         {
//             base.OnEnable();
//             showHeader = true;
//             selectionChanged += OnItemSelected;
//         }
//
//         private void OnItemSelected(AdvancedDropdownItem item)
//         {
//             if (item is ComponentDropdownItem cdi && !string.IsNullOrEmpty(cdi.menuPath))
//             {
//                 SendUsabilityAnalyticsEvent(new AnalyticsEventData
//                 {
//                     name = item.name,
//                     filter = searchString,
//                     isNewScript = false
//                 });
//                 
//                 var monoScripts = MonoImporter.GetAllRuntimeMonoScripts();
//                 var scriptMap = new Dictionary<string, MonoScript>();
//                 foreach (var monoScript in monoScripts)
//                     scriptMap[monoScript.name] = monoScript;
//                 
//                 var key = cdi.searchableName.Replace(" ", "");
//                 var script = scriptMap[key];
//                 
//                 onSelected?.Invoke(script);
//                 
//                 var gos = m_GameObjects;
//                 EditorApplication.ExecuteMenuItemOnGameObjects(cdi.menuPath, gos);
//             }
//         }
//
//         protected override void OnDisable()
//         {
//             EditorPrefs.SetString(kComponentSearch, searchString);
//         }
//
//         internal new void OnGUI()
//         {
//             if (m_GameObjects.Any(g => !g))
//             {
//                 // Close the popup window if one of the object is now invalid (i.e. deleted from an undo operation)
//                 Close();
//                 GUIUtility.ExitGUI();
//                 return;
//             }
//
//             base.OnGUI();
//         }
//
//         protected override Vector2 CalculateWindowSize(Rect buttonRect)
//         {
//             return new Vector2(buttonRect.width, kMaxWindowHeight);
//         }
//
//         protected override bool SpecialKeyboardHandling(Event evt)
//         {
//             var createScriptMenu = state.GetSelectedChild(renderedTreeItem);
//             if (createScriptMenu is NewScriptDropdownItem nsdi)
//             {
//                 // When creating new script name we want to dedicate both left/right arrow and backspace
//                 // to editing the script name so they can't be used for navigating the menus.
//                 // The only way to get back using the keyboard is pressing Esc.
//                 if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
//                 {
//                     OnCreateNewScript(nsdi);
//                     evt.Use();
//                     GUIUtility.ExitGUI();
//                 }
//
//                 if (evt.keyCode == KeyCode.Escape)
//                 {
//                     GoToParent();
//                     evt.Use();
//                 }
//
//                 return true;
//             }
//             return false;
//         }
//
//         void OnCreateNewScript(NewScriptDropdownItem item)
//         {
//             item.Create(m_GameObjects, searchString);
//             SendUsabilityAnalyticsEvent(new AnalyticsEventData
//             {
//                 name = item.className,
//                 filter = searchString,
//                 isNewScript = true
//             });
//             Close();
//         }
//
//         internal void SendUsabilityAnalyticsEvent(AnalyticsEventData eventData)
//         {
//             var openTime = m_ComponentOpenTime;
//             UsabilityAnalytics.SendEvent("executeAddComponentWindow", openTime, DateTime.UtcNow - openTime, false, eventData);
//         }
//
//         // [UsedByNativeCode]
//         // internal static bool ValidateAddComponentMenuItem()
//         // {
//         //     if (FirstInspectorWithGameObject() != null)
//         //         return true;
//         //     return false;
//         // }
//         //
//         // [UsedByNativeCode]
//         // internal static void ExecuteAddComponentMenuItem()
//         // {
//         //     var insp = FirstInspectorWithGameObject();
//         //     if (insp != null)
//         //     {
//         //         insp.ShowTab();
//         //         insp.m_OpenAddComponentMenu = true;
//         //     }
//         // }
//
//         private static InspectorWindow FirstInspectorWithGameObject()
//         {
//             foreach (var insp in InspectorWindow.GetInspectors())
//                 if (insp.GetInspectedObject() is GameObject)
//                     return insp;
//             return null;
//         }
//     }
// }