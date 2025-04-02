using System;
using System.Collections.Generic;
using UnityEngine;

public class CardImages : MonoBehaviour
{
    [SerializeField] private Sprite[] cardSprites;
    [SerializeField] private Sprite cardBackSprite;
    public static Sprite CardBackSprite;
    public static Dictionary<string, Sprite> CardSpriteDictionary = new();

    void Awake()
    {
        CardBackSprite = cardBackSprite;
        int spriteIndex = 0;
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Value value in Enum.GetValues(typeof(Value)))
            {
                var c = new Card(suit, value);
                CardSpriteDictionary[c.ToString()] = cardSprites[spriteIndex++];
            }
        }

        Debug.Log($"CardImagesDict size = {CardSpriteDictionary.Count}");
    }
}