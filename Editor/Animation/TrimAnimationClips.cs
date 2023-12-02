#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrimAnimationClipsMenuItem : MonoBehaviour
{
    [MenuItem("Assets/Animation/Trim Animation Clips", priority = 302)]
    static void TrimAnimationClips()
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

                //去除scale曲线
                if (binding.propertyName.ToLower().Contains("scale"))
                    AnimationUtility.SetEditorCurve(theAnimation, binding, null);

                //浮点数精度压缩到f4
                for (int i = 0; i < keys.Length; i++)
                {
                    var keyFrame = keys[i];
                    
                    keyFrame.value = float.Parse(keyFrame.value.ToString("F4"));
                    keyFrame.inTangent = float.Parse(keyFrame.inTangent.ToString("F4"));
                    keyFrame.outTangent = float.Parse(keyFrame.outTangent.ToString("F4"));
                    
                    keys[i] = keyFrame;
                }
                curve.keys = keys;

                // 减少重复帧
                for (int i = 0, j = 0; i < curve.keys.Length; i++)
                {
                    var keyFrame = curve.keys[i];
                    
                    if (Mathf.Abs((keyFrame.value - curve.keys[j].value) / curve.keys[j].value) < 0.01 && i != 0)
                        curve.RemoveKey(i);
                    else j = i;
                }


                theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"{theAnimation} Done.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CompressAnimationClip Failed !!! animation : {theAnimation} error: {e}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
}

#endif
