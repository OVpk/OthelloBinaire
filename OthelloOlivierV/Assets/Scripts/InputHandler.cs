using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private Transform boardRef;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetClickIndex(out int posIndex))
            {
                GameManager.instance.SelectSquare(posIndex);
            }
        }
    }

    private bool TryGetClickIndex(out int posIndex)
    {
        posIndex = -1;
        
        if (Camera.main == null || boardRef == null)
            return false;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        Plane board = new Plane(boardRef.up, boardRef.position);
        
        if (board.Raycast(ray, out float distance))
        {
            Vector3 worldHit = ray.GetPoint(distance);
            Vector3 localHit = boardRef.InverseTransformPoint(worldHit);
            
            int x = Mathf.RoundToInt(localHit.x);
            int y = Mathf.RoundToInt(-localHit.z);
            
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                posIndex = y * 8 + x;
                return true;
            }
        }
        
        return false;
    }
}
