using UnityEngine;

public class CoinDropManager : MonoBehaviour
{
    public static CoinDropManager Instance { get; private set; }

    [Header("Spawn")]
    [SerializeField] private GameObject dropPrefab;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnArea; // Centro del área
    [SerializeField] private float areaWidth = 2f;  // Ancho total en X
    [SerializeField] private float areaHeight = 2f; // Alto total en Y

    [Header("Force X (random between min and max)")]
    [SerializeField] private float minXForce = 3f;
    [SerializeField] private float maxXForce = 10f;

    [Header("Force Y (random between min and max)")]
    [SerializeField] private float minYForce = 5f;
    [SerializeField] private float maxYForce = 15f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 center = spawnArea != null ? spawnArea.position : transform.position;

        float randomX = Random.Range(-areaWidth * 0.5f, areaWidth * 0.5f);
        float randomY = Random.Range(-areaHeight * 0.5f, areaHeight * 0.5f);

        return new Vector3(center.x + randomX, center.y + randomY, center.z);
    }

    public void SpawnDropFromCoin(Vector3 coinPosition)
    {
        if (dropPrefab == null)
            return;

        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject obj = Instantiate(dropPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomX = Random.Range(minXForce, maxXForce);
            float randomY = Random.Range(minYForce, maxYForce);

            Vector2 randomForce = new Vector2(randomX, randomY);
            rb.AddForce(randomForce, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = spawnArea != null ? spawnArea.position : transform.position;

        float halfWidth = areaWidth * 0.5f;
        float halfHeight = areaHeight * 0.5f;

        // Esquinas del rectángulo
        Vector3 topLeft = new Vector3(center.x - halfWidth, center.y + halfHeight, center.z);
        Vector3 topRight = new Vector3(center.x + halfWidth, center.y + halfHeight, center.z);
        Vector3 bottomRight = new Vector3(center.x + halfWidth, center.y - halfHeight, center.z);
        Vector3 bottomLeft = new Vector3(center.x - halfWidth, center.y - halfHeight, center.z);

        Gizmos.color = new Color(0f, 1f, 1f, 0.6f); // Cian translúcido

        // Dibujar las 4 líneas del rectángulo
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // Opcional: dibujar una cruz en el centro
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f); // Amarillo
        Gizmos.DrawLine(
            new Vector3(center.x - 0.2f, center.y, center.z),
            new Vector3(center.x + 0.2f, center.y, center.z)
        );
        Gizmos.DrawLine(
            new Vector3(center.x, center.y - 0.2f, center.z),
            new Vector3(center.x, center.y + 0.2f, center.z)
        );
    }
}