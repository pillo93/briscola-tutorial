using UnityEngine;
using UnityEngine.UI;

public class CardUi : MonoBehaviour
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
}