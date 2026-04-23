using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class HUDController : MonoBehaviour
{
    private Label p1LivesLabel;
    private Label p2LivesLabel;
    private Label p1AreaLabel;
    private Label p2AreaLabel;
    private Label timerLabel;
    
    // Countdown
    private Label countdownLabel;
    
    // End Screen
    private VisualElement endScreen;
    private Label resultTitle;
    private Label statsLabel;
    private Button backToLobbyButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        p1LivesLabel = root.Q<Label>("p1Lives");
        p2LivesLabel = root.Q<Label>("p2Lives");
        p1AreaLabel = root.Q<Label>("p1Score");
        p2AreaLabel = root.Q<Label>("p2Score");
        timerLabel = root.Q<Label>("timer");
        
        countdownLabel = root.Q<Label>("countdownLabel");
        
        endScreen = root.Q<VisualElement>("endScreen");
        resultTitle = root.Q<Label>("resultTitle");
        statsLabel = root.Q<Label>("statsLabel");
        backToLobbyButton = root.Q<Button>("backToLobbyButton");

        if (backToLobbyButton != null)
        {
            backToLobbyButton.clicked += OnBackToLobbyClicked;
        }
    }

    private void OnDisable()
    {
        if (backToLobbyButton != null)
        {
            backToLobbyButton.clicked -= OnBackToLobbyClicked;
        }
    }

    private void Update()
    {
        UpdatePlayerStats();
        UpdateTimer();
        UpdateCountdown();
    }

    private void UpdatePlayerStats()
    {
        var players = Object.FindObjectsByType<PlayerData>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.playerId.Value == 0) // Player 1
            {
                if (p1LivesLabel != null) p1LivesLabel.text = $"Lives: {player.currentLives.Value}";
            }
            else if (player.playerId.Value == 1) // Player 2 / AI
            {
                if (p2LivesLabel != null) p2LivesLabel.text = $"Lives: {player.currentLives.Value}";
            }
        }
    }

    private void UpdateTimer()
    {
        if (MatchManager.Instance != null && timerLabel != null)
        {
            timerLabel.text = Mathf.CeilToInt(MatchManager.Instance.remainingTime.Value).ToString();
        }
    }

    private void UpdateCountdown()
    {
        if (MatchManager.Instance == null || countdownLabel == null) return;

        if (MatchManager.Instance.isMatchStarted.Value)
        {
            countdownLabel.style.display = DisplayStyle.None;
            return;
        }

        countdownLabel.style.display = DisplayStyle.Flex;
        
        // Comprobar si estamos esperando o en cuenta atrás
        int countdown = MatchManager.Instance.countdownTimer.Value;
        int requiredPlayers = LobbyManager.IsLocalMode ? 1 : 2;

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.ConnectedClients.Count < requiredPlayers)
        {
            int clientCount = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
            countdownLabel.text = $"WAITING FOR PLAYERS... ({clientCount}/{requiredPlayers})";
        }
        else if (countdown > 0)
        {
            countdownLabel.text = countdown.ToString();
        }
        else
        {
            countdownLabel.text = "GO!";
        }
    }

    public void UpdateAreaScores(float p1Percent, float p2Percent)
    {
        if (p1AreaLabel != null) p1AreaLabel.text = $"Area: {p1Percent:F1}%";
        if (p2AreaLabel != null) p2AreaLabel.text = $"Area: {p2Percent:F1}%";
    }

    public void ShowEndScreen(float p1Score, float p2Score)
    {
        if (endScreen == null) return;

        endScreen.style.display = DisplayStyle.Flex;
        statsLabel.text = $"P1 Area: {p1Score:F1}% | P2 Area: {p2Score:F1}%";

        // Determinar ganador local
        ulong localId = NetworkManager.Singleton.LocalClientId;
        bool isP1 = (localId == 0);
        
        if (p1Score > p2Score)
        {
            resultTitle.text = isP1 ? "YOU WIN!" : "YOU LOSE!";
            resultTitle.style.color = isP1 ? new Color(0.18f, 0.8f, 0.44f) : new Color(0.9f, 0.3f, 0.23f);
        }
        else if (p2Score > p1Score)
        {
            resultTitle.text = isP1 ? "YOU LOSE!" : "YOU WIN!";
            resultTitle.style.color = isP1 ? new Color(0.9f, 0.3f, 0.23f) : new Color(0.18f, 0.8f, 0.44f);
        }
        else
        {
            resultTitle.text = "TIE!";
            resultTitle.style.color = Color.white;
        }
    }

    private void OnBackToLobbyClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("MainMenu");
    }
}
