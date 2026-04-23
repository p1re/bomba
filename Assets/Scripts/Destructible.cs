using UnityEngine;
using Unity.Netcode;

public class Destructible : NetworkBehaviour
{
    public float destructionTime = 1f;
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    public GameObject[] spawnableItems;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Invoke(nameof(DespawnDestructible), destructionTime);
        }
    }

    private void DespawnDestructible()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    public override void OnNetworkDespawn()
    {
        // Notificar a la IA si fue ella quien lo destruyó (o simplemente por el hecho de destruirse)
        BombermanAgent agent = Object.FindFirstObjectByType<BombermanAgent>();
        if (agent != null) agent.OnWallDestroyed();

        if (IsServer && spawnableItems.Length > 0 && Random.value < itemSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            GameObject item = Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
            item.GetComponent<NetworkObject>().Spawn();
        }
    }
}