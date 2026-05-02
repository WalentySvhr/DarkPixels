using UnityEngine;
using System.Collections;

public class PeriodicAdUI : MonoBehaviour
{
    [Header("Налаштування часу (у секундах)")]
    public float cooldownTime = 300f;
    public float activeTime = 60f;

    [Header("Кнопка")]
    public GameObject adButtonObject;

    void Start()
    {
        adButtonObject.SetActive(false);
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldownTime);

            adButtonObject.SetActive(true);
            Debug.Log("З'явилася періодична пропозиція золота!");

            yield return new WaitForSeconds(activeTime);

            if (adButtonObject.activeSelf)
            {
                adButtonObject.SetActive(false);
                Debug.Log("Пропозиція золота зникла.");
            }
        }
    }

    public void OnRewardClaimed()
    {
        adButtonObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine());
    }
}