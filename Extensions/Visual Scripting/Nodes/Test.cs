using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_VISUAL_SCRIPTING
using Unity.VisualScripting;

namespace Luna.Unity.VisualScripting
{
    public class Test : ScriptableUnit
    {
        [DoNotSerialize] // No need to serialize ports
        public ValueInput myValueA; // Adding the ValueInput variable for myValueA

        [DoNotSerialize] // No need to serialize ports
        public ValueInput myValueB; // Adding the ValueInput variable for myValueB

        [DoNotSerialize] // No need to serialize ports
        public ValueOutput result; // Adding the ValueOutput variable for result

        private string resultValue;

        protected override void Definition()
        {
            base.Definition();

            //Making the myValueA input value port visible, setting the port label name to myValueA and setting its default value to Hello.
            myValueA = ValueInput<string>("myValueA", "Hello ");
            //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
            myValueB = ValueInput<string>("myValueB", string.Empty);
            //Making the result output value port visible, setting the port label name to result and setting its default value to the resultValue variable.
            result = ValueOutput<string>("result", (flow) => { return resultValue; });
        }

        public override void Execute(Flow flow)
        {
            Debug.Log($"Test {System.DateTime.Now}");
        }
    }
}


#endif