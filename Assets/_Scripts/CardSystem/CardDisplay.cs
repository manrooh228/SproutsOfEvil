using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image artworkImage;

    void Start()
    {
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        artworkImage.sprite = cardData.artwork;
    }
}
