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
        if (cardsInHand.Count > 0) return; // Чтобы не дублировать ход
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

        // Шансы: 50% Common, 30% Rare, 20% Epic
        if (roll < 50) selectedData = GetRandomFromList(commonCards);
        else if (roll < 80) selectedData = GetRandomFromList(rareCards);
        else selectedData = GetRandomFromList(epicCards);

        if (selectedData != null) SpawnCard(selectedData);
    }

    private CardData GetRandomFromList(List<CardData> list)
    {
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
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
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
        }
        UpdateHandVisuals();
        CheckAutoEndTurn();
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
