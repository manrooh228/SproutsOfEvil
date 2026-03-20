using Assets._Scripts.BattleSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("Настройки Префабов")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform deckTransform;
    [SerializeField] private Transform discardTransform;

    [Header("Параметры Руки")]
    public float cardSpacing;
    public float curveIntensity = 20f;
    public float rotationIntensity = 5f;
    [SerializeField] private int cardsPerTurn = 5;

    [Header("База данных карт")]
    [SerializeField] private List<CardData> allAvailableCards;

    [SerializeField] private List<GameObject> cardsInHand = new List<GameObject>();

    [SerializeField] private List<CardData> commonCards = new List<CardData>();
    [SerializeField] private List<CardData> rareCards = new List<CardData>();
    [SerializeField] private List<CardData> epicCards = new List<CardData>();
    public List<CardData> GetCommonPool() => commonCards;
    public List<CardData> GetRarePool() => rareCards;
    public List<CardData> GetEpicPool() => epicCards;


    private Dictionary<CardData, int> cardCooldowns = new Dictionary<CardData, int>();

    private EnergyManager energyManager;




    void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        SortCardsByRarity();
    }

    private void SortCardsByRarity()
    {
        foreach (var card in allAvailableCards)
        {
            if (card.rarity == CardRarity.Common) commonCards.Add(card);
            else if (card.rarity == CardRarity.Rare) rareCards.Add(card);
            else if (card.rarity == CardRarity.Epic) epicCards.Add(card);
        }
    }

    // Привязать к кнопке "Начать Бой" или "Конец Хода"
    public void StartPlayerTurn()
    {
        if (cardsInHand.Count > 0) return; // Чтобы не дублировать 
        StartCoroutine(DrawStartingHand());
    }

    private IEnumerator DrawStartingHand()
    {
        for (int i = 0; i < cardsPerTurn; i++)
        {
            DrawRandomCardByRarity();
            yield return new WaitForSeconds(0.15f); // Задержка для красоты вылета
        }
        
    }

    private void DrawRandomCardByRarity()
    {
        int roll = Random.Range(0, 100);
        CardData selectedData = null;

        // Проверяем, есть ли уже Эпическая карта в руке
        bool hasEpicInHand = CheckIfEpicInHand();

        if (roll < 60)
        {
            selectedData = GetRandomFromList(commonCards);
        }
        else if (roll < 90)
        {
            selectedData = GetRandomFromList(rareCards) ?? GetRandomFromList(commonCards);
        }
        else
        {
            // Пытаемся вытащить Epic
            if (!hasEpicInHand)
            {
                selectedData = GetRandomFromList(epicCards);
            }

            // Если Epic уже есть в руке ИЛИ список эпиков пуст/в откате
            if (selectedData == null)
            {
                // Шанс 25% на Rare, иначе 75% на Common
                int subRoll = Random.Range(0, 100);
                if (subRoll < 25)
                    selectedData = GetRandomFromList(rareCards) ?? GetRandomFromList(commonCards);
                else
                    selectedData = GetRandomFromList(commonCards);
            }
        }

        if (selectedData != null) SpawnCard(selectedData);
    }

    private bool CheckIfEpicInHand()
    {
        foreach (GameObject cardObj in cardsInHand)
        {
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null && display.cardData.rarity == CardRarity.Epic)
            {
                return true;
            }
        }
        return false;
    }



    private CardData GetRandomFromList(List<CardData> list)
    {
        if (list.Count == 0) return null;

        List<CardData> available = list.FindAll(c => !cardCooldowns.ContainsKey(c));

        if (available.Count == 0) return null;

        return available[Random.Range(0, available.Count)];
    }

    private void SpawnCard(CardData data)
    {
        GameObject newCard = Instantiate(cardPrefab, transform);
        newCard.transform.localPosition = deckTransform.localPosition;
        newCard.transform.localScale = Vector3.one;

        newCard.GetComponent<CardDisplay>().cardData = data;
        cardsInHand.Add(newCard);

        UpdateHandVisuals();
    }

    public void UpdateHandVisuals()
    {
        int count = cardsInHand.Count;
        for (int i = 0; i < count; i++)
        {
            float relativeIndex = (count > 1) ? (i - (count - 1) / 2f) / ((count - 1) / 2f) : 0;
            float xPos = (i - (count - 1) / 2f) * cardSpacing;
            float yPos = -Mathf.Pow(relativeIndex, 2) * curveIntensity;
            float angle = -relativeIndex * rotationIntensity;

            Vector3 targetPos = new Vector3(xPos, yPos, 0);
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);

            StartCoroutine(AnimateCardToHand(cardsInHand[i], targetPos, targetRot, i));
        }
    }

    IEnumerator AnimateCardToHand(GameObject card, Vector3 targetPos, Quaternion targetRot, int index)
    {
        float duration = 0.4f;
        float elapsed = 0;
        Vector3 startPos = card.transform.localPosition;
        Quaternion startRot = card.transform.localRotation;

        card.transform.SetSiblingIndex(index);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            card.transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);
            card.transform.localRotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        card.transform.localPosition = targetPos;
        card.transform.localRotation = targetRot;
    }

    public void OnCardPlayed(GameObject card)
    {
        CardData data = card.GetComponent<CardDisplay>().cardData;

        // Если у карты прописан откат, добавляем её в список ожидания
        if (data.cooldownTurns > 0)
        {
            if (cardCooldowns.ContainsKey(data)) cardCooldowns[data] = data.cooldownTurns;
            else cardCooldowns.Add(data, data.cooldownTurns);

            Debug.Log($"Карта {data.cardName} ушла на перезарядку: {data.cooldownTurns} ход(а).");
        }

        if (cardsInHand.Contains(card)) cardsInHand.Remove(card);

        UpdateHandVisuals();
        CheckAutoEndTurn();
    }

    private void ReduceCooldowns()
    {
        List<CardData> keys = new List<CardData>(cardCooldowns.Keys);
        foreach (var card in keys)
        {
            cardCooldowns[card]--;
            if (cardCooldowns[card] <= 0)
            {
                cardCooldowns.Remove(card);
                Debug.Log($"Карта {card.cardName} снова доступна!");
            }
        }
    }

    public void CheckAutoEndTurn()
    {
        if (cardsInHand.Count == 0 || !CanPlayerPlayAnyCard())
        {
            Debug.Log("Нет доступных ходов.");
            EndTurn();
        }
    }

    private bool CanPlayerPlayAnyCard()
    {
        foreach (var cardObj in cardsInHand)
        {
            if (energyManager.CanAfford(cardObj.GetComponent<CardDisplay>().cardData.energyCost))
                return true;
        }
        return false;
    }
        
    public void EndTurn()
    {
        ReduceCooldowns();
        FindFirstObjectByType<Player>().ProcessStatuses();
        Debug.Log("Завершение...");
        StopAllCoroutines(); // Прерываем текущие анимации выдачи
        StartCoroutine(DiscardHandRoutine());
    }

    IEnumerator DiscardHandRoutine()
    {
        List<GameObject> toDiscard = new List<GameObject>(cardsInHand);
        cardsInHand.Clear();

        foreach (GameObject card in toDiscard)
        {
            StartCoroutine(AnimateToDiscard(card));
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        // --- ЛОГИКА ХОДА ВРАГА ---
        Debug.Log("Ход Врага Начался...");

        Enemy enemy = FindFirstObjectByType<Enemy>();
        if (enemy != null)
        {
            // Ждем, пока враг выполнит все свои действия (ExecuteTurn)
            yield return StartCoroutine(enemy.ExecuteTurn());
        }

        Debug.Log("Ход Врага Окончен. Возвращаем ход игроку.");
        // --- КОНЕЦ ХОДА ВРАГА ---

        energyManager.ResetEnergy();
        StartPlayerTurn();
    }

    IEnumerator AnimateToDiscard(GameObject card)
    {
        float duration = 0.5f;
        float elapsed = 0;
        Vector3 startPos = card.transform.position;

        while (elapsed < duration)
        {
            card.transform.position = Vector3.Lerp(startPos, discardTransform.position, elapsed / duration);
            card.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(card);
    }

    public void RemoveCard(GameObject card)

    {
        if (cardsInHand.Contains(card))

        {
            cardsInHand.Remove(card);

            UpdateHandVisuals();

        }
    }


}
