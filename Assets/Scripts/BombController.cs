using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

public class BombController : NetworkBehaviour
{
    [Header("Bomb")]
    public KeyCode inputKey = KeyCode.Space;
    public GameObject bombPrefab;
    public float bombFuseTime = 3f;
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab; // Restaurado para evitar pérdida de referencia
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab; // Restaurado para evitar pérdida de referencia

    private void Awake()
    {
        // Forzamos valores iniciales por si acaso en el Inspector hay otros
        bombAmount = 1;
        explosionRadius = 1;
        bombsRemaining = bombAmount;
    }

    public override void OnNetworkSpawn()
    {
        if (destructibleTiles == null)
        {
            GameObject mapObj = GameObject.Find("Destructibles") ?? GameObject.Find("Destructible");
            if (mapObj != null) 
                destructibleTiles = mapObj.GetComponent<Tilemap>();
            else 
                Debug.LogWarning("No se encontró el Tilemap de bloques destructibles. Renombra tu Grid a 'Destructibles'.");
        }
    }

    private void Update()
    {
        // Bloquear si el match no ha empezado
        if (MatchManager.Instance != null && !MatchManager.Instance.isMatchStarted.Value) return;

        if (IsSpawned && !IsOwner) return;

        if (Input.GetKeyDown(inputKey)) {
            TryPlaceBomb();
        }
    }

    public void TryPlaceBomb()
    {
        if (bombsRemaining > 0) {
            bombsRemaining--;
            if (IsSpawned) {
                PlaceBombServerRpc(transform.position);
            } else {
                // Modo local/entrenamiento: colocar bomba directamente
                Vector2 pos = transform.position;
                pos.x = Mathf.Floor(pos.x) + 0.5f;
                pos.y = Mathf.Floor(pos.y) + 0.5f;
                StartCoroutine(PlaceBombRoutineLocal(pos));
            }
        }
    }

    private IEnumerator PlaceBombRoutineLocal(Vector2 position)
    {
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        
        yield return new WaitForSeconds(bombFuseTime);

        if (bomb != null) {
            position = bomb.transform.position;
            position.x = Mathf.Floor(position.x) + 0.5f;
            position.y = Mathf.Floor(position.y) + 0.5f;
            
            SpawnExplosionLocal(position, Vector2.zero, 0, Color.white);

            ExplodeLocal(position, Vector2.up, explosionRadius, Color.white);
            ExplodeLocal(position, Vector2.down, explosionRadius, Color.white);
            ExplodeLocal(position, Vector2.left, explosionRadius, Color.white);
            ExplodeLocal(position, Vector2.right, explosionRadius, Color.white);

            Destroy(bomb);
        }
        bombsRemaining++;
    }

