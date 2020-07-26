using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
    public Text[] scoreTexts;
    public Text bestScoreText;
    public Text turnsText;
    public GameObject endGamePanel;

    public void SetScore(int score)
    {
        Array.ForEach(scoreTexts, (Text t) => { t.text = score.ToString(); });
    }

    public void SetTurns(int turns)
    {
        turnsText.text = turns.ToString();
    }

    public void SetBestScore(int score)
    {
        bestScoreText.text = score.ToString();
    }

    public void ShowEndGame(bool show)
    {
        endGamePanel.SetActive(show);
    }
}
