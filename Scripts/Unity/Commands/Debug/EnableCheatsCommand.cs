using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class EnableCheatsCommand : UnityCommand
    {
        public override string name => "enable_cheats";

        public override string helpMessage => "Enable or disable the execution of cheat console commands.";

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                OutputUsage("enable");
                return;
            }

            if(ArgumentIsTrue(args[0]))
            {
                if(UnityConsole.allowCheatCommandExecution)
                {
                    Console.Log("Cheats are already enabled.");
                }
                else
                {
                    UnityConsole.allowCheatCommandExecution = true;
                    Console.Log("Cheats enabled.");
                }
            }
            else
            {
                if (UnityConsole.allowCheatCommandExecution)
                {
                    UnityConsole.allowCheatCommandExecution = false;
                    Console.Log("Cheats disabled.");
                }
                else
                {
                    Console.Log("Cheats are already disabled.");
                }
            }
        }
    }
}
