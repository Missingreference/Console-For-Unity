using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class LoadSceneCommand : UnityCommand
    {
        public override string name => "load";

        public override string helpMessage => "Load a scene by index or by name. First argument can be a scene index or a scene name. The second argument is either true or false for whether the scene will be loaded additively or not. The third argument can optionally create the scene with a local physics mode. 0 being a normal scene, 1 being a 2D local physics world and 2 being a 3D local physics world.";

        public override void UnityExecute(params string[] args)
        {
            
            if (args.Length == 0)
            {
                OutputUsage("name");
                Console.Log("or");
                OutputUsage("name", "is additive");
                Console.Log("or");
                OutputUsage("name", "is additive", "local physics mode");
                return;
            }

            LoadSceneMode loadSceneMode = LoadSceneMode.Single;
            int localPhysicsMode = 0;
            if (args.Length >= 2)
            {
                loadSceneMode = ArgumentIsTrue(args[1]) ? LoadSceneMode.Additive : LoadSceneMode.Single;
                if(args.Length >= 3)
                {
                    if(!ArgumentIsWithinValidRange(args[2], 0, 2, out localPhysicsMode))
                    {
                        Console.Log("Third argument 'local physics mode' must be a number from 0 to 2. 0 being a normal scene, 1 being a 2D local physics world and 2 being a 3D local physics world.");
                        return;
                    }
                }
            }


            if (!int.TryParse(args[0], out int sceneIndex) || !SceneManager.GetSceneByBuildIndex(sceneIndex).IsValid())
            {
                if (!SceneExistsInBuild(args[0]))
                {
                    if (int.TryParse(args[0], out _))
                    {
                        Console.Log("No scene exists with name or index '" + args[0] + "'.");
                    }
                    else
                    {
                        Console.Log("No scene exists with name '" + args[0] + "'.");
                    }
                    return;
                }

                Console.Log("Loading scene with name '" + args[0] + "'...");
                SceneManager.LoadScene(args[0], new LoadSceneParameters(loadSceneMode, (LocalPhysicsMode)localPhysicsMode));
            }
            else
            {
                Console.Log("Loading scene with index '" + sceneIndex + "'...");
                SceneManager.LoadScene(sceneIndex, new LoadSceneParameters(loadSceneMode, (LocalPhysicsMode)localPhysicsMode));
            }
        }

        private bool SceneExistsInBuild(string sceneName)
        {
            int builtInSceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < builtInSceneCount; i++)
            {
                if(SceneManager.GetSceneByBuildIndex(i).name == sceneName)
                    return true;
            }
            return false;
        }
    }
}
