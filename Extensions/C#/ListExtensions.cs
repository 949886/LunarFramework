// Created by LunarEclipse on 2024-6-5 5:21.

using System.Collections.Generic;

namespace Luna.Extensions
{
    public static class ListExtensions
    {
        // Reverses the list and returns a new list.
        public static List<T> Reversed<T>(this List<T> list)
        {
            var newList = new List<T>(list);
            newList.Reverse();
            return newList;
        }
    }
}