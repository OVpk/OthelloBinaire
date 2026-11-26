using System;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoresText;
    
    private readonly Func<int, int, string> formatScoresInText = (p1, p2) => $"P1 : {p1} | P2 : {p2}";

    private void Start()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnScoresReveal += ShowScores;
    }
    
    private void OnDestroy()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnScoresReveal -= ShowScores;
    }
    
    public void QuitGame() => Application.Quit();
    
    public void RestartGame() => GameManager.instance.RestartGame();
    
    private void ShowScores(int j1Score, int j2Score)
    {
        scoresText.text = formatScoresInText(j1Score, j2Score);
    }
}
