// Created by LunarEclipse on 2024-6-19 0:40.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luna.UI
{
    public partial class Widget
    {
        // Widget database file name
        public const string WIDGETS_DB_FILE_NAME = "Widgets.g";
        
        public static List<GameObject> All { get; private set; } = new ();
        public static Dictionary<Type, GameObject> Dictionary { get; private set; } = new ();
        
        // Find all StatefulWidget in the project at startup.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Load stateful widgets scriptable object
            var widgets = Resources.Load<Widgets>(WIDGETS_DB_FILE_NAME);
            if (widgets != null)
            {
                All = widgets.Prefabs;
                foreach (var widget in All)
                {
                    Debug.Log($"[Widget Database] Found widget: {widget.name}");
                    Dictionary.TryAdd(widget.GetComponent<Widget>().GetType(), widget);
                }
            }
        }
        
        // public static readonly Dictionary<string, Widget> All = new ();
        //
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void Initialize()
        // {
        //     var widgets = Resources.FindObjectsOfTypeAll<Widget>();
        //     foreach (var widget in widgets)
        //     {
        //         All.Add(widget.name, widget);
        //         Debug.Log($"[Widget] Found Widget: {widget.name}");
        //     }
        // }
    }
}