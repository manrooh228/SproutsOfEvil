using Assets._Scripts.BattleSystem;
using Assets._Scripts.CardSystem;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalScale;
    private int originalSiblingIndex;
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;

    private EnergyManager energyManager;
    private HandManager handManager;

    [Header("Effects")]
    public GameObject textPrefab; // Тот же префаб текста
    public Transform headPoint;

    void Start()
    {
        energyManager = FindFirstObjectByType<EnergyManager>();
        handManager = FindFirstObjectByType<HandManager>();
    }

    void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInChildren<CardAnimation>().SetHover(true); // Стоп анимация
        transform.localScale = originalScale * 1.2f;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        GetComponentInChildren<CardAnimation>().SetHover(false);
        transform.localScale = originalScale;
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;

        // Теперь детектим только Арену (или Врага, если он перекрывает Арену)
        if (hovered != null && (hovered.CompareTag("Arena") || hovered.CompareTag("Enemy")))
        {
            UseCard();
        }
        else
        {
            ReturnToHand();
        }
    }

    private void ReturnToHand()
    {
        transform.position = startPosition;
        transform.SetSiblingIndex(originalSiblingIndex);
        transform.localScale = originalScale;
    }

    // FUNC METHODS
    private void UseCard()
    {
        Player player = FindFirstObjectByType<Player>();
        Enemy targetEnemy = FindFirstObjectByType<Enemy>();
        CardData data = GetComponent<CardDisplay>().cardData;

        if (energyManager.CanAfford(data.energyCost))
        {
            energyManager.SpendEnergy(data.energyCost);

            if (data.damage > 0 && targetEnemy != null)
            {
                int totalDmg = player != null ? player.GetTotalDamage(data.damage) : data.damage;
                targetEnemy?.TakeDamage(totalDmg);
            }

            if (data.block > 0) player.AddArmor(data.block);
            if (data.heal > 0) player.Heal(data.heal);

            if (data.duration > 0 && data.effectType != StatusEffectType.None)
            {
                player.ApplyStatus(data.effectType, data.effectValue, data.duration);

            }

            handManager.OnCardPlayed(gameObject);
            Destroy(gameObject);
        }
        else
        {
            ReturnToHand();
        }


    }
}
