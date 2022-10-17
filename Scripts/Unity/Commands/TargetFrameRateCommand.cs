using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class TargetFrameRateCommand : UnityCommand
    {
        public override string name => "target_framerate";

        public override string helpMessage => "Set the target framerate. Setting the value to -1 will use platform default.";

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                OutputUsage("value");
                return;
            }

            if (!int.TryParse(args[0], out int value))
            {
                Console.Log("'" + args[0] + "' is not a valid number.");
                return;
            }

            Application.targetFrameRate = value;
        }
    }
}
