using System;
using UnityEngine;

public enum Player { P1, P2 }

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    
    public event Action<Board, Board> OnBoardChange;
    public event Action<int, int> OnScoresReveal;
    
    public Board CurrentBoard => currentBoard;
    private Board currentBoard;
    private Player currentPlayer;
    private bool impossibleTurnOccurred = false;
    private bool isGameOver = false;
    
    private readonly Func<Player, Player> nextPlayer = p => p == Player.P1 ? Player.P2 : Player.P1;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitBoard();
    }

    private void InitBoard()
    {
        ulong startBlack = (1UL << (8 * 3 + 4)) | (1UL << (8 * 4 + 3));
        ulong startWhite = (1UL << (8 * 3 + 3)) | (1UL << (8 * 4 + 4));
        
        ApplyNewBoard(new Board(startBlack, startWhite));
        
        currentPlayer = Player.P1;
        isGameOver = false;
    }

    public void SelectSquare(int posIndex)
    {
        if (isGameOver) return;
        
        if (currentBoard.IsValidMove(posIndex, currentPlayer == Player.P1))
        {
            PlayToken(posIndex, currentPlayer);
            ChangeTurn();
        }
    }

    private void PlayToken(int posIndex, Player player)
    {
        Board newBoard = currentBoard.PlaceToken(posIndex, player == Player.P1);
        
        ApplyNewBoard(newBoard);
    }

    private void ApplyNewBoard(Board newBoard)
    {
        OnBoardChange?.Invoke(currentBoard, newBoard);
        
        currentBoard = newBoard;
    }

    private void ChangeTurn()
    {
        currentPlayer = nextPlayer(currentPlayer);

        if (currentBoard.GetValidPositions(currentPlayer == Player.P1) == 0)
            ImpossibleTurnOccurred();
        else
            impossibleTurnOccurred = false;
    }

    private void ImpossibleTurnOccurred()
    {
        if (impossibleTurnOccurred)
        {
            EndGame();
            return;
        }
        
        impossibleTurnOccurred = true;
        ChangeTurn();
    }

    private void EndGame()
    {
        isGameOver = true;
        CountScores();
    }

    private void CountScores()
    {
        int blackScore = currentBoard.GetTokenCount(true);
        int whiteScore = currentBoard.GetTokenCount(false);
        
        OnScoresReveal?.Invoke(blackScore, whiteScore);
    }

    public void RestartGame() => InitBoard();
}
