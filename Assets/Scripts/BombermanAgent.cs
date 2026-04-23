using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Netcode; // Necesario para el teletransporte de red
using Unity.Netcode.Components; // Necesario para NetworkTransform

public class BombermanAgent : Agent
{
    private MovementController moveController;
    private BombController bombController;
    private Rigidbody2D rb;

    [Header("Configuración de Entrenamiento")]
    public Transform opponent;
    // Spawn 2 para IA (14.5, -7.5) | Spawn 1 para Rival (-15.5, 8.5)
    public Vector3 initialPosition = new Vector3(14.5f, -7.5f, 0f);
    public Vector3 opponentInitialPosition = new Vector3(-15.5f, 8.5f, 0f);

    public override void Initialize()
    {
        moveController = GetComponent<MovementController>();
        bombController = GetComponent<BombController>();
        rb = GetComponent<Rigidbody2D>();

        // Verificar si estamos conectados a un cerebro externo (Python)
        if (Academy.Instance.IsCommunicatorOn)
        {
            Debug.Log($"[AGENT] {gameObject.name} conectado al entrenador externo (Python).");
        }
        else
        {
            Debug.LogWarning($"[AGENT] {gameObject.name} NO está conectado a un entrenador externo. Usará inferencia local o Heurística.");
        }
    }

    private float lastDistanceToOpponent;

    public override void OnEpisodeBegin()
    {
        Debug.Log($"[AGENT] Iniciando nuevo episodio para {gameObject.name}");
        
        // 1. Teletransportar IA con autoridad de red
        TeleportSafe(transform, initialPosition);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (moveController != null) moveController.SetNextDirection(Vector2.zero);

        // 2. Teletransportar Oponente con autoridad de red
        if (opponent != null)
        {
            TeleportSafe(opponent, opponentInitialPosition);
            Rigidbody2D oppRb = opponent.GetComponent<Rigidbody2D>();
            if (oppRb != null) oppRb.linearVelocity = Vector2.zero;
            lastDistanceToOpponent = Vector3.Distance(transform.localPosition, opponent.localPosition);
        }

        // 3. Limpiar el tablero de objetos dinámicos (solo si somos el servidor/host)
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            CleanByTag("Bomb");
            CleanByTag("Explosion");
            CleanByTag("Item");
        }
    }

    // Método para mover objetos evitando el error de "non-authoritative side"
    private void TeleportSafe(Transform target, Vector3 position)
    {
        var netObj = target.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var netTrans = target.GetComponent<NetworkTransform>();
            if (netTrans != null)
                netTrans.Teleport(position, target.rotation, target.localScale);
            else
                target.position = position;
        }
        else
        {
            target.position = position; // Cambiado a position para ser más robusto
        }
    }

    private void CleanByTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (var obj in objects) Destroy(obj);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Posiciones (6 obs)
        sensor.AddObservation(transform.localPosition);
        if (opponent != null) sensor.AddObservation(opponent.localPosition);
        else sensor.AddObservation(Vector3.zero);

        // 2. Estadísticas del jugador (5 obs) - ¡NUEVO Sensor de Peligro!
        sensor.AddObservation(moveController.speed.Value / 10f);
        sensor.AddObservation(bombController.bombAmount / 5f);
        sensor.AddObservation(bombController.explosionRadius / 5f);
        
        // Sensor de "Estoy en peligro" (Si estoy en la misma X o Y que una bomba cercana)
        bool inDanger = false;
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (var b in bombs) {
            float dx = Mathf.Abs(transform.position.x - b.transform.position.x);
            float dy = Mathf.Abs(transform.position.y - b.transform.position.y);
            // Si está en línea recta a menos de la distancia del radio (aprox 3 unidades)
            if ((dx < 0.5f && dy < 3.5f) || (dy < 0.5f && dx < 3.5f)) {
                inDanger = true;
                break;
            }
        }
        sensor.AddObservation(inDanger ? 1f : 0f);
        
        Collider2D onBomb = Physics2D.OverlapPoint(transform.position, 1 << LayerMask.NameToLayer("Bomb"));
        sensor.AddObservation(onBomb != null ? 1f : 0f);

        // 3. Sensores Raycast (8 dir x 4 tipos = 32 obs) - ¡MEJORADO!
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right, 
                                 new Vector2(1,1).normalized, new Vector2(1,-1).normalized, 
                                 new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized };
        
        float rayDistance = 5f;
        foreach (Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayDistance);
            if (hit.collider != null)
            {
                sensor.AddObservation(hit.distance / rayDistance);
                sensor.AddObservation(hit.collider.CompareTag("Destructible") ? 1f : 0f);
                sensor.AddObservation(hit.collider.CompareTag("Bomb") || hit.collider.CompareTag("Explosion") ? 1f : 0f);
                sensor.AddObservation(hit.collider.CompareTag("Untagged") || hit.collider.name.Contains("Wall") ? 1f : 0f); // Muro indestructible
            }
            else
            {
                sensor.AddObservation(1f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // ... (movimiento igual)
        int moveAction = actions.DiscreteActions[0];
        Vector2 direction = Vector2.zero;
        if (moveAction == 1) direction = Vector2.up;
        else if (moveAction == 2) direction = Vector2.down;
        else if (moveAction == 3) direction = Vector2.left;
        else if (moveAction == 4) direction = Vector2.right;

        moveController.SetNextDirection(direction);

        if (actions.DiscreteActions[1] == 1) 
        {
            // --- BOMBEO TÁCTICO ---
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 2.0f);
            bool usefulBomb = false;
            foreach (var col in nearby) {
                // Útil si hay muro destructible O el oponente cerca
                if (col.CompareTag("Destructible") || col.transform == opponent) {
                    usefulBomb = true;
                    break;
                }
            }
            
            if (usefulBomb) AddReward(0.2f); // ¡Bien!
            else AddReward(-0.1f);          // Castigo por poner bombas "a la nada"

            bombController.TryPlaceBomb();
        }

        // --- SUPERVIVENCIA ---
        // Pequeño castigo por quedarse parado sobre una bomba
        Collider2D standingOnBomb = Physics2D.OverlapPoint(transform.position, 1 << LayerMask.NameToLayer("Bomb"));
        if (standingOnBomb != null) AddReward(-0.01f);

        // --- RECOMPENSAS DE AGRESIVIDAD ---
        if (opponent != null)
        {
            float currentDistance = Vector3.Distance(transform.localPosition, opponent.localPosition);
            if (currentDistance < lastDistanceToOpponent) AddReward(0.001f);
            else if (currentDistance > lastDistanceToOpponent) AddReward(-0.001f);
            lastDistanceToOpponent = currentDistance;
        }

        AddReward(-0.0005f);
    }

    public void OnWallDestroyed()
    {
        AddReward(0.5f); // ¡MUY IMPORTANTE!
    }

    public void OnItemPickedUp()
    {
        AddReward(0.3f); // Muy bueno
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;
        discrete[0] = 0;
        if (Input.GetKey(KeyCode.UpArrow)) discrete[0] = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) discrete[0] = 2;
        else if (Input.GetKey(KeyCode.LeftArrow)) discrete[0] = 3;
        else if (Input.GetKey(KeyCode.RightArrow)) discrete[0] = 4;

        discrete[1] = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.RightControl)) ? 1 : 0;
    }
}