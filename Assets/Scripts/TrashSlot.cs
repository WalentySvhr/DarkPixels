using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();

            if (sourceSlot != null && sourceSlot.currentItem != null)
            {
                // Видаляємо предмет із загальної бази. 
                // InventoryManager має САМ оновити всі слоти після цього.
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.Remove(sourceSlot.currentItem);
                }

                // sourceSlot.ClearSlot(); <--- МИ ПРИБРАЛИ ЦЕЙ РЯДОК

                Debug.Log("<color=red>Предмет знищено в кошику!</color>");
            }
        }
    }
}