//
//  DictionaryExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System;
using System.Collections;
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
    
    public class BiMap<T1, T2>: IEnumerable<KeyValuePair<T1, T2>>
    {
        private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();
        
        public T2 this[T1 key]
        {
            get { return _forward[key]; }
            set { _forward[key] = value; _reverse[value] = key; }
        }
        
        public T1 this[T2 key]
        {
            get { return _reverse[key]; }
            set { _reverse[key] = value; _forward[value] = key; }
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return _forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
