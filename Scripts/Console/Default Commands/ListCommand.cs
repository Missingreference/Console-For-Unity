using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elanetic.Console.Commands
{
    public class ListCommand : ConsoleCommand
    {
        public override string name => "list";

        public override string helpMessage => "Print all available console commands.";

        public override void Execute(params string[] args)
        {
            string[] allCommands = Console.GetAllCommands();

            StringBuilder stringBuild = new StringBuilder();
            for(int i = 0; i < allCommands.Length; i++)
            {
                stringBuild.Append(allCommands[i]);
                stringBuild.Append("\n");
            }

            Console.Log(stringBuild.ToString());
        }
    }
}
