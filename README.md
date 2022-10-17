# Console-For-Unity
Implementation of Console repository for Unity.

### Installation
To simply add Console For Unity to your Unity project as a package:

-In the Unity Editor open up the Package Manager by going to Window -> Package Manager.

-At the top left of the Package Manager window press the plus button and press 'Add package from git URL' or similar.

-Submit ```https://github.com/Missingreference/Console-For-Unity.git``` as the URL and the Package Manager should add the package to your project.

### Required Packages
Be aware that this repository requires multiple package dependancies to function. Add them as packages just like the Console For Unity package.

**UGUI Extras** - ```https://github.com/Missingreference/UGUI-Extras.git```

A collection of UGUI components and tools to help build the Console UI.

**Core Tools For Unity** - ```https://github.com/Missingreference/Core-Tools-For-Unity.git```

A utility package. It includes native cursor icon functions and helper functions.

*com.unity.ugui* and *com.unity.textmeshpro* are required dependencies that should be added automatically.

Use DEBUG mode to enable safety checks for most functions.


### Notes
This implementation uses Unity's UGUI and TextMeshPro to create the UI of the ConsoleUI. This UI will color the text based on the type of log outputted from the console. The UI was created to look and function very similar to Valve's Source engine developer console(https://developer.valvesoftware.com/wiki/Developer_Console) including an adjustable window and performant TextArea that shows the outputted text. When calling UnityEngine.Debug.Log and other associated logging functions they are outputted to this Console class by this implementation hooking into the UnityEngine.Application.logMessageReceived event. Any calls to Console.Log and associated logging functions do not output to Unity's editor console window/log files.

UnityCommand as an abstract class will execute the commands during an early execution order point during the Update loop that way commands are executed on Unity's main thread and at a predictable time of the execution of the game. Since commands like 'clear' and 'quit' cannot be implemented in the core of the Console, 'clear' will remove all text from the TextArea and 'quit' will call UnityEngine.Application.Quit() implemented each as a UnityCommand.

Assets are included for the background window and an open source mono-spaced font called Ubuntu Mono(https://fonts.google.com/specimen/Ubuntu+Mono).

All UI is created from script rather than a prefab. Check ConsoleUI class for the implementation.