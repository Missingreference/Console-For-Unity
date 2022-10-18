using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Elanetic.Console
{
    /// <summary>
    /// Manage debug messages and execute commands to the state of the runtime.
    /// The Console class itself does not handle the visual aspect of the console. Use the Output events to subscribe to the console output and handle it from there.
    /// </summary>
    static public class Console
    {
        /// <summary>
        /// Event for when text is printed to the console. 
        /// </summary>
        static public Action<string> onOutputLog;
        /// <summary>
        /// Event for when text is printed to the console. The outputted text is meant to represent a warning.
        /// </summary>
        static public Action<string> onOutputWarning;
        /// <summary>
        /// Event for when text is printed to the console. The outputted text is meant to represent an error.
        /// </summary>
        static public Action<string> onOutputError;

        static private Dictionary<string, ConsoleCommand> m_CommandLookup = new Dictionary<string, ConsoleCommand>(255);
        static private List<string> m_AllCommandNames = new List<string>(255);

        static Console()
        {
            ReloadCommands();
        }

        /// <summary>
        /// Print a message to the console. Does not add a newline to the output.
        /// </summary>
        static public void Print(object message)
        {
            onOutputLog?.Invoke(message.ToString());
        }

        /// <summary>
        /// Print a warning to the console. Does not add a newline to the output.
        /// </summary>
        static public void PrintWarning(object message)
        {
            onOutputWarning?.Invoke(message.ToString());
        }

        /// <summary>
        /// Print an error to the console. Does not add a newline to the output.
        /// </summary>
        static public void PrintError(object message)
        {
            onOutputError?.Invoke(message.ToString());
        }

        /// <summary>
        /// Simply print a message to the console. Adds a newline to the output.
        /// </summary>
        static public void Log(object message)
        {
            string messageString = message.ToString();
            if(!messageString.EndsWith(Environment.NewLine))
            {
                messageString += Environment.NewLine;
            }
            onOutputLog?.Invoke(messageString);
        }

        /// <summary>
        /// Print a warning to the console. Adds a newline to the output.
        /// </summary>
        static public void LogWarning(object message)
        {
            string messageString = message.ToString();
            if(!messageString.EndsWith(Environment.NewLine))
            {
                messageString += Environment.NewLine;
            }
            onOutputWarning?.Invoke(messageString);
        }

        /// <summary>
        /// Print an error to the console. Adds a newline to the output.
        /// </summary>
        static public void LogError(object message)
        {
            string messageString = message.ToString();
            if(!messageString.EndsWith(Environment.NewLine))
            {
                messageString += Environment.NewLine;
            }
            onOutputError?.Invoke(messageString);
        }

        /// <summary>
        /// Find and execute a command by name.
        /// </summary>
        static public void Execute(string commandName, params string[] args)
        {
            ConsoleCommand command = FindCommandByName(commandName);
            if(command == null)
            {
                Log("'" + commandName + "' command could not be found.");
                return;
            }

            command.Execute(args);
        }

        /// <summary>
        /// Parse string and execute resulting command. 
        /// </summary>
        static public void Parse(string command)
        {
            if(command == null)
            {
                //Nothing to parse
                return;
            }

            command = command.Trim();

            if(command.Length == 0)
            {
                //Nothing to parse
                return;
            }

            string commandName = "";
            int i = 1;
            for(; i < command.Length; i++)
            {
                if(command[i] != ' ') continue;

                commandName = command.Substring(0, i);

                while(command[i] == ' ') i++;

                break;
            }

            if(commandName == "") commandName = command;

            List<string> splits = new List<string>();
            for(; i < command.Length; i++)
            {
                char character = command[i];
                if(character == '"' || character == '\'')
                {
                    //Start string
                    int startIndex = i;

                    bool foundEndString = false;
                    for(int h = i+1; h < command.Length; h++)
                    {
                        if(command[h] == character && (h == command.Length-1 || command[h+1] == ' '))
                        {
                            if(startIndex != h - 1) //Check if the string is empty
                            {
                                foundEndString = true;
                                startIndex++;
                                splits.Add(command.Substring(startIndex, h - startIndex));
                                i = h;
                            }

                            break;
                        }
                    }

                    if(!foundEndString)
                    {
                        while(i < command.Length - 1 && command[i + 1] != ' ')
                        {
                            i++;
                        }

                        splits.Add(command.Substring(startIndex, i - startIndex + 1));
                    }
                }
                else
                {
                    int startIndex = i;
                    while(i < command.Length-1 && command[i+1] != ' ')
                    {
                        i++;
                    }

                    splits.Add(command.Substring(startIndex, i - startIndex + 1));
                }

                if(i < command.Length - 1)
                {
                    while(command[i+1] == ' ') i++;
                }
            }
            
            /*
            string s = "Command: '" + commandName + "' ";
            for(i = 0; i < splits.Count; i++)
            {
                s += "Arg[" + i.ToString() + "]: '" + splits[i] + "' ";
            }
            Console.Log(s);
            */

            Execute(commandName, splits.ToArray());
        }

        /// <summary>
        /// Retrieve the ConsoleCommand instance of a loaded command by its name.
        /// </summary>
        static public ConsoleCommand FindCommandByName(string commandName)
        {
            if(m_CommandLookup.TryGetValue(commandName.ToLower(), out ConsoleCommand command))
            {
                return command;
            }
            return null;
        }

        /// <summary>
        /// Get the names of all of the loaded commands.
        /// </summary>
        static public string[] GetAllCommands()
        {
            //Copy to array that way the internal list is not modified.
            return m_AllCommandNames.ToArray();
        }

        /// <summary>
        /// Reinitiliaze all console commands. It is recommended that no references to existing console commands exist before calling this function so that the garbage collector can call the deconstructor on any of those instances before creating new ones.
        /// </summary>
        static public void ReloadCommands()
        {
            m_CommandLookup = new Dictionary<string, ConsoleCommand>(255);
            m_AllCommandNames = new List<string>(255);

            //Clean up created commands
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for(int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                Type[] types = assembly.GetTypes();
                for(int h = 0; h < types.Length; h++)
                {
                    Type type = types[h];
                    if(type.IsSubclassOf(typeof(ConsoleCommand)) && !type.IsAbstract)
                    {
                        ConsoleCommand command;
                        try
                        {
                            command = (ConsoleCommand)Activator.CreateInstance(type);
                        }
                        catch(Exception exception)
                        {
                            LogError("Exception: " + exception.Message + " \nStackTrace: " + exception.StackTrace);
                            LogError("Console Command class '" + type.Name + "' threw an exception while creating an instance of the command and will not be loaded.");
                            continue;
                        }

                        LoadCommand(command);
                    }
                }
            }

            m_AllCommandNames.Sort();

            Log(m_AllCommandNames.Count.ToString() + " commands loaded.");
        }

        static private void LoadCommand(ConsoleCommand command)
        {
            //Verify
            for(int i = 0; i < command.name.Length; i++)
            {
                if(char.IsWhiteSpace(command.name[i]))
                {
                    LogError("Command '" + command.name + "' contains invalid whitespace characters and will not be loaded.");
                    return;
                }
            }

            if(command.name.Contains(Environment.NewLine))
            {
                LogError("Command '" + command.name + "' contains invalid NewLine characters and will not be loaded.");
                return;
            }

            string commandName = command.name.ToLower();
            if(m_AllCommandNames.Contains(commandName))
            {
                LogError("A command with the name '" + commandName + "' has already been loaded. Duplicate command will not be loaded. Existing: '" + m_CommandLookup[commandName].GetType().Name + "' Duplicate: '" + command.GetType().Name + "'");
                return;
            }

            //Add
            m_AllCommandNames.Add(commandName);
            m_CommandLookup.Add(commandName, command);
        }
    }
}
