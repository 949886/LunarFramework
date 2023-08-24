//
//  DateExtension.cs
//  Luna
//
//  Created by LunarEclipse on 2017-10-23.
//  Copyright © 2017 LunarEclipse. All rights reserved.
//

using System;

namespace Luna.Extensions
{
    public static class DateExtension
    {
        public static long Timestamp(this DateTime str)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(DateTime.Now - startTime).TotalMilliseconds;
        }
    }
}