using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("Configuración del Match")]
    public NetworkVariable<float> remainingTime = new NetworkVariable<float>(60f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> countdownTimer = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isMatchStarted = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private HUDController hud;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        hud = Object.FindAnyObjectByType<HUDController>();
        if (IsServer)
        {
            remainingTime.Value = 60f;
            countdownTimer.Value = 5;
            isMatchStarted.Value = false;
            isGameOver.Value = false;
            StartCoroutine(WaitAndCountdownRoutine());
        }
    }

    private IEnumerator WaitAndCountdownRoutine()
    {
        int requiredPlayers = LobbyManager.IsLocalMode ? 1 : 2;
        
        while (NetworkManager.Singleton.ConnectedClients.Count < requiredPlayers)
        {
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        while (countdownTimer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            countdownTimer.Value--;
        }

        isMatchStarted.Value = true;
        StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        while (remainingTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime.Value -= 1f;
            UpdateScores();
        }

        EndMatch();
    }

    private void UpdateScores()
    {
        if (TilePainter.Instance != null && IsServer)
        {
            float p1, p2;
            TilePainter.Instance.GetScores(out p1, out p2);
            UpdateScoresClientRpc(p1, p2);
        }
    }

    [ClientRpc]
    private void UpdateScoresClientRpc(float p1, float p2)
    {
        if (hud != null) hud.UpdateAreaScores(p1, p2);
    }

    private void EndMatch()
    {
        if (!IsServer) return;

        isGameOver.Value = true;
        isMatchStarted.Value = false;
        
        float p1, p2;
        TilePainter.Instance.GetScores(out p1, out p2);
        
        ShowEndScreenClientRpc(p1, p2);
        
        string winnerName = p1 > p2 ? "PLAYER 1" : (p2 > p1 ? "PLAYER 2" : "TIE");
        var reporter = GetComponent<MatchResultReporter>();
        if (reporter != null) reporter.ReportResult(winnerName, p1, p2);
    }

    [ClientRpc]
    private void ShowEndScreenClientRpc(float p1, float p2)
    {
        if (hud != null) hud.ShowEndScreen(p1, p2);
    }
}
