using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elanetic.Console.Unity.Commands
{
    public class ListScenesCommand : UnityCommand
    {
        public override string name => "list_scenes";

        public override string helpMessage => "List all scenes currently loaded and all scenes included in the build settings.";

        public override void UnityExecute(params string[] args)
        {
            int sceneCount = SceneManager.sceneCount;
            string output;
            if(sceneCount == 1)
            {
                output = "Currently " + sceneCount.ToString() + " Loaded Scene: \n";
            }
            else
            {
                output = "Currently " + sceneCount.ToString() + " Loaded Scenes: \n";
            }
            
            for (int i = 0; i < sceneCount; i++)
            {
                output += "[" + i.ToString() + "] " + SceneManager.GetSceneAt(i).name + "\n";
            }

            output += "\n";

            sceneCount = SceneManager.sceneCountInBuildSettings;

            if(sceneCount == 1)
            {
                output += sceneCount.ToString() + " Built-In Scene: \n";
            }
            else
            {
                output += sceneCount.ToString() + " Built-In Scenes: \n";
            }

            for (int i = 0; i < sceneCount; i++)
            {
                output += "[" + i.ToString() + "] " + SceneManager.GetSceneByBuildIndex(i).name + "\n";
            }

            Console.Log(output);
        }
    }
}
