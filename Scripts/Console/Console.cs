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
            command = command.Trim();
            string[] splitStrings = command.Split(null as string[], StringSplitOptions.RemoveEmptyEntries);

            if(command == string.Empty || splitStrings.Length == 0)
            {
                //Nothing to parse
                return;
            }

            if(splitStrings.Length > 1)
            {
                string[] args = new string[splitStrings.Length - 1];
                Array.Copy(splitStrings, 1, args, 0, args.Length);

                Execute(splitStrings[0], args);
            }
            else
            {
                Execute(splitStrings[0], new string[0]);
            }
        }

        static public ConsoleCommand FindCommandByName(string commandName)
        {
            if(m_CommandLookup.TryGetValue(commandName.ToLower(), out ConsoleCommand command))
            {
                return command;
            }
            return null;
        }

        static public string[] GetAllCommands()
        {
            //Copy to array that way the internal list is not modified.
            return m_AllCommandNames.ToArray();
        }

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

        /// <summary>
        /// Trim the string so that it starts with and ends with a maximum of one Environment.NewLine. Removes extra whitespace as well if a line simply contains whitespace.
        /// </summary>
        static private string TrimNewLines(string message)
        {
            string newLine = Environment.NewLine;
            int index = 0;
            int lastNewLine = -1;
            bool IsNewLine()
            {
                if(index + newLine.Length > message.Length)
                    return false;

                for(int i = 0; i < newLine.Length; i++)
                {
                    if(message[index + i] != newLine[i])
                        return false;
                }
                return true;
            }

            //Trim start

            while(index < message.Length)
            {
                if(message[index] == ' ')
                {
                    index++;
                    continue;
                }
                if(IsNewLine())
                {
                    lastNewLine = index;
                    index += newLine.Length;
                    continue;
                }

                break;
            }

            if(index == message.Length) return newLine;

            if(lastNewLine >= 0)
            {
                message = message.Remove(0, lastNewLine);
            }

            //Trim end

            index = message.Length - 1;
            lastNewLine = -1;

            while(index >= 0)
            {
                int oldIndex = index;
                index = index - newLine.Length + 1;
                if(IsNewLine())
                {
                    lastNewLine = index;
                    index--;
                    continue;
                }

                index = oldIndex;

                if(message[index] == ' ')
                {
                    index--;
                    continue;
                }

                break;
            }

            if(lastNewLine >= 0)
            {
                int startIndex = lastNewLine + newLine.Length;
                message = message.Remove(startIndex, message.Length - startIndex);
            }

            return message;
        }
    }
}
