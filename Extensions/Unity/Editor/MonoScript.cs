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
            monoScript = UnityEditor.MonoScript.FromType(type);
        }
        
        public MonoScript(string className, string nameSpace, string assemblyName)
        {
            monoScript = UnityEditor.MonoScript.FromTypeInternal(className, nameSpace, assemblyName);
        }

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

        public static MonoScript From(string className, string nameSpace, string assemblyName)
        {
            return new MonoScript(className, nameSpace, assemblyName);
        }
        
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