using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilitySlotUI : MonoBehaviour
{
    [Header("Компоненти UI всередині *Префабу*")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Button skillIconButton;
    [SerializeField] private Image equippedOverlay;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;

    [Header("Налаштування кольорів для станів")]
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color unequippedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color equippedColor = new Color(1f, 1f, 1f, 1f);

    [Header("Налаштування Текстів")]
    [SerializeField] private string levelPrefix = "Рівень ";

    // Текст блокування, який можна вільно редагувати та стилізувати в інспекторі
    [SerializeField] private string lockedStatusText = "<color=red>Заблоковано</color>";

    [TextArea(2, 4)]
    [SerializeField] private string missingDescriptionTemplate = "<color=orange>Опис вміння відсутній!</color>";

    private AbilitySO targetAbility;

    // 🔴 ВИДАЛЕНО: private static AbilitySO currentlyEquippedAbility; 
    // Більше не зберігаємо стан екіпірування в UI, щоб уникнути розсинхронізації при завантаженні сейвів.

    public void Initialize(AbilitySO data)
    {
        targetAbility = data;
        UpdateSlotUI();

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(UpgradeSkill);

        if (skillIconButton != null)
        {
            skillIconButton.onClick.RemoveAllListeners();
            skillIconButton.onClick.AddListener(EquipThisSkill);
        }
    }

    public void UpdateSlotUI()
    {
        if (targetAbility == null) return;

        if (skillIcon != null) skillIcon.sprite = targetAbility.abilityIcon;
        if (titleText != null) titleText.text = targetAbility.abilityName;

        if (levelText != null)
        {
            levelText.text = targetAbility.currentLevel <= 0 ? lockedStatusText : $"{levelPrefix}{targetAbility.currentLevel}";
        }

        if (descriptionText != null)
        {
            string finalDescription = targetAbility.descriptionTemplate;
            if (string.IsNullOrEmpty(finalDescription)) finalDescription = missingDescriptionTemplate;
            else
            {
                // Якщо рівень 0, показуємо стати для 1-го рівня, щоб гравець знав, що купує
                int displayLevel = targetAbility.currentLevel == 0 ? 1 : targetAbility.currentLevel;
                int displayDamage = targetAbility.baseDamage + (displayLevel - 1) * targetAbility.damageIncreasePerLevel;

                // Розрахунок мани для відображення в UI
                float displayMana = targetAbility.manaCost + (displayLevel - 1) * targetAbility.manaCostIncreasePerLevel;

                finalDescription = finalDescription.Replace("{name}", targetAbility.abilityName);
                finalDescription = finalDescription.Replace("{level}", displayLevel.ToString());
                finalDescription = finalDescription.Replace("{damage}", displayDamage.ToString());
                finalDescription = finalDescription.Replace("{radius}", targetAbility.radius.ToString());

                // Обробка нових тегів {mana} та {price}
                finalDescription = finalDescription.Replace("{mana}", displayMana.ToString());
                finalDescription = finalDescription.Replace("{price}", targetAbility.GetUpgradeCost().ToString());
            }
            descriptionText.text = finalDescription;
        }

        if (costText != null) costText.text = targetAbility.GetUpgradeCost().ToString();

        if (upgradeButton != null)
        {
            var btnText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = targetAbility.currentLevel <= 0 ? "Купити" : "Прокачати";
            }
        }

        UpdateSlotVisualState();
    }

    public void UpdateSlotVisualState()
    {
        if (skillIcon == null) return;

        if (targetAbility.currentLevel <= 0)
        {
            skillIcon.color = lockedColor;
            if (skillIconButton != null) skillIconButton.interactable = false;
            if (equippedOverlay != null) equippedOverlay.gameObject.SetActive(false);
        }
        // 🌟 ОНОВЛЕНО: Перевіряємо безпосередньо через єдине джерело правди — CombatAbilityButton
        else if (CombatAbilityButton.Instance != null && CombatAbilityButton.Instance.equippedAbility == targetAbility)
        {
            skillIcon.color = equippedColor;
            if (skillIconButton != null) skillIconButton.interactable = true;
            if (equippedOverlay != null) equippedOverlay.gameObject.SetActive(true);
        }
        else
        {
            skillIcon.color = unequippedColor;
            if (skillIconButton != null) skillIconButton.interactable = true;
            if (equippedOverlay != null) equippedOverlay.gameObject.SetActive(false);
        }
    }

    private void UpgradeSkill()
    {
        int price = targetAbility.GetUpgradeCost();

        if (InventoryManager.Instance != null && InventoryManager.Instance.coins >= price)
        {
            // Списуємо монети та додаємо рівень у пам'яті гри
            InventoryManager.Instance.ChangeCoins(-price);
            targetAbility.currentLevel++;

            // Миттєво оновлюємо UI слотів, щоб гравець бачив нову ціну, шкоду та вартість мани
            var allSlots = transform.parent.GetComponentsInChildren<AbilitySlotUI>();
            foreach (var slot in allSlots)
            {
                slot.UpdateSlotUI();
            }

            Debug.Log($"[Магазин] Скіл {targetAbility.abilityName} прокачано до {targetAbility.currentLevel} лвл у пам'яті.");
        }
    }

    private void EquipThisSkill()
    {
        if (targetAbility != null && targetAbility.currentLevel >= 1)
        {
            if (CombatAbilityButton.Instance != null)
            {
                // 🌟 ОНОВЛЕНО: Керуємо екіпіруванням безпосередньо через CombatAbilityButton
                if (CombatAbilityButton.Instance.equippedAbility == targetAbility)
                {
                    CombatAbilityButton.Instance.EquipAbility(null); // Знімаємо магію, якщо клікнули по вже активній
                }
                else
                {
                    CombatAbilityButton.Instance.EquipAbility(targetAbility); // Екіпіруємо нову
                }

                // Оновлюємо візуальний вигляд усіх слотів у меню магії
                var allSlots = transform.parent.GetComponentsInChildren<AbilitySlotUI>();
                foreach (var slot in allSlots)
                {
                    slot.UpdateSlotVisualState();
                }
            }
        }
    }
}