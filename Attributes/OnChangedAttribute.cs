using System.Diagnostics;
using System.Linq;
using System.Reflection;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// From: https://stackoverflow.com/questions/48223969/onvaluechanged-for-fields-in-a-scriptable-object
public class OnChangedAttribute : PropertyAttribute
{
    #if UNITY_EDITOR
    public string methodName;
    public OnChangedAttribute(string methodNameNoArguments)
    {
        methodName = methodNameNoArguments;
    }
    #endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OnChangedAttribute))]
public class OnChangedAttributePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label);
        if(EditorGUI.EndChangeCheck())
        {
            OnChangedAttribute at = attribute as OnChangedAttribute;
            MethodInfo method = property.serializedObject.targetObject.GetType().GetMethods().Where(m => m.Name == at.methodName).First();

            if (method != null && method.GetParameters().Count() == 0)// Only instantiate methods with 0 parameters
                method.Invoke(property.serializedObject.targetObject, null);
        }
    }
}
#endif


// #if UNITY_EDITOR

// [CustomPropertyDrawer(typeof(OnChangedAttribute))]
// public class OnChangedCallAttributePropertyDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginChangeCheck();
//         EditorGUI.PropertyField(position, property, label);
//         if (!EditorGUI.EndChangeCheck()) return;

//         var targetObject = property.serializedObject.targetObject;
        
//         var callAttribute = attribute as OnChangedAttribute;
//         var methodName = callAttribute?.methodName;

//         var classType = targetObject.GetType();
//         var methodInfo = classType.GetMethods().FirstOrDefault(info => info.Name == methodName);

//         // Update the serialized field
//         property.serializedObject.ApplyModifiedProperties();
        
//         // If we found a public function with the given name that takes no parameters, invoke it
//         if (methodInfo != null && !methodInfo.GetParameters().Any())
//         {
//             methodInfo.Invoke(targetObject, null);
//         }
//         else
//         {
//             // TODO: Create proper exception
//             Debug.LogError($"OnChangedCall error : No public function taking no " +
//                            $"argument named {methodName} in class {classType.Name}");
//         }
//     }
// }
// #endif
