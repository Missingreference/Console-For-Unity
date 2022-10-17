using System;
using System.Collections;
using System.Collections.Generic;

namespace Elanetic.Console
{
    /// <summary>
    /// The base class for creating a console command. Assume that the Execute method is called from any thread.
    /// </summary>
    public abstract class ConsoleCommand
    {
        /// <summary>
        /// The name of the console command. Once loaded the name cannot be changed. The name cannot contain spaces.
        /// </summary>
        public abstract string name { get; }
        /// <summary>
        /// The help message for this console command.
        /// </summary>
        public abstract string helpMessage { get; }
        /// <summary>
        /// Called when executed by the console.
        /// </summary>
        public abstract void Execute(params string[] args);

        protected void OutputUsage(params string[] argumentNames)
        {
            string output = "Usage: " + name;
            for (int i = 0; i < argumentNames.Length; i++)
            {
                output += " [" + argumentNames[i] + "]";
            }
            Console.Log(output);
        }

        protected bool ArgumentIsTrue(string argument)
        {
            if(argument.ToLower() == "true") return true;
            if(int.TryParse(argument, out int result) && result > 0) //Should this be "result != 0"?
                return true;
            return false;
        }

        protected bool ArgumentIsWithinValidRange(string argument, int min, int max, out int value)
        {
            if(int.TryParse(argument, out int result) && result >= min && result <= max)
            {
                value = result;
                return true;
            }
            value = -1;
            return false;
        }
    }
}