//
//  DictionaryExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Extensions
{
    public static class DictionaryExtension
    {
        public static Value Get<Key, Value>(this Dictionary<Key, Value> dict, Key key)
        {
            if (dict == null || !dict.ContainsKey(key))
                return default;
            return dict[key];
        }
    }
}
