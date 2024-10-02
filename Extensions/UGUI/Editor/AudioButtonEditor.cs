#if UNITY_EDITOR

using Luna.Extensions;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Luna.Extensions.UGUI
{
    [CustomEditor(typeof(AudioButton))]
    public class AudioButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            AudioButton targetMenuButton = (AudioButton)target;
        
            EditorGUILayout.LabelField("Glow", EditorStyles.boldLabel);

            targetMenuButton.clickAudio = EditorGUILayout.ObjectField("Click audio", targetMenuButton.clickAudio, typeof(AudioClip), false) as AudioClip;
            targetMenuButton.submitAudio = EditorGUILayout.ObjectField("Submit audio", targetMenuButton.submitAudio, typeof(AudioClip), false) as AudioClip;
            targetMenuButton.hoverAudio = EditorGUILayout.ObjectField("Hover audio", targetMenuButton.hoverAudio, typeof(AudioClip), false) as AudioClip;
            targetMenuButton.selectAudio = EditorGUILayout.ObjectField("Select audio", targetMenuButton.selectAudio, typeof(AudioClip), false) as AudioClip;
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Focus", EditorStyles.boldLabel);
            targetMenuButton.focusOnEnable = EditorGUILayout.Toggle("Focus on enable", targetMenuButton.focusOnEnable);
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Button", EditorStyles.boldLabel);
        
            base.OnInspectorGUI();
        
            EditorUtility.SetDirty(targetMenuButton);
        }
    }
}

#endif