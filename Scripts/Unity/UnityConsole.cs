using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

using Elanetic.Console.Unity.UI;

namespace Elanetic.Console.Unity
{
    /// <summary>
    /// The main class to manage Console class execution and ensure any Unity specific commands are executed on Unity's main thread.
    /// </summary>
    static public class UnityConsole
    {
        static public ConsoleUI consoleUI
        {
            get
            {
                if(m_ConsoleUI == null)
                {
                    CreateConsoleUI();
                }
                return m_ConsoleUI;
            }
        }
        static public bool allowCheatCommandExecution
        {
            get => m_AllowCheatCommandExecution;
            set
            {
                if(m_AllowCheatCommandExecution != value)
                {
                    if(m_AllowCheatCommandExecution)
                    {
                        onCheatsEnabled?.Invoke();
                    }
                    m_AllowCheatCommandExecution = value;
                }
            }
        }
        static public Action onCheatsEnabled;

        static private ConsoleUI m_ConsoleUI;
        static private GameObject m_EventSystemGameObject;
        static private bool m_AllowCheatCommandExecution = false;
        static private ConcurrentQueue<QueuedCommand> m_CommandQueue = new ConcurrentQueue<QueuedCommand>();
        private struct QueuedCommand
        {
            public UnityCommand command;
            public string[] args;
        }

        static UnityConsole()
        {
#if UNITY_EDITOR
            m_AllowCheatCommandExecution = true;
#endif
            //Called on Unity's main thread
            Application.logMessageReceived += OnUnityLogReceived;
            //Can be called from any thread
            //Application.logMessageReceivedThreaded += OnUnityLogReceived;
        }

        static public void QueueCommand(UnityCommand command, params string[] args)
        {
            if(m_ConsoleCommandExecutor == null)
            {
                m_ConsoleCommandExecutor = new GameObject("Console Executor").AddComponent<UnityCommandExecutor>();
                m_ConsoleCommandExecutor.gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                GameObject.DontDestroyOnLoad(m_ConsoleCommandExecutor.gameObject);
            }
            m_ConsoleCommandExecutor.gameObject.SetActive(true);
            m_ConsoleCommandExecutor.enabled = true;
            m_CommandQueue.Enqueue(new QueuedCommand() { command = command, args = args });
        }

        /// <summary>
        /// Create a coroutine on the console executor.
        /// </summary>
        static public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (m_ConsoleCommandExecutor == null)
            {
                m_ConsoleCommandExecutor = new GameObject("Console Executor").AddComponent<UnityCommandExecutor>();
                m_ConsoleCommandExecutor.gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                GameObject.DontDestroyOnLoad(m_ConsoleCommandExecutor.gameObject);
            }

            m_ConsoleCommandExecutor.gameObject.SetActive(true);

            return m_ConsoleCommandExecutor.StartCoroutine(routine);
        }

        /// <summary>
        /// Stop a coroutine started by the console executor.
        /// </summary>
        /// <param name="coroutine"></param>
        static public void StopCoroutine(Coroutine coroutine)
        {
            if(m_ConsoleCommandExecutor != null)
            {
                m_ConsoleCommandExecutor.StopCoroutine(coroutine);
            }
        }

        static private void ExecuteCommands()
        {
            while(m_CommandQueue.TryDequeue(out QueuedCommand queuedCommand))
            {
                try
                {
                    queuedCommand.command.UnityExecute(queuedCommand.args);
                }
                catch (Exception ex)
                {
                    Console.LogError("Error occurred while executing command '" + queuedCommand.command.name + "': " + ex.GetType().Name + ": " + ex.Message);
                }
            }
        }


        static public void Show()
        {
            Cursor.visible = true;
            if(m_ConsoleUI == null)
            {
                CreateConsoleUI();
            }
            if(EventSystem.current == null)
            {
                EventSystem eventSystem = new GameObject("Event System").AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                GameObject.DontDestroyOnLoad(eventSystem.gameObject);
            }

            SceneManager.sceneUnloaded += OnSceneUnload;

            m_ConsoleUI.gameObject.SetActive(true);

            m_ConsoleUI.inputField.Select();
            m_ConsoleUI.inputField.ActivateInputField();
        }

        static public void Hide()
        {
            if(m_ConsoleUI != null)
            {
                m_ConsoleUI.gameObject.SetActive(false);
            }

            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        static private void CreateConsoleUI()
        {
            GameObject canvasObject = new GameObject("Console Canvas");
            GameObject.DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
#if UNITY_STANDALONE_OSX
            canvasScaler.scaleFactor = 2.0f;
#endif
            GraphicRaycaster graphicRaycaster = canvasObject.AddComponent<GraphicRaycaster>();
            canvas.pixelPerfect = true;
            canvas.sortingOrder = short.MaxValue;

            GameObject consoleUIObject = new GameObject("Console");
            consoleUIObject.transform.SetParent(canvasObject.transform);
            m_ConsoleUI = consoleUIObject.AddComponent<ConsoleUI>();

            //We create this at this point so later we don't get errors when creating gameobjects during OnSceneUnload
            m_EventSystemGameObject = new GameObject("Event System");
            GameObject.DontDestroyOnLoad(m_EventSystemGameObject);
        }

        static private void OnSceneUnload(Scene scene)
        {
            if(EventSystem.current == null)
            {
                //An event system must always exist so that we can interact with the Console UI
                EventSystem eventSystem = m_EventSystemGameObject.AddComponent<EventSystem>();
                m_EventSystemGameObject.AddComponent<StandaloneInputModule>();
            }
        }

        static private void OnUnityLogReceived(string condition, string stackTrace, LogType logType)
        {
            switch(logType)
            {
                case LogType.Log:
                    Console.Log(condition);
                    break;
                case LogType.Warning:
                    Console.LogWarning(condition);
                    break;
                case LogType.Error:
                    Console.LogError(condition);
                    break;
                case LogType.Exception:
                    Console.LogError(condition);
                    break;
                case LogType.Assert:
                    Console.LogError(condition);
                    break;
            }
        }

        static private UnityCommandExecutor m_ConsoleCommandExecutor = null;

        //Handle command execution here
        [DefaultExecutionOrder(-9999)]
        private class UnityCommandExecutor : MonoBehaviour
        {

            private void Update()
            {
                
                ExecuteCommands();

                //Disable so that we removed the overhead of calling update every frame
                enabled = false;
            }
        }
    }
}
