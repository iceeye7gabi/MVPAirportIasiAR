using AirportAR.AR;
using AirportAR.Chatbot;
using AirportAR.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AirportAR
{
    /// <summary>
    /// Builds the simplified demo UI at runtime: menu, camera discover, FAQ, about.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DemoAppBootstrap : MonoBehaviour
    {
        AppController appController;
        Font defaultFont;

        void Awake()
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            EnsureCoreServices();
            BuildUi();
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }
        }

        void EnsureCoreServices()
        {
            if (FindObjectOfType<AudioListener>() == null)
            {
                var audioGo = new GameObject("AudioListener");
                audioGo.AddComponent<AudioListener>();
                DontDestroyOnLoad(audioGo);
            }

            if (MobileARSessionBootstrap.Instance == null)
            {
                new GameObject("MobileARSession", typeof(MobileARSessionBootstrap));
            }
        }

        void BuildUi()
        {
            var canvasGo = new GameObject("DemoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            appController = canvasGo.AddComponent<AppController>();

            GameObject mainMenu = CreatePanel(canvasGo.transform, "MainMenuPanel", UiTheme.BrandDark);
            GameObject discover = CreateTransparentPanel(canvasGo.transform, "DiscoverPanel");
            GameObject faq = CreatePanel(canvasGo.transform, "FaqPanel", UiTheme.PanelLight);
            GameObject about = CreatePanel(canvasGo.transform, "AboutPanel", UiTheme.PanelLight);

            BuildMainMenu(mainMenu.transform);
            BuildDiscoverPanel(discover.transform);
            BuildFaqPanel(faq.transform);
            BuildAboutPanel(about.transform);

            AssignPanels(mainMenu, discover, faq, about);
        }

        void AssignPanels(GameObject mainMenu, GameObject discover, GameObject faq, GameObject about)
        {
            SetField("mainMenuPanel", mainMenu);
            SetField("discoverPanel", discover);
            SetField("faqPanel", faq);
            SetField("aboutPanel", about);

            void SetField(string name, GameObject value)
            {
                var field = typeof(AppController).GetField(name,
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(appController, value);
            }
        }

        GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = bgColor;
            return panel;
        }

        void BuildMainMenu(Transform parent)
        {
            UiTheme.AddMainMenuLogo(parent, defaultFont);

            var subtitle = UiTheme.CreateText(parent, "Subtitle", defaultFont, 28, TextAnchor.UpperCenter, UiTheme.TextOnDark);
            StretchTop(subtitle.rectTransform, 280f, 120f, 60f, 60f);
            subtitle.text = "Explorează aeroportul, pune întrebări și află mai multe despre această aplicație demo.";

            var buttonColumn = new GameObject("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buttonColumn.transform.SetParent(parent, false);
            var columnRect = buttonColumn.GetComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(0.5f, 0.5f);
            columnRect.anchorMax = new Vector2(0.5f, 0.5f);
            columnRect.pivot = new Vector2(0.5f, 0.5f);
            columnRect.anchoredPosition = new Vector2(0f, -160f);
            columnRect.sizeDelta = new Vector2(920f, 360f);

            var layout = buttonColumn.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            UiTheme.CreatePrimaryButton(buttonColumn.transform, defaultFont, "Descoperă aeroportul", appController.ShowDiscover);
            UiTheme.CreatePrimaryButton(buttonColumn.transform, defaultFont, "Întrebări și răspunsuri", appController.ShowFaq);
            UiTheme.CreatePrimaryButton(buttonColumn.transform, defaultFont, "Despre aplicație", appController.ShowAbout);
        }

        GameObject CreateTransparentPanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            StretchFull(panel.GetComponent<RectTransform>());
            return panel;
        }

        void BuildDiscoverPanel(Transform parent)
        {
            var previewGo = new GameObject("CameraPreview", typeof(RectTransform), typeof(RawImage));
            previewGo.transform.SetParent(parent, false);
            previewGo.transform.SetAsFirstSibling();
            StretchFull(previewGo.GetComponent<RectTransform>());
            var rawImage = previewGo.GetComponent<RawImage>();
            rawImage.color = Color.black;
            rawImage.raycastTarget = false;

            var preview = parent.gameObject.AddComponent<SimpleCameraPreview>();
            SetPrivateField(preview, "previewImage", rawImage);

            var voiceAssistant = parent.gameObject.AddComponent<DiscoverVoiceAssistant>();
            var directionOverlay = parent.gameObject.AddComponent<DiscoverDirectionOverlay>();

            var tapArea = new GameObject("TapArea", typeof(RectTransform), typeof(Image), typeof(Button));
            tapArea.transform.SetParent(parent, false);
            StretchFull(tapArea.GetComponent<RectTransform>());
            var tapImage = tapArea.GetComponent<Image>();
            tapImage.color = new Color(0f, 0f, 0f, 0.01f);
            tapArea.GetComponent<Button>().onClick.AddListener(voiceAssistant.OnScreenTapped);

            var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
            topBar.transform.SetParent(parent, false);
            StretchTop(topBar.GetComponent<RectTransform>(), 0f, 190f, 0f, 0f);
            UiTheme.ApplyRounded(topBar.GetComponent<Image>(), new Color(UiTheme.BrandDark.r, UiTheme.BrandDark.g, UiTheme.BrandDark.b, 0.82f));
            topBar.GetComponent<Image>().raycastTarget = false;
            UiTheme.AddLogoHeader(topBar.transform, defaultFont, 16f, 150f);

            var bubbleRoot = new GameObject("AssistantBubble", typeof(RectTransform), typeof(Image));
            bubbleRoot.transform.SetParent(parent, false);
            StretchBottom(bubbleRoot.GetComponent<RectTransform>(), 380f, 300f, 36f, 36f);
            UiTheme.ApplyRounded(bubbleRoot.GetComponent<Image>(), UiTheme.BubbleBg);
            bubbleRoot.SetActive(false);

            var bubbleText = UiTheme.CreateText(bubbleRoot.transform, "BubbleText", defaultFont, 30, TextAnchor.UpperLeft, UiTheme.TextOnDark);
            StretchFull(bubbleText.rectTransform, 28f, 28f, 24f, 24f);
            bubbleText.alignment = TextAnchor.UpperLeft;
            bubbleText.fontStyle = FontStyle.Normal;
            bubbleText.raycastTarget = false;
            SetPrivateField(voiceAssistant, "assistantBubbleRoot", bubbleRoot);
            SetPrivateField(voiceAssistant, "assistantBubbleText", bubbleText);

            var overlayRoot = new GameObject("DirectionOverlay", typeof(RectTransform));
            overlayRoot.transform.SetParent(parent, false);
            StretchFull(overlayRoot.GetComponent<RectTransform>());
            overlayRoot.SetActive(false);

            var lineOrigin = new Vector2(0.5f, 0.38f);
            var forwardAnchor = new Vector2(0.5f, 0.58f);
            var leftAnchor = new Vector2(0.18f, 0.48f);
            var rightAnchor = new Vector2(0.82f, 0.48f);

            var forwardHint = CreateDirectionHint(overlayRoot.transform, "↑", "Check-in", forwardAnchor, new Vector2(0f, 0f),
                voiceAssistant.OnForwardTapped);
            var leftHint = CreateDirectionHint(overlayRoot.transform, "←", "Security", leftAnchor, new Vector2(0f, 0f),
                voiceAssistant.OnLeftTapped);
            var rightHint = CreateDirectionHint(overlayRoot.transform, "→", "Ciambella · Toalete", rightAnchor, new Vector2(0f, 0f),
                voiceAssistant.OnRightTapped);
            var forwardLine = CreateDirectionLine(overlayRoot.transform, lineOrigin, forwardAnchor);
            var leftLine = CreateDirectionLine(overlayRoot.transform, lineOrigin, leftAnchor);
            var rightLine = CreateDirectionLine(overlayRoot.transform, lineOrigin, rightAnchor);

            SetPrivateField(directionOverlay, "overlayRoot", overlayRoot);
            SetPrivateField(directionOverlay, "forwardHint", forwardHint);
            SetPrivateField(directionOverlay, "forwardLine", forwardLine);
            SetPrivateField(directionOverlay, "leftHint", leftHint);
            SetPrivateField(directionOverlay, "leftLine", leftLine);
            SetPrivateField(directionOverlay, "rightHint", rightHint);
            SetPrivateField(directionOverlay, "rightLine", rightLine);
            SetPrivateField(voiceAssistant, "directionOverlay", directionOverlay);

            var discover = parent.gameObject.AddComponent<AirportDiscoverController>();
            SetPrivateField(discover, "cameraPreview", preview);
            SetPrivateField(discover, "voiceAssistant", voiceAssistant);

            var hint = UiTheme.CreateText(parent, "Hint", defaultFont, 26, TextAnchor.UpperCenter, UiTheme.TextOnDark);
            StretchTop(hint.rectTransform, 200f, 90f, 40f, 40f);
            hint.fontStyle = FontStyle.Bold;
            hint.raycastTarget = false;
            SetPrivateField(discover, "hintText", hint);

            var status = UiTheme.CreateText(parent, "Status", defaultFont, 24, TextAnchor.UpperCenter, UiTheme.TextOnDark);
            StretchTop(status.rectTransform, 270f, 80f, 40f, 40f);
            status.alignment = TextAnchor.UpperCenter;
            status.raycastTarget = false;
            SetPrivateField(preview, "statusText", status);

            var placeholder = UiTheme.CreateText(parent, "EditorPlaceholder", defaultFont, 26, TextAnchor.MiddleCenter, UiTheme.TextOnLight);
            StretchTop(placeholder.rectTransform, 420f, 220f, 60f, 60f);
            placeholder.text =
                "Pe telefon: atinge ecranul → bun venit + 3 săgeți.\n" +
                "Rotește stânga/dreapta → vezi o singură direcție.\n" +
                "Atinge săgeata vizibilă pentru informații.\n\n" +
                "În Editor: textul apare în bulină (fără sunet).";
            placeholder.raycastTarget = false;
            SetPrivateField(discover, "editorPlaceholder", placeholder.gameObject);

            CreateBottomBackButton(parent, appController.ShowMainMenu);

            overlayRoot.transform.SetAsLastSibling();
            bubbleRoot.transform.SetAsLastSibling();
            topBar.transform.SetAsLastSibling();
        }

        void BuildFaqPanel(Transform parent)
        {
            UiTheme.AddLogoHeader(parent, defaultFont, 32f, 140f);

            var title = UiTheme.CreateText(parent, "Title", defaultFont, 36, TextAnchor.UpperCenter, UiTheme.TextOnLight);
            StretchTop(title.rectTransform, 170f, 70f, 40f, 40f);
            title.text = "Întrebări despre Aeroportul Iași";
            title.fontStyle = FontStyle.Bold;

            var historyCard = CreateCard(parent, 250f, 680f);
            historyCard.AddComponent<RectMask2D>();
            var history = UiTheme.CreateText(historyCard.transform, "ChatHistory", defaultFont, 30, TextAnchor.UpperLeft, UiTheme.TextOnLight);
            StretchFull(history.rectTransform, 20f, 20f, 16f, 16f);
            history.alignment = TextAnchor.UpperLeft;
            history.verticalOverflow = VerticalWrapMode.Truncate;
            history.lineSpacing = 1.15f;

            var inputGo = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(parent, false);
            StretchBottom(inputGo.GetComponent<RectTransform>(), 250f, 84f, 40f, 40f);
            UiTheme.ApplyRounded(inputGo.GetComponent<Image>(), Color.white);
            var input = inputGo.GetComponent<InputField>();
            var inputText = UiTheme.CreateText(inputGo.transform, "Text", defaultFont, 28, TextAnchor.MiddleLeft, UiTheme.TextOnLight);
            StretchFull(inputText.rectTransform, 20f, 20f, 10f, 10f);
            input.textComponent = inputText;

            var scrollGo = new GameObject("SuggestedScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            StretchBottom(scrollGo.GetComponent<RectTransform>(), 350f, 360f, 40f, 40f);
            UiTheme.ApplyRounded(scrollGo.GetComponent<Image>(), new Color(1f, 1f, 1f, 0.55f));

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>(), 8f, 8f, 8f, 8f);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var suggested = new GameObject("SuggestedQuestions", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            suggested.transform.SetParent(viewport.transform, false);
            var suggestedRect = suggested.GetComponent<RectTransform>();
            suggestedRect.anchorMin = new Vector2(0f, 1f);
            suggestedRect.anchorMax = new Vector2(1f, 1f);
            suggestedRect.pivot = new Vector2(0.5f, 1f);
            suggestedRect.anchoredPosition = Vector2.zero;
            suggestedRect.sizeDelta = new Vector2(0f, 0f);
            var suggestedLayout = suggested.GetComponent<VerticalLayoutGroup>();
            suggestedLayout.spacing = 10f;
            suggestedLayout.childControlWidth = true;
            suggestedLayout.childControlHeight = true;
            suggestedLayout.childForceExpandWidth = true;
            suggestedLayout.childForceExpandHeight = false;
            suggested.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = suggestedRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var chatbot = parent.gameObject.AddComponent<ChatbotManager>();
            SetPrivateField(chatbot, "chatHistoryText", history);
            SetPrivateField(chatbot, "inputField", input);
            SetPrivateField(chatbot, "suggestedQuestionsContainer", suggested.transform);

            var actionRow = new GameObject("FaqActions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            actionRow.transform.SetParent(parent, false);
            StretchBottom(actionRow.GetComponent<RectTransform>(), 140f, 90f, 40f, 40f);
            var rowLayout = actionRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            var sendBtn = UiTheme.CreatePrimaryButton(actionRow.transform, defaultFont, "Trimite", () => chatbot.SendMessage());
            SetPrivateField(chatbot, "sendButton", sendBtn.GetComponent<Button>());
            UiTheme.CreateSecondaryButton(actionRow.transform, defaultFont, "Înapoi", appController.ShowMainMenu, 420f, 90f);
        }

        void BuildAboutPanel(Transform parent)
        {
            UiTheme.AddLogoHeader(parent, defaultFont, 32f, 140f);

            var title = UiTheme.CreateText(parent, "Title", defaultFont, 34, TextAnchor.UpperCenter, UiTheme.TextOnLight);
            StretchTop(title.rectTransform, 170f, 70f, 40f, 40f);
            title.text = "Despre aplicație";
            title.fontStyle = FontStyle.Bold;

            var bodyCard = CreateCard(parent, 260f, 680f);
            var body = UiTheme.CreateText(bodyCard.transform, "Body", defaultFont, 26, TextAnchor.UpperLeft, UiTheme.TextOnLight);
            StretchFull(body.rectTransform, 24f, 24f, 20f, 20f);
            body.alignment = TextAnchor.UpperLeft;
            body.text =
                "Ghid Aeroport Iași este o aplicație demo creată pentru a prezenta cum ar putea funcționa " +
                "un asistent digital pentru pasagerii Aeroportului Internațional Iași.\n\n" +
                "• Secțiunea Descoperă — vezi împrejurimile prin camera telefonului\n" +
                "• Secțiunea Întrebări — răspunsuri despre aeroport și servicii\n" +
                "• Prototip pentru hackathon — funcționalități extinse vor veni incremental\n\n" +
                "Aceasta este o versiune demo, nu reprezintă sistemul oficial al aeroportului.";

            CreateBottomBackButton(parent, appController.ShowMainMenu);
        }

        GameObject CreateBottomCenterButton(Transform parent, string label, float bottomOffset)
        {
            var btnGo = UiTheme.CreateAccentButton(parent, defaultFont, label, null, 520f, 96f);
            var rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomOffset);
            return btnGo;
        }

        void CreateBottomBackButton(Transform parent, System.Action onClick, float bottomOffset = 120f)
        {
            var btnGo = UiTheme.CreateSecondaryButton(parent, defaultFont, "Înapoi", onClick, 920f, 88f);
            var rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomOffset);
        }

        GameObject CreateCard(Transform parent, float top, float height)
        {
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            StretchTop(card.GetComponent<RectTransform>(), top, height, 40f, 40f);
            UiTheme.ApplyRounded(card.GetComponent<Image>(), Color.white);
            return card;
        }

        GameObject CreateDirectionHint(Transform parent, string arrow, string label, Vector2 anchor, Vector2 position,
            UnityEngine.Events.UnityAction onClick)
        {
            var hintGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            hintGo.transform.SetParent(parent, false);
            var rect = hintGo.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(280f, 150f);
            rect.anchoredPosition = position;
            UiTheme.ApplyRounded(hintGo.GetComponent<Image>(), new Color(UiTheme.BrandDark.r, UiTheme.BrandDark.g, UiTheme.BrandDark.b, 0.88f));
            hintGo.GetComponent<Image>().raycastTarget = true;

            var button = hintGo.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var arrowText = UiTheme.CreateText(hintGo.transform, "Arrow", defaultFont, 56, TextAnchor.UpperCenter, UiTheme.BrandGreen);
            StretchTop(arrowText.rectTransform, 8f, 70f, 0f, 0f);
            arrowText.text = arrow;
            arrowText.fontStyle = FontStyle.Bold;
            arrowText.raycastTarget = false;

            var labelText = UiTheme.CreateText(hintGo.transform, "Label", defaultFont, 24, TextAnchor.LowerCenter, UiTheme.TextOnDark);
            StretchBottom(labelText.rectTransform, 8f, 56f, 8f, 8f);
            labelText.text = label;
            labelText.fontStyle = FontStyle.Bold;
            labelText.raycastTarget = false;
            return hintGo;
        }

        GameObject CreateDirectionLine(Transform parent, Vector2 from, Vector2 to)
        {
            var lineGo = new GameObject("Line", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(parent, false);
            var rect = lineGo.GetComponent<RectTransform>();
            rect.anchorMin = from;
            rect.anchorMax = from;
            rect.pivot = new Vector2(0.5f, 0f);

            Vector2 delta = to - from;
            float length = delta.magnitude * 1100f;
            float angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
            rect.sizeDelta = new Vector2(6f, length);
            rect.localEulerAngles = new Vector3(0f, 0f, -angle);

            var image = lineGo.GetComponent<Image>();
            image.color = UiTheme.BrandGreen;
            image.raycastTarget = false;
            return lineGo;
        }

        static void StretchFull(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        static void StretchTop(RectTransform rect, float top, float height, float left, float right)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-left - right, height);
            rect.offsetMin = new Vector2(left, rect.offsetMin.y);
            rect.offsetMax = new Vector2(-right, rect.offsetMax.y);
        }

        static void StretchBottom(RectTransform rect, float bottom, float height, float left, float right)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottom);
            rect.sizeDelta = new Vector2(-left - right, height);
        }

        static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
