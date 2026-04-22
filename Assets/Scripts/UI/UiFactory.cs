using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdleSoccerClubMVP.UI
{
    public static class UiFactory
    {
        public static Font DefaultFont
        {
            get { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }

        public static RectTransform EnsureRectTransform(GameObject gameObject)
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            return rectTransform;
        }

        public static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static GameObject CreateBlock(string name, Transform parent, Color color)
        {
            GameObject block = new GameObject(name);
            block.transform.SetParent(parent, false);
            RectTransform rectTransform = EnsureRectTransform(block);
            rectTransform.localScale = Vector3.one;
            Image image = block.AddComponent<Image>();
            image.color = color;
            return block;
        }

        public static GameObject CreateVerticalPanel(string name, Transform parent, Color color, int padding = 12, float spacing = 8f)
        {
            GameObject panel = CreateBlock(name, parent, color);
            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            return panel;
        }

        public static GameObject CreateHorizontalPanel(string name, Transform parent, Color color, int padding = 12, float spacing = 8f)
        {
            GameObject panel = CreateBlock(name, parent, color);
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return panel;
        }

        public static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rectTransform = EnsureRectTransform(textObject);
            rectTransform.localScale = Vector3.one;

            Text text = textObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, UnityAction onClick, Color? background = null, int fontSize = 16, float minHeight = 48f)
        {
            GameObject buttonObject = CreateBlock(name, parent, background ?? new Color(0.18f, 0.32f, 0.48f, 0.95f));
            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = minHeight;

            Text buttonText = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter, Color.white);
            RectTransform textRect = EnsureRectTransform(buttonText.gameObject);
            Stretch(textRect);
            return button;
        }

        public static GameObject CreateSpacer(string name, Transform parent, float minHeight = 12f, float minWidth = 0f, float flexibleHeight = 0f, float flexibleWidth = 0f)
        {
            GameObject spacer = new GameObject(name);
            spacer.transform.SetParent(parent, false);
            RectTransform rectTransform = EnsureRectTransform(spacer);
            rectTransform.localScale = Vector3.one;
            LayoutElement layoutElement = spacer.AddComponent<LayoutElement>();
            layoutElement.minHeight = minHeight;
            layoutElement.minWidth = minWidth;
            layoutElement.flexibleHeight = flexibleHeight;
            layoutElement.flexibleWidth = flexibleWidth;
            return spacer;
        }

        public static LayoutElement SetLayoutElement(GameObject gameObject, float minHeight = -1f, float preferredHeight = -1f, float flexibleHeight = -1f, float minWidth = -1f, float preferredWidth = -1f, float flexibleWidth = -1f)
        {
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            if (minHeight >= 0f)
            {
                layoutElement.minHeight = minHeight;
            }

            if (preferredHeight >= 0f)
            {
                layoutElement.preferredHeight = preferredHeight;
            }

            if (flexibleHeight >= 0f)
            {
                layoutElement.flexibleHeight = flexibleHeight;
            }

            if (minWidth >= 0f)
            {
                layoutElement.minWidth = minWidth;
            }

            if (preferredWidth >= 0f)
            {
                layoutElement.preferredWidth = preferredWidth;
            }

            if (flexibleWidth >= 0f)
            {
                layoutElement.flexibleWidth = flexibleWidth;
            }

            return layoutElement;
        }

        public static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                Object.Destroy(parent.GetChild(index).gameObject);
            }
        }
    }
}
