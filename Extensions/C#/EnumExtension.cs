//
//  EnumExtension.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-09 03:20:17.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.ComponentModel;
using System.Globalization;

namespace Luna.Extensions
{
    public static class EnumExtensions
    {
        // https://stackoverflow.com/questions/630803/associating-enums-with-strings-in-c-sharp
        public static string ToDescription(this Enum val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[]) val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
