using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Windows;
using System.Linq;

namespace Elanetic.Console.Unity.UI
{
    public class ConsoleInputField : TMP_InputField
    {
        /// <summary>
        /// How many entries of submissions should be kept track of.
        /// </summary>
        public int submissionHistoryAmount
        {
            get => m_SubmissionHistoryAmount;
            set
            {
                if(value < 0)
                {
                    throw new ArgumentOutOfRangeException("Submission Histry Amount property must be a value of 0 or more.");
                }

                m_SubmissionHistoryAmount = value;
                if(m_Submissions.Count > m_SubmissionHistoryAmount)
                {
                    m_Submissions.RemoveRange(m_SubmissionHistoryAmount, m_Submissions.Count - m_SubmissionHistoryAmount);
                }
            }
        }

        public int submissionCount => m_Submissions.Count;

        /// <summary>
        /// The current index of the submission. If it is -1 that means the input field is currently being edited by the user rather than showing the submission history
        /// </summary>
        public int currentSubmissionIndex { get; private set; } = -1;

        /// <summary>
        /// The current index of the suggestion. If it is -1 that means the input field is currently being edited by the user rather than showing the suggestions.
        /// </summary>
        public int currentSuggestionIndex { get; private set; } = -1;

        public string userText => m_UserInputString;

        /// <summary>
        /// Called whenever the up or down arrows are pressed or when the input field receives input while looking at the submission history.
        /// </summary>
        public Action onCycleSubmissions;
        /// <summary>
        /// Called whenever the up or down arrows are pressed or when the input field receives input while looking at the suggestions.
        /// </summary>
        public Action onCycleSuggestions;

        private string m_UserInputString;
        private List<string> m_Submissions = new List<string>(10);
        private int m_SubmissionHistoryAmount = 10;
        private List<string> m_CurrentSuggestions = new List<string>();

        private string[] m_AllSuggestions = new string[0];

        protected override void Awake()
        {
            base.Awake();

            onSubmit.AddListener(OnTextSubmit);
            onValueChanged.AddListener(OnTextChange);
        }

        void OnGUI()
        {
            if(!isFocused) return;

            Event currentEvent = Event.current;
            if(currentEvent.type == EventType.KeyDown)
            {

                EventModifiers currentEventModifiers = currentEvent.modifiers;
                bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
                bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
                bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
                if(!ctrl && !shift && !alt)
                {
                    if (currentEvent.keyCode == KeyCode.UpArrow)
                    {
                        if(currentSuggestionIndex >= 0)
                        {
                            CycleSuggestionsNext();
                        }
                        else if(currentSubmissionIndex >= 0 || string.IsNullOrWhiteSpace(text))
                        {
                            CycleSubmissionNext();
                        }
                        else
                        {
                            GetSuggestions(text, 0, 1, ref m_CurrentSuggestions);
                            //If suggestions exist cycle suggestions
                            if(m_CurrentSuggestions.Count > 0)
                            {
                                CycleSuggestionsNext();
                            }
                            else
                            {
                                CycleSubmissionNext();
                            }
                        }

                        Event.PopEvent(currentEvent);
                        currentEvent.Use();
                    }
                    else if (currentEvent.keyCode == KeyCode.DownArrow)
                    {
                        if(currentSuggestionIndex >= 0)
                        {
                            CycleSuggestionsPrevious();
                        }
                        else if(currentSubmissionIndex >= 0 || string.IsNullOrWhiteSpace(text))
                        {
                            CycleSubmissionPrevious();
                        }
                        else
                        {
                            GetSuggestions(text, 0, 1, ref m_CurrentSuggestions);
                            //If suggestions exist cycle suggestions
                            if(m_CurrentSuggestions.Count > 0)
                            {
                                CycleSuggestionsPrevious();
                            }
                            else
                            {
                                CycleSubmissionPrevious();
                            }
                        }

                        Event.PopEvent(currentEvent);
                        currentEvent.Use();
                    }
                    else if (currentEvent.keyCode == KeyCode.Tab)
                    {
                        if(currentSuggestionIndex >= 0)
                        {
                            CycleSuggestionsNext();
                        }
                        else if(currentSubmissionIndex < 0 && !string.IsNullOrWhiteSpace(text))
                        {
                            GetSuggestions(text, 0, 1, ref m_CurrentSuggestions);
                            //If suggestions exist cycle suggestions
                            if(m_CurrentSuggestions.Count > 0)
                            {
                                CycleSuggestionsNext();
                            }
                        }

                        Event.PopEvent(currentEvent);
                        currentEvent.Use();
                    }
                }
            }
        }

        private void CycleSubmissionNext()
        {
            if(currentSubmissionIndex >= m_Submissions.Count-1)
            {
                return;
            }

            currentSubmissionIndex++;

            string submissionText = m_Submissions[currentSubmissionIndex];
            SetTextWithoutNotify(submissionText);
            caretPosition = submissionText.Length;

            onCycleSubmissions?.Invoke();
        }

