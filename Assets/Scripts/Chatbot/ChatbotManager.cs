using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.Chatbot
{
    /// <summary>
    /// Rule-based FAQ about Aeroportul Internațional Iași (no external APIs).
    /// </summary>
    public class ChatbotManager : MonoBehaviour
    {
        [SerializeField] Text chatHistoryText;
        [SerializeField] InputField inputField;
        [SerializeField] Button sendButton;
        [SerializeField] Transform suggestedQuestionsContainer;
        [SerializeField] Button suggestedQuestionButtonPrefab;

        readonly List<string> chatLines = new List<string>();

        static readonly string[] SuggestedQuestions =
        {
            "Unde se află Aeroportul Iași?",
            "Ce cod IATA are aeroportul?",
            "Cum ajung în centrul Iașiului?",
            "Ce facilități are aeroportul?",
            "Ce este această aplicație?"
        };

        void Start()
        {
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }

            BuildSuggestedQuestions();
            AppendBotMessage(
                "Bun venit! Alege o întrebare sau scrie tu. " +
                "Pot răspunde despre Aeroportul Internațional Iași și despre această aplicație demo.");
        }

        void BuildSuggestedQuestions()
        {
            if (suggestedQuestionsContainer == null)
            {
                return;
            }

            foreach (Transform child in suggestedQuestionsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (string question in SuggestedQuestions)
            {
                Button btn = suggestedQuestionButtonPrefab != null
                    ? Instantiate(suggestedQuestionButtonPrefab, suggestedQuestionsContainer)
                    : CreateSuggestedButton(suggestedQuestionsContainer);

                var label = btn.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = question;
                }

                string q = question;
                btn.onClick.AddListener(() =>
                {
                    if (inputField != null)
                    {
                        inputField.text = q;
                    }

                    SendMessage();
                });
            }
        }

        Button CreateSuggestedButton(Transform parent)
        {
            var go = new GameObject("SuggestedQuestion", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.85f, 0.88f, 0.92f);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = new Color(0.15f, 0.2f, 0.3f);
            text.alignment = TextAnchor.MiddleLeft;
            text.fontSize = 18;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 48f);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 0f);
            textRect.offsetMax = new Vector2(-12f, 0f);

            return go.GetComponent<Button>();
        }

        public void SendMessage()
        {
            string userText = inputField != null ? inputField.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(userText))
            {
                return;
            }

            AppendUserMessage(userText);
            if (inputField != null)
            {
                inputField.text = string.Empty;
            }

            AppendBotMessage(AnswerQuestion(userText));
        }

        string AnswerQuestion(string input)
        {
            string text = input.ToLowerInvariant();
            Debug.Log($"[ChatbotManager] FAQ query: {input}");

            if (ContainsAny(text, "buna", "salut", "hello", "hi"))
            {
                return "Salut! Cu ce te pot ajuta despre Aeroportul Iași?";
            }

            if (ContainsAny(text, "unde se afla", "locatie", "location", "adresa", "address", "unde este"))
            {
                return
                    "Aeroportul Internațional Iași (IAS) se află la aproximativ 8 km est de centrul municipiului Iași, " +
                    "pe șoseaua Moara de Foc. Este principalul aeroport al regiunii Moldova.";
            }

            if (ContainsAny(text, "iata", "cod", "ias"))
            {
                return
                    "Codul IATA al aeroportului este IAS (Iași). " +
                    "Aeroportul deservește zboruri interne și internaționale.";
            }

            if (ContainsAny(text, "centrul", "centru", "oras", "oraș", "iasi", "iași", "transport", "autobuz", "taxi"))
            {
                return
                    "Din aeroport poți ajunge în centrul Iașiului cu taxi, aplicații de ride-sharing sau transport public " +
                    "(autobuze spre centru). Durata este de obicei 20–30 minute, în funcție de trafic.";
            }

            if (ContainsAny(text, "facilitati", "facilități", "servicii", "cafenea", "magazin", "wifi", "parcare"))
            {
                return
                    "Aeroportul oferă zone de check-in, sală de așteptare, cafenea/bar, magazine duty-free (pentru zboruri externe), " +
                    "toalete, Wi-Fi și parcare. Facilitățile exacte pot varia în funcție de terminal și programul zborurilor.";
            }

            if (ContainsAny(text, "zbor", "destinatii", "destinații", "linii", "curse", "flights"))
            {
                return
                    "Aeroportul Iași are legături către mai multe destinații din România și Europa. " +
                    "Programul zborurilor se schimbă sezonier — verifică site-ul oficial al aeroportului sau al companiilor aeriene pentru ore actualizate.";
            }

            if (ContainsAny(text, "check-in", "checkin", "bagaj", "security", "securitate"))
            {
                return
                    "Check-in-ul se face la ghișeele companiilor aeriene sau online, înainte de sosire. " +
                    "După check-in urmează controlul de securitate, apoi zona de îmbarcare. " +
                    "Recomandăm să ajungi cu cel puțin 2 ore înainte pentru zboruri externe.";
            }

            if (ContainsAny(text, "aplicatie", "aplicație", "app", "demo", "ce este", "ce face"))
            {
                return
                    "Aceasta este o aplicație demo pentru hackathon. Oferă trei secțiuni: " +
                    "Descoperă aeroportul (cameră), întrebări și răspunsuri, și informații despre aplicație. " +
                    "Nu este aplicația oficială a aeroportului — vom adăuga funcții pas cu pas.";
            }

            if (ContainsAny(text, "oficial", "real", "harta", "hartă", "navigatie", "navigație", "ar"))
            {
                return
                    "Navigația AR avansată nu face parte încă din această versiune simplificată. " +
                    "Momentan poți explora prin cameră și pune întrebări. " +
                    "Nu folosim harta oficială a aeroportului în acest prototip.";
            }

            if (ContainsAny(text, "program", "ore", "deschis", "contact", "telefon"))
            {
                return
                    "Aeroportul funcționează în funcție de programul zborurilor. " +
                    "Pentru informații oficiale actualizate, consultă site-ul Aeroportului Internațional Iași.";
            }

            return
                "Nu am găsit un răspuns exact. Încearcă: „Unde se află Aeroportul Iași?”, " +
                "„Cum ajung în centru?” sau „Ce facilități are aeroportul?”";
        }

        static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (text.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        void AppendUserMessage(string message)
        {
            chatLines.Add($"Tu: {message}");
            RefreshChatHistory();
        }

        void AppendBotMessage(string message)
        {
            chatLines.Add($"Răspuns: {message}");
            RefreshChatHistory();
        }

        void RefreshChatHistory()
        {
            if (chatHistoryText != null)
            {
                chatHistoryText.text = string.Join("\n\n", chatLines);
            }
        }
    }
}
