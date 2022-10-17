using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class ListResolutions : UnityCommand
    {
        public override string name => "list_resolutions";

        public override string helpMessage => "List all available video resolutions and refresh rates.";

        public override void UnityExecute(params string[] args)
        {
            Resolution[] resolutions = Screen.resolutions;
            string s = "";
            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution res = resolutions[i];
                s += res.width.ToString() + "x" + res.height.ToString() + " @ " + res.refreshRate.ToString() + "Hz";
                s += "\n";
            }
            Console.Log(s);
        }
    }
}
