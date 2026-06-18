using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [System.Serializable]
    private class ActiveAbilityState
    {
        public AbilitySO data;
        public bool isActive;
        public float nextTickTime;
        public GameObject spawnedFX;

        public ActiveAbilityState(AbilitySO data)
        {
            this.data = data;
            this.isActive = false;
            this.nextTickTime = 0f;
            this.spawnedFX = null;
        }
    }

    [Header("Налаштування Вмінь гравця")]
    [SerializeField] private List<AbilitySO> activeAbilities = new List<AbilitySO>();

    private PlayerMana playerMana;
    private Dictionary<AbilitySO, ActiveAbilityState> abilityStates = new Dictionary<AbilitySO, ActiveAbilityState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Жорстке скидання для редактора Unity, щоб рівні не залишалися з минулого запуску
        foreach (var ability in activeAbilities)
        {
            if (ability != null)
            {
                ability.currentLevel = 0;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(ability);
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log("[AbilityManager] Рівні абілок скинуто в 0 перед завантаженням JSON.");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    private void Start()
    {
        playerMana = GetComponent<PlayerMana>();
        if (playerMana == null)
        {
            Debug.LogError($"[AbilityManager] На об'єкті {gameObject.name} не знайдено PlayerMana!");
        }

        foreach (var ability in activeAbilities)
        {
            var state = abilityStates.ContainsKey(ability); // Безпечна ініціалізація у Start
            RegisterAbility(ability);
        }

        // 🌟 НОВЕ: Відновлюємо екіпірований скіл після завантаження
        StartCoroutine(RestoreEquippedAbilityDelayed());
    }

    public void RegisterAbility(AbilitySO newAbility)
    {
        if (newAbility == null) return;

        if (!abilityStates.ContainsKey(newAbility))
        {
            abilityStates.Add(newAbility, new ActiveAbilityState(newAbility));
            if (!activeAbilities.Contains(newAbility))
            {
                activeAbilities.Add(newAbility);
            }
        }
    }

    public void UseAbility(AbilitySO abilityData)
    {
        if (abilityData == null || playerMana == null) return;

        if (!abilityStates.ContainsKey(abilityData)) RegisterAbility(abilityData);

        if (abilityData.type == AbilityType.Instant) ExecuteInstantAbility(abilityData);
        else if (abilityData.type == AbilityType.Toggleable) ToggleAbility(abilityData);
    }

    private void ExecuteInstantAbility(AbilitySO data)
    {
        float currentManaCost = data.GetCurrentManaCost();

        if (playerMana.TrySpendMana(currentManaCost))
        {
            Debug.Log($"Активовано разовий скіл: {data.abilityName} (Витрачено мани: {currentManaCost})");
            SpawnVisualEffect(data);
        }
    }

    private void ToggleAbility(AbilitySO data)
    {
        var state = abilityStates[data];
        state.isActive = !state.isActive;

        if (state.isActive)
        {
            state.nextTickTime = 0f;

            if (data.visualEffectPrefab != null && state.spawnedFX == null)
            {
                state.spawnedFX = Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
                state.spawnedFX.transform.localPosition = Vector3.zero;

                float visualScale = data.radius * 2f * 0.5f;
                state.spawnedFX.transform.localScale = new Vector3(visualScale, visualScale, 1f);
            }
        }
        else
        {
            StopToggleableAbility(data);
        }
    }

    private void Update()
    {
        foreach (var pair in abilityStates)
        {
            AbilitySO data = pair.Key;
            ActiveAbilityState state = pair.Value;

            if (data.type == AbilityType.Toggleable && state.isActive)
            {
                state.nextTickTime -= Time.deltaTime;

                if (state.nextTickTime <= 0f)
                {
                    float costPerTick = data.GetCurrentManaCost() * data.tickRate;

                    if (playerMana.TrySpendMana(costPerTick))
                    {
                        ApplyAoEDamage(data);
                        state.nextTickTime = data.tickRate;

                        if (data.visualEffectPrefab != null && state.spawnedFX == null)
                        {
                            state.spawnedFX = Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
                            state.spawnedFX.transform.localPosition = Vector3.zero;

                            float visualScale = data.radius * 2f;
                            state.spawnedFX.transform.localScale = new Vector3(visualScale, visualScale, 1f);
                        }
                    }
                    else
                    {
                        StopToggleableAbility(data);
                    }
                }
            }
        }
    }

    private void ApplyAoEDamage(AbilitySO data)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.radius);
        int currentDamage = data.GetCurrentDamage();

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Vector2 knockbackDirection = data.knockbackForce > 0f ?
                    ((Vector2)(enemyCollider.transform.position - transform.position)).normalized : Vector2.zero;

                enemy.TakeDamage(currentDamage, knockbackDirection, data.knockbackForce, false);
            }
        }
    }

    private void StopToggleableAbility(AbilitySO data)
    {
        if (abilityStates.ContainsKey(data))
        {
            var state = abilityStates[data];
            state.isActive = false;

            if (state.spawnedFX != null)
            {
                Destroy(state.spawnedFX);
                state.spawnedFX = null;
            }

            if (CombatAbilityButton.Instance != null && CombatAbilityButton.Instance.equippedAbility == data)
            {
                CombatAbilityButton.Instance.ForceUntoggle();
            }
        }
    }

    private void SpawnVisualEffect(AbilitySO data)
    {
        if (data.visualEffectPrefab != null)
        {
            Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (activeAbilities != null)
        {
            foreach (var ability in activeAbilities)
            {
                if (ability != null && ability.type == AbilityType.Toggleable)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawWireSphere(transform.position, ability.radius);
                }
            }
        }
    }

    // 🌟 НОВЕ: Метод, який SaveManager викликає перед збереженням
    public string GetEquippedAbilityIDForSave()
    {
        if (CombatAbilityButton.Instance != null && CombatAbilityButton.Instance.equippedAbility != null)
        {
            return CombatAbilityButton.Instance.equippedAbility.GetSaveKey();
        }
        return "";
    }

    // 🌟 ВИПРАВЛЕНО: Замінено пряме присвоєння на виклик методу EquipAbility()
    private System.Collections.IEnumerator RestoreEquippedAbilityDelayed()
    {
        // Чекаємо кінця кадру, щоб кнопка (CombatAbilityButton) точно встигла ініціалізуватися
        yield return new WaitForEndOfFrame();

        if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.CurrentData.equippedAbilityID))
        {
            string savedID = SaveManager.Instance.CurrentData.equippedAbilityID;

            // Завантажуємо всі можливі вміння з Resources, щоб знайти потрібне
            AbilitySO[] allAbilities = Resources.LoadAll<AbilitySO>("Abilities");
            foreach (var ability in allAbilities)
            {
                if (ability != null && ability.GetSaveKey() == savedID)
                {
                    if (CombatAbilityButton.Instance != null)
                    {
                        // Було: CombatAbilityButton.Instance.equippedAbility = ability;
                        // Стало (правильно):
                        CombatAbilityButton.Instance.EquipAbility(ability);

                        Debug.Log($"<color=cyan>[AbilityManager]</color> Відновлено скіл на кнопці: {ability.abilityName}");
                    }
                    break;
                }
            }
        }
    }
}