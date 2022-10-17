using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Elanetic.Console.Unity.Commands
{
    public class CreateGameObjectCommand : UnityCommand
    {
        public override string name => "create_gameobject";

        public override string helpMessage => "Create a new gameobject with a name. Optionally set the position.";

        public override bool isCheatCommand => true;

        public override void UnityExecute(params string[] args)
        {
            if(args.Length == 0)
            {
                new GameObject();
                return;
            }

            GameObject gameObject = new GameObject(args[0]);
        }
    }
}
