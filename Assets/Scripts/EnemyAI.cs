using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float checkRadius = 5f; 
    
    // Переконайся, що тут саме false
    public bool isAggroedByDamage = false; 

    private Transform target;
    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // Логіка:
        // 1. Спочатку перевіряємо, чи ми вже "заагрені" від удару
        if (isAggroedByDamage)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        // 2. Якщо ні, то перевіряємо чи гравець в радіусі
        else if (distance <= checkRadius)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        // 3. Якщо нічого не підходить, стоїмо
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        // Якщо moveDirection == 0, ворог не рухається
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
        
        if (moveDirection.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveDirection.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}