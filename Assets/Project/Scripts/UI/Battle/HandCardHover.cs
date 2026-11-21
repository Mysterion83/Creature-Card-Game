using UnityEngine;
using UnityEngine.EventSystems;

public class HandCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsHovered { get; private set; }

    public void OnPointerEnter(PointerEventData eventData) => IsHovered = true;
    public void OnPointerExit(PointerEventData eventData) => IsHovered = false;

    private void OnDisable() => IsHovered = false;
}
