// Created by LunarEclipse on 2024-7-14 8:18.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Luna.Extensions;
using Luna.Luna.UI;
using Luna.Utils;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

using AudioDict = System.Collections.Generic.Dictionary<string, (UnityEngine.AudioClip audioClip, string path)>;

namespace Luna
{
    public class AssetsPostprocessor : AssetPostprocessor
    {
        const string DB_FILE_PATH = "Audios.g";
        const string CS_FILE_PATH = "Assets/Scripts/R.cs";
        
        const float MAX_SFX_LENGTH = 20f;
        
        const bool USE_PASCALCASE = true;
        
        [MenuItem("Tools/LunarFramework/Generate R.cs")]
        public static void GenerateR()
        {
            Initialize();
        }
        
        // Find all audio files.
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            RemoveAllMissingAudios();
            foreach (var asset in importedAssets)
            {
                if (asset.EndsWith(".wav") || asset.EndsWith(".mp3") || asset.EndsWith(".ogg"))
                {
                    Debug.Log("AudioPostprocessor: Found audio clip: " + asset);
                    var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(asset);
                    if (audioClip != null)
                    {
                        ProcessAudioClip(audioClip);
                    }
                }
            }
        }
        
        // Find all audio clips when the editor starts or recompiles.
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            RemoveAllMissingAudios();
            
            // Get the default addressable settings
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            
            var audioClips = new AudioDict();
            
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (path.StartsWith("Packages/"))
                    continue;
                
                var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (audioClip != null)
                {
                    // Debug.Log("AudioPostprocessor: Found audio clip: " + audioClip.name);
                    ProcessAudioClip(audioClip);
                    
                    var identifier = Path.GetFileName(Path.ChangeExtension(path, null));
                    identifier = ProcessName(identifier);
                    if (audioClips.ContainsKey(identifier))
                        identifier = identifier + "_" + guid;
                    audioClips.Add(identifier, (audioClip, path));
                }
            }
            
            GenerateCode(audioClips);
        }
        
        private static void ProcessAudioClip(AudioClip audioClip)
        {
#if USE_ADDRESSABLES
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup audioGroup = settings.FindGroup("Audios");
            if (audioGroup == null)
            {
                Debug.Log("Creating new addressable group: Audios");
                audioGroup = settings.CreateGroup("Audios", false, false, false, settings.DefaultGroup.Schemas);
            }
            
            var path = AssetDatabase.GetAssetPath(audioClip);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), audioGroup);
            entry.address = path;
            
            // Remove existing labels.
	        // entry.labels.Clear();
	        
	        // Create new labels if they don't exist.
	        if (!settings.GetLabels().Contains("Audio"))
		        settings.AddLabel("Audio");
            
            entry.SetLabel("Audio", true);
#else
            if (audioClip != null)
            {
                // Create the scriptable object if it doesn't exist.
                Audios audios = Resources.Load<Audios>(DB_FILE_PATH);
                if (audios == null)
                {
                    // Create directory if it doesn't exist.
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        AssetDatabase.CreateFolder("Assets", "Resources");
                
                    audios = ScriptableObject.CreateInstance<Audios>();
                    AssetDatabase.CreateAsset(audios, $"Assets/Resources/{DB_FILE_PATH}.asset");
                }

                if (audioClip.length < MAX_SFX_LENGTH)
                {
                    audios.Add(audioClip);
                    EditorUtility.SetDirty(audios);
                }
            }
#endif
        }

        private static void RemoveAllMissingAudios()
        {
            Audios audios = Resources.Load<Audios>(DB_FILE_PATH);
            if (audios != null)
            {
                bool removed = false;
                for (int i = audios.Clips.Count - 1; i >= 0; i--)
                {
                    if (audios.Clips[i] == null)
                    {
                        Debug.Log($"Removed missing prefab from {DB_FILE_PATH}");
                        audios.Clips.RemoveAt(i);
                        removed = true;
                    }
                }

                if (removed)
                {
                    EditorUtility.SetDirty(audios);
                }
            }
        }

        
        private static string LoadGeneratedCode()
        {
            // Load R.cs file.
            if (File.Exists(CS_FILE_PATH))
            {
                Debug.Log("AudioPostprocessor: Loading R.cs file...");
                string code = File.ReadAllText(CS_FILE_PATH);
                return code;
            }
            
            return null;
        }
        
        private static void DeleteGeneratedCode()
        {
            // Delete R.cs file.
            if (File.Exists(CS_FILE_PATH))
            {
                Debug.Log("AudioPostprocessor: Deleting old R.cs file...");
                File.Delete(CS_FILE_PATH);
            }
        }
        
        private static void GenerateCode(AudioDict audios)
        {
            // Generate R.cs file which contains all the audio clips as constants.
            var guid = new GUID("daaaeea437194510a848f9df60651f20"); // R.txt
            var template = EditorUtils.LoadAsset<TextAsset>(guid).text;
            var code = template.Replace("$$AUDIO_VARS$$", GenerateConstants(audios));
            
            Debug.Log("AudioPostprocessor: Generating R.cs file...");
            
            // Compare the old and new code.
            var oldCode = LoadGeneratedCode();
            if (oldCode == code || string.IsNullOrEmpty(code))
            {
                Debug.Log("AudioPostprocessor: No changes detected. Skipping...");
                return;
            }
            
            // Create the directory if it doesn't exist.
            var directory = Path.GetDirectoryName(CS_FILE_PATH);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            File.WriteAllText(CS_FILE_PATH, code);
        }

        private static string GenerateConstants(AudioDict audios)
        {
            string code = "";
            foreach (var pair in audios)
            {
                var path = pair.Value.path;
                var name = pair.Key;
                
                if (path.Contains("Resources/"))
                {
                    var resPath = path.Split(new string[] { "Resources/" }, System.StringSplitOptions.None).Last();
                    resPath = Path.ChangeExtension(resPath, null);
                    code += $@"        public static AudioClip {name} => Resources.Load<AudioClip>(""{resPath}"");" + "\n";
                }
                else
                {
#if USE_ADDRESSABLES
                    code += $@"        public static Asset<AudioClip> {name} => new(""{path}"");" + "\n";      
#endif
                }
                
            }
            return code;
        }
        
        private static string ProcessName(string oldName)
        {
            var name = oldName;
            
            // Remove chinese special characters.
            var pattern = @"[^\w]";
            name = Regex.Replace(name, pattern, "_");
            
            // Convert to PascalCase.
            if (USE_PASCALCASE)
                name = name.ToPascalCase();
            
            // If starts with a number, add an underscore.
            if (char.IsDigit(name[0]))
                name = "_" + name;
            
            return name;
        }
    }
}

#endif
