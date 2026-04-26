using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// --- ALIAS PARA EVITAR EL CHOQUE DE PAQUETES ---
using Lobby = Unity.Services.Lobbies.Models.Lobby;
using Allocation = Unity.Services.Relay.Models.Allocation;
using JoinAllocation = Unity.Services.Relay.Models.JoinAllocation;
using RelayService = Unity.Services.Relay.RelayService;

public class LobbyManager : MonoBehaviour
{
    private Lobby currentLobby;
    private float heartbeatTimer;
    private float pollTimer;
    
    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    private Button playButton;
    private Button playLocalButton;
    private Label waitingText;

    public static bool IsLocalMode = false;

    private void OnEnable()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var root = uiDocument.rootVisualElement;
            playButton = root.Q<Button>("playButton");
            playLocalButton = root.Q<Button>("playLocalButton");
            waitingText = root.Q<Label>("waitingText");

            if (playButton != null) playButton.clicked += OnPlayButtonClicked;
            if (playLocalButton != null) playLocalButton.clicked += OnPlayLocalButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.clicked -= OnPlayButtonClicked;
        if (playLocalButton != null) playLocalButton.clicked -= OnPlayLocalButtonClicked;
    }

    public void OnPlayLocalButtonClicked()
    {
        IsLocalMode = true;
        Debug.Log("Iniciando sesión de ENTRENAMIENTO IA (Modo Local)...");
        SetWaitingText("Iniciando entrenamiento IA...");

        if (NetworkManager.Singleton != null)
        {
            // Intentamos iniciar el host, pero si falla (ej. puerto ocupado), 
            // igual cargamos la escena porque el entrenamiento IA ahora es independiente
            if (!NetworkManager.Singleton.StartHost()) {
                Debug.LogWarning("No se pudo iniciar el Host de Red. Cargando escena en modo OFFLINE para entrenamiento.");
                SceneManager.LoadScene("Assets/Scenes/Bomberman.unity");
            }
            else
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Assets/Scenes/Bomberman.unity", LoadSceneMode.Single);
            }
        }
        else
        {
            // Si ni siquiera hay NetworkManager, cargamos la escena por el método tradicional
            SceneManager.LoadScene("Assets/Scenes/Bomberman.unity");
        }
    }

    async void Start()
    {
        try 
        {
            if (UnityServices.State.ToString() == "Uninitialized")
            {
                // Esto asegura que cada ventana del editor tenga un jugador distinto
                var options = new InitializationOptions();
                options.SetProfile("Player_" + Random.Range(0, 100000).ToString());
                await UnityServices.InitializeAsync(options);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("User Signed in: " + AuthenticationService.Instance.PlayerId);
            }
            
            SetWaitingText("Conectado. Presiona JUGAR.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en autenticación: " + e.Message);
            SetWaitingText("Error de conexión.");
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        CheckPlayersInLobby();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby != null && AuthenticationService.Instance.IsSignedIn && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }
        }
    }

    private async void CheckPlayersInLobby()
    {
        if (currentLobby != null)
        {
            pollTimer -= Time.deltaTime;
            if (pollTimer < 0f)
            {
                pollTimer = 1.5f; 
                
                try {
                    currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                    SetWaitingText($"Sala encontrada. Jugadores: {currentLobby.Players.Count}/2");

                    if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                    {
                        if (currentLobby.Players.Count >= 2 && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                        {
                            currentLobby = null; 
                            NetworkManager.Singleton.SceneManager.LoadScene("Assets/Scenes/Bomberman.unity", LoadSceneMode.Single);
                        }
                    }
                } 
                catch (LobbyServiceException e) {
                    Debug.Log("Lobby poll error: " + e.Message);
                }
            }
        }
    }

    private void SetWaitingText(string txt)
    {
        if (waitingText != null) waitingText.text = txt;
    }

    public void OnPlayButtonClicked()
    {
        IsLocalMode = false; // Resetear modo local al buscar partida online
        if (NetworkManager.Singleton == null)
        {
            SetWaitingText("ERROR: Falta el NetworkManager en esta escena.");
            return;
        }

        if (playButton != null) playButton.style.display = DisplayStyle.None;
        SetWaitingText("Buscando sala...");
        
        QuickJoinLobby();
    }

    private async void QuickJoinLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(currentLobby.Data["JoinCode"].Value);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("No open lobbies to join. Creating a new one instead..." + e);
            CreateLobby();
        }
    }

    private async void CreateLobby()
    {
        try
        {
            SetWaitingText("Creando sala...");

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new CreateLobbyOptions {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject> {
                    { "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: joinCode) }
                }
            };
            
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("BombermanRoom", 2, options);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            SetWaitingText($"Sala Creada. Jugadores: 1/2");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
            SetWaitingText("Error al crear la sala.");
        }
    }
}
