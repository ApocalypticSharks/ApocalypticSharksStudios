using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Builds obstacle layout and spawns cultists when hosting the enemy test scene.
/// </summary>
public class EnemyTestSceneSetup : MonoBehaviour
{
    [SerializeField] private GameObject cultistPrefab;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Vector2[] cultistSpawnPoints =
    {
        new Vector2(4f, 2f),
        new Vector2(-4f, -2f)
    };

    private bool obstaclesBuilt;
    private bool cultistsSpawned;

    private void Awake()
    {
        BuildObstacles();
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnServerStarted += SpawnCultistsIfNeeded;
        if (NetworkManager.Singleton.IsServer)
            SpawnCultistsIfNeeded();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= SpawnCultistsIfNeeded;
    }

    private void BuildObstacles()
    {
        if (obstaclesBuilt)
            return;

        obstaclesBuilt = true;
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        var root = new GameObject("Obstacles");
        root.transform.SetParent(transform, false);

        CreateWall(root.transform, obstacleLayer, new Vector2(0f, 3.2f), new Vector2(8f, 0.5f));
        CreateWall(root.transform, obstacleLayer, new Vector2(0f, -3.2f), new Vector2(8f, 0.5f));
        CreateWall(root.transform, obstacleLayer, new Vector2(-2.5f, 0f), new Vector2(0.5f, 4f));
        CreateWall(root.transform, obstacleLayer, new Vector2(2.5f, 0.8f), new Vector2(0.5f, 2.5f));
    }

    private void CreateWall(Transform parent, int layer, Vector2 center, Vector2 size)
    {
        var wall = new GameObject("Wall");
        wall.layer = layer;
        wall.transform.SetParent(parent, false);
        wall.transform.position = new Vector3(center.x, center.y, 0f);

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;

        if (wallSprite != null)
        {
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = wallSprite;
            renderer.color = new Color(0.18f, 0.16f, 0.14f, 1f);
            renderer.sortingOrder = 0;
            wall.transform.localScale = new Vector3(size.x / 0.25f, size.y / 0.25f, 1f);
        }
    }

    private void SpawnCultistsIfNeeded()
    {
        if (cultistsSpawned || cultistPrefab == null)
            return;

        cultistsSpawned = true;

        foreach (var point in cultistSpawnPoints)
        {
            var cultist = Instantiate(cultistPrefab, point, Quaternion.identity);
            cultist.GetComponent<NetworkObject>().Spawn();
        }
    }
}
