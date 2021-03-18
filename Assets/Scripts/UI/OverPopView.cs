using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverPopView : MonoBehaviour
{
    public TMP_Text scoreText, bestScoreText;
    public Button btn;

    private void Start()
    {
        btn.onClick.AddListener(() => Destroy(gameObject));
    }

    public void SetText(int score, int best)
    {
        scoreText.text = $"Score:<indent=50%>{score}</indent>";
        bestScoreText.text = $"Best Score:<indent=50%>{best}</indent>";
    }
}
