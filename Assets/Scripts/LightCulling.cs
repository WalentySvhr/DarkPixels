using UnityEngine;

public class LightCullingUnity6 : MonoBehaviour
{
    private Transform playerTransform;

    [Header("Optimization Settings")]
    [SerializeField] private float activationDistance = 12f; // Радіус активації
    [SerializeField] private float checkInterval = 0.4f;     // Як часто перевіряти (сек)

    [Header("Target To Toggle")]
    [SerializeField] private GameObject lightHolder;         // Об'єкт, який будемо вимикати

    private float timer;
    private bool isLightOn = true;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Якщо ти забув перетягнути об'єкт в інспекторі, 
        // скрипт спробує взяти перший дочірній об'єкт автоматично
        if (lightHolder == null && transform.childCount > 0)
        {
            lightHolder = transform.GetChild(0).gameObject;
        }

        timer = Random.Range(0f, checkInterval);
    }

    void Update()
    {
        if (playerTransform == null || lightHolder == null) return;

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckDistance();
        }
    }

    void CheckDistance()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool shouldBeOn = distance <= activationDistance;

        if (shouldBeOn != isLightOn)
        {
            isLightOn = shouldBeOn;

            // Повністю деактивуємо об'єкт зі світлом. 
            // В Unity 6 це примусово видаляє джерело з системи освітлення.
            lightHolder.SetActive(isLightOn);
        }
    }
}