using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreCommand : Command
{

    private TMP_Text scoreText;
    private int newScore;

    public ScoreCommand(TMP_Text scoreText, int newScore)
    {
        this.scoreText = scoreText;
        this.newScore = newScore;
    }

    protected override void StartCommandExecution()
    {
        var c1 = GameObject.Find("EnemyCardSpot").transform.GetChild(0);
        var c2 = GameObject.Find("PlayerCardSpot").transform.GetChild(0);

        c2.transform.DOMove(scoreText.transform.position, .3f)
            .OnComplete(delegate { GameObject.Destroy(c2.gameObject); });
        var seq = DOTween.Sequence();
        seq.Append(c1.transform.DOMove(scoreText.transform.position, .3f)
            .OnComplete(delegate
            {
                scoreText.color = Color.green;
                scoreText.text = "Score: " + newScore.ToString();
                GameObject.Destroy(c1.gameObject);
            }));
        seq.Append(scoreText.DOColor(Color.white, 1f));
        seq.OnComplete(CommandExecutionComplete);
    }
}
