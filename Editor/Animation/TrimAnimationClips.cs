#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrimAnimationClipsMenuItem : MonoBehaviour
{
    [MenuItem("Assets/Custom/TrimAnimationClips", priority = 302)]
    [System.Obsolete]
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
                case "SceneAsset":

                    break;
                case "DefaultAsset":
                    if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject)))
                    {

                    }
                    //UnknownItem
                    break;
                case "MonoScript":
                    //IsAScript
                    break;
                case "GameObject":
                    //IsAGameObject
                    if (PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) != PrefabAssetType.NotAPrefab)
                    {

                    }
                    //IsAPrefab
                    break;
                case "Shader":
                    //IsAShader
                    break;
                case "AudioMixerController":
                    //IsAnAudioMixer
                    break;
                default:
                    Debug.Log($"Default: {Selection.activeObject.GetType().Name}");
                    break;
            }
        }

    }

    [System.Obsolete]
    static void HandleAnimationClip(AnimationClip theAnimation)
    {
        Debug.Log($"Handle {theAnimation}");
        
        try
        {
            AssetDatabase.StartAssetEditing();
            
            //去除scale曲线
            foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
            {
                string name = theCurveBinding.propertyName.ToLower();
                if (name.Contains("scale"))
                    AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
            }

            var bindings = AnimationUtility.GetCurveBindings(theAnimation);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(theAnimation, binding);
                var keys = curve.keys;

                //去除scale曲线
                if (binding.propertyName.ToLower().Contains("scale"))
                    AnimationUtility.SetEditorCurve(theAnimation, binding, null);

                //浮点数精度压缩到f3
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
                    {
                        curve.RemoveKey(i);
                        continue;
                    }
                    else j = i;
                }


                theAnimation.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"{theAnimation} Done.");
        }
        catch (System.Exception e)
        {
            Debug.LogError(string.Format("CompressAnimationClip Failed !!! animation : {0} error: {1}", theAnimation, e));
        }
        finally
        {
            //在 "finally" 代码块中添加
            //对 StopAssetEditing 的调用可确保
            //在离开此函数时重置 AssetDatabase 状态
            AssetDatabase.StopAssetEditing();
        }
    }
}

#endif
