//
//  ArrayExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System;

namespace Luna.Extensions
{
    public static class ArrayExtension
    {
        public static T[] Subarray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
