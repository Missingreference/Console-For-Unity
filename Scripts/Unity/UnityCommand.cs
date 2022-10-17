using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity
{
    /// <summary>
    /// A variation of Console Command special to Unity that ensures that the command is executed on the main thread.
    /// </summary>
    public abstract class UnityCommand : ConsoleCommand
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual bool isCheatCommand => false;

        /// <summary>
        /// Queue this command to be executed on Unity's main thread.
        /// </summary>
        public sealed override void Execute(params string[] args)
        {
            if(isCheatCommand && !UnityConsole.allowCheatCommandExecution)
            {
                Console.Log("Executing cheat commands is not enabled. Set enable_cheats 1 to allow these commands.");
                return;
            }

            UnityConsole.QueueCommand(this, args);
        }

        /// <summary>
        /// Called by Elanetic.Console.Unity.UnityConsole class from an instantiated Monobehaviour on Unity's main thread.
        /// </summary>
        public abstract void UnityExecute(params string[] args);

        /// <summary>
        /// Create a coroutine on the console executor. Be sure to stop any running coroutines in the class deconstructor since Console.ReloadCommands can create a new instance of the command.
        /// </summary>
        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            return UnityConsole.StartCoroutine(routine);
        }

        /// <summary>
        /// Stop a coroutine on the console executor.
        /// </summary>
        protected void StopCoroutine(Coroutine coroutine)
        {
            UnityConsole.StopCoroutine(coroutine);
        }
    }

}