    private void ExplodeLocal(Vector2 position, Vector2 direction, int length, Color color)
    {
        if (length <= 0) return;
        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask)) 
        {
            ClearDestructibleLocal(position, color);
            return;
        }

        SpawnExplosionLocal(position, direction, length > 1 ? 1 : 2, color);
        ExplodeLocal(position, direction, length - 1, color);
    }

    private void SpawnExplosionLocal(Vector2 position, Vector2 direction, int type, Color color)
    {
        if (explosionPrefab == null) return;
        Explosion explosion = Instantiate(explosionPrefab.gameObject, position, Quaternion.identity).GetComponent<Explosion>();
        
        if (type == 0) explosion.SetActiveRenderer(explosion.start);
        else if (type == 1) explosion.SetActiveRenderer(explosion.middle);
        else explosion.SetActiveRenderer(explosion.end);

        if (direction != Vector2.zero) explosion.SetDirection(direction);
        
        explosion.SetPaintColor(color);
        explosion.DestroyAfter(explosionDuration);
    }

    private void ClearDestructibleLocal(Vector2 position, Color color)
    {
        Tilemap map = GetDestructibleMap();
        if (map == null) return;

        Vector3Int cell = map.WorldToCell(position);
        if (map.GetTile(cell) != null)
        {
            map.SetTile(cell, null);
            if (TilePainter.Instance != null) TilePainter.Instance.Paint(position, color);
            if (destructiblePrefab != null) {
                Instantiate(destructiblePrefab.gameObject, position, Quaternion.identity);
            }
        }
    }

    [ServerRpc]
    private void PlaceBombServerRpc(Vector2 position, ServerRpcParams rpcParams = default)
    {
        position.x = Mathf.Floor(position.x) + 0.5f;
        position.y = Mathf.Floor(position.y) + 0.5f;

        StartCoroutine(PlaceBombRoutine(position, rpcParams.Receive.SenderClientId));
    }

    private IEnumerator PlaceBombRoutine(Vector2 position, ulong clientId)
    {
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn();

        // Obtener el color de ESTE jugador directamente desde su componente
        Color playerColor = Color.white;
        PlayerColorProvider colorProvider = GetComponent<PlayerColorProvider>();
        if (colorProvider != null) {
            playerColor = colorProvider.GetColor();
        }

        yield return new WaitForSeconds(bombFuseTime);

        if (bomb != null) {
            position = bomb.transform.position;
            position.x = Mathf.Floor(position.x) + 0.5f;
            position.y = Mathf.Floor(position.y) + 0.5f;
            
            SpawnExplosionClientRpc(position, Vector2.zero, 0, playerColor);

            Explode(position, Vector2.up, explosionRadius, playerColor);
            Explode(position, Vector2.down, explosionRadius, playerColor);
            Explode(position, Vector2.left, explosionRadius, playerColor);
            Explode(position, Vector2.right, explosionRadius, playerColor);

            var netObj = bomb.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn();
            else
                Destroy(bomb);
        }
        
        // Restaurar bomba directamente en el objeto
        RestoreBombClientRpc();
    }

    [ClientRpc]
    private void RestoreBombClientRpc()
    {
        // Al ser un ClientRpc ejecutado en el objeto que posee el script, 
        // solo el dueño o el servidor que tiene este objeto sumará la bomba.
        bombsRemaining++;
    }

    private void Explode(Vector2 position, Vector2 direction, int length, Color color)
    {
        if (length <= 0) {
            return;
        }

        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask)) 
        {
            ClearDestructible(position, color);
            return;
        }

        SpawnExplosionClientRpc(position, direction, length > 1 ? 1 : 2, color);
        Explode(position, direction, length - 1, color);
    }

    [ClientRpc]
    private void SpawnExplosionClientRpc(Vector2 position, Vector2 direction, int type, Color color)
    {
        if (explosionPrefab == null) return;
        
        Explosion explosion = Instantiate(explosionPrefab.gameObject, position, Quaternion.identity).GetComponent<Explosion>();
        
        if (type == 0) explosion.SetActiveRenderer(explosion.start);
        else if (type == 1) explosion.SetActiveRenderer(explosion.middle);
        else explosion.SetActiveRenderer(explosion.end);

        if (direction != Vector2.zero)
            explosion.SetDirection(direction);

        explosion.SetPaintColor(color);
        explosion.DestroyAfter(explosionDuration);
    }

    private Tilemap GetDestructibleMap()
    {
        if (destructibleTiles == null)
        {
            GameObject mapObj = GameObject.Find("Destructibles") ?? GameObject.Find("Destructible");
            if (mapObj != null) destructibleTiles = mapObj.GetComponent<Tilemap>();
        }
        return destructibleTiles;
    }

    private void ClearDestructible(Vector2 position, Color color)
    {
        Tilemap map = GetDestructibleMap();
        if (map == null) return;

        Vector3Int cell = map.WorldToCell(position);
        TileBase tile = map.GetTile(cell);

        if (tile != null)
        {
            map.SetTile(cell, null);
            ClearDestructibleClientRpc(cell);
            
            // Pintar también bajo el bloque que acabamos de romper
            if (TilePainter.Instance != null) TilePainter.Instance.Paint(position, color);

            if (destructiblePrefab != null) {
                GameObject dest = Instantiate(destructiblePrefab.gameObject, position, Quaternion.identity);
                dest.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    [ClientRpc]
    private void ClearDestructibleClientRpc(Vector3Int cell)
    {
        if (IsServer) return; 
        Tilemap map = GetDestructibleMap();
        if (map != null) map.SetTile(cell, null);
    }

    public void AddBomb()
    {
        if (IsServer) {
            bombAmount++;
            bombsRemaining++;
            if (IsSpawned) AddBombClientRpc();
        }
    }

    [ClientRpc]
    private void AddBombClientRpc()
    {
        if (IsServer) return; // El servidor ya lo sumó en AddBomb()
        bombAmount++;
        bombsRemaining++;
    }

    public void ResetStats()
    {
        StopAllCoroutines(); // CRÍTICO: Detiene bombas en vuelo para que no sumen al terminar
        bombAmount = 1;
        explosionRadius = 1;
        bombsRemaining = 1;
        
        if (IsServer && IsSpawned) {
            ResetStatsClientRpc();
        }
    }

    [ClientRpc]
    private void ResetStatsClientRpc()
    {
        if (IsServer) return; // El servidor ya lo hizo en ResetStats()
        bombAmount = 1;
        explosionRadius = 1;
        bombsRemaining = 1;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb")) {
            other.isTrigger = false;
        }
    }
}
