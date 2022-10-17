using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class ClearCommand : UnityCommand
    {
        public override string name => "clear";

        public override string helpMessage => "Clear the console command of all text.";

        public override void UnityExecute(params string[] args)
        {
            if(UnityConsole.consoleUI != null)
                UnityConsole.consoleUI.Clear();
        }
    }
}
