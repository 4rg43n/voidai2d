using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuSource : MonoBehaviour, IPointerClickHandler
{
    public bool consumeRightClick = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (ContextMenuManager.Instance == null) return;

        // Ask the manager to build from providers on this object/parents
        ContextMenuManager.Instance.ShowFor(gameObject, eventData.position);

        if (consumeRightClick)
            eventData.Use();
    }
}
