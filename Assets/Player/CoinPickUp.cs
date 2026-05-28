using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float pullSpeed = 40f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float minCollectionDistance = 0.2f;

    [Header("Oscillation")]
    [SerializeField] private float oscillationDistance = 0.2f; // Distancia de la oscilación inicial
    [SerializeField] private float oscillationDuration = 0.2f; // Duración de la oscilación
    [SerializeField] private float oscillationSpeed = 8f;      // Velocidad de la oscilación

    private Transform playerTransform;
    private bool isCollected = false;
    private bool isPulled = false;
    private bool isOscillating = false;
    private float currentSpeed = 0f;
    private float oscillationTimer = 0f;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 oscillationTarget = Vector3.zero;

    private void OnEnable()
    {
        isCollected = false;
        isPulled = false;
        isOscillating = false;
        currentSpeed = pullSpeed;
        oscillationTimer = 0f;
        startPosition = transform.position;
        playerTransform = null;
    }

    private void Update()
    {
        if (isCollected) return;

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

        // Activar oscilación cuando entra en rango
        if (!isPulled && !isOscillating && distance <= radius)
        {
            isPulled = true;
            isOscillating = true;
            oscillationTimer = 0f;
            startPosition = transform.position;

            // Calcular dirección OPUESTA al jugador para la oscilación inicial
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            oscillationTarget = transform.position - (Vector3)directionToPlayer * oscillationDistance;

            CoinDropController dropController = GetComponent<CoinDropController>();
            if (dropController != null)
                dropController.StartBeingPulled();
        }

        // Fase 1: Oscilación inicial (en dirección opuesta al jugador)
        if (isOscillating)
        {
            oscillationTimer += Time.deltaTime;

            // Mover hacia el punto opuesto al jugador
            float progress = Mathf.Clamp01(oscillationTimer / oscillationDuration);

            // Easing suave (ease-out)
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = Vector3.Lerp(startPosition, oscillationTarget, easedProgress);

            if (oscillationTimer >= oscillationDuration)
            {
                // Terminar oscilación, empezar atracción fuerte
                isOscillating = false;
                currentSpeed = pullSpeed;
            }
            return; // No atraer todavía
        }

        // Fase 2: Atracción fuerte hacia el jugador
        if (isPulled)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            // Acelerar constantemente
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // Mover hacia el jugador
            transform.position += (Vector3)direction * currentSpeed * Time.deltaTime;

            // Recoger cuando está cerca
            float newDistance = Vector2.Distance(transform.position, playerTransform.position);
            if (newDistance <= minCollectionDistance)
            {
                CollectCoin();
            }
        }
    }

    private void CollectCoin()
    {
        if (isCollected) return;
        isCollected = true;

        if (SlotMachine.Instance != null)
            SlotMachine.Instance.OnCoinCollected(value);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CollectCoin();
    }
}