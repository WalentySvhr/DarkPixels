using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OfflineProfitNPC : MonoBehaviour
{
    [Header("Візуальні підказки")]
    public GameObject readyIndicator;
    public GameObject miniMapIcon;

    private bool isPlayerNear = false;

    private void Update()
    {
        bool hasCoins = false;
        if (OfflineProfitManager.Instance != null)
        {
            hasCoins = Mathf.FloorToInt(OfflineProfitManager.Instance.accumulatedCoins) > 0;
        }

        if (readyIndicator != null) readyIndicator.SetActive(hasCoins);
        if (miniMapIcon != null) miniMapIcon.SetActive(hasCoins);
    }

    private void OnMouseDown()
    {


        if (!isPlayerNear)
        {

            return;
        }

        if (OfflineProfitManager.Instance == null)
        {

            return;
        }

        if (OfflineProfitWindow.Instance == null)
        {

            return;
        }

        int amount = Mathf.FloorToInt(OfflineProfitManager.Instance.accumulatedCoins);


        OfflineProfitWindow.Instance.OpenWindow(amount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}