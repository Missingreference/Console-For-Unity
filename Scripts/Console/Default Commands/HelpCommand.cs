using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elanetic.Console.Commands
{
    public class HelpCommand : ConsoleCommand
    {
        public override string name => "help";

        public override string helpMessage => "Print the help message for a command to the console.";

        public override void Execute(params string[] args)
        {
            if(args.Length == 0)
            {
                Console.Log("Usage: help [command]");
                return;
            }

            string targetCommand = args[0];
            ConsoleCommand command = Console.FindCommandByName(targetCommand);

            if(command == null)
            {
                Console.Log("Command '" + targetCommand + "' could not be found.");
                return;
            }

            Console.Log("'" + targetCommand + "' -" + command.helpMessage);
        }
    }
}
