using System;
using Unity.Mathematics;

public enum SquareState { Empty, Black, White }
public enum DirX { Left = -1, None = 0, Right = 1 }
public enum DirY { Up = -1, None = 0, Down = 1 }

public readonly struct Board
{
    private readonly ulong blackBoard;
    private readonly ulong whiteBoard;
    
    private const ulong RightLimitOfBoard = 0b_11111110_11111110_11111110_11111110_11111110_11111110_11111110_11111110;
    private const ulong LeftLimitOfBoard = 0b_01111111_01111111_01111111_01111111_01111111_01111111_01111111_01111111;

    public Board(ulong blackBoard, ulong whiteBoard)
    {
        this.blackBoard = blackBoard;
        this.whiteBoard = whiteBoard;
    }
    
    public static ulong GetChangedPositions(Board oldBoard, Board newBoard) => (oldBoard.blackBoard ^ newBoard.blackBoard) | (oldBoard.whiteBoard ^ newBoard.whiteBoard);
    
    public int GetTokenCount(bool isBlack) => math.countbits(isBlack ? blackBoard : whiteBoard);
    
    public SquareState GetSquareState(int posIndex)
    {
        ulong mask = 1UL << posIndex;
        if ((blackBoard & mask) != 0) return SquareState.Black;
        if ((whiteBoard & mask) != 0) return SquareState.White;
        return SquareState.Empty;
    }
    
    public bool IsValidMove(int posIndex, bool isBlackTurn) => (GetValidPositions(isBlackTurn) & (1UL << posIndex)) != 0;
    
    public ulong GetValidPositions(bool isBlackTurn)
    {
        ulong player = isBlackTurn ? blackBoard : whiteBoard;
        ulong opponent = isBlackTurn ? whiteBoard : blackBoard;
        ulong empty = ~(blackBoard | whiteBoard);
        
        ulong validMoves = 0UL;
        
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Right, dy: DirY.None, keepPath: false);  // Droite
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Right, dy: DirY.Down, keepPath: false);  // Diag Bas-Droite
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Right, dy: DirY.Up, keepPath: false); // Diag Haut-Droite
        
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Left, dy: DirY.None, keepPath: false); // Gauche
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Left, dy: DirY.Down, keepPath: false);  // Diag Bas-Gauche
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.Left, dy: DirY.Up, keepPath: false); // Diag Haut-Gauche
        
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.None, dy: DirY.Down, keepPath: false);  // Bas
        validMoves |= FindValidCapturePos(originMask: player, opponentMask: opponent, validationMask: empty, dx: DirX.None, dy: DirY.Up, keepPath: false); // Haut
        
        return validMoves;
    }

    public Board PlaceToken(int posIndex, bool isBlack)
    {
        ulong tokenMask = 1UL << posIndex;
        
        ulong player = isBlack ? blackBoard : whiteBoard;
        ulong opponent = isBlack ? whiteBoard : blackBoard;

        ulong tokensToFlip = 0UL;
        
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Right, dy: DirY.None, keepPath: true);  // Droite
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Right, dy: DirY.Down, keepPath: true);  // Diag Bas-Droite
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Right, dy: DirY.Up, keepPath: true); // Diag Haut-Droite
        
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Left, dy: DirY.None, keepPath: true); // Gauche
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Left, dy: DirY.Down, keepPath: true);  // Diag Bas-Gauche
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.Left, dy: DirY.Up, keepPath: true); // Diag Haut-Gauche
        
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.None, dy: DirY.Down, keepPath: true);  // Bas
        tokensToFlip |= FindValidCapturePos(originMask: tokenMask, opponentMask: opponent, validationMask: player, dx: DirX.None, dy: DirY.Up, keepPath: true); // Haut
        
        player |= tokenMask | tokensToFlip;
        opponent &= ~tokensToFlip;
        
        return isBlack
            ? new Board(player, opponent)
            : new Board(opponent, player);
    }
    
    private ulong FindValidCapturePos(ulong originMask, ulong opponentMask, ulong validationMask, DirX dx, DirY dy, bool keepPath)
    {
        int directionIndex = ((int)dy * 8) + (int)dx;
        
        bool isToLeft = directionIndex < 0;
        int shiftAmount = Math.Abs(directionIndex);
        
        if (dx == DirX.Left)
            opponentMask &= LeftLimitOfBoard;
        else if (dx == DirX.Right)
            opponentMask &= RightLimitOfBoard;
        
        ulong capturePath = isToLeft
            ? (originMask >> shiftAmount) & opponentMask
            : (originMask << shiftAmount) & opponentMask;
        
        for (int i = 0; i < 5; i++)
        {
            capturePath |= isToLeft
                ? (capturePath >> shiftAmount) & opponentMask
                : (capturePath << shiftAmount) & opponentMask;
        }
        
        ulong finalShift = isToLeft ? (capturePath >> shiftAmount) : (capturePath << shiftAmount);
        
        if (dx == DirX.Left)
            finalShift &= LeftLimitOfBoard;
        else if (dx == DirX.Right)
            finalShift &= RightLimitOfBoard;
        
        ulong validEnds = finalShift & validationMask;
        
        if (keepPath)
            return validEnds != 0 ? capturePath : 0UL;
        else
            return validEnds;
    }
}