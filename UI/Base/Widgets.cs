// Created by LunarEclipse on 2024-6-19 3:2.

using System.Collections.Generic;
using UnityEngine;

namespace Luna.UI
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class Widgets : ScriptableObject
    {
        public List<GameObject> Prefabs = new List<GameObject>();

        public void Add(GameObject prefab)
        {
            if (Prefabs.Contains(prefab))
            {
                Debug.LogWarning("Prefab already exists in the list.");
                return;
            }
            Prefabs.Add(prefab);
            Debug.Log("Added prefab: " + prefab.name);
        }
    }
}