using UnityEngine;
using Unity.Netcode;

public class PlayerColorProvider : NetworkBehaviour
{
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SetColorRoutine());
        }
    }

    private System.Collections.IEnumerator SetColorRoutine()
    {
        // Esperamos un frame para asegurar que PlayerData haya inicializado el playerId
        yield return null;

        PlayerData pd = GetComponent<PlayerData>();
        if (pd != null)
        {
            // ID 1 -> Blue (Player 2 / AI)
            // ID 0 -> Red (Player 1 / Host)
            playerColor.Value = (pd.playerId.Value == 1) ? Color.blue : Color.red;
            
            Debug.Log($"[COLOR] {gameObject.name} (ID {pd.playerId.Value}) color asignado: {(pd.playerId.Value == 1 ? "Azul" : "Rojo")}");
        }
    }

    public Color GetColor()
    {
        return playerColor.Value;
    }
}
