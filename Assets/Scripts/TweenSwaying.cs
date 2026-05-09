using UnityEngine;
using DG.Tweening;

public class TweenSwaying : MonoBehaviour
{
    [Header("Sway Settings (DOTween)")]
    [SerializeField] private bool _enableSway = true;
    [SerializeField] private float _swayAmount = 0.5f;   // Амплитуда вверх-вниз
    [SerializeField] private float _swayDuration = 2f;  // Время одного цикла
    [SerializeField] private Vector3 _tiltAxis = new Vector3(0, 0, 3f); // Ось наклона (по умолчанию Z)  

    private void Start()
    {
        if (_enableSway)
        {
            Invoke(nameof(StartSwaying), Random.Range(0f, 1f)); // Немного задержки, чтобы не начинать сразу    
        }
    }

    private void StartSwaying()
    {
        // 1. Покачивание вверх-вниз (Local Position)
        transform.DOLocalMoveY(transform.localPosition.y + _swayAmount, _swayDuration)
            .SetEase(Ease.InOutSine) // Плавный вход и выход
            .SetLoops(-1, LoopType.Yoyo); // Бесконечное повторение туда-сюда

        // 2. Легкий наклон по Z или X (вращение)
        // Делаем задержку или другую длительность, чтобы движение выглядело естественнее
        transform.DOLocalRotate(_tiltAxis, _swayDuration * 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
