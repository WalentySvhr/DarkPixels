using UnityEngine;

public class LocalTeleport : MonoBehaviour

{
    [Header("Налаштування телепорту")]
    public Transform targetLocation; // Точка, куди перемістити гравця
    public bool isActive = false;    // Чи відкриті двері
    public string locationName = "Відкритий світ";
    public LocationAnnouncer announcer; // Перетягни сюди Canvas зі скриптом

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи це гравець і чи двері активні
        if (isActive && collision.CompareTag("Player"))
        {
            // Миттєво змінюємо позицію гравця
            collision.transform.position = targetLocation.position;
            if (announcer != null)
            {
                announcer.ShowLocation(locationName);
            }
            Debug.Log("Локальний телепорт спрацював!");

            // Якщо камера Cinemachine "тупить", можна змусити її миттєво перестрибнути
            // (Розкоментуй нижче, якщо використовуєш Cinemachine)
            // var vcam = GameObject.FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
            // if (vcam != null) vcam.OnTargetObjectWarped(collision.transform, targetLocation.position - collision.transform.position);
        }
    }

    public void OpenDoor()
    {
        isActive = true;
        // Візуальний ефект (зміна кольору на зелений, щоб гравець зрозумів, що двері працюють)
        GetComponent<SpriteRenderer>().color = Color.green;
    }
}