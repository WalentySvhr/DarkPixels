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

        public ActiveAbilityState(AbilitySO data)
        {
            this.data = data;
            this.isActive = false;
            this.nextTickTime = 0f;
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

        // ПРИМУСОВЕ ЗАВАНТАЖЕННЯ JSON ОДРАЗУ ПІСЛЯ ОБНУЛЕННЯ
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
            RegisterAbility(ability);
        }
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
        // ЗМІНЕНО: тепер береться мана з урахуванням рівня
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

        if (state.isActive) state.nextTickTime = 0f;
        else StopToggleableAbility(data);
    }

    private void Update()
    {
        foreach (var pair in abilityStates)
        {
            AbilitySO data = pair.Key;
            ActiveAbilityState state = pair.Value;

            if (data.type == AbilityType.Toggleable && state.isActive)
            {
                // Таймер йде постійно, поки аура увімкнена
                state.nextTickTime -= Time.deltaTime;

                if (state.nextTickTime <= 0f)
                {
                    // Вираховуємо, скільки мани потрібно рівно на ОДИН удар.
                    // (Ціна за секунду * частоту ударів).
                    float costPerTick = data.GetCurrentManaCost() * data.tickRate;

                    // Спробуємо витратити ману одразу всією необхідною порцією
                    if (playerMana.TrySpendMana(costPerTick))
                    {
                        // Мани вистачило! Б'ємо і скидаємо таймер.
                        ApplyAoEDamage(data);
                        state.nextTickTime = data.tickRate;
                    }
                    else
                    {
                        // Якщо повноцінної суми мани немає — тримаємо таймер на нулі.
                        // Аура "напоготові" і вдарить миттєво, щойно гравець накопичить costPerTick.
                        state.nextTickTime = 0f;
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
        if (abilityStates.ContainsKey(data)) abilityStates[data].isActive = false;
    }

    private void SpawnVisualEffect(AbilitySO data)
    {
        if (data.visualEffectPrefab != null)
        {
            Instantiate(data.visualEffectPrefab, transform.position, Quaternion.identity, transform);
        }
    }
}