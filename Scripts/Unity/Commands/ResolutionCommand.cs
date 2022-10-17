using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class ResolutionCommand : UnityCommand
    {
        public override string name => "resolution";

        public override string helpMessage => "Print the current resolution.";

        public override void UnityExecute(params string[] args)
        {
            string info = "Current Resolution: " + Screen.currentResolution.width.ToString() + "x" + Screen.currentResolution.height.ToString() + " @ " + Screen.currentResolution.refreshRate.ToString() + "  Window Mode: " + Screen.fullScreenMode.ToString() + " VSync: " + GetVSyncStatus() + "\n";
            info += "Main Display Info: " + Screen.mainWindowDisplayInfo.name + " " + Screen.mainWindowDisplayInfo.width + "x" + Screen.mainWindowDisplayInfo.height + " @ " + Screen.mainWindowDisplayInfo.refreshRate + "DPI: " + Screen.dpi.ToString();

            Console.Log(info);
        }

        private string GetVSyncStatus()
        {
            if(QualitySettings.vSyncCount == 0)
            {
                return "Disabled: No VSync";
            }
            else if(QualitySettings.vSyncCount == 1)
            {
                return "Enabled: Sync Every Frame";
            }
            else
            {
                return "Enabled: Sync Every " + QualitySettings.vSyncCount + " Frames";
            }
        }
    }
}
