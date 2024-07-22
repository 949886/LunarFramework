// Created by LunarEclipse on 2024-7-18 3:16.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.UI.Misc
{
    public static class EnumerableExtension
    {
        // From: https://stackoverflow.com/questions/2019417/how-to-access-random-item-in-list
        public static T GetRandomly<T>(this IEnumerable<T> source)
        {
            return source.GetRandomly(1).Single();
        }

        public static IEnumerable<T> GetRandomly<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}