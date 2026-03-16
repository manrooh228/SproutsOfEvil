using Assets._Scripts.BattleSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalScale;
    private int originalSiblingIndex;
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private Transform originalParent;


    private EnergyManager energyManager;
    void Start() => energyManager = FindFirstObjectByType<EnergyManager>();

    void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.2f;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        transform.localScale = originalScale;
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;

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

        // Проверяем: попали ли мы в Арену ИЛИ прямо во Врага
        if (hovered != null && (hovered.CompareTag("Arena")))
        {
            
            UseCard(hovered);
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



    //FUNC METHODS
    private void UseCard(GameObject target)
    {
        CardData data = GetComponent<CardDisplay>().cardData;

        if (energyManager.CanAfford(data.energyCost))
        {
            energyManager.SpendEnergy(data.energyCost);

            // Если бросили на врага - наносим урон
            if (target.CompareTag("Enemy"))
            {
                Debug.Log("Hitted enemy" + target.name);
                target.GetComponent<Enemy>().TakeDamage(data.damage);
            }

            // Логика брони (если есть)
            // player.AddArmor(data.armor);

            transform.parent.GetComponent<HandManager>().RemoveCard(gameObject);
            Destroy(gameObject);
        }
        else
        {
            // Возврат если мало энергии
            ReturnToHand();
        }
    }


}
