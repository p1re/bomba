using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerData : NetworkBehaviour
{
    // ID: 0 para Humano, 1 para IA
    public NetworkVariable<int> playerId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentLives = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public Vector3 spawnPoint;
    private MovementController moveController;
    private BombController bombController;
    
    private NetworkVariable<bool> isInvulnerable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        moveController = GetComponent<MovementController>();
        bombController = GetComponent<BombController>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawnPoint = transform.position;
            currentLives.Value = 3;

            // Lógica Maestra de IDs:
            if (LobbyManager.IsLocalMode)
            {
                // Modo Local: IA (ID 1) vs Humano (ID 0)
                bool isAI = (GetComponent<Unity.MLAgents.Agent>() != null);
                playerId.Value = isAI ? 1 : 0;
            }
            else
            {
                // Modo Online: Host (ID 0) vs Cliente (ID 1+)
                playerId.Value = (int)OwnerClientId;
            }
            
            Debug.Log($"[DATA] {gameObject.name} inicializado como Jugador {playerId.Value} (Modo: {(LobbyManager.IsLocalMode ? "Local" : "Online")})");
        }
        isInvulnerable.OnValueChanged += OnInvulnerabilityChanged;
    }

    public void ResetLives()
    {
        if (IsServer)
        {
            currentLives.Value = 3;
            isInvulnerable.Value = false;
            StopAllCoroutines();
            SetRenderersEnabled(true);
        }
    }

    private void OnInvulnerabilityChanged(bool oldVal, bool newVal)
    {
        if (newVal) StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (isInvulnerable.Value)
        {
            SetRenderersEnabled(false);
            yield return new WaitForSeconds(0.1f);
            SetRenderersEnabled(true);
            yield return new WaitForSeconds(0.1f);
        }
        SetRenderersEnabled(true);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        foreach (var r in renderers) r.enabled = enabled;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc()
    {
        if (isInvulnerable.Value) return;

        if (currentLives.Value > 0)
        {
            currentLives.Value--;
            Debug.Log($"[DAMAGE] Player {playerId.Value} ({gameObject.name}) hit! Lives: {currentLives.Value}");
            
            if (currentLives.Value <= 0)
            {
                if (LocalGameManager.Instance != null && LocalGameManager.Instance.isLocalMode) {
                    LocalGameManager.Instance.NotifyDeath(gameObject);
                } else {
                    Respawn();
                }
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine(2f));
            }
        }
    }

    private IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable.Value = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable.Value = false;
    }

    private void Respawn()
    {
        moveController.ResetStats();
        bombController.ResetStats();
        moveController.ApplyInitialPosition();
        currentLives.Value = 3;
        StartCoroutine(InvulnerabilityRoutine(2f));
    }
}
