using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject tokenPrefab;
    
    private const int height = 8 , width = 8;
    private readonly Color white = Color.white;
    private readonly Color black = Color.black;

    private SpriteRenderer[] tokenRenderers =  new SpriteRenderer[height*width];
    
    private void Awake()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Vector3 pos = new Vector3(x, 0, -y);
                
                GameObject squareInstance = Instantiate(squarePrefab, transform);
                squareInstance.transform.localPosition = pos;
                
                GameObject tokenInstance = Instantiate(tokenPrefab, transform);
                tokenInstance.transform.localPosition = pos;
                
                tokenRenderers[index] = tokenInstance.GetComponent<SpriteRenderer>();
                ShowToken(index, false);
            }
        }
    }

    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnBoardChange += UpdateBoard;
            UpdateBoard(new Board(0, 0), GameManager.instance.CurrentBoard);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnBoardChange -= UpdateBoard;
    }

    private void ShowToken(int posIndex, bool mustDisplay) => tokenRenderers[posIndex].gameObject.SetActive(mustDisplay);
    
    private void UpdateBoard(Board oldBoard, Board newBoard)
    {
        ulong changedMask = Board.GetChangedPositions(oldBoard, newBoard);
        
        for (int i = 0; i < height*width; i++)
        {
            if ((changedMask & (1UL << i)) != 0)
            {
                SquareState state = newBoard.GetSquareState(i);
                
                if (state == SquareState.Empty)
                {
                    ShowToken(i, false);
                }
                else
                {
                    ShowToken(i, true);
                    tokenRenderers[i].color = (state == SquareState.Black) ? black : white;
                }
            }
        }
    }
}
