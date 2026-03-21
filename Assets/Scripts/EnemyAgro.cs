using UnityEngine;

public class EnemyAgro : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;          // Швидкість руху моба
    public float stopDistance = 1.2f; // Дистанція, на якій моб зупиниться перед гравцем

    [Header("State")]
    public bool isAggro = false;      // Чи агресивний моб зараз
    
    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        // Шукаємо гравця за тегом "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Якщо моб агряться і гравець існує
        if (isAggro && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > stopDistance)
            {
                // Рух до гравця
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);

                // Поворот спрайту ворога в бік руху
                if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                else if (direction.x < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                // Зупинка, якщо підійшов впритул
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    // Метод, який ми викличемо з EnemyHealth при отриманні дамагу
    public void StartAggro()
    {
        isAggro = true;
        Debug.Log(gameObject.name + " розлютився!");
    }
}