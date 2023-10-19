using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_VISUAL_SCRIPTING

using Unity.VisualScripting;

namespace Luna.Extensions.VisualScripting
{
    public abstract class ScriptableUnit : Unit
    {
        [DoNotSerialize] // No need to serialize ports.
        public ControlInput input; //Adding the ControlInput port variable

        [DoNotSerialize] // No need to serialize ports.
        public ControlOutput output;//Adding the ControlOutput port variable.

        protected override void Definition()
        {
            //Making the ControlInput port visible, setting its key and running the anonymous action method to pass the flow to the outputTrigger port.
            input = ControlInput("", (flow) => { 
                Execute(flow);
                return output; 
            });
            //Making the ControlOutput port visible and setting its key.
            output = ControlOutput("");
        }

        public abstract void Execute(Flow flow);
    }
}

#endif
