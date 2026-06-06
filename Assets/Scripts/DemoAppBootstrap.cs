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
        static readonly Color PrimaryBlue = new Color(0.12f, 0.45f, 0.92f);
        static readonly Color LightGrey = new Color(0.93f, 0.94f, 0.96f);
        static readonly Color TextDark = new Color(0.15f, 0.2f, 0.3f);

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

            GameObject mainMenu = CreatePanel(canvasGo.transform, "MainMenuPanel");
            GameObject discover = CreateTransparentPanel(canvasGo.transform, "DiscoverPanel");
            GameObject faq = CreatePanel(canvasGo.transform, "FaqPanel");
            GameObject about = CreatePanel(canvasGo.transform, "AboutPanel");

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

        GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            StretchFull(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = LightGrey;
            return panel;
        }

        void BuildMainMenu(Transform parent)
        {
            AddTitle(parent, "Ghid Aeroport Iași");
            AddBody(parent,
                "Explorează aeroportul, pune întrebări și află mai multe despre această aplicație demo.",
                260f, 140f);

            float y = -480f;
            AddMenuButton(parent, "Descoperă aeroportul", y, appController.ShowDiscover);
            y -= 100f;
            AddMenuButton(parent, "Întrebări și răspunsuri", y, appController.ShowFaq);
            y -= 100f;
            AddMenuButton(parent, "Despre aplicație", y, appController.ShowAbout);
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

            var preview = parent.gameObject.AddComponent<SimpleCameraPreview>();
            SetPrivateField(preview, "previewImage", rawImage);

            var discover = parent.gameObject.AddComponent<AirportDiscoverController>();
            SetPrivateField(discover, "cameraPreview", preview);

            var hint = CreateText(parent, "Hint", 24, TextAnchor.UpperCenter, Color.white);
            StretchTop(hint.rectTransform, 140f, 80f, 40f, 40f);
            SetPrivateField(discover, "hintText", hint);

            var status = CreateText(parent, "Status", 20, TextAnchor.UpperCenter, Color.white);
            StretchTop(status.rectTransform, 220f, 120f, 40f, 40f);
            status.alignment = TextAnchor.UpperCenter;
            SetPrivateField(preview, "statusText", status);

            var placeholder = CreateText(parent, "EditorPlaceholder", 22, TextAnchor.MiddleCenter, TextDark);
            StretchTop(placeholder.rectTransform, 400f, 200f, 60f, 60f);
            placeholder.text =
                "Pe telefon, camera se deschide automat.\n\n" +
                "În Unity Editor nu există feed de cameră — testează pe iPhone.";
            SetPrivateField(discover, "editorPlaceholder", placeholder.gameObject);

            AddMenuButton(parent, "Înapoi", -1080f, appController.ShowMainMenu);
        }

        void BuildFaqPanel(Transform parent)
        {
            AddTitle(parent, "Întrebări despre Aeroportul Iași", 80f);

            var history = CreateText(parent, "ChatHistory", 22, TextAnchor.UpperLeft, TextDark);
            StretchTop(history.rectTransform, 160f, 720f, 40f, 40f);
            history.alignment = TextAnchor.UpperLeft;

            var inputGo = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(parent, false);
            StretchBottom(inputGo.GetComponent<RectTransform>(), 250f, 70f, 40f, 40f);
            inputGo.GetComponent<Image>().color = Color.white;
            var input = inputGo.GetComponent<InputField>();
            var inputText = CreateText(inputGo.transform, "Text", 22, TextAnchor.MiddleLeft, TextDark);
            StretchFull(inputText.rectTransform, 12f, 12f, 8f, 8f);
            input.textComponent = inputText;

            var suggested = new GameObject("SuggestedQuestions", typeof(RectTransform), typeof(VerticalLayoutGroup));
            suggested.transform.SetParent(parent, false);
            StretchBottom(suggested.GetComponent<RectTransform>(), 340f, 320f, 40f, 40f);
            suggested.GetComponent<VerticalLayoutGroup>().spacing = 6f;

            var chatbot = parent.gameObject.AddComponent<ChatbotManager>();
            SetPrivateField(chatbot, "chatHistoryText", history);
            SetPrivateField(chatbot, "inputField", input);
            SetPrivateField(chatbot, "suggestedQuestionsContainer", suggested.transform);

            var sendBtn = AddMenuButton(parent, "Trimite", -980f, () => chatbot.SendMessage());
            SetPrivateField(chatbot, "sendButton", sendBtn.GetComponent<Button>());

            AddMenuButton(parent, "Înapoi", -1080f, appController.ShowMainMenu);
        }

        void BuildAboutPanel(Transform parent)
        {
            AddTitle(parent, "Despre aplicație", 80f);
            AddBody(parent,
                "Ghid Aeroport Iași este o aplicație demo creată pentru a prezenta cum ar putea funcționa " +
                "un asistent digital pentru pasagerii Aeroportului Internațional Iași.\n\n" +
                "• Secțiunea Descoperă — vezi împrejurimile prin camera telefonului\n" +
                "• Secțiunea Întrebări — răspunsuri despre aeroport și servicii\n" +
                "• Prototip pentru hackathon — funcționalități extinse vor veni incremental\n\n" +
                "Aceasta este o versiune demo, nu reprezintă sistemul oficial al aeroportului.",
                280f, 620f);
            AddMenuButton(parent, "Înapoi", -980f, appController.ShowMainMenu);
        }

        GameObject AddMenuButton(Transform parent, string label, float y, System.Action onClick)
        {
            var btnGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(900f, 80f);
            rect.anchoredPosition = new Vector2(0f, y);
            btnGo.GetComponent<Image>().color = PrimaryBlue;

            var text = CreateText(btnGo.transform, "Label", 28, TextAnchor.MiddleCenter, Color.white);
            StretchFull(text.rectTransform);
            text.text = label;

            if (onClick != null)
            {
                btnGo.GetComponent<Button>().onClick.AddListener(() => onClick());
            }

            return btnGo;
        }

        Text CreateText(Transform parent, string name, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = defaultFont;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        void AddTitle(Transform parent, string title, float top = 120f)
        {
            var text = CreateText(parent, "Title", 40, TextAnchor.UpperCenter, PrimaryBlue);
            StretchTop(text.rectTransform, top, 80f, 40f, 40f);
            text.text = title;
        }

        void AddBody(Transform parent, string body, float top, float height)
        {
            var text = CreateText(parent, "Body", 22, TextAnchor.UpperCenter, TextDark);
            StretchTop(text.rectTransform, top, height, 40f, 40f);
            text.text = body;
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
