using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem instance;

    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private TileBase whiteTile;

    public GameObject townHall;
    public GameObject smith;

    private PlaceableObject objectToPlace;

    #region Unity methods
    private void Awake()
    {
        instance = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InitializeWithObject(townHall);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            InitializeWithObject(smith);
        }
        if (!objectToPlace)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            objectToPlace.Rotate();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CanBePlaced(objectToPlace))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
            }
            else
            {
                Destroy(objectToPlace.gameObject);
            }
        }
    }
    #endregion

    #region Utils
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        { 
            return Vector3.zero;
        }
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPosition = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPosition);
        return position;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] blocks = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var cell in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(cell.x, cell.y, 0);
            blocks[counter] = tilemap.GetTile(pos);
            counter++;
        }
        return blocks;
    }
    #endregion

    #region Building placement
    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);
        GameObject gameObject = GameObject.Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = gameObject.GetComponent<PlaceableObject>();
        gameObject.AddComponent<ObjectDrag>();
    }

    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placeableObject.Size;

        TileBase[] baseArray = GetTilesBlock(area, mainTilemap);

        foreach (var b in baseArray)
        {
            if (b == whiteTile)
            { 
                return false;
            }
        }

        return true;
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        mainTilemap.BoxFill(start, whiteTile,start.x,
            start.y, start.x + size.x, start.y + size.y);
    }
    #endregion
}
