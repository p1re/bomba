using UnityEngine;
using Unity.Netcode;

public class ItemPickup : NetworkBehaviour
{
    public enum ItemType
    {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease,
    }

    public ItemType type;

    private void OnItemPickup(GameObject player)
    {
        BombermanAgent agent = player.GetComponent<BombermanAgent>();
        if (agent != null) agent.OnItemPickedUp();

        switch (type)
        {
            case ItemType.ExtraBomb:
                player.GetComponent<BombController>().AddBomb();
                break;

            case ItemType.BlastRadius:
                player.GetComponent<BombController>().explosionRadius++;
                break;

            case ItemType.SpeedIncrease:
                player.GetComponent<MovementController>().speed.Value++;
                break;
        }

        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player")) {
            OnItemPickup(other.gameObject);
        }
    }
}