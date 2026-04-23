using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

public class LocalGameManager : NetworkBehaviour
{
    public static LocalGameManager Instance;
    
    [Header("Config")]
    public bool isLocalMode = false;
    public GameObject playerPrefab; // Tu prefab que tiene el script BombermanAgent
    private bool isResetting = false;

    private Tilemap destructibleTiles;
    private List<Vector3Int> initialTilePositions = new List<Vector3Int>();
    private List<TileBase> initialTiles = new List<TileBase>();

    private void Awake()
    {
        Instance = this;
        isLocalMode = LobbyManager.IsLocalMode;
    }

    private void Start()
    {
        SaveInitialMap();
        if (isLocalMode) {
            Invoke(nameof(SetupLocalGame), 0.5f); // Un poco más de delay para asegurar que el jugador local haya spawneado
        }
    }

    private void SaveInitialMap()
    {
        GameObject mapObj = GameObject.Find("Destructibles") ?? GameObject.Find("Destructible");
        if (mapObj != null) {
            destructibleTiles = mapObj.GetComponent<Tilemap>();
            BoundsInt bounds = destructibleTiles.cellBounds;
            foreach (var pos in bounds.allPositionsWithin) {
                TileBase tile = destructibleTiles.GetTile(pos);
                if (tile != null) {
                    initialTilePositions.Add(pos);
                    initialTiles.Add(tile);
                }
            }
            Debug.Log($"[MAP] Guardados {initialTiles.Count} bloques destructibles.");
        }
    }

    private void SetupLocalGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[LOCAL] Configurando sesión de entrenamiento IA...");
            
            // 1. Encontrar al jugador humano local
            GameObject human = null;
            MovementController[] allPlayers = Object.FindObjectsByType<MovementController>(FindObjectsSortMode.None);
            foreach (var p in allPlayers)
            {
                if (p.IsOwner && !p.GetComponent<BombermanAgent>())
                {
                    human = p.gameObject;
                    PlayerData pd = human.GetComponent<PlayerData>();
                    if (pd != null) pd.playerId.Value = 0;
                    break;
                }
            }

            if (human == null)
            {
                Debug.LogWarning("[LOCAL] No se encontró al jugador humano local por IsOwner, buscando por Tag...");
                human = GameObject.FindWithTag("Player");
                if (human != null) {
                    PlayerData pd = human.GetComponent<PlayerData>();
                    if (pd != null) pd.playerId.Value = 0;
                }
            }

            // 2. Spawnear la IA si no existe
            Vector3 aiPos = new Vector3(14.5f, -7.5f, 0f);
            if (playerPrefab != null) {
                GameObject ai = Instantiate(playerPrefab, aiPos, Quaternion.identity);
                ai.name = "Player_AI";
                ai.GetComponent<NetworkObject>().Spawn();

                PlayerData aiData = ai.GetComponent<PlayerData>();
                if (aiData != null) aiData.playerId.Value = 1;

                BombermanAgent agent = ai.GetComponent<BombermanAgent>();
                if (agent != null) {
                    if (human != null) 
                    {
                        agent.opponent = human.transform;
                        Debug.Log($"[LOCAL] Oponente asignado: {human.name}");
                    }
                    
                    // Configurar BehaviorParameters para entrenamiento (Inference -> Default)
                    var bp = ai.GetComponent<BehaviorParameters>();
                    if (bp != null)
                    {
                        bp.BehaviorType = BehaviorType.Default;
                        Debug.Log("[LOCAL] BehaviorType establecido en Default para Entrenamiento.");
                    }
                }
            }
        }
    }

    private GameObject lastVictim;

    public void NotifyDeath(GameObject victim)
    {
        if (isResetting) return;
        isResetting = true;
        lastVictim = victim;

        // Quitamos el delay del Invoke y llamamos directamente o con un tiempo mínimo
        Invoke(nameof(RestartRound), 0.1f);
    }

    private void RestartRound()
    {
        if (isLocalMode && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            isResetting = false;

            // ... (limpieza igual)
            CleanObjects<NetworkObject>("Bomb");
            CleanObjects<NetworkObject>("Item");
            
            ItemPickup[] items = Object.FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
            foreach (var item in items) {
                if (item.GetComponent<NetworkObject>().IsSpawned) item.GetComponent<NetworkObject>().Despawn();
                else Destroy(item.gameObject);
            }

            Destructible[] activeDestructibles = Object.FindObjectsByType<Destructible>(FindObjectsSortMode.None);
            foreach (var d in activeDestructibles) {
                if (d.GetComponent<NetworkObject>().IsSpawned) d.GetComponent<NetworkObject>().Despawn();
                else Destroy(d.gameObject);
            }

            GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
            foreach (var e in explosions) Destroy(e);

            // 2. Restaurar el Mapa
            ResetMap();

            // 3. Resetear Jugadores y FIN DE EPISODIO
            MovementController[] players = Object.FindObjectsByType<MovementController>(FindObjectsSortMode.None);
            foreach (var p in players) {
                p.ResetStats();
                var bc = p.GetComponent<BombController>();
                if (bc != null) bc.ResetStats();
                
                var pd = p.GetComponent<PlayerData>();
                if (pd != null) pd.ResetLives();

                BombermanAgent agent = p.GetComponent<BombermanAgent>();
                if (agent != null) {
                    if (lastVictim == agent.gameObject) {
                        agent.SetReward(-5.0f); // CASTIGO AGRESIVO POR MORIR
                        Debug.Log("IA MUERE - Castigo -5.0");
                    } else if (lastVictim != null && lastVictim != agent.gameObject) {
                        agent.SetReward(2.0f);  // PREMIO POR GANAR
                        Debug.Log("IA GANA - Premio +2.0");
                    }
                    agent.EndEpisode(); // Terminamos el episodio justo después del reset del mapa
                }

                p.ApplyInitialPosition();
            }

            lastVictim = null;
            Debug.Log("[LOOP] Mapa y Jugadores reiniciados");
        }
    }

    private void CleanObjects<T>(string tag) where T : Component
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (var obj in objects) {
            var netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
            else Destroy(obj);
        }
    }

    private void ResetMap()
    {
        if (destructibleTiles != null) {
            destructibleTiles.ClearAllTiles();
            for (int i = 0; i < initialTilePositions.Count; i++) {
                destructibleTiles.SetTile(initialTilePositions[i], initialTiles[i]);
            }
            
            // Sincronizar con clientes solo si el objeto está spawneado en red
            if (IsSpawned) SyncMapClientRpc();
        }
    }

    [ClientRpc]
    private void SyncMapClientRpc()
    {
        if (IsServer) return;
        if (destructibleTiles != null) {
            destructibleTiles.ClearAllTiles();
            for (int i = 0; i < initialTilePositions.Count; i++) {
                destructibleTiles.SetTile(initialTilePositions[i], initialTiles[i]);
            }
        }
    }
}
