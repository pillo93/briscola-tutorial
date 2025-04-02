using DG.Tweening;
using UnityEngine;

public class DealCardCommand : Command
{

    GameObject cardPrefab;
    Transform parent;
    Card card;
    bool isOwned;

    public DealCardCommand(GameObject cardPrefab, Transform parent, Card card, bool isOwned)
    {
        this.cardPrefab = cardPrefab;
        this.parent = parent;
        this.card = card;
        this.isOwned = isOwned;
    }

    protected override void StartCommandExecution()
    {
        var deckTransform = GameObject.Find("Deck").transform;
        var go = GameObject.Instantiate(cardPrefab, parent);
        go.transform.position = deckTransform.position;
        var cui = go.GetComponent<CardUi>();
        cui.SetCard(card, isOwned);
        go.transform.DOLocalMove(Vector3.zero, .4f)
            .OnComplete(CommandExecutionComplete);
    }

}