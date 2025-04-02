using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUi : MonoBehaviour, IPointerClickHandler
{
    public Card card { get; private set; }
    private bool isOwned;

    public void SetCard(Card c, bool isOwned)
    {
        var image = GetComponent<Image>();
        this.isOwned = isOwned;
        card = c;
        if (!isOwned)
        {
            image.sprite = CardImages.CardBackSprite;
        }
        else
        {
            image.sprite = CardImages.CardSpriteDictionary[c.ToString()];    
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOwned || !Client.Local.isTurnActive) return;
        Client.Local.lastPlayedCard = this;
        Client.Local.PlayCardServerRpc(card);
    }
}