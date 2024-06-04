//
//  Subscribe.cs
//  EventBus
//
//  Created by LunarEclipse on 2018-11-29.
//  Copyright © 2018 LunarEclipse. All rights reserved.
//

using System;

namespace Luna.Core.Event
{
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Field |
                    AttributeTargets.Method |
                    AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = true)]
    public class Subscribe : Attribute
    {
        public readonly ThreadMode threadMode = ThreadMode.DEFAULT;

        public Subscribe() {}

        public Subscribe(ThreadMode threadMode)
        {
            this.threadMode = threadMode;
        }

        ~Subscribe()
        {
            Console.WriteLine("Subscribe Deinit");
        }
    }
}