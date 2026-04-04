using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Налаштування слота")]
    public bool isWeaponEquipmentSlot = false; // ГАЛОЧКА ДЛЯ СЛОТА ЗБРОЇ
    public bool isAmuletEquipmentSlot = false; // ГАЛОЧКА ДЛЯ СЛОТА КУЛОНА

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
        // ЯКЩО ЦЕ СЛОТ КУЛОНА — ЗАСТОСОВУЄМО ЙОГО ЕФЕКТИ
        if (isAmuletEquipmentSlot && newItem is AmuletData amulet)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.AddBonusHealth(amulet.bonusMaxHealth);
                Debug.Log("Одягнено кулон! Додано здоров'я: " + amulet.bonusMaxHealth);
            }
        }
    }

    public void ClearSlot()
    {
        // 1. СПОЧАТКУ ЗНІМАЄМО БОНУСИ Й ЕКІПІРОВКУ (Поки currentItem ще не стерто з пам'яті!)

        // Знімаємо зброю
        if (isWeaponEquipmentSlot)
        {
            PlayerCombat combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) combat.EquipWeapon(null);
        }

        // Знімаємо кулон
        if (isAmuletEquipmentSlot && currentItem is AmuletData oldAmulet)
        {
            PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.RemoveBonusHealth(oldAmulet.bonusMaxHealth);
                Debug.Log("Кулон знято! Здоров'я зменшилось.");
            }
        }

        // 2. І ТІЛЬКИ ПОТІМ ОЧИЩАЄМО ДАНІ САМОГО СЛОТА
        currentItem = null;
        currentAmount = 0;
        icon.sprite = null;
        icon.enabled = false;
        if (stackText != null) stackText.gameObject.SetActive(false);
    }

    public void OnSlotClicked()
    {
        // Якщо це слот зброї або кулона, клік по ньому нічого не робить (ми їх тільки тягаємо)
        if (isWeaponEquipmentSlot || isAmuletEquipmentSlot) return;

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
            // УНІВЕРСАЛЬНА ПЕРЕВІРКА: чи є цей слот або слот-джерело слотом екіпіровки?
            bool thisIsEquipSlot = this.isWeaponEquipmentSlot || this.isAmuletEquipmentSlot;
            bool sourceIsEquipSlot = sourceSlot.isWeaponEquipmentSlot || sourceSlot.isAmuletEquipmentSlot;

            // --- ЗАХИСТ ВІД НЕПРАВИЛЬНИХ ПРЕДМЕТІВ ---
            if (this.isWeaponEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is WeaponData)) return;
            if (this.isAmuletEquipmentSlot && sourceSlot.currentItem != null && !(sourceSlot.currentItem is AmuletData)) return;
            if (sourceSlot.isWeaponEquipmentSlot && this.currentItem != null && !(this.currentItem is WeaponData)) return;
            if (sourceSlot.isAmuletEquipmentSlot && this.currentItem != null && !(this.currentItem is AmuletData)) return;

            Item itemToMove = sourceSlot.currentItem;
            int amountToMove = sourceSlot.currentAmount;
            Item itemToReplace = this.currentItem;
            int amountToReplace = this.currentAmount;

            // СЦЕНАРІЙ 1: З РЮКЗАКА В ЕКІПІРОВКУ (Зброя АБО Амулет)
            if (thisIsEquipSlot && !sourceIsEquipSlot)
            {
                this.AddItem(itemToMove, amountToMove);
                sourceSlot.ClearSlot();
                InventoryManager.Instance.Remove(itemToMove);

                if (itemToReplace != null)
                {
                    sourceSlot.AddItem(itemToReplace, amountToReplace);
                    InventoryManager.Instance.Add(itemToReplace);
                }
            }
            // СЦЕНАРІЙ 2: З ЕКІПІРОВКИ В РЮКЗАК
            else if (!thisIsEquipSlot && sourceIsEquipSlot)
            {
                this.AddItem(itemToMove, amountToMove);
                sourceSlot.ClearSlot();
                InventoryManager.Instance.Add(itemToMove);

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