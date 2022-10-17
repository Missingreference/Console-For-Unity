using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class CreateSceneCommand : UnityCommand
    {
        public override string name => "create_scene";

        public override string helpMessage => "Create a new scene with name. Optionally create the scene with a local physics mode. 0 being a normal scene, 1 being a 2D local physics world and 2 being a 3D local physics world.";

        public override bool isCheatCommand => true;

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                OutputUsage("name");
                Console.Log("or");
                OutputUsage("name", "local physics mode");
                return;
            }


            if (SceneManager.GetSceneByName(args[0]).IsValid())
            {
                Console.Log("Failed to create scene. A scene with the name '" + args[0] + "' is already loaded.");
                return;
            }

            if (args.Length == 2)
            {
                if (ArgumentIsWithinValidRange(args[1], 0, 2, out int localPhysicsMode))
                {
                    Console.Log("Second argument 'local physics mode' must be a number from 0 to 2. 0 being a normal scene, 1 being a 2D local physics world and 2 being a 3D local physics world.");
                    return;
                }

                SceneManager.CreateScene(args[0], new CreateSceneParameters((LocalPhysicsMode)localPhysicsMode));
            }
            else
            {
                SceneManager.CreateScene(args[0]);
            }
        }
    }
}
