using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class SetResolution : UnityCommand
    {
        public override string name => "setresolution";

        public override string helpMessage => "Set the video resolution and refresh rate.";

        public override void UnityExecute(params string[] args)
        {
            if(args.Length < 2)
            {
                OutputUsage("width", "height", "refresh rate");
                Console.Log("or");
                OutputUsage("width", "height");
                return;
            }

            if (!int.TryParse(args[0], out int width))
            {
                Console.Log("'" + args[0] + "' is not a valid number.");
                return;
            }
            if (!int.TryParse(args[1], out int height))
            {
                Console.Log("'" + args[1] + "' is not a valid number.");
                return;
            }

            int refreshRate;

            if (args.Length >= 3)
            {
                if (!int.TryParse(args[2], out refreshRate))
                {
                    Console.Log("'" + args[2] + "' is not a valid number.");
                    return;
                }
            }
            else
            {
                refreshRate = Screen.currentResolution.refreshRate;
            }

            Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
        }
    }
}
