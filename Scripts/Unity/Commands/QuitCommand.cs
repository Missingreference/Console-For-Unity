using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Elanetic.Console.Unity.Commands
{
    public class QuitCommand : UnityCommand
    {
        public override string name => "quit";

#if UNITY_EDITOR
        public override string helpMessage => "Exit playmode.";
#else
        public override string helpMessage => "Exit the game.";
#endif
        
        public override void UnityExecute(params string[] args)
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
            return;
#else
            Application.Quit();
#endif
        }
    }
}
