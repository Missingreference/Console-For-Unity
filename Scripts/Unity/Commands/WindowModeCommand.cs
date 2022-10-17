using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Elanetic.Console.Unity.Commands
{
    public class WindowModeCommand : UnityCommand
    {
        public override string name => "window_mode";

        public override string helpMessage => "Set the window mode for the application.\nWindow Modes:\n" + OutputValidWindowModes();

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                Console.Log("Current Window Mode: " + Screen.fullScreenMode.ToString());
                OutputUsage("mode");
                return;
            }

            if (!int.TryParse(args[0], out int value) || value < 0 || value >= WINDOWED_MODE_NAMES.Length)
            {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
                Console.Log("'" + args[0] + "' is not a valid window mode. Expected number value from 0 to 2.");
#else
                Console.Log("'" + args[0] + "' is not a valid window mode. Expected number value from 0 to 1.");
#endif
                return;
            }

            Screen.fullScreenMode = ConvertInputIndexToFullScreenMode(value);
        }

        private readonly string[] WINDOWED_MODE_NAMES =
        {
#if UNITY_STANDALONE_WIN
            "Exclusive Fullscreen",
#elif UNITY_STANDALONE_OSX
            "Maximized Fullscreen",
#endif
            "Borderless Fullscreen",
            "Windowed",
        };

        private readonly string[] WINDOWED_MODE_DESCRIPTIONS =
        {
#if UNITY_STANDALONE_WIN
            "Use the full display exclusively for the application. OS relevant UI do not overlay this application.",
#elif UNITY_STANDALONE_OSX
            "Use the full display as it's own desktop instance where it will hide the menu bar and application dock.",
#endif
            "Borderless fullscreen window. The application will use letter boxing to fit the resolution to the display. OS relevant UI can overlay this application.",
            "Non-fullscreen window that can be moved and resized. Other applications and OS relevant UI can overlap this application's window",
        };

        private string OutputValidWindowModes()
        {
            string output = "";
            for (int i = 0; i < WINDOWED_MODE_NAMES.Length; i++)
            {
                output += "[" + i.ToString() + "] " + WINDOWED_MODE_NAMES[i] + ": " + " - " + WINDOWED_MODE_DESCRIPTIONS[i] + "\n";
            }
            return output;
        }

        private FullScreenMode ConvertInputIndexToFullScreenMode(int inputIndex)
        {
#if UNITY_STANDALONE_WIN
            switch (inputIndex)
            {
                case 0:
                    return FullScreenMode.ExclusiveFullScreen;
                case 1:
                    return FullScreenMode.FullScreenWindow;
                case 2:
                    return FullScreenMode.Windowed;
            }
#elif UNITY_STANDALONE_OSX
            switch (inputIndex)
            {
                case 0:
                    return FullScreenMode.ExclusiveFullScreen;
                case 1:
                    return FullScreenMode.FullScreenWindow;
                case 2:
                    return FullScreenMode.Windowed;
            }
#else
            switch (inputIndex)
            {
                case 0:
                    return FullScreenMode.FullScreenWindow;
                case 1:
                    return FullScreenMode.Windowed;
            }
#endif

            //Should not ever be reached
            return FullScreenMode.FullScreenWindow;
        }
    }
}