        private void CycleSubmissionPrevious()
        {
            if(currentSubmissionIndex == -1)
            {
                return;
            }

            currentSubmissionIndex--;
            if(currentSubmissionIndex == -1)
            {
                SetTextWithoutNotify(m_UserInputString);
                caretPosition = m_UserInputString.Length;
            }
            else
            {
                string submissionText = m_Submissions[currentSubmissionIndex];
                SetTextWithoutNotify(submissionText);
                caretPosition = submissionText.Length;
            }

            onCycleSubmissions?.Invoke();
        }

        private void CycleSuggestionsNext()
        {
            if(currentSuggestionIndex == -1)
            {
                GetSuggestions(text, currentSuggestionIndex + 1, 2, ref m_CurrentSuggestions);
            }
            else
            {
                GetSuggestions(m_UserInputString, currentSuggestionIndex + 1, 1, ref m_CurrentSuggestions);
            }
            if(m_CurrentSuggestions.Count == 0) return;

            currentSuggestionIndex++;

            string suggestionText = m_CurrentSuggestions[0];
            if(m_CurrentSuggestions.Count > 1 && text == suggestionText)
            {
                currentSuggestionIndex++;
                suggestionText = m_CurrentSuggestions[1];
            }

            SetTextWithoutNotify(suggestionText);
            caretPosition = suggestionText.Length;

            onCycleSuggestions?.Invoke();
        }

        private void CycleSuggestionsPrevious()
        {
            if(currentSuggestionIndex == -1)
                return;

            currentSuggestionIndex--;

            if(currentSuggestionIndex == -1)
            {
                SetTextWithoutNotify(m_UserInputString);
                caretPosition = m_UserInputString.Length;
            }
            else
            {
                GetSuggestions(m_UserInputString, currentSuggestionIndex, 1, ref m_CurrentSuggestions);

#if DEBUG
                if(m_CurrentSuggestions.Count == 0)
                    throw new InvalidOperationException("Only call CycleSuggestionNext if there are suggestions to show.");
#endif

                string suggestionText = m_CurrentSuggestions[0];
                SetTextWithoutNotify(suggestionText);
                caretPosition = suggestionText.Length;
            }

            onCycleSuggestions?.Invoke();
        }

        private void OnTextChange(string text)
        {
            m_UserInputString = text;

            if (currentSubmissionIndex != -1)
            {
                currentSubmissionIndex = -1;

                onCycleSubmissions?.Invoke();
            }
            else if(currentSuggestionIndex != -1)
            {
                currentSuggestionIndex = -1;

                onCycleSuggestions?.Invoke();
            }
        }

        private void OnTextSubmit(string text)
        {
            if(string.IsNullOrWhiteSpace(text)) return;

            if (m_Submissions.Count == 0 || m_Submissions[0] != text)
            {
                m_Submissions.Insert(0, text);
                if (m_Submissions.Count > m_SubmissionHistoryAmount)
                {
                    m_Submissions.RemoveAt(m_Submissions.Count - 1);
                }
            }
        }

        public string GetSubmission(int index)
        {
            return m_Submissions[index];
        }

        //If you plan to call Console.ReloadCommands then you will need to ensure that you call this again afterwards
        public void SetSuggestions(string[] suggestions)
        {
            m_AllSuggestions = suggestions;
        }

        private List<int> m_SkippedSuggestions = new List<int>();

        public void GetSuggestions(string text, int skipCount, int count, ref List<string> output)
        {
            if(output == null) 
                output = new List<string>();
            else 
                output.Clear();

            m_SkippedSuggestions.Clear();

            if(skipCount >= m_AllSuggestions.Length || count <= 0 || string.IsNullOrWhiteSpace(text)) return;

            string textLowered = text.ToLower();

            for(int i = 0; i < m_AllSuggestions.Length; i++)
            {
                string command = m_AllSuggestions[i];
                if(command.StartsWith(textLowered))
                {
                    if(skipCount > 0)
                    {
                        m_SkippedSuggestions.Add(i);
                        skipCount--;
                        continue;
                    }

                    output.Add(command);

                    if(output.Count == count)
                        return;
                }
            }


            if(textLowered.Length > 1) //Dont show commands that contain a single character
            {
                for(int i = 0; i < m_AllSuggestions.Length; i++)
                {
                    bool hasAlreadySkipped = false;
                    for(int h = 0; h < m_SkippedSuggestions.Count; h++)
                    {
                        if(m_SkippedSuggestions[h] == i)
                        {
                            hasAlreadySkipped = true;
                            break;
                        }
                    }

                    if(hasAlreadySkipped) continue;

                    string command = m_AllSuggestions[i];
                    if(output.Contains(command)) continue;
                    if(m_AllSuggestions[i].Contains(textLowered))
                    {
                        if(skipCount > 0)
                        {
                            skipCount--;
                            continue;
                        }

                        output.Add(command);
                        if(output.Count == count)
                            return;
                    }
                }
            }
        }
        
    }
}
