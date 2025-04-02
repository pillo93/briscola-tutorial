using UnityEngine;
using DG.Tweening;

public class PlayCardCommand : Command
{

    Card cardPlayed;
    CardUi cardUi;
    Transform playArea;

    public PlayCardCommand(Card cardPlayed, CardUi cardUi, Transform playArea)
    {
        this.cardPlayed = cardPlayed;
        this.cardUi = cardUi;
        this.playArea = playArea;
    }

    protected override void StartCommandExecution()
    {
        cardUi.SetCard(cardPlayed, true);
        cardUi.enabled = false;
        cardUi.transform.SetParent(playArea);
        cardUi.transform.DOLocalMove(Vector3.zero, .5f)
            .OnComplete(CommandExecutionComplete);
    }

}
