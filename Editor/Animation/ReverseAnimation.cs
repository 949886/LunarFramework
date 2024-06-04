/*------------------------------------------------------------------------------
Original Author: Pikachuxxxx
Adapted By: Brandon Lyman

This script is an adaptation of Pikachuxxxx's utiltiy to reverse an animation 
clip in Unity. Please find the original Github Gist here:

https://gist.github.com/Pikachuxxxx/8101da6d14a5afde80c7c180e3a43644
https://gist.github.com/8101da6d14a5afde80c7c180e3a43644.git

ABSOLUTELY ALL CREDIT FOR THIS SCRIPT goes to Pikachuxxxx. Thank you so much for
your original script!

Unfortunately, their method that utilizes 
"AnimationUtility.GetAllCurves()" is obsolete, according to the official
unity documentation:

https://docs.unity3d.com/ScriptReference/AnimationUtility.GetAllCurves.html 

The editor suggests using "AnimationUtility.GetCurveBindings()" in its stead,
and this script reveals how that can be accomplished as it is slightly
different from the original methodology. I also added in some logic to 
differentiate between the original clip and the new clip being created, as 
I experienced null reference exceptions after the original "ClearAllCurves()" 
call. Additionally, I placed the script's logic in a ScriptableWizard class to 
fit the needs for my project. For more information on ScriptableWizards, please
refer to this Unity Learn Tutorial:

https://learn.unity.com/tutorial/creating-basic-editor-tools#5cf6c8f2edbc2a160a8a0951

Hope this helps and please comment with any questions. Thanks!

------------------------------------------------------------------------------*/

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ReverseAnimationClip : ScriptableWizard
{
    public string NewFileName = "";

    [MenuItem("Assets/Animation/Reverse Animation Clip to New File...", priority = 303)]
    private static void ReverseAnimationClipWizard()
    {
        ScriptableWizard.DisplayWizard<ReverseAnimationClip>("Reverse Animation Clip...", "Reverse");
    }

    private void OnWizardCreate()
    {
        string directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
        fileName = fileName.Split('.')[0];

        string copiedFilePath = "";
        if (NewFileName != null && NewFileName != "")
            copiedFilePath = directoryPath + Path.DirectorySeparatorChar + NewFileName + fileExtension;
        else copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;

        AnimationClip originalClip = GetSelectedClip();

        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

        AnimationClip reversedClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

        if (originalClip == null) return;

        float clipLength = originalClip.length;
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
        Debug.Log(curveBindings.Length);
        reversedClip.ClearCurves();
        foreach (EditorCurveBinding binding in curveBindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
            Keyframe[] keys = curve.keys;
            int keyCount = keys.Length;
            WrapMode postWrapmode = curve.postWrapMode;
            curve.postWrapMode = curve.preWrapMode;
            curve.preWrapMode = postWrapmode;
            for (int i = 0; i < keyCount; i++)
            {
                Keyframe K = keys[i];
                K.time = clipLength - K.time;
                float tmp = -K.inTangent;
                K.inTangent = -K.outTangent;
                K.outTangent = tmp;
                keys[i] = K;
            }
            curve.keys = keys;
            reversedClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
        }

        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(originalClip);
        if (events.Length > 0)
        {
            for (int i = 0; i < events.Length; i++)
            {
                events[i].time = clipLength - events[i].time;
            }
            AnimationUtility.SetAnimationEvents(reversedClip, events);
        }

        Debug.Log("[[ReverseAnimationClip.cs]] Successfully reversed " +
        "animation clip " + fileName + ".");
    }

    private AnimationClip GetSelectedClip()
    {
        Object[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
        if (clips.Length > 0)
        {
            return clips[0] as AnimationClip;
        }
        return null;
    }
    
    
    [MenuItem("Assets/Animation/Reverse Animation Clip", priority = 302)]
    static void ReverseAnimationClips()
    {
        if (Selection.activeObject == null) return;

        foreach (var obj in Selection.objects)
        {
            switch (Selection.activeObject.GetType().Name)
            {
                case "AnimationClip":
                    // Debug.Log($"{AssetDatabase.GetAssetPath(obj.GetInstanceID())}.");
                    HandleAnimationClip(obj as AnimationClip);
                    break;
                case "SceneAsset": break;
                case "DefaultAsset":// Unknown Item
                    if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject))) { }
                    break;
                case "MonoScript":  // Script
                    break;
                case "GameObject":
                    //Is a GameObject
                    if (PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) != PrefabAssetType.NotAPrefab) { }
                    //Is a Prefab
                    else { }
                    break;
                case "Shader": 
                    break;
                case "AudioMixerController": 
                    break;
                default:
                    Debug.Log($"Default: {Selection.activeObject.GetType().Name}");
                    break;
            }
        }
    }
    
    [MenuItem("Assets/Animation/Reverse Animation Clip Asynchronously", priority = 302)]
    static async void ReverseAnimationClipsAsync()
    {
        if (Selection.activeObject == null) return;

        foreach (var obj in Selection.objects)
        {
            switch (Selection.activeObject.GetType().Name)
            {
                case "AnimationClip":
                    // Debug.Log($"{AssetDatabase.GetAssetPath(obj.GetInstanceID())}.");
                    await HandleAnimationClipAsync(obj as AnimationClip);
                    break;
                case "SceneAsset": break;
                case "DefaultAsset":// Unknown Item
                    if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject))) { }
                    break;
                case "MonoScript":  // Script
                    break;
                case "GameObject":
                    //Is a GameObject
                    if (PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) != PrefabAssetType.NotAPrefab) { }
                    //Is a Prefab
                    else { }
                    break;
                case "Shader": 
                    break;
                case "AudioMixerController": 
                    break;
                default:
                    Debug.Log($"Default: {Selection.activeObject.GetType().Name}");
                    break;
            }
        }
    }
    
    static void HandleAnimationClip(AnimationClip theAnimation)
    {
        Debug.Log($"Handle {theAnimation}");
        
        try
        {
            AssetDatabase.StartAssetEditing();

            var bindings = AnimationUtility.GetCurveBindings(theAnimation);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, binding);
                var keys = curve.keys;
                
                int keyCount = keys.Length;
                
                // Swap wrap modes
                (curve.postWrapMode, curve.preWrapMode) = 
                    (curve.preWrapMode, curve.postWrapMode);
                
                // Reverse keys
                for (int i = 0; i < keyCount; i++)
                {
                    Keyframe key = keys[i];
                    key.time = theAnimation.length - key.time;
                    float tmp = -key.inTangent;
                    key.inTangent = -key.outTangent;
                    key.outTangent = tmp;
                    keys[i] = key;
                }
                
                curve.keys = keys;
                theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"{theAnimation} Done.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Handle AnimationClip {theAnimation} Failed !!! error: {e}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
    
    static async Task HandleAnimationClipAsync(AnimationClip theAnimation)
    {
        Debug.Log($"Handle {theAnimation}");
        
        try
        {
            AssetDatabase.StartAssetEditing();

            var tasks = new List<Task<(EditorCurveBinding, AnimationCurve, Keyframe[])>>();
            
            var bindings = AnimationUtility.GetCurveBindings(theAnimation);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, binding);
                var keys = curve.keys;
                var length = theAnimation.length;
                
                var task = Task.Run(() =>
                {
                    int keyCount = keys.Length;
                    
                    // Swap wrap modes
                    (curve.postWrapMode, curve.preWrapMode) = 
                        (curve.preWrapMode, curve.postWrapMode);

                    // Reverse keys
                    for (int i = 0; i < keyCount; i++)
                    {
                        Keyframe key = keys[i];
                        key.time = length - key.time;
                        float tmp = -key.inTangent;
                        key.inTangent = -key.outTangent;
                        key.outTangent = tmp;
                        keys[i] = key;
                    }
                    
                    return (binding, curve, keys);
                });
                tasks.Add(task);
            }
            
            var result = await Task.WhenAll(tasks); 

            result.ToList().ForEach(value =>
            {
                var binding = value.Item1;
                var curve = value.Item2;
                var keys = value.Item3;
                curve.keys = keys;
                theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            });
            
            AssetDatabase.SaveAssets();
            Debug.Log($"{theAnimation} Done.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Handle AnimationClip {theAnimation} Failed !!! error: {e}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
}

#endif
