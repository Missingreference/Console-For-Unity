using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class VersionCommand : UnityCommand
    {
        public override string name => "version";

        public override string helpMessage => "Print all version information to the console.";

        public override void UnityExecute(params string[] args)
        {
            string info = "";

            info += "Product: " + Application.productName + " Version: " + Application.version;
            info += "\n";
            info += "Unity Engine Version: " + Application.unityVersion + " Pro Activated: " + Application.HasProLicense().ToString() + " Genuine: " + Application.genuine.ToString();
            info += "\n";
            info += "Platform: " + SystemInfo.operatingSystem;

            Console.Log(info);
        }
    }
}
