using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; 

public class SwipeController : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] private Image _image; 

    private Vector2 swipeStartPos;

    public float swipeThreshold = 50f;

    public static Action onSwipeUp;
    public static Action onSwipeDown;
    public static Action onSwipeRight;
    public static Action onSwipeLeft;

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 swipeEndPos = eventData.position;
        float swipeDistance = Vector2.Distance(swipeStartPos, swipeEndPos);

        if (swipeDistance > swipeThreshold)
        {
            Vector2 swipeDirection = swipeEndPos - swipeStartPos;
            DetectSwipeDirection(swipeDirection.normalized);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        swipeStartPos = eventData.position;
    }

    private void DetectSwipeDirection(Vector2 swipeDirection)
    {
        float horizontalSwipe = Mathf.Abs(swipeDirection.x);
        float verticalSwipe = Mathf.Abs(swipeDirection.y);

        if (horizontalSwipe > verticalSwipe)
        {
            if (swipeDirection.x > 0)
                MoveRight();
            else
                MoveLeft(); 
        }
        else
        {
            if (swipeDirection.y > 0)
                onSwipeUp?.Invoke();
            else
                onSwipeDown?.Invoke();
        }
    }


    private float _curentValueX;
    private const float radius = 200f;
    private const float moveDuration = .1f;
    private void MoveLeft()
    {
        onSwipeLeft?.Invoke();

        if (_curentValueX <= -radius) return;

        float endValue = _curentValueX - radius;
        _curentValueX = endValue;

        transform.DOKill();
        _image.transform.DOLocalMoveX(endValue, moveDuration)
            .SetEase(Ease.Linear);
    }
    private void MoveRight()
    {
        onSwipeRight?.Invoke();

        if (_curentValueX >= radius) return;

        float endValue = _curentValueX + radius;
        _curentValueX = endValue;

        transform.DOKill();
        _image.transform.DOLocalMoveX(endValue, moveDuration)
            .SetEase(Ease.Linear);
    }
}
