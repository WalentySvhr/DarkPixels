using UnityEngine;
using UnityEngine.UI;

public class CombatAbilityButton : MonoBehaviour
{
    // Глобальний доступ (Синглтон)
    public static CombatAbilityButton Instance { get; private set; }

    [Header("UI Компоненти кнопки")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image activeOverlay; // Напівпрозора рамка активації аури

    // Публічна властивість для читання зі слотів магазину
    public AbilitySO equippedAbility { get; private set; }

    private bool isToggled = false;

    private void Awake()
    {
        // Ініціалізуємо синглтон при старті сцени
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        UpdateVisuals();
    }

    // Метод для призначення магії на цю кнопку з UI магазину
    public void EquipAbility(AbilitySO newAbility)
    {
        // 1. ПЕРЕВІРКА НА ЗНЯТТЯ: Якщо гравець знімає вміння (newAbility == null)
        if (newAbility == null && equippedAbility != null)
        {
            // Якщо це була активована аура/зона і вона зараз ГОРИТЬ (isToggled == true)
            if (isToggled && equippedAbility.type == AbilityType.Toggleable && AbilityManager.Instance != null)
            {
                // Смикаємо UseAbility, щоб AbilityManager вимкнув ефект аури на сцені
                AbilityManager.Instance.UseAbility(equippedAbility);
                Debug.Log($"[HUD Button] Активну ауру {equippedAbility.abilityName} вимкнено перед видаленням.");
            }
        }

        // 2. ОНОВЛЕННЯ ДАНИХ: Записуємо нове вміння та СКИДАЄМО ВСІ СТАНИ
        equippedAbility = newAbility;
        isToggled = false; // Нове вміння або порожній слот ЗАВЖДИ стартують як вимкнені

        // 3. ВІЗУАЛ: Перемальовуємо кнопку на HUD
        UpdateVisuals();
    }

    private void OnButtonClicked()
    {
        if (equippedAbility == null || AbilityManager.Instance == null) return;

        // Передаємо команду активації в менеджер вмінь
        AbilityManager.Instance.UseAbility(equippedAbility);

        // Якщо це аура, перемикаємо стан підсвітки кнопки
        if (equippedAbility.type == AbilityType.Toggleable)
        {
            isToggled = !isToggled;
        }
        else
        {
            isToggled = false; // Звичайні скіли (напр. фаєрбол) не залишаються натиснутими
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        Button mainButton = GetComponent<Button>();

        // СТАН 1: Скіл НЕ екіпіровано або його ЗНЯЛИ (порожній слот на HUD)
        if (equippedAbility == null)
        {
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            if (activeOverlay != null) activeOverlay.gameObject.SetActive(false);

            if (mainButton != null)
            {
                mainButton.interactable = false; // Кнопка повністю блокується на HUD
                if (mainButton.targetGraphic != null) mainButton.targetGraphic.color = Color.white;
            }
        }
        // СТАН 2: Скіл успішно вибрано і він відображається
        else
        {
            if (iconImage != null)
            {
                if (equippedAbility.abilityIcon != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = equippedAbility.abilityIcon;
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                    Debug.LogWarning($"[HUD Button] У вміння {equippedAbility.abilityName} відсутня іконка в SO!");
                }
            }

            // Керуємо рамкою підсвітки
            if (activeOverlay != null)
            {
                activeOverlay.gameObject.SetActive(isToggled);
            }

            if (mainButton != null)
            {
                mainButton.interactable = true; // Робимо кнопку КЛІКАБЕЛЬНОЮ на головному екрані

                // Зміна кольору кнопки залежно від стану аури
                if (mainButton.targetGraphic != null)
                {
                    mainButton.targetGraphic.color = isToggled ? new Color(0.8f, 1f, 0.8f, 1f) : Color.white;
                }
            }
        }
    }

    // Метод примусового вимкнення підсвітки (наприклад, якщо закінчилася мана)
    public void ForceUntoggle()
    {
        isToggled = false;
        if (activeOverlay != null) activeOverlay.gameObject.SetActive(false);

        Button mainButton = GetComponent<Button>();
        if (mainButton != null && mainButton.targetGraphic != null)
        {
            mainButton.targetGraphic.color = Color.white;
        }
    }
}