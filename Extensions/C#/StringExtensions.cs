//
//  StringExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System.Globalization;

namespace Luna.Extensions
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string str)
        {
            var info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(str.ToLower().Replace("_", " ")).Replace(" ", string.Empty);
        }
    }
}
