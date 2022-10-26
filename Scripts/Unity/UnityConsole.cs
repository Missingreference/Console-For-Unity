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
using static Elanetic.Console.Unity.UI.ConsoleUI;
using Unity.Collections.LowLevel.Unsafe;

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

        static private object m_LockObject = new object();
        static private Queue<ConsoleLogEntry> m_PrimaryLogQueue { get; set; }
        static private Queue<ConsoleLogEntry> m_SecondaryLogQueue { get; set; }
        static private int m_DataLength = 0;
        static private char[] m_CharData;
        static private Color32[] m_ColorData;

        private struct ConsoleLogEntry
        {
            public string message;
            public Color32 color;
        }

        static UnityConsole()
        {
#if UNITY_EDITOR
            m_AllowCheatCommandExecution = true;
#endif

            Console.onOutputLog += OnConsoleLog;
            Console.onOutputWarning += OnConsoleWarning;
            Console.onOutputError += OnConsoleError;

            //Can be called from any thread
            Application.logMessageReceivedThreaded += OnUnityLogReceived;

            m_PrimaryLogQueue = new Queue<ConsoleLogEntry>();
            m_SecondaryLogQueue = new Queue<ConsoleLogEntry>();
            m_CharData = new char[25000];
            m_ColorData = new Color32[25000];

            Debug.Log("Static call");
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


            if(m_ConsoleCommandExecutor == null)
            {
                m_ConsoleCommandExecutor = new GameObject("Console Executor").AddComponent<UnityCommandExecutor>();
                m_ConsoleCommandExecutor.gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
                GameObject.DontDestroyOnLoad(m_ConsoleCommandExecutor.gameObject);

                lock(m_LockObject)
                {
                    if(m_PrimaryLogQueue.Count == 0)
                    {
                        m_ConsoleCommandExecutor.enabled = false;
                    }
                }
                m_ConsoleCommandExecutor.enabled = true;
            }
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

        static private void OnConsoleLog(string message)
        {
            lock(m_LockObject)
            {
                m_PrimaryLogQueue.Enqueue(new ConsoleLogEntry() { message = message, color = Color.white });
            }
        }

        static private void OnConsoleWarning(string message)
        {
            lock(m_LockObject)
            {
                m_PrimaryLogQueue.Enqueue(new ConsoleLogEntry() { message = message, color = Color.yellow });
            }
        }

        static private void OnConsoleError(string message)
        {
            lock(m_LockObject)
            {
                m_PrimaryLogQueue.Enqueue(new ConsoleLogEntry() { message = message, color = Color.red });
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

        static private void AppendEntry(string message, Color32 color)
        {
            unsafe
            {
                int m_MaxCharacters = 25000;
                int messageLength = message.Length;
                if(messageLength >= m_MaxCharacters)
                {
                    fixed(char* dest = &m_CharData[0])
                    fixed(char* src = message)
                        UnsafeUtility.MemCpy(dest, src + (messageLength - m_MaxCharacters), m_MaxCharacters * 2);

                    fixed(Color32* dest = &m_ColorData[0])
                        UnsafeUtility.MemCpyReplicate(dest, &color, 4, m_MaxCharacters);

                    m_DataLength = m_MaxCharacters;
                }
                else if(m_DataLength + messageLength > m_MaxCharacters)
                {
                    int srcIndex = messageLength - (m_MaxCharacters - m_DataLength);
                    //Move data
                    fixed(char* dest = &m_CharData[0])
                    fixed(char* src = &m_CharData[srcIndex])
                        UnsafeUtility.MemMove(dest, src, (m_MaxCharacters - srcIndex) * 2);

                    fixed(Color32* dest = &m_ColorData[0])
                    fixed(Color32* src = &m_ColorData[srcIndex])
                        UnsafeUtility.MemMove(dest, src, (m_MaxCharacters - srcIndex) * 4);

                    int destIndex = m_MaxCharacters - messageLength;
                    fixed(char* src = message)
                    fixed(char* dest = &m_CharData[destIndex])
                        UnsafeUtility.MemCpy(dest, src, messageLength * 2);

                    fixed(Color32* dest = &m_ColorData[destIndex])
                        UnsafeUtility.MemCpyReplicate(dest, &color, 4, messageLength);

                    m_DataLength = m_MaxCharacters;
                }
                else
                {
                    fixed(char* src = message)
                    fixed(char* dest = &m_CharData[m_DataLength])
                        UnsafeUtility.MemCpy(dest, src, messageLength * 2);

                    fixed(Color32* dest = &m_ColorData[m_DataLength])
                        UnsafeUtility.MemCpyReplicate(dest, &color, 4, messageLength);

                    m_DataLength += messageLength;
                }

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

                lock(m_LockObject)
                {
                    if(m_PrimaryLogQueue.Count == 0) return;

                    Queue<ConsoleLogEntry> hold = m_PrimaryLogQueue;
                    m_PrimaryLogQueue = m_SecondaryLogQueue;
                    m_SecondaryLogQueue = hold;
                }

                if(consoleUI != null)
                {
                    while(m_SecondaryLogQueue.TryDequeue(out ConsoleLogEntry entry))
                    {
                        AppendEntry(entry.message, entry.color);
                    }

                    if(m_DataLength > 0)
                    {
                        consoleUI.OutputText(m_CharData, m_ColorData, m_DataLength);
                        m_DataLength = 0;
                    }
                }

                //Disable so that we removed the overhead of calling update every frame
                //enabled = false;
            }
        }
    }
}
