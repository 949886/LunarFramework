using System;
using UnityEngine;

public partial class RTSInput
{
    public static readonly RTSInput instance = new RTSInput(enable: true);
    
    public @RTSInput(bool enable): this()
    {
        if (enable) Enable();
    }
}
