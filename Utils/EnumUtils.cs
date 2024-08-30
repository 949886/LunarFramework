//
//  EnumUtils.cs
//  VupSystem
//
//  Created by LunarEclipse on 2019-08-09 07:07:20.
//  Copyright © 2019 LunarEclipse. All rights reserved.
//

using System;
using System.ComponentModel;
using System.Globalization;

namespace Luna.Utils
{
    public class EnumUtils
    {
        
        /// Get Enum value with Description attribute from string
        ///
        /// Example:
        /// ```csharp
        /// public enum TestEnum
        /// {
        ///     [Description("Test1")]
        ///     Test1,
        ///     [Description("Test2")]
        ///     Test2
        /// }
        ///
        /// var testEnum = EnumUtils.FromDescription<TestEnum>("Test1");
        /// ```
        public static object FromDescription(Type enumType, string value)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum)
                throw new ArgumentException("Arg1 must be Enum", nameof(enumType));
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("Arg2 can not be null or empty", nameof(enumType));

            value = value.Trim();

            object parsedEnum = null;
            foreach (var field in enumType.GetFields())
            {
                var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var description = attributes.Length > 0 ? attributes[0].Description : string.Empty;
                if (description != String.Empty && description == value)
                {
                    Type underlyingType = Enum.GetUnderlyingType(enumType);
                    object obj = Convert.ChangeType(field.GetRawConstantValue(), underlyingType,
                        (IFormatProvider)CultureInfo.InvariantCulture);
                    parsedEnum = Enum.ToObject(enumType, obj);
                }
            }

            if (parsedEnum == null)
                throw new OverflowException("Desc not exists.");
            else return parsedEnum;
        }
    }
}