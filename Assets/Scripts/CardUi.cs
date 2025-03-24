using UnityEngine;
using UnityEngine.UI;

public class CardUi : MonoBehaviour
{
    private Image cardImage;
    private Card card;

    public void SetCard(Card c)
    {
        //card = c;
        string s = c.ToString();
        Debug.Log(s);
        GetComponent<Image>().sprite = CardImages.CardSpriteDictionary[s];
    }
}