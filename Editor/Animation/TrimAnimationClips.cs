#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                    // HandleAnimationClip(obj as AnimationClip);
                    HandleAnimationClipAsync(obj as AnimationClip);
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
    
    static void HandleAnimationClip(AnimationClip theAnimation, bool copy = true)
    {
        Debug.Log($"Handle {theAnimation}");
        
        try
        {
            AssetDatabase.StartAssetEditing();

            var bindings = AnimationUtility.GetCurveBindings(theAnimation);
            var newAnimation = new AnimationClip();
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, binding);
                var keys = curve.keys;

                //去除scale曲线
                if (binding.propertyName.ToLower().Contains("scale"))
                    AnimationUtility.SetEditorCurve(theAnimation, binding, null);
                
                // 减少重复帧
                var keysToRemove = new List<Keyframe>();
                for (int i = 1, j = 0; i < keys.Length; i++)
                {
                    var keyframe = keys[i];
                    
                    if (Mathf.Approximately(keyframe.value, curve.keys[j].value) 
                        // || Mathf.Abs(curve.keys[j].value / (keyFrame.value - curve.keys[j].value)) > 1.0 / 0.2
                        )
                    {
                        // curve.RemoveKey(i);
                        keysToRemove.Add(keyframe);
                    }
                    else j = i;
                }
                keys = keys.Except(keysToRemove).ToArray();

                //浮点数精度压缩到f4
                for (int i = 0; i < keys.Length; i++)
                {
                    var keyFrame = keys[i];
                    
                    keyFrame.time = float.Parse(keyFrame.time.ToString("F6"));
                    keyFrame.value = float.Parse(keyFrame.value.ToString("F6"));
                    keyFrame.inTangent = float.Parse(keyFrame.inTangent.ToString("F4"));
                    keyFrame.outTangent = float.Parse(keyFrame.outTangent.ToString("F4"));
                    keyFrame.inWeight = float.Parse(keyFrame.inWeight.ToString("F4"));
                    keyFrame.outWeight = float.Parse(keyFrame.outWeight.ToString("F4"));
                    
                    keys[i] = keyFrame;
                }
                
                curve.keys = keys;
                
                if (copy) newAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
                else theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            if (copy)
            {
                // Save it to the subdirectory "trimed/{theAnimation.name}.anim"
                var newPath = AssetDatabase.GetAssetPath(theAnimation);
                newPath = newPath.Substring(0, newPath.LastIndexOf("/")) + "/trimed/" + theAnimation.name + ".anim";
                
                // Create the directory if it doesn't exist
                var dirctory = Path.GetDirectoryName(newPath);
				if (!Directory.Exists(dirctory))
                    Directory.CreateDirectory(dirctory);
                
                AssetDatabase.CreateAsset(newAnimation, newPath);
            }
            else
            {
                EditorUtility.CopySerialized (theAnimation, newAnimation);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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

    static async Task HandleAnimationClipAsync(AnimationClip theAnimation, bool copy = true)
    {
        Debug.Log($"Handle {theAnimation}");
        
        try
        {
            AssetDatabase.StartAssetEditing();

            var bindings = AnimationUtility.GetCurveBindings(theAnimation);
            var newAnimation = new AnimationClip();
            var tasks = new List<Task<(EditorCurveBinding, AnimationCurve, Keyframe[])>>();
            
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, binding);
                var keys = curve.keys;
                var length = theAnimation.length;
                
                var task = Task.Run(() =>
                {
                    // fix
                    if (keys.Length > 1)
                        for (int i = 1; i < keys.Length; i++)
                        {
                            var key1 = keys[i - 1];
                            var key2 = keys[i];

                            if ((Mathf.Abs(key1.value + key2.value) / (Mathf.Abs(key1.value) + Mathf.Abs(key2.value))) < 0.05)
                            {
                                Debug.Log($"Fix {binding.propertyName} {key1.value} {key2.value}");
                                keys[i].value = -key2.value;
                            }
                        }


                    // 减少重复帧
                    // var keysToRemove = new List<Keyframe>();
                    // for (int i = 1, j = 0; i < keys.Length; i++)
                    // {
                    //     var keyframe = keys[i];
                    //
                    //     if (Mathf.Approximately(keyframe.value, curve.keys[j].value) ||
                    //         (i % 10 != 0 && i != (keys.Length - 1) && Mathf.Abs(curve.keys[j].value / (keyframe.value - curve.keys[j].value)) > (1 / 0.5))
                    //         // || Mathf.Abs(curve.keys[j].value / (keyframe.value - curve.keys[j].value)) > 1.0 / 0.2
                    //        )
                    //     {
                    //         // curve.RemoveKey(i);
                    //         keysToRemove.Add(keyframe);
                    //     }
                    //     else j = i;
                    // }
                    // keys = keys.Except(keysToRemove).ToArray();

                    //浮点数精度压缩到f4
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var keyFrame = keys[i];
                        
                        keyFrame.time = float.Parse(keyFrame.time.ToString("F6"));
                        keyFrame.value = float.Parse(keyFrame.value.ToString("F6"));
                        keyFrame.inTangent = float.Parse(keyFrame.inTangent.ToString("F4"));
                        keyFrame.outTangent = float.Parse(keyFrame.outTangent.ToString("F4"));
                        keyFrame.inWeight = float.Parse(keyFrame.inWeight.ToString("F4"));
                        keyFrame.outWeight = float.Parse(keyFrame.outWeight.ToString("F4"));
                    
                        keys[i] = keyFrame;
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
                
                //去除scale曲线
                // if (binding.propertyName.ToLower().Contains("scale"))
                //     AnimationUtility.SetEditorCurve(newAnimation, binding, null);

                for (int i = 0; i < keys.Length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                }

                if (copy) newAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
                else theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            });

            if (copy)
            {
                // Save it to the subdirectory "trimed/{theAnimation.name}.anim"
                var newPath = AssetDatabase.GetAssetPath(theAnimation);
                newPath = newPath.Substring(0, newPath.LastIndexOf("/")) + "/trimed/" + theAnimation.name + ".anim";
                
                // Create the directory if it doesn't exist
                var dirctory = Path.GetDirectoryName(newPath);
				if (!Directory.Exists(dirctory))
                    Directory.CreateDirectory(dirctory);
                
                AssetDatabase.CreateAsset(newAnimation, newPath);
            }
            else
            {
                EditorUtility.CopySerialized (theAnimation, newAnimation);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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


    static private AnimationClip DuplicateClip(AnimationClip source)
    {
        AnimationClip newClip = new AnimationClip
        {
            frameRate = source.frameRate,
            legacy = source.legacy,
            name = source.name + "_Copy",
            wrapMode = source.wrapMode,
        };

        // Copy all animation curves
        foreach (var binding in AnimationUtility.GetCurveBindings(source))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
            AnimationUtility.SetEditorCurve(newClip, binding, curve);
        }

        return newClip;
    }
}


#endif
