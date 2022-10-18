using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
                Scene scene = SceneManager.GetSceneAt(i);
                output += "[" + i.ToString() + "] " + scene.name + "\n";
                //output += "[" + i.ToString() + "](" + scene.buildIndex + ") '" + scene.name + "' Path: " + scene.path + " Handle: " + scene.handle + " Is SubScene: " + scene.isSubScene + " Is Valid: " + scene.IsValid() + " Is Loaded: " + scene.isLoaded + " Is Dirty: " + scene.isDirty + "\n";
            }

            output += "\n";

            //Output Built-In Scenes
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
                string sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

                output += "[" + i.ToString() + "] " + sceneName + "\n";
            }

            Console.Log(output);
        }
    }
}
