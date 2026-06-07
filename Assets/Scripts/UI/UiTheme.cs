using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.UI
{
    /// <summary>
    /// Brand styling aligned with Aeroportul Iași visual identity.
    /// </summary>
    public static class UiTheme
    {
        public static readonly Color BrandDark = new Color(0.11f, 0.17f, 0.22f);
        public static readonly Color BrandGreen = new Color(0.55f, 0.78f, 0.15f);
        public static readonly Color BrandGreenDark = new Color(0.45f, 0.66f, 0.12f);
        public static readonly Color PanelLight = new Color(0.95f, 0.96f, 0.97f);
        public static readonly Color TextOnDark = Color.white;
        public static readonly Color TextOnLight = new Color(0.12f, 0.16f, 0.22f);
        public static readonly Color BubbleBg = new Color(0.05f, 0.08f, 0.11f, 0.88f);

        static Sprite roundedSprite;
        static Sprite logoSprite;

        public static Sprite RoundedSprite
        {
            get
            {
                if (roundedSprite == null)
                {
                    roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                }

                return roundedSprite;
            }
        }

        public static Sprite LogoSprite
        {
            get
            {
                if (logoSprite == null)
                {
                    logoSprite = Resources.Load<Sprite>("UI/IasiAirportLogo");
                }

                return logoSprite;
            }
        }

        public static void ApplyRounded(Image image, Color color)
        {
            image.color = color;
            if (RoundedSprite != null)
            {
                image.sprite = RoundedSprite;
                image.type = Image.Type.Sliced;
            }
        }

        public static Image AddMainMenuLogo(Transform parent, Font font)
        {
            var headerGo = new GameObject("MainMenuLogo", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(parent, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerRect.pivot = new Vector2(0.5f, 0.5f);
            headerRect.anchoredPosition = new Vector2(0f, 360f);
            headerRect.sizeDelta = new Vector2(980f, 240f);
            headerGo.GetComponent<Image>().color = Color.clear;

            var logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
            logoGo.transform.SetParent(headerGo.transform, false);
            StretchFull(logoGo.GetComponent<RectTransform>());
            var logoImage = logoGo.GetComponent<Image>();
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;

            Sprite logo = LogoSprite;
            if (logo != null)
            {
                logoImage.sprite = logo;
                logoImage.color = Color.white;
            }
            else
            {
                logoImage.color = Color.clear;
                var fallback = CreateText(logoGo.transform, "LogoFallback", font, 40, TextAnchor.MiddleCenter, TextOnDark);
                StretchFull(fallback.rectTransform);
                fallback.text = "AEROPORTUL IAȘI";
                fallback.fontStyle = FontStyle.Bold;
            }

            return logoImage;
        }

        public static Image AddLogoHeader(Transform parent, Font font, float top = 36f, float height = 150f)
        {
            var headerGo = new GameObject("BrandHeader", typeof(RectTransform), typeof(Image));
            headerGo.transform.SetParent(parent, false);
            var headerRect = headerGo.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -top);
            headerRect.sizeDelta = new Vector2(-80f, height);
            headerGo.GetComponent<Image>().color = Color.clear;

            var logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
            logoGo.transform.SetParent(headerGo.transform, false);
            var logoRect = logoGo.GetComponent<RectTransform>();
            StretchFull(logoRect);
            var logoImage = logoGo.GetComponent<Image>();
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;

            Sprite logo = LogoSprite;
            if (logo != null)
            {
                logoImage.sprite = logo;
                logoImage.color = Color.white;
            }
            else
            {
                logoImage.color = Color.clear;
                var fallback = CreateText(logoGo.transform, "LogoFallback", font, 34, TextAnchor.MiddleCenter, TextOnDark);
                StretchFull(fallback.rectTransform);
                fallback.text = "AEROPORTUL IAȘI";
                fallback.fontStyle = FontStyle.Bold;
            }

            return logoGo.GetComponent<Image>();
        }

        public static GameObject CreatePrimaryButton(
            Transform parent,
            Font font,
            string label,
            System.Action onClick,
            Color? color = null)
        {
            return CreateButton(parent, font, label, onClick, color ?? BrandGreen, 920f, 96f, 32);
        }

        public static GameObject CreateSecondaryButton(
            Transform parent,
            Font font,
            string label,
            System.Action onClick,
            float width = 920f,
            float height = 88f)
        {
            return CreateButton(parent, font, label, onClick, BrandDark, width, height, 30);
        }

        public static GameObject CreateAccentButton(
            Transform parent,
            Font font,
            string label,
            System.Action onClick,
            float width = 520f,
            float height = 96f)
        {
            return CreateButton(parent, font, label, onClick, BrandGreen, width, height, 30);
        }

        static GameObject CreateButton(
            Transform parent,
            Font font,
            string label,
            System.Action onClick,
            Color color,
            float width,
            float height,
            int fontSize)
        {
            var btnGo = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);

            var image = btnGo.GetComponent<Image>();
            ApplyRounded(image, color);

            var layout = btnGo.GetComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = height;
            layout.minHeight = height;

            var text = CreateText(btnGo.transform, "Label", font, fontSize, TextAnchor.MiddleCenter, TextOnDark);
            StretchFull(text.rectTransform, 24f, 24f, 12f, 12f);
            text.text = label;
            text.fontStyle = FontStyle.Bold;

            var button = btnGo.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            button.colors = colors;

            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            return btnGo;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            Font font,
            int size,
            TextAnchor anchor,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.1f;
            return text;
        }

        public static void StretchFull(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
