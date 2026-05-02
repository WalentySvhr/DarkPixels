using UnityEngine;

public class AdButton : MonoBehaviour
{
    // У меню Інспектора ти зможеш вибрати, яку нагороду дає саме ЦЯ кнопка
    public AdsChecker.RewardType rewardForThisButton;

    public void OnClick()
    {
        // Кнопка каже нашому менеджеру: "Покажи рекламу і видай ось таку нагороду"
        AdsChecker.Instance.RequestAd(rewardForThisButton);
    }
}