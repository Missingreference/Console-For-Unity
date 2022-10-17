using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elanetic.Console.Commands
{
    public class FindCommand : ConsoleCommand
    {
        public override string name => "find";

        public override string helpMessage => "Search all command names and help messages that contain the specified text.";

        public override void Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                OutputUsage("text");
                return;
            }

            string searchString = "";
            for (int i = 0; i < args.Length - 1; i++)
            {
                searchString += args[i] + " ";
            }
            searchString += args[args.Length - 1];

            searchString = searchString.ToLower();

            string output = "";

            string[] commands = Console.GetAllCommands();
            for (int i = 0; i < commands.Length; i++)
            {
                ConsoleCommand command = Console.FindCommandByName(commands[i]);
                if (command.name.ToLower().Contains(searchString) || command.helpMessage.ToLower().Contains(searchString))
                {
                    output += command.name + "\n";
                }
            }

            if(output == "")
            {
                output = "No commands found from query '" + searchString + "'";
            }

            Console.Log(output);
        }
    }
}
