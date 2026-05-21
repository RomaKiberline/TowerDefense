using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
    public GameObject currentTowerPrefab;
    public GameObject shopPanel;

    [Header("Tilemaps")]
    public Tilemap grassTilemap;

    void Update()
    {
        if (GameManager.instance.currentState != GameManager.GameState.Preparation)
            return;

        if (currentTowerPrefab == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = grassTilemap.WorldToCell(mousePos);

            if (!grassTilemap.HasTile(cellPos))
            {
                Debug.Log("Не можна ставити за межами карти!");
                return;
            }

            Vector3 spawnWorldPos = grassTilemap.GetCellCenterWorld(cellPos);
            Vector2 checkPos = new Vector2(spawnWorldPos.x, spawnWorldPos.y);

            if (IsCellBlocked(checkPos))
            {
                Debug.Log("Не можна ставити на дорогу або на іншу вежу!");
                return;
            }

            if (HasTowerNearby(cellPos))
            {
                Debug.Log("Занадто близько! Став вежі через один блок.");
                return;
            }

            PlaceTowerAt(checkPos);
        }
    }

    bool IsCellBlocked(Vector2 checkPos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(checkPos);

        foreach (Collider2D hit in hits)
        {
            if (hit.isTrigger) continue;

            if (hit.CompareTag("Road") || hit.CompareTag("Tower"))
                return true;
        }

        return false;
    }

    bool HasTowerNearby(Vector3Int cellPos)
    {
        Vector3Int[] nearbyCells =
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, -1, 0)
        };

        foreach (Vector3Int offset in nearbyCells)
        {
            Vector3 worldPos = grassTilemap.GetCellCenterWorld(cellPos + offset);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

            foreach (Collider2D hit in hits)
            {
                if (hit.isTrigger) continue;

                if (hit.CompareTag("Tower"))
                    return true;
            }
        }

        return false;
    }

    void PlaceTowerAt(Vector2 spawnPos)
    {
        Tower towerScript = currentTowerPrefab.GetComponent<Tower>();

        if (towerScript == null)
        {
            CancelPlacement();
            return;
        }

        if (!GameManager.instance.SpendGold(towerScript.cost))
        {
            Debug.Log("Недостатньо золота!");
            CancelPlacement();
            return;
        }

        Instantiate(currentTowerPrefab, spawnPos, Quaternion.identity);
        CancelPlacement();
    }

    public void SetTower(GameObject towerPrefab)
    {
        if (GameManager.instance.currentState != GameManager.GameState.Preparation)
            return;

        if (towerPrefab == null)
            return;

        Tower towerScript = towerPrefab.GetComponent<Tower>();
        if (towerScript == null)
            return;

        if (GameManager.instance.gold < towerScript.cost)
        {
            Debug.Log("Недостатньо золота!");
            currentTowerPrefab = null;

            if (shopPanel != null)
                shopPanel.SetActive(true);

            return;
        }

        currentTowerPrefab = towerPrefab;

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    void CancelPlacement()
    {
        currentTowerPrefab = null;

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }
}