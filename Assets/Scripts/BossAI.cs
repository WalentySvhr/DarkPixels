using UnityEngine;

public class BossAI : MonoBehaviour
{
    public Transform player;
    public float attackRange = 5f;    // Дистанція, на якій він почне атакувати
    public float attackCooldown = 2f; // Пауза між ударами

    private float nextAttackTime;
    private bool facingRight = true;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        // Автоматично шукаємо гравця, якщо забули прикріпити в інспекторі
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. ПОВОРОТ ДО ГРАВЦЯ (щоб завжди дивився на нас)
        LookAtPlayer();

        // 2. ПЕРЕВІРКА ДИСТАНЦІЇ ДЛЯ АТАКИ
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void LookAtPlayer()
    {
        float diff = player.position.x - transform.position.x;

        // Додаємо невелику мертву зону (0.2), щоб він не смикався, коли гравець прямо в ньому
        if (diff > 0.2f && !facingRight) Flip();
        else if (diff < -0.2f && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Attack()
    {
        Debug.Log("БОС АТАКУЄ!");
        if (anim != null)
        {
            anim.SetTrigger("attack"); // Переконайся, що в Animator параметр називається саме "attack"
        }
    }
}