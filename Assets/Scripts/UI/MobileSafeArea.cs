using UnityEngine;

namespace IdleSoccerClubMVP.UI
{
    [DisallowMultipleComponent]
    public sealed class MobileSafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Rect safeArea = Screen.safeArea;
            Vector2 minAnchor = safeArea.position;
            Vector2 maxAnchor = safeArea.position + safeArea.size;
            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
