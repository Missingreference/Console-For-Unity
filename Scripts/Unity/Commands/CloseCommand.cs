using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class CloseCommand : UnityCommand
    {
        public override string name => "close";

        public override string helpMessage => "Close the console window.";

        public override void UnityExecute(params string[] args)
        {
            UnityConsole.Hide();
        }
    }
}
