using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elanetic.Console.Commands
{
    public class PrintCommand : ConsoleCommand
    {
        public override string name => "print";

        public override string helpMessage => "Print a message to the console.";

        public override void Execute(params string[] args)
        {
            if(args.Length == 0)
            {
                OutputUsage("text");
                return;
            }

            StringBuilder stringBuild = new StringBuilder();

            for(int i = 0; i < args.Length-1; i++)
            {
                stringBuild.Append(args[i]);
                stringBuild.Append(" ");
            }
            //We add the last string that way we don't add an extra space at the end of the message
            stringBuild.Append(args[args.Length-1]);

            Console.Log(stringBuild.ToString());
        }
    }
}
