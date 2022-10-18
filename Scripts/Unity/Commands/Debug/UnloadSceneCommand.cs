using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class UnloadSceneCommand : UnityCommand
    {
        public override string name => "unload";

        public override string helpMessage => "Unload a scene by name.";

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                OutputUsage("scene");
                return;
            }

            if(SceneManager.sceneCount <=1)
            {
                Console.Log("Cannot unload the scene since one scene remains. At least one scene must be loaded at all times.");
                return;
            }

            if(int.TryParse(args[0], out int sceneIndex) && sceneIndex >= 0 && sceneIndex < SceneManager.sceneCount)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(sceneIndex));
            }
            else if(!SceneManager.GetSceneByName(args[0]).IsValid())
            {
                Console.Log("No scene with name '" + args[0] + "' is currently loaded.");
                return;
            }
            else
            {
                SceneManager.UnloadSceneAsync(args[0]);
            }
        }
    }
}
