using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Цей клас дозволить нам створити список прямо в Інспекторі
[System.Serializable]
public class AdOffer
{
    public string offerName; // Просто назва для тебе (напр. "Золото")
    public AdsChecker.RewardType rewardType; // Який тип нагороди давати
    public Sprite offerIcon; // Іконка нагороди
    public string offerText; // Текст на кнопці
}

public class UniversalAdButton : MonoBehaviour
{
    [Header("Список можливих нагород")]
    public List<AdOffer> offers;

    [Header("UI Посилання")]
    public GameObject buttonContainer; // Сама кнопка (щоб вмикати/вимикати її повністю)
    public Image rewardIconImage;      // Картинка всередині кнопки
    public TextMeshProUGUI rewardText; // Текст всередині кнопки

    [Header("Таймери (в секундах)")]
    public float showDuration = 30f;   // Скільки часу кнопка висить на екрані
    public float hideCooldown = 180f;  // Скільки часу чекати до наступної появи

    private AdOffer currentOffer;

    void Start()
    {
        buttonContainer.SetActive(false); // Ховаємо кнопку на старті
        StartCoroutine(AdButtonRoutine());
    }

    private IEnumerator AdButtonRoutine()
    {
        while (true)
        {
            // 1. Чекаємо заданий час (відкат)
            yield return new WaitForSeconds(hideCooldown);

            // 2. Вибираємо рандомну нагороду і показуємо кнопку
            if (TryPickOffer())
            {
                buttonContainer.SetActive(true);

                // 3. Чекаємо, поки вийде час показу
                yield return new WaitForSeconds(showDuration);

                // 4. Якщо гравець не натиснув - ховаємо кнопку
                buttonContainer.SetActive(false);
            }
        }
    }

    private bool TryPickOffer()
    {
        if (offers == null || offers.Count == 0) return false;

        // Створюємо список доступних пропозицій
        List<AdOffer> availableOffers = new List<AdOffer>();

        foreach (var offer in offers)
        {
            // Якщо це діаманти, перевіряємо, чи вони не на відкаті в AdsChecker
            if (offer.rewardType == AdsChecker.RewardType.FreeDiamonds)
            {
                if (AdsChecker.Instance != null && AdsChecker.Instance.CanWatchDiamondAd(out _))
                {
                    availableOffers.Add(offer);
                }
            }
            else
            {
                availableOffers.Add(offer); // Інші нагороди доступні завжди
            }
        }

        // Якщо нічого недоступно, повертаємо false
        if (availableOffers.Count == 0) return false;

        // Вибираємо рандомну нагороду з доступних
        currentOffer = availableOffers[Random.Range(0, availableOffers.Count)];

        // Оновлюємо візуал
        rewardIconImage.sprite = currentOffer.offerIcon;
        rewardText.text = currentOffer.offerText;

        return true;
    }

    // Цей метод треба повісити на подію OnClick() самої кнопки
    public void OnClickWatchUniversalAd()
    {
        if (AdsChecker.Instance != null && currentOffer != null)
        {
            // Викликаємо показ реклами
            AdsChecker.Instance.RequestAd(currentOffer.rewardType);

            // Одразу ховаємо кнопку
            buttonContainer.SetActive(false);

            // Перезапускаємо таймери з нуля
            StopAllCoroutines();
            StartCoroutine(AdButtonRoutine());
        }
    }
}