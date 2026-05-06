using System.Collections.Generic;
using UnityEngine;

// Пул UI-полосок здоровья. Регистрируется в ServiceLocator, враги берут бары через Get / Release.
public class HealthBarPool : MonoBehaviour
{
    [SerializeField] private HealthBarUI _prefab;
    [SerializeField] private RectTransform _canvasParent;
    [SerializeField] private int _initialSize = 10;
    [SerializeField] private Vector3 _defaultOffset = new Vector3(0f, 2f, 0f);

    private readonly Stack<HealthBarUI> _free = new Stack<HealthBarUI>();
    private Camera _camera;

    public Vector3 DefaultOffset => _defaultOffset;

    private void Awake()
    {
        ServiceLocator.RegisterService(this);
        _camera = Camera.main;

        for (int i = 0; i < _initialSize; i++)
            _free.Push(CreateInstance());
    }

    private HealthBarUI CreateInstance()
    {
        HealthBarUI bar = Instantiate(_prefab, _canvasParent);
        bar.gameObject.SetActive(false);
        return bar;
    }

    public HealthBarUI Get(Transform target, Vector3 offset)
    {
        HealthBarUI bar = _free.Count > 0 ? _free.Pop() : CreateInstance();
        bar.transform.SetAsLastSibling();
        bar.Bind(target, offset, _camera);
        return bar;
    }

    public void Release(HealthBarUI bar)
    {
        if (bar == null) return;
        bar.Unbind();
        _free.Push(bar);
    }
}
