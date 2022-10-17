using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elanetic.Console.Commands
{
    public class ListTypesCommand : ConsoleCommand
    {
        public override string name => "list_command_types";

        public override string helpMessage => "Print all types of console commands.";

        public override void Execute(params string[] args)
        {
            string[] allCommands = Console.GetAllCommands();

            List<Type> types = new List<Type>();
            List<int> counts = new List<int>();

            types.Add(typeof(ConsoleCommand));
            counts.Add(0);

            for (int i = 0; i < allCommands.Length; i++)
            {
                ConsoleCommand command = Console.FindCommandByName(allCommands[i]);
                if(command == null) continue;

                Type commandType = command.GetType();
                while(commandType != typeof(ConsoleCommand))
                {
                    commandType = commandType.BaseType;
                    if(commandType.IsAbstract)
                    {
                        break;
                    }
                }

                bool found = false;
                for (int h = 0; h < types.Count; h++)
                {
                    if (types[h] == commandType)
                    {
                        counts[h]++;
                        found = true;
                    }
                }

                if(!found)
                {
                    types.Add(commandType);
                    counts.Add(1);
                }

            }

            

            StringBuilder stringBuild = new StringBuilder();
            stringBuild.Append(types.Count);
            stringBuild.Append(" Console Command Types\n");

            stringBuild.Append("[0] Base: ");
            stringBuild.Append(counts[0]);
            if (counts[0] == 1)
            {
                stringBuild.Append(" command\n");
            }
            else
            {
                stringBuild.Append(" commands\n");
            }

            for (int i = 1; i < types.Count; i++)
            {
                stringBuild.Append("[");
                stringBuild.Append(i);
                stringBuild.Append("] ");
                stringBuild.Append(types[i].Name);
                stringBuild.Append(": ");
                stringBuild.Append(counts[i]);
                if (counts[i] == 1)
                {
                    stringBuild.Append(" command\n");
                }
                else
                {
                    stringBuild.Append(" commands\n");
                }
            }

            Console.Log(stringBuild.ToString());
        }
    }
}
