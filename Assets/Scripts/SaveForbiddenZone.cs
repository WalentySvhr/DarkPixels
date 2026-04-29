using UnityEngine;

public class SaveForbiddenZone : MonoBehaviour
{
    public static bool CanSave = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CanSave = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CanSave = true;
        }
    }
}