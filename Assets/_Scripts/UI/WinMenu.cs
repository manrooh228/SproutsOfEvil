using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.UI
{
    public class WinMenu : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private GameObject cardRewardPrefab; // Упрощенный префаб карты для меню
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private Button claimButton;

        private List<CardData> selectedRewards = new List<CardData>();

        public void SetupFixedRewards(List<CardData> enemyLoot)
        {
            if (enemyLoot == null || enemyLoot.Count == 0)
            {
                Debug.LogWarning("У врага нет лута!");
                return;
            }

            // Очищаем на всякий случай текущий список наград
            selectedRewards.Clear();

            // Просто спавним ВСЁ, что передал враг
            foreach (CardData reward in enemyLoot)
            {
                if (reward != null)
                {
                    selectedRewards.Add(reward);

                    // Создаем визуальный префаб карты в контейнере меню
                    GameObject cardObj = Instantiate(cardRewardPrefab, rewardContainer);
                    cardObj.GetComponent<CardDisplay>().cardData = reward;
                }
            }

            // Привязываем кнопку закрытия/принятия
            claimButton.onClick.RemoveAllListeners(); // На всякий случай чистим старые подписки
            claimButton.onClick.AddListener(ClaimAndClose);
        }

        //private CardData RollReward(List<CardData> c, List<CardData> r, List<CardData> e)
        //{
        //    int roll = Random.Range(0, 100);
        //    if (roll < 70) return c[Random.Range(0, c.Count)];
        //    if (roll < 95) return r[Random.Range(0, r.Count)];
        //    return e[Random.Range(0, e.Count)];
        //}

        private void ClaimAndClose()
        {
            HandManager hand = FindFirstObjectByType<HandManager>();
            foreach (var card in selectedRewards)
            {
                hand.AddNewCardToPool(card);
            }

            // Тут можно загрузить следующую сцену или вернуться на карту
            Destroy(gameObject);
        }
    }
}