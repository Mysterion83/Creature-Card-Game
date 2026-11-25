using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _speed = 60f;
    [SerializeField] private float _startDelay = 1f;
    [SerializeField] private float _endDelay = 2f;
    [SerializeField] private bool _loop = true;

    private Coroutine _routine;

    private void Awake()
    {
        if (_scrollRect == null) _scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    private void OnEnable()
    {
        if (_scrollRect != null) _routine = StartCoroutine(ScrollCredits());
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
    }

    private IEnumerator ScrollCredits()
    {
        do
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 1f;

            yield return new WaitForSecondsRealtime(_startDelay);

            while (_scrollRect.verticalNormalizedPosition > 0f)
            {
                float scrollable = _scrollRect.content.rect.height - _scrollRect.viewport.rect.height;
                if (scrollable <= 0f) yield break;

                float step = _speed * Time.unscaledDeltaTime / scrollable;
                _scrollRect.verticalNormalizedPosition = Mathf.Max(0f, _scrollRect.verticalNormalizedPosition - step);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(_endDelay);
        }
        while (_loop);
    }
}
