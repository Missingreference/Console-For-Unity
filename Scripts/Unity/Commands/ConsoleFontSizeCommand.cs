//Deprecating font size setting due to definitive font viewing fix.
/*
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class ConsoleFontSizeCommand : UnityCommand
    {
        public override string name => "con_font_size";

        public override string helpMessage => "Set the font size of the text of the output console window. Font size can be a decimal number";

        public float minFontSize = 0.1f;
        public float maxFontSize = 100.0f;

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                Console.Log("Current Font Size: " + UnityConsole.consoleUI.fontSize);
                OutputUsage("size");
                return;
            }

            if (!float.TryParse(args[0], out float size))
            {
                Console.Log("Inputted argument '" + args[0] + "' is not a valid number.");
                return;
            }
            else if (size <= 0.0f)
            {
                Console.Log("Font size cannot be zero or less.");
                return;
            }
            else
            {
                UnityConsole.consoleUI.SetFontSize(Mathf.Clamp(size, minFontSize, maxFontSize));
                return;
            }
        }
    }
}
*/