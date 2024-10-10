// Created by LunarEclipse on 2024-7-30 17:6.

#if UNITY_EDITOR && USE_ADDRESSABLES 

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Luna
{
    [InitializeOnLoad]
    public class AddressableAssetsMonitor
    {
        static AddressableAssetsMonitor()
        {
            AddressableAssetSettingsDefaultObject.Settings.OnModification += OnModification;
        }

        static void OnModification(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent e, object obj)
        {
            // Debug.Log($"[AddressableAssetsMonitor.OnModification] \nsettings: {settings} \nevent: {e} \nobj: {obj}");
            
            if (e == AddressableAssetSettings.ModificationEvent.EntryCreated || 
                e == AddressableAssetSettings.ModificationEvent.EntryAdded)
            {
                if (obj is List<AddressableAssetEntry> entryList)
                {
                    var entry = entryList[0];
                    if (entry != null)
                    {
                        Debug.Log($"[AddressableAssetsMonitor.OnModification] \nentry: {entry}");
                        // entry.SetLabel("label", true);
                    }
                }
            }
        }
    }
}

#endif