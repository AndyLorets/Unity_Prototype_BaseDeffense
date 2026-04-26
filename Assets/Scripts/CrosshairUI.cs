using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private CrosshairController _crosshair;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private Image _image;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        _crosshair.OnColorChanged += SetColor;
    }

    private void OnDisable()
    {
        _crosshair.OnColorChanged -= SetColor;
    }

    private void LateUpdate()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, _crosshair.ScreenPosition, null, out Vector2 localPoint);
        _rect.localPosition = localPoint;
    }

    private void SetColor(Color color)
    {
        if (_image != null) _image.color = color;
    }
}
