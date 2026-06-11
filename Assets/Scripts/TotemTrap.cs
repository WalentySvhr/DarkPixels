using UnityEngine;

public class TotemTrap : MonoBehaviour
{
    [Header("Налаштування стрільби")]
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряда
    [SerializeField] private Transform firePoint;         // Точка спавну (якщо порожньо — буде сам тотем)
    [SerializeField] private float fireRate = 2.5f;       // Інтервал між пострілами (в секундах)
    [SerializeField] private int projectileCount = 4;     // Кількість снарядів за один залп (4, 6, 8 тощо)
    [SerializeField] private float projectileSpeed = 5f;  // Швидкість польоту снаряда
    [SerializeField] private int damage = 10;             // Шкода гравцю

    [Header("Анімація")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";

    private float fireCountdown = 0f;

    private void Start()
    {
        if (firePoint == null) firePoint = transform;
        if (animator == null) animator = GetComponent<Animator>();

        // Рандомізуємо початковий таймер, щоб тотеми на карті не стріляли абсолютно одночасно
        fireCountdown = Random.Range(0f, fireRate);
    }

    private void Update()
    {
        // Якщо гравець у башті (або використовуй свій глобальний менеджер паузи/стану гри)
        // Тут логіка активності пастки, якщо потрібно

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            ShootBurst();
            fireCountdown = fireRate;
        }
    }

    private void ShootBurst()
    {
        if (animator != null) animator.SetTrigger(shootTriggerName);

        // Вираховуємо крок кута між снарядами (наприклад, 360 / 4 = 90 градусів)
        float angleStep = 360f / projectileCount;
        float currentAngle = 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // Переводимо градуси в радіани для тригонометрії
            float rad = currentAngle * Mathf.Deg2Rad;

            // Вираховуємо вектор напрямку на основі кута
            Vector2 targetDirection = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)).normalized;

            // Рендеримо/спавнимо снаряд
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Налаштовуємо снаряд (передаємо напрямок, швидкість і шкоду)
            TotemProjectile projectileScript = proj.GetComponent<TotemProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(targetDirection, projectileSpeed, damage);
            }

            currentAngle += angleStep;
        }
    }
}