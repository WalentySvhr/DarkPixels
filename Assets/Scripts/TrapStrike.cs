using UnityEngine;
using System.Collections;

public class TrapSpike : MonoBehaviour
{
    public enum TrapType { AlwaysActive, Cyclic }

    [Header("Тип пастки")]
    public TrapType trapType = TrapType.Cyclic;

    [Header("Налаштування шкоди")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Таймінги (Тільки для Cyclic)")]
    [SerializeField] private float inactiveDuration = 2f;
    [SerializeField] private float activeDuration = 1.5f;

    [Header("Візуалізація")]
    [SerializeField] private Animator animator;

    private bool isTrapActive = true;
    private bool isPlayerOnTrap = false;
    private float nextDamageTime = 0f;
    private PlayerHealth playerHealth;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (trapType == TrapType.AlwaysActive)
        {
            SetTrapState(true);
        }
        else
        {
            StartCoroutine(TrapCycleRoutine());
        }
    }

    private IEnumerator TrapCycleRoutine()
    {
        while (true)
        {
            // 1. Шипи сховані (безпечно)
            SetTrapState(false);
            yield return new WaitForSeconds(inactiveDuration);

            // 2. Шипи вилазять (небезпечно)
            SetTrapState(true);

            // Чекаємо стільки секунд, скільки вказано в Інспекторі
            yield return new WaitForSeconds(activeDuration);
        }
    }

    private void SetTrapState(bool isActive)
    {
        isTrapActive = isActive;

        if (animator != null)
        {
            // Якщо ми ховаємо шипи, ОБО'В'ЯЗКОВО повертаємо швидкість анімації в 1,
            // щоб аніматор зміг розморозитися і запустити перехід назад
            if (!isActive)
            {
                animator.speed = 1f;
            }

            animator.SetBool("IsActive", isActive);
        }

        if (isTrapActive && isPlayerOnTrap)
        {
            TryDealDamage();
        }
    }

    // ==========================================
    // 🔥 ЦЕЙ МЕТОД ВИКЛИКАЄТЬСЯ ЧЕРЕЗ ANIMATION EVENT
    // ==========================================
    public void FreezeSpikesOnFrame()
    {
        // Якщо пастка зараз має бути активною за логікою коду,
        // ми просто «зарожуємо» анімацію на поточному кадрі
        if (animator != null && isTrapActive)
        {
            animator.speed = 0f;
            Debug.Log("<color=yellow>[TRAP]</color> Анімація заморожена на гарячому кадрі!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            isPlayerOnTrap = true;

            if (isTrapActive)
            {
                TryDealDamage();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isPlayerOnTrap && isTrapActive && Time.time >= nextDamageTime)
        {
            TryDealDamage();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnTrap = false;
            playerHealth = null;
        }
    }

    private void TryDealDamage()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            nextDamageTime = Time.time + damageCooldown;
        }
    }
}