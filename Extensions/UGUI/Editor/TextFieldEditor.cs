#if UNITY_EDITOR && USE_TEXTMESHPRO

using TMPro.EditorUtilities;
using UnityEditor;

namespace Luna.Extensions.UGUI
{
    [CustomEditor(typeof(TextField))]
    public class TextFieldEditor: TMP_InputFieldEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Text Field", EditorStyles.boldLabel);
            var textField = target as TextField;
            textField.editOnFocus = EditorGUILayout.Toggle("Edit On Focus", textField.editOnFocus);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("TextMesh Pro", EditorStyles.boldLabel);
            
            base.OnInspectorGUI();
        }
    }
}

#endif