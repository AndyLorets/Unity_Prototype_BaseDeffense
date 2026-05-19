using UnityEngine;

// Перемещает UI-элементы при смене ориентации экрана.
// Добавь на любой GameObject в сцене, привяжи нужные RectTransform-ы в инспекторе.
public class UIOrientationLayout : MonoBehaviour
{
    [System.Serializable]
    public struct ElementLayout
    {
        public RectTransform element;

        [Header("Portrait")]
        public Vector2 portraitAnchorMin;
        public Vector2 portraitAnchorMax;
        public Vector2 portraitAnchoredPos;
        public Vector2 portraitSizeDelta;

        [Header("Landscape")]
        public Vector2 landscapeAnchorMin;
        public Vector2 landscapeAnchorMax;
        public Vector2 landscapeAnchoredPos;
        public Vector2 landscapeSizeDelta;
    }

    [SerializeField] private ElementLayout[] _elements;

    private void OnEnable()
    {
        OrientationManager.OnOrientationChanged += Apply;
    }

    private void OnDisable()
    {
        OrientationManager.OnOrientationChanged -= Apply;
    }

    private void Start()
    {
        if (OrientationManager.Instance != null)
            Apply(OrientationManager.Instance.IsPortrait);
    }

    private void Apply(bool portrait)
    {
        foreach (var layout in _elements)
        {
            if (layout.element == null) continue;

            if (portrait)
            {
                layout.element.anchorMin = layout.portraitAnchorMin;
                layout.element.anchorMax = layout.portraitAnchorMax;
                layout.element.anchoredPosition = layout.portraitAnchoredPos;
                layout.element.sizeDelta = layout.portraitSizeDelta;
            }
            else
            {
                layout.element.anchorMin = layout.landscapeAnchorMin;
                layout.element.anchorMax = layout.landscapeAnchorMax;
                layout.element.anchoredPosition = layout.landscapeAnchoredPos;
                layout.element.sizeDelta = layout.landscapeSizeDelta;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Portrait")]
    private void PreviewPortrait() => Apply(true);

    [ContextMenu("Preview Landscape")]
    private void PreviewLandscape() => Apply(false);
#endif
}
