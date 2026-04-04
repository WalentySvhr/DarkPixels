using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Налаштування слота")]
    public bool isWeaponEquipmentSlot = false; // ГАЛОЧКА ДЛЯ СЛОТА ЗБРОЇ

    public Image icon;
    public TextMeshProUGUI stackText;

    public Item currentItem;
    public int currentAmount;

    private Canvas groupCanvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        groupCanvas = GetComponentInParent<Canvas>();
        canvasGroup = icon.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
    }

    public void AddItem(Item newItem, int amount)
    {
        if (newItem == null) return;

        currentItem = newItem;
        currentAmount = amount;

        icon.sprite = newItem.icon;
        icon.enabled = true;
        icon.color = Color.white;

        if (stackText != null)
        {
            stackText.text = (newItem.isStackable && amount > 1) ? amount.ToString() : "";
            stackText.gameObject.SetActive(newItem.isStackable && amount > 1);
        }

        // ЯКЩО ЦЕ СЛОТ ЗБРОЇ — ОДЯГАЄМО ЇЇ ПРИ ДОДАВАННІ
        if (isWeaponEquipmentSlot && newItem is WeaponData weaponData)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(weaponData);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;
        if (stackText != null) stackText.gameObject.SetActive(false);

        // ЯКЩО ЦЕ СЛОТ ЗБРОЇ І МИ ЙОГО ОЧИСТИЛИ — ЗНІМАЄМО ЗБРОЮ
        if (isWeaponEquipmentSlot)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(null); // Передаємо null, щоб зняти
        }
    }

    public void OnSlotClicked()
    {
        // Якщо це слот зброї, клік по ньому нічого не робить (ми тільки тягаємо)
        if (isWeaponEquipmentSlot) return;

        if (currentItem != null)
        {
            InventoryManager.Instance.UseItem(currentItem);
        }
    }

    // --- ЛОГІКА ПЕРЕТЯГУВАННЯ ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        icon.transform.SetParent(groupCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        icon.rectTransform.anchoredPosition += eventData.delta / groupCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        icon.transform.SetParent(this.transform);
        icon.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();

        if (sourceSlot != null && sourceSlot != this)
        {
            // ЗАХИСТ: Якщо тягнемо в слот зброї, перевіряємо, чи це зброя
            if (this.isWeaponEquipmentSlot && sourceSlot.currentItem != null)
            {
                if (!(sourceSlot.currentItem is WeaponData))
                {
                    Debug.Log("Сюди можна класти ТІЛЬКИ зброю!");
                    return;
                }
            }

            // ЗАХИСТ: Якщо замінюємо екіпіровану зброю на щось інше
            if (sourceSlot.isWeaponEquipmentSlot && this.currentItem != null)
            {
                if (!(this.currentItem is WeaponData))
                {
                    Debug.Log("Не можна замінити екіпіровану зброю на не-зброю!");
                    return;
                }
            }

            Item itemToMove = sourceSlot.currentItem;
            int amountToMove = sourceSlot.currentAmount;

            Item itemToReplace = this.currentItem;
            int amountToReplace = this.currentAmount;

            // СЦЕНАРІЙ 1: Тягнемо З РЮКЗАКА В ЕКІПІРОВКУ
            if (this.isWeaponEquipmentSlot && !sourceSlot.isWeaponEquipmentSlot)
            {
                this.AddItem(itemToMove, amountToMove); // Візуально одягаємо
                InventoryManager.Instance.Remove(itemToMove); // ВИДАЛЯЄМО З РЮКЗАКА (Бекенд)

                // Якщо зняли стару зброю, кладемо її в рюкзак
                if (itemToReplace != null)
                {
                    InventoryManager.Instance.Add(itemToReplace);
                }
            }
            // СЦЕНАРІЙ 2: Тягнемо З ЕКІПІРОВКИ В РЮКЗАК (Знімаємо зброю)
            else if (!this.isWeaponEquipmentSlot && sourceSlot.isWeaponEquipmentSlot)
            {
                sourceSlot.ClearSlot(); // Знімаємо зброю
                InventoryManager.Instance.Add(itemToMove); // ДОДАЄМО В РЮКЗАК (Бекенд)

                // Якщо кинули прямо на іншу зброю в рюкзаку - екіпіруємо її
                if (itemToReplace != null)
                {
                    sourceSlot.AddItem(itemToReplace, amountToReplace);
                    InventoryManager.Instance.Remove(itemToReplace);
                }
            }
            // СЦЕНАРІЙ 3: Перетягування всередині самого рюкзака
            else
            {
                this.AddItem(itemToMove, amountToMove);
                if (itemToReplace != null) sourceSlot.AddItem(itemToReplace, amountToReplace);
                else sourceSlot.ClearSlot();
            }
        }
    }
}