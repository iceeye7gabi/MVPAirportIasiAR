namespace AirportAR.Chatbot
{
    public class ChatbotResponse
    {
        public string Message;
        public ChatbotIntent Intent;
        public string NavigateZoneId;
        public bool ShowNavigateButton;

        public ChatbotResponse(string message, ChatbotIntent intent = ChatbotIntent.Unknown,
            string navigateZoneId = null, bool showNavigateButton = false)
        {
            Message = message;
            Intent = intent;
            NavigateZoneId = navigateZoneId;
            ShowNavigateButton = showNavigateButton;
        }
    }
}
