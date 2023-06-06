using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.Collections.LowLevel.Unsafe;

using Elanetic.UI.Unity;

namespace Elanetic.Console.Unity.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ConsoleUI : MonoBehaviour
    {

        public int currentCharacterCount { get; private set; }
        public int currentLineCount { get => outputTextArea.lineCount; }
        public float fontSize { get; private set; } = 12.0f;

        //Settings
        /// <summary>
        /// The maximum amount of characters the window will hold before deleting text to make room.
        /// </summary>
        public int maxCharacters
        {
            get => m_MaxCharacters;
            set
            {
                m_MaxCharacters = value;
            }
        }

        /// <summary>
        /// The amount of characters to delete to make room for new text to be appended. 
        /// If the newest entry to append is bigger than this value then that amount of text will be removed instead. This leads to a potential perfomance issue if chunkDeleteAmount value is too small.
        /// </summary>
        public int chunkDeleteAmount
        {
            get => m_ChunkDeleteAmount;
            set
            {
                if(value < 0)
                {
                    m_ChunkDeleteAmount = 0;
                }
                else
                {
                    m_ChunkDeleteAmount = value;
                }
            }
        }

        public int suggestionCount { get; set; } = 5;

        //UI References
        public RectTransform rectTransform => (RectTransform)transform;
        public TextMeshProUGUI titleText { get; private set; }
        public AdjustableWindow window { get; private set; }
        public Image inputWindow { get; private set; }
        public ConsoleInputField inputField { get; private set; }
        public RectTransform outputRect { get; private set; }
        public Image outputWindow { get; private set; }
        public TextArea outputTextArea { get; private set; }
        public Scrollbar scrollBar { get; private set; }
        public Image scrollBarBackground { get; private set; }
        public Button scrollUpButton { get; private set; }
        public Button scrollDownButton { get; private set; }
        public Button closeButton { get; private set; }
        public TextMeshProUGUI closeButtonText { get; private set; }
        public TextMeshProUGUI suggestionText { get; private set; }

        private int m_MaxCharacters = 25000;
        private int m_ChunkDeleteAmount = 2500;
        private float m_Spacing = 8.0f;
        private float m_ScrollbarWidth = 16.0f;

        void Awake()
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Console/Fonts/TMP_Ubuntu_Mono-Regular 12");

            //Set default size
            rectTransform.sizeDelta = new Vector2(Screen.width * 0.5f, Screen.height * 0.8f);

            //Set default position
            rectTransform.localPosition = new Vector2(Screen.width * 0.2f, Screen.height * 0.05f);


            //Background Window
            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            background.sprite = Resources.Load<Sprite>("Console/Rect_Light_Background_Dark_Border_Step_4");
            background.pixelsPerUnitMultiplier = 3.0f;
            background.type = Image.Type.Sliced;
            background.maskable = false;
            window = background.gameObject.AddComponent<AdjustableWindow>();
            window.topMoveSize = fontSize + m_Spacing + m_Spacing;

            //Title
            GameObject titleObject = new GameObject("Title");
            titleObject.transform.SetParent(transform);
            RectTransform titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1.0f);
            titleRect.anchorMax = new Vector2(1.0f, 1.0f);
            titleRect.pivot = new Vector2(0.5f, 1.0f);
            titleRect.offsetMin = new Vector2(m_Spacing , -fontSize - m_Spacing);
            titleRect.offsetMax = new Vector2(-m_Spacing, -m_Spacing);
            Canvas.ForceUpdateCanvases();
            //titleRect.anchoredPosition = new Vector3(titleRect.anchoredPosition.x, -6.5f);
            titleText = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
            titleText.text = "Console";
            titleText.font = font;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 1;
            titleText.fontSizeMax = fontSize;
            //titleText.fontSize = defaultFontSize;
            titleText.overflowMode = TextOverflowModes.Overflow;
            titleText.enableWordWrapping = false;
            titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            titleText.verticalAlignment = VerticalAlignmentOptions.Middle;
            titleText.raycastTarget = false;
            titleText.margin = new Vector4(m_Spacing * 0.5f, 0, 0, 0);

            //Output Area
            outputRect = new GameObject("Output Rect").AddComponent<RectTransform>();
            outputRect.SetParent(transform);
            outputRect.localScale = Vector3.one;
            outputRect.rotation = Quaternion.identity;
            outputRect.anchorMin = Vector2.zero;
            outputRect.anchorMax = Vector2.one;
            outputRect.offsetMin = new Vector2(m_Spacing, fontSize + (m_Spacing * 3.0f));
            outputRect.offsetMax = new Vector2(-m_Spacing, -m_Spacing - fontSize);


            //Output Window
            GameObject outputWindowObject = new GameObject("Output Window");
            outputWindowObject.transform.SetParent(outputRect);
            outputWindowObject.transform.localPosition = Vector3.zero;
            outputWindowObject.transform.localScale = Vector3.one;

            outputWindow = outputWindowObject.AddComponent<Image>();
            outputWindow.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            outputWindow.rectTransform.anchorMin = Vector2.zero;
            outputWindow.rectTransform.anchorMax = Vector2.one;
            outputWindow.rectTransform.offsetMin = Vector2.zero;
            outputWindow.rectTransform.offsetMax = new Vector2(-m_ScrollbarWidth, 0.0f);

            outputTextArea = outputWindowObject.AddComponent<TextArea>();
            outputTextArea.font = font;
            outputTextArea.fontSize = fontSize;
            outputTextArea.scrollBar = scrollBar;
            
            outputWindow.gameObject.AddComponent<RectMask2D>();
            outputTextArea.onTargetLineChanged += OnOutputWindowTargetLineChanged;


            //Scrollbar
            GameObject scrollBarObject = new GameObject("Scroll Bar");
            scrollBarObject.transform.SetParent(outputRect);
            scrollBarObject.transform.localScale = Vector3.one;
            scrollBarObject.transform.rotation = Quaternion.identity;
            scrollBarBackground = scrollBarObject.AddComponent<Image>();
            scrollBarBackground.color = new Color(0.235f, 0.235f, 0.235f, 1.0f);
            scrollBar = scrollBarObject.AddComponent<Scrollbar>();
            scrollBar.transition = Selectable.Transition.None;
            scrollBar.navigation = new Navigation();
            scrollBar.direction = Scrollbar.Direction.TopToBottom;
            scrollBarBackground.rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
            scrollBarBackground.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            scrollBarBackground.rectTransform.offsetMin = new Vector2(-m_ScrollbarWidth, m_ScrollbarWidth);//defaultFontSize + (m_Spacing * 3.0f));
            scrollBarBackground.rectTransform.offsetMax = new Vector2(0.0f, -m_ScrollbarWidth);// -m_Spacing - defaultFontSize);
            //scrollBarBackground.rectTransform.sizeDelta = new Vector2(m_ScrollbarWidth, scrollBarBackground.rectTransform.sizeDelta.y);
            //scrollBarBackground.rectTransform.anchoredPosition = new Vector2(-(scrollBarBackground.rectTransform.sizeDelta.x*0.5f) - m_Spacing, scrollBarBackground.rectTransform.anchoredPosition.y);

            RectTransform scrollArea = new GameObject("Scroll Area").AddComponent<RectTransform>();
            scrollArea.SetParent(scrollBarObject.transform);
            scrollArea.localScale = Vector3.one;
            scrollArea.rotation = Quaternion.identity;
            scrollArea.anchorMin = new Vector2(0, 0);
            scrollArea.anchorMax = new Vector2(1, 1);
            scrollArea.offsetMin = Vector2.zero;
            scrollArea.offsetMax = Vector2.zero;

            Image scrollHandle = new GameObject("Handle").AddComponent<Image>();
            scrollHandle.transform.SetParent(scrollArea);
            scrollHandle.transform.localScale = Vector3.one;
            scrollHandle.transform.localRotation = Quaternion.identity;
            scrollHandle.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
            scrollHandle.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            scrollHandle.rectTransform.offsetMin = Vector2.zero;
            scrollHandle.rectTransform.offsetMax = Vector2.zero;
            scrollHandle.color = new Color(0.2f,0.2f,0.2f,1.0f);

            scrollBar.handleRect = scrollHandle.rectTransform;
            scrollBar.size = 1.0f;

            scrollUpButton = new GameObject("Scroll Up Button").AddComponent<Button>();
            scrollUpButton.transform.SetParent(outputRect);
            scrollUpButton.transform.localScale = Vector3.one;
            scrollUpButton.transform.localRotation = Quaternion.identity;
            scrollUpButton.interactable = false;
            scrollUpButton.transition = Selectable.Transition.ColorTint;
            scrollUpButton.navigation = new Navigation();
            scrollUpButton.onClick.AddListener(OnScrollUpButtonPressed);
            Image scrollUpButtonImage = scrollUpButton.gameObject.AddComponent<Image>();
            scrollUpButtonImage.rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
            scrollUpButtonImage.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            scrollUpButtonImage.rectTransform.offsetMin = new Vector2(-m_ScrollbarWidth, -m_ScrollbarWidth);
            scrollUpButtonImage.rectTransform.offsetMax = Vector2.zero;
            scrollUpButton.image = scrollUpButtonImage;
            Color enabledColor = new Color(0.215f, 0.215f, 0.215f, 1.0f);
            Color disabledColor = new Color(0.255f, 0.255f, 0.255f, 1.0f);
            ColorBlock colorBlock = new ColorBlock() { fadeDuration = 0.025f, normalColor = enabledColor, disabledColor = disabledColor, highlightedColor = enabledColor, pressedColor = disabledColor, selectedColor = enabledColor, colorMultiplier = 1.0f };
            scrollUpButton.colors = colorBlock;


            scrollDownButton = new GameObject("Scroll Down Button").AddComponent<Button>();
            scrollDownButton.transform.SetParent(outputRect);
            scrollDownButton.transform.localScale = Vector3.one;
            scrollDownButton.transform.localRotation = Quaternion.identity;
            scrollDownButton.interactable = false;
            scrollDownButton.transition = Selectable.Transition.ColorTint;
            scrollDownButton.navigation = new Navigation();
            scrollDownButton.onClick.AddListener(OnScrollDownButtonPressed);
            Image scrollDownButtonImage = scrollDownButton.gameObject.AddComponent<Image>();
            scrollDownButtonImage.rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
            scrollDownButtonImage.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
            scrollDownButtonImage.rectTransform.offsetMin = new Vector2(-m_ScrollbarWidth, 0.0f);
            scrollDownButtonImage.rectTransform.offsetMax = new Vector2(0.0f, m_ScrollbarWidth);
            scrollDownButton.image = scrollDownButtonImage;
            scrollDownButton.colors = colorBlock;


            //Input Field
            GameObject inputWindowObject = new GameObject("Input Window");
            inputWindowObject.transform.SetParent(transform);
            inputWindowObject.transform.localPosition = Vector3.zero;
            inputWindowObject.transform.localScale = Vector3.one;

            inputWindow = inputWindowObject.AddComponent<Image>();
            inputWindow.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            inputWindow.rectTransform.anchorMin = Vector2.zero;
            inputWindow.rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
            inputWindow.rectTransform.offsetMin = new Vector2(m_Spacing, m_Spacing);
            inputWindow.rectTransform.offsetMax = new Vector2(-m_Spacing, fontSize + m_Spacing + m_Spacing);

            inputField = inputWindow.gameObject.AddComponent<ConsoleInputField>();
            GameObject textArea = new GameObject("Text Area");
            inputWindow.gameObject.AddComponent<RectMask2D>();
            textArea.transform.SetParent(inputField.transform);
            textArea.transform.localRotation = Quaternion.identity;
            textArea.transform.localScale = Vector3.one;
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(2.0f, 0.0f);
            textAreaRect.offsetMax = Vector2.zero;
            inputField.textViewport = textAreaRect;
            inputField.transition = Selectable.Transition.None;

            GameObject inputFieldTextObject = new GameObject("Input Text");
            inputFieldTextObject.transform.SetParent(textAreaRect);
            inputFieldTextObject.transform.localRotation = Quaternion.identity;
            inputFieldTextObject.transform.localScale = Vector3.one;
            TextMeshProUGUI inputFieldText = inputFieldTextObject.AddComponent<TextMeshProUGUI>();
            inputFieldText.rectTransform.anchorMin = Vector2.zero;
            inputFieldText.rectTransform.anchorMax = Vector2.one;
            inputFieldText.rectTransform.offsetMin = Vector2.zero;
            inputFieldText.rectTransform.offsetMax = Vector2.zero;
            inputFieldText.enableAutoSizing = false;
            inputFieldText.font = font;
            inputField.textComponent = inputFieldText;
            inputFieldText.richText = false;
            inputFieldText.fontSize = fontSize;
            inputFieldText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            inputFieldText.verticalAlignment = VerticalAlignmentOptions.Middle;
            inputFieldText.characterSpacing = 2.0f;
            inputFieldText.wordSpacing = 5.0f;
            inputField.pointSize = fontSize;

            inputField.onValueChanged.AddListener(OnInputValueChanged);
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.onCycleSubmissions += OnSubmissionCycle;
            inputField.onCycleSuggestions += OnSuggestionCycle;

            Image suggestionImage = new GameObject("Suggestion Window").AddComponent<Image>();
            suggestionImage.color = new Color(0.215f,0.215f, 0.215f, 1.0f);
            suggestionImage.transform.SetParent(inputField.transform);
            suggestionImage.transform.localScale = Vector3.one;
            suggestionImage.transform.localRotation = Quaternion.identity;
            suggestionImage.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            suggestionImage.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            suggestionImage.rectTransform.pivot = Vector2.zero;
            suggestionImage.rectTransform.offsetMin = Vector2.zero;
            suggestionImage.raycastTarget = false;
            suggestionImage.maskable = false;

            suggestionText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            suggestionText.transform.SetParent(suggestionImage.transform);
            suggestionText.transform.localScale = Vector3.one;
            suggestionText.transform.localRotation = Quaternion.identity;
            suggestionText.rectTransform.pivot = Vector2.zero;
            suggestionText.rectTransform.localPosition = Vector3.zero;
            suggestionText.font = font;
            suggestionText.fontSize = fontSize;
            suggestionText.color = new Color(0.85f, 0.85f, 0.85f, 1.0f);
            suggestionText.verticalAlignment = VerticalAlignmentOptions.Bottom;
            suggestionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            suggestionText.enableWordWrapping = false;
            suggestionText.overflowMode = TextOverflowModes.Overflow;
            suggestionText.rectTransform.anchorMin = Vector2.zero;
            suggestionText.rectTransform.anchorMax = Vector2.zero;
            suggestionText.rectTransform.offsetMin = Vector2.zero;
            suggestionText.raycastTarget = false;
            suggestionText.maskable = false;
            suggestionImage.gameObject.SetActive(false);


            closeButton = new GameObject("Close Button").AddComponent<Button>();
            closeButton.transform.SetParent(transform);
            closeButton.transform.localScale = Vector3.one;
            closeButton.transform.localRotation = Quaternion.identity;
            closeButton.transition = Selectable.Transition.None;
            Image closeButtonImage = closeButton.gameObject.AddComponent<Image>();
            closeButtonImage.color = new Color(0.235f, 0.235f, 0.235f, 0.5f);
            closeButton.onClick.AddListener(OnCloseButtonPressed);
            closeButtonImage.rectTransform.anchorMin = Vector2.one;
            closeButtonImage.rectTransform.anchorMax = Vector2.one;
            closeButtonImage.rectTransform.offsetMin = new Vector2(-fontSize - (m_Spacing * 1.5f), -fontSize - (m_Spacing * 0.5f));
            closeButtonImage.rectTransform.offsetMax = new Vector2(-m_Spacing * 1.5f, -m_Spacing*0.5f);
            closeButtonImage.rectTransform.sizeDelta = new Vector2(16.0f, 16.0f);

            closeButtonImage.sprite = null;

            closeButtonText = new GameObject("X Text").AddComponent<TextMeshProUGUI>();
            closeButtonText.text = "X";
            closeButtonText.font = font;
            closeButtonText.fontSize = fontSize;
            closeButtonText.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);
            closeButtonText.rectTransform.SetParent(closeButton.transform);
            closeButtonText.rectTransform.localScale = Vector3.one;
            closeButtonText.rectTransform.localRotation = Quaternion.identity;
            closeButtonText.rectTransform.anchorMin = Vector2.zero;
            closeButtonText.rectTransform.anchorMax = Vector2.one;
            closeButtonText.rectTransform.offsetMin = Vector2.zero;
            closeButtonText.rectTransform.offsetMax = Vector2.zero;
            closeButtonText.overflowMode = TextOverflowModes.Overflow;
            closeButtonText.enableWordWrapping = false;
            closeButtonText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            closeButtonText.verticalAlignment = VerticalAlignmentOptions.Middle;

            outputTextArea.scrollBar = scrollBar;

            window.minSize = new Vector2((m_Spacing * 2.0f) + m_ScrollbarWidth + (fontSize * 8.0f), (m_Spacing * 3.0f) + (m_ScrollbarWidth * 5.0f) + (fontSize * 3.0f));
        }

        void Start()
        {
            //By reenabling the input field it helps instantiate the caret object. For some reason not doing this and just instantiating the InputField does not create the caret.
            inputField.enabled = false;
            inputField.enabled = true;

            //If you plan to call Console.ReloadCommands then you will need to ensure that you call this again afterwards
            inputField.SetSuggestions(Console.GetAllCommands());
        }

        public void Clear()
        {
            currentCharacterCount = 0;
            outputTextArea.Clear();
            scrollBar.size = 1.0f;
        }

        /// <summary>
        /// Set the font size of the title, output window, and input field.
        /// </summary>
        private void SetFontSize(float size)
        {
            if(size >= 16.0f) //The title does not need to be so large to scale it back after a certian font size
            {
                float scale = 0.25f;
                float targetSize = (((Mathf.Min(((size - 16.0f) / 16.0f), 1.0f) * scale)) * size) + 16.0f;
                titleText.rectTransform.offsetMin = new Vector2(m_Spacing, -targetSize - m_Spacing);
                titleText.fontSizeMax = targetSize;
            }
            else
            {
                titleText.rectTransform.offsetMin = new Vector2(m_Spacing, -size - m_Spacing);
                titleText.fontSizeMax = size;
            }

            outputTextArea.fontSize = size;

            inputWindow.rectTransform.offsetMax = new Vector2(-m_Spacing, size + m_Spacing + m_Spacing);
            inputField.pointSize = size;
            suggestionText.fontSize = size;
            if(suggestionText.transform.parent.gameObject.activeInHierarchy)
            {
                suggestionText.rectTransform.offsetMax = suggestionText.GetPreferredValues();
                ((RectTransform)suggestionText.transform.parent).offsetMax = suggestionText.rectTransform.offsetMax;
            }

            outputRect.offsetMin = new Vector2(m_Spacing, size + (m_Spacing * 3.0f));
            outputRect.offsetMax = new Vector2(-m_Spacing, -m_Spacing - titleText.fontSizeMax);

            window.topMoveSize = size + m_Spacing + m_Spacing;

            ((RectTransform)closeButton.transform).offsetMin = new Vector2(-titleText.fontSizeMax - (m_Spacing * 1.5f), -titleText.fontSizeMax - (m_Spacing * 0.5f));
            closeButtonText.fontSize = titleText.fontSizeMax;

            int maxLines = 256;
            float perc = 1.0f - ((float)(outputTextArea.lineCount - outputTextArea.maxVisibleLineCount) / (float)maxLines);
            scrollBar.size = Mathf.Clamp(perc, 0.1f, 1.0f);

            Canvas canvas = GetComponentInParent<Canvas>(true).rootCanvas;
            float canvasWidth = ((RectTransform)canvas.transform).rect.width;
            float canvasHeight = ((RectTransform)canvas.transform).rect.height;

            window.minSize = new Vector2(Mathf.Min((m_Spacing * 2.0f) + m_ScrollbarWidth + (size * 8.0f), canvasWidth), Mathf.Min((m_Spacing * 3.0f) + (m_ScrollbarWidth * 5.0f) + (size * 3.0f), canvasHeight));

            fontSize = size;
        }

        public void OutputText(char[] charData, Color32[] colorData, int length)
        {

            if(outputTextArea.lastLineIndex - outputTextArea.targetLineIndex <= 4)
            {
                outputTextArea.autoScrollToBottom = true;
            }
            else
            {
                outputTextArea.autoScrollToBottom = false;
            }


            currentCharacterCount += length;
            if(currentCharacterCount > maxCharacters)
            {
                int amountToRemove = currentCharacterCount - maxCharacters;
                if(amountToRemove < chunkDeleteAmount)
                {
                    amountToRemove = chunkDeleteAmount;
                }
                if(amountToRemove > currentCharacterCount - length)
                {
                    amountToRemove = currentCharacterCount - length;
                }
                outputTextArea.RemoveText(0, amountToRemove);
                currentCharacterCount -= amountToRemove;
            }

            outputTextArea.Append(charData, colorData, 0, length);

            int maxLines = 256;
            float perc = 1.0f - ((float)(outputTextArea.lineCount - outputTextArea.maxVisibleLineCount) / (float)maxLines);
            scrollBar.size = Mathf.Clamp(perc, 0.1f, 1.0f);
        }

        private void OutputText(string message, Color color)
        {
            currentCharacterCount += message.Length;
            if(currentCharacterCount > maxCharacters)
            {
                int amountToRemove = currentCharacterCount - maxCharacters;
                if(amountToRemove < chunkDeleteAmount)
                {
                    amountToRemove = chunkDeleteAmount;
                }
                if(amountToRemove > currentCharacterCount - message.Length)
                {
                    amountToRemove = currentCharacterCount - message.Length;
                }
                outputTextArea.RemoveText(0, amountToRemove);
                currentCharacterCount -= amountToRemove;
            }

            outputTextArea.Append(message, color);

            int maxLines = 256;
            float perc = 1.0f - ((float)(outputTextArea.lineCount - outputTextArea.maxVisibleLineCount) / (float)maxLines);
            scrollBar.size = Mathf.Clamp(perc, 0.1f, 1.0f);
        }

        private void OnInputValueChanged(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                HideSuggestions();
            }
            else
            {
                ShowSuggestions();
            }
        }

        private void OnSubmissionCycle()
        {
            if (inputField.currentSubmissionIndex == -1 && !string.IsNullOrWhiteSpace(inputField.text))
            {
                ShowSuggestions();
            }
            else
            {
                HideSuggestions();
            }
        }

        private void OnSuggestionCycle()
        {
            if(!string.IsNullOrWhiteSpace(inputField.text))
            {
                ShowSuggestions();
            }
            else
            {
                HideSuggestions();
            }
        }

        private List<string> m_SuggestionList = new List<string>(5);
        private string m_UserString;

        private void ShowSuggestions()
        {
            string inputText = inputField.userText;

            inputField.GetSuggestions(inputText, inputField.currentSuggestionIndex, suggestionCount, ref m_SuggestionList);
            
            if(inputField.currentSuggestionIndex > 0 && m_SuggestionList.Count < suggestionCount)
            {
                inputField.GetSuggestions(inputText, Mathf.Max(0, inputField.currentSuggestionIndex - (suggestionCount - m_SuggestionList.Count)), suggestionCount, ref m_SuggestionList);
            }

            if (m_SuggestionList.Count > 0)
            {
                StringBuilder sBuilder = new StringBuilder();
                for (int i = m_SuggestionList.Count-1; i > 0; i--)
                {
                    sBuilder.Append(m_SuggestionList[i]);
                    sBuilder.Append('\n');
                }
                sBuilder.Append(m_SuggestionList[0]);

                suggestionText.text = sBuilder.ToString();
                suggestionText.rectTransform.offsetMax = suggestionText.GetPreferredValues();
                ((RectTransform)suggestionText.transform.parent).offsetMax = suggestionText.rectTransform.offsetMax;
                suggestionText.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                suggestionText.transform.parent.gameObject.SetActive(false);
            }

        }

        private void HideSuggestions()
        {
            suggestionText.transform.parent.gameObject.SetActive(false);
            m_SuggestionList.Clear();
        }

        private void OnSubmit(string message)
        {
            string trimmedMessage = message.Trim();
            outputTextArea.autoScrollToBottom = true;
            OutputText("] " + trimmedMessage + Environment.NewLine, new Color32(200,200,200,255));
            Console.Parse(message);

            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }

        private void OnScrollUpButtonPressed()
        {
            if(outputTextArea.targetLineIndex == 0) return;
            outputTextArea.ScrollUp(1);
            if(outputTextArea.targetLineIndex == 0)
            {
                scrollUpButton.interactable = false;
                
            }
            scrollDownButton.interactable = true;
        }

        private void OnScrollDownButtonPressed()
        {
            if (outputTextArea.targetLineIndex == outputTextArea.lastLineIndex) return;
            outputTextArea.ScrollDown(1);
            if(outputTextArea.targetLineIndex == outputTextArea.lastLineIndex)
            {
                scrollDownButton.interactable = false;
            }
            scrollUpButton.interactable = true;
        }

        private void OnCloseButtonPressed()
        {
            gameObject.SetActive(false);
        }

        private void OnOutputWindowTargetLineChanged()
        {
            scrollUpButton.interactable = outputTextArea.targetLineIndex != 0;
            scrollDownButton.interactable = outputTextArea.targetLineIndex != outputTextArea.lastLineIndex;
        }
    }
}
