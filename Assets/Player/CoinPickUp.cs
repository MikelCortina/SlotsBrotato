using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float pullSpeed = 15f;

    private Transform playerTransform;

    private void OnEnable()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (SlotMachine.Instance != null)
            SlotMachine.Instance.OnCoinCollected(value);

        Destroy(gameObject);
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            playerTransform = player.transform;
        }

        PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        float radius = playerStats.GetCoinPickupRadius();
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= radius)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position += (Vector3)direction * pullSpeed * Time.deltaTime;

            float newDistance = Vector2.Distance(transform.position, playerTransform.position);
            if (newDistance <= radius * 0.3f)
            {
                if (SlotMachine.Instance != null)
                    SlotMachine.Instance.OnCoinCollected(value);

                Destroy(gameObject);
            }
        }
    }
}