using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform attackPoint;
    public LayerMask enemyLayers;

    public WeaponData currentWeaponData;
    private GameObject spawnedWeapon;
    private float nextAttackTime = 0f;

    [Header("Start Weapon")]
    public WeaponData startingWeapon;

    void Start()
    {
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        if (newData == null) return;

        currentWeaponData = newData;

        if (spawnedWeapon != null) Destroy(spawnedWeapon);

        if (newData.visualPrefab != null)
        {
            spawnedWeapon = Instantiate(newData.visualPrefab, weaponHolder);
            spawnedWeapon.transform.localPosition = Vector3.zero;
            spawnedWeapon.transform.localRotation = Quaternion.identity;

            // Оновлюємо швидкість анімації зброї під її кулдаун
            newData.UpdateAnimationSpeed(spawnedWeapon.GetComponent<Animator>());
        }
    }

    public void OnAttackButton()
    {
        if (currentWeaponData == null || Time.time < nextAttackTime) return;

        StartCoroutine(PerformAttack());
        nextAttackTime = Time.time + currentWeaponData.cooldown;
    }

    IEnumerator PerformAttack()
    {
        // 1. Запуск анімації на моделі зброї
        if (spawnedWeapon != null)
        {
            Animator anim = spawnedWeapon.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Attack");
        }

        // 2. Пауза до моменту візуального удару
        yield return new WaitForSeconds(currentWeaponData.damageDelay);

        // 3. Логіка нанесення урону
        if (currentWeaponData.type == WeaponType.Melee)
        {
            MeleeDamage();
        }
        else if (currentWeaponData.type == WeaponType.Ranged)
        {
            RangedShot();
        }
    }

    void MeleeDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // ОБЧИСЛЮЄМО НАПРЯМОК ВІДШТОВХУВАННЯ
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;

                // ПЕРЕДАЄМО ВСІ 3 ПАРАМЕТРИ (урон, напрямок, сила)
                // Силу (напр. 10f) можна теж винести у WeaponData згодом
                enemyHealth.TakeDamage(currentWeaponData.damage, knockbackDir, 10f);

                Debug.Log("Попав по ворогу: " + enemy.name);
            }
        }
    }

    void RangedShot()
    {
        if (currentWeaponData.projectilePrefab != null)
        {
            GameObject proj = Instantiate(currentWeaponData.projectilePrefab, attackPoint.position, attackPoint.rotation);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.AddForce(attackPoint.right * currentWeaponData.shootForce, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || currentWeaponData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, currentWeaponData.attackRange);
    }
}