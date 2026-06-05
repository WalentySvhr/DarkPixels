using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    // Менеджери викликають саме це ім'я. Воно має збігатися в усіх скриптах.
    public void StartInteraction()
    {
        // Сюди можна додати логіку: наприклад, зупинку анімації ходьби
        Debug.Log("Взаємодію з " + gameObject.name);
    }

    public void StopInteraction()
    {
        // Сюди можна додати логіку: відновлення анімації ходьби
        Debug.Log("Зупинив взаємодію з " + gameObject.name);
    }
}