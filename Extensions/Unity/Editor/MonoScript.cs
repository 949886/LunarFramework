// Created by LunarEclipse on 2024-10-02 17:10.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Luna.Internal
{
    public class MonoScript
    {
        public readonly UnityEditor.MonoScript monoScript;
        
        public MonoScript(MonoBehaviour monoBehaviour)
        {
            monoScript = UnityEditor.MonoScript.FromMonoBehaviour(monoBehaviour);
        }
        
        public MonoScript(ScriptableObject scriptableObject)
        {
            monoScript = UnityEditor.MonoScript.FromScriptableObject(scriptableObject);
        }
        
        public MonoScript(Type type)
        {
#if UNITY_6000_0_OR_NEWER
            monoScript = UnityEditor.MonoScript.FromType(type);
#else
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var go = new GameObject();
                var c = go.AddComponent(type);
                monoScript = UnityEditor.MonoScript.FromMonoBehaviour(c as MonoBehaviour);
                UnityEngine.Object.DestroyImmediate(go);
            }
            else if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                var so = ScriptableObject.CreateInstance(type);
                monoScript = UnityEditor.MonoScript.FromScriptableObject(so);
            }
            // else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            //     monoScript = UnityEditor.MonoScript.FromScriptedObject(c as UnityEngine.Object);
            else throw new ArgumentException($"Type {type} is not a valid type for MonoScript");
#endif
        }
        
#if UNITY_6000_0_OR_NEWER
        public MonoScript(string className, string nameSpace, string assemblyName)
        {
            monoScript = UnityEditor.MonoScript.FromTypeInternal(className, nameSpace, assemblyName);
        }
#endif

        private MonoScript(UnityEditor.MonoScript monoScript)
        {
            this.monoScript = monoScript;
        }

        public static MonoScript FromMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            return new MonoScript(monoBehaviour);
        }
        
        public static MonoScript FromScriptableObject(ScriptableObject scriptableObject)
        {
            return new MonoScript(scriptableObject);
        }
        
        public static MonoScript FromType(Type type)
        {
            return new MonoScript(type);
        }

#if UNITY_6000_0_OR_NEWER
        public static MonoScript From(string className, string nameSpace, string assemblyName)
        {
            return new MonoScript(className, nameSpace, assemblyName);
        }
#endif
        
        public Type GetClass()
        {
            return monoScript.GetClass();
        }
        
        public string GetNamespace()
        {
            return monoScript.GetNamespace();
        }
        
        public string GetAssemblyName()
        {
            return monoScript.GetAssemblyName();
        }
        
        public static implicit operator UnityEditor.MonoScript(MonoScript monoScript)
        {
            return monoScript.monoScript;
        }

        public static implicit operator MonoScript(TextAsset monoScript)
        {
            return new MonoScript(monoScript as UnityEditor.MonoScript);
        }
    }
}

#endif