using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUi : MonoBehaviour, IPointerClickHandler
{
    private Card card;

    public void SetCard(Card c)
    {
        var image = GetComponent<Image>();
        if (c == null)
        {
            image.sprite = CardImages.CardBackSprite;
        }
        else
        {
            card = c;
            image.sprite = CardImages.CardSpriteDictionary[c.ToString()];    
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClick {card}");
        if (card == null) return;
        Client.Local.PlayCardServerRpc(card);
    }
}