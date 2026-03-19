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
                targetEnemy.TakeDamage(data.damage);
            }

            if (data.block > 0 && player != null)
            {
                player.AddArmor(data.block);
            }
            // Оповещаем HandManager, чтобы он обновил руку и проверил конец хода
            handManager.OnCardPlayed(gameObject);

            // Объект уничтожается внутри HandManager.OnCardPlayed или здесь
            Destroy(gameObject);
        }
        else
        {
            ReturnToHand();
        }


    }
}
