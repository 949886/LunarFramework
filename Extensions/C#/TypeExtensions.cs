// Created by LunarEclipse on 2024-10-02 15:10.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Luna.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetDerivedTypes(this Type type, bool includeAbstract = false)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(type) && (includeAbstract || !t.IsAbstract));
        }
    }
}