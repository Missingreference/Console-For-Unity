using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class ListGameObjectsCommand : UnityCommand
    {
        public override string name => "list_gameobjects";

        public override string helpMessage => "List all gameobjects in a specific scene.";

        public override void UnityExecute(params string[] args)
        {
            Scene targetScene;

            if(args.Length == 0) 
            {
                targetScene = SceneManager.GetActiveScene();
            }
            else if(int.TryParse(args[0], out int sceneIndex) && sceneIndex >= 0 && sceneIndex < SceneManager.sceneCount)
            {
                targetScene = SceneManager.GetSceneAt(sceneIndex);
            }
            else
            {
                targetScene = SceneManager.GetSceneByName(args[0]);
                if(!targetScene.IsValid())
                {
                    Console.Log("No scene with name '" + args[0] + "' is currently loaded.");
                    return;
                }
            }

            GameObject[] allGameObjects = targetScene.GetRootGameObjects();

            StringBuilder outputStringBuilder = new StringBuilder();

            for(int i = 0; i < allGameObjects.Length; i++)
            {
                PrintGameObject(outputStringBuilder, allGameObjects[i].transform, 0);
            }

            Console.Log(outputStringBuilder.ToString());
        }
        private void PrintGameObject(StringBuilder outputStringBuilder, Transform transform, int depth)
        {
            outputStringBuilder.Insert(outputStringBuilder.Length, "-", depth);
            outputStringBuilder.Append("[");
            outputStringBuilder.Append(transform.GetSiblingIndex());
            outputStringBuilder.Append("]");
            outputStringBuilder.Append(transform.name);
            outputStringBuilder.Append(Environment.NewLine);

            for(int i = 0; i < transform.childCount; i++)
            {
                PrintGameObject(outputStringBuilder, transform.GetChild(i), depth+1);
            }
        }
    }
}