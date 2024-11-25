using System;
using UnityEngine;

public class WidgetComponent : MonoBehaviour
{
    [SerializeField] private Widget widgetPrefab;
    [SerializeField] private Transform attachTransform;
    private Camera _mainCamera;

    private Widget _widget;
    private void Start()
    {
        _widget = Instantiate(widgetPrefab);
        _widget.SetOwner(gameObject);

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas)
        {
            _widget.transform.SetParent(canvas.transform);
        }

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_widget)
        {
            _widget.transform.position = _mainCamera.WorldToScreenPoint(attachTransform.position);
        }
    }
}
