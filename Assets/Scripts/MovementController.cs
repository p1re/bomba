using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class MovementController : NetworkBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }
    public NetworkVariable<Vector2> direction = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> speed = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private Vector2 nextDirection = Vector2.zero;
    private bool isAI = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        isAI = (GetComponent<Unity.MLAgents.Agent>() != null);
        
        // Forzar capa Player para asegurar colisiones con explosiones
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public override void OnNetworkSpawn()
    {
        direction.OnValueChanged += OnDirectionChanged;
        
        // Solo el servidor o el dueño pueden posicionar inicialmente
        if (IsServer || IsOwner) {
            ApplyInitialPosition();
        }
    }

    public void ApplyInitialPosition()
    {
        Vector3 targetPos;

        if (LobbyManager.IsLocalMode || !IsSpawned) {
            targetPos = isAI ? new Vector3(14.5f, -7.5f, 0f) : new Vector3(-15.5f, 8.5f, 0f);
        } else {
            targetPos = (OwnerClientId == 0) ? new Vector3(-15.5f, 8.5f, 0f) : new Vector3(14.5f, -7.5f, 0f);
        }

        // Solo usamos Teleport de red si NO estamos en modo local y tenemos autoridad
        var nt = GetComponent<NetworkTransform>();
        if (nt != null && IsSpawned && !LobbyManager.IsLocalMode) {
            if (IsServer) {
                try {
                    nt.Teleport(targetPos, Quaternion.identity, Vector3.one);
                } catch (System.Exception e) {
                    Debug.LogWarning("Error en Teleport de red, usando posicionamiento normal: " + e.Message);
                }
            }
        }

        // Posicionamiento físico (esto siempre funciona)
        transform.position = targetPos;
        if (rigidbody != null) {
            rigidbody.position = targetPos;
            rigidbody.linearVelocity = Vector2.zero;
        }

        // ACTIVACIÓN CRÍTICA: Asegurarnos de que los scripts estén encendidos
        enabled = true;
        var bc = GetComponent<BombController>();
        if (bc != null) bc.enabled = true;
        
        if (spriteRendererDeath != null) spriteRendererDeath.enabled = false;
        if (spriteRendererDown != null) spriteRendererDown.enabled = true;
    }

    private void OnDirectionChanged(Vector2 previous, Vector2 current)
    {
        if (!IsOwner) UpdateVisuals(current);
    }

    private void Update()
    {
        // Bloquear si el match no ha empezado
        if (MatchManager.Instance != null && !MatchManager.Instance.isMatchStarted.Value) {
            nextDirection = Vector2.zero;
            UpdateVisuals(Vector2.zero);
            return;
        }

        // Permitir control si somos dueños o si no estamos en red (entrenamiento/local)
        if (IsSpawned && !IsOwner) return;

        HandleInput();

        if (IsSpawned) {
            if (direction.Value != nextDirection)
            {
                direction.Value = nextDirection;
                UpdateVisuals(nextDirection);
            }
        } else {
            UpdateVisuals(nextDirection);
        }
    }

    private void HandleInput()
    {
        // Si es IA, el teclado no hace nada
        if (isAI) return;

        Vector2 inputDir = Vector2.zero;
        if (Input.GetKey(inputUp)) inputDir = Vector2.up;
        else if (Input.GetKey(inputDown)) inputDir = Vector2.down;
        else if (Input.GetKey(inputLeft)) inputDir = Vector2.left;
        else if (Input.GetKey(inputRight)) inputDir = Vector2.right;

        if (inputDir != Vector2.zero || !Input.anyKey) {
            nextDirection = inputDir;
        }
    }

    public void SetNextDirection(Vector2 newDir) => nextDirection = newDir;

    public void ResetStats()
    {
        if (IsServer) speed.Value = 5f;
        else if (!IsSpawned) speed.Value = 5f; // Para modo entrenamiento/local
    }

    private void FixedUpdate()
    {
        // Si estamos en red pero no somos dueños, el movimiento lo dicta el dueño/servidor
        if (IsSpawned && !IsOwner) return;

        Vector2 moveDir = IsSpawned ? direction.Value : nextDirection;
        rigidbody.MovePosition(rigidbody.position + (moveDir * speed.Value * Time.fixedDeltaTime));
    }

    private void UpdateVisuals(Vector2 currentDirection)
    {
        if (activeSpriteRenderer == null) return;
        AnimatedSpriteRenderer spriteRenderer = activeSpriteRenderer;
        if (currentDirection == Vector2.up) spriteRenderer = spriteRendererUp;
        else if (currentDirection == Vector2.down) spriteRenderer = spriteRendererDown;
        else if (currentDirection == Vector2.left) spriteRenderer = spriteRendererLeft;
        else if (currentDirection == Vector2.right) spriteRenderer = spriteRendererRight;

        spriteRendererUp.enabled = (spriteRenderer == spriteRendererUp);
        spriteRendererDown.enabled = (spriteRenderer == spriteRendererDown);
        spriteRendererLeft.enabled = (spriteRenderer == spriteRendererLeft);
        spriteRendererRight.enabled = (spriteRenderer == spriteRendererRight);
        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = (currentDirection == Vector2.zero);
    }

    [ClientRpc]
    private void DeathSequenceClientRpc()
    {
        // Desactivar el control y el movimiento
        enabled = false;
        if (rigidbody != null) rigidbody.linearVelocity = Vector2.zero;
        
        var bc = GetComponent<BombController>();
        if (bc != null) bc.enabled = false;
        
        // Ocultar TODOS los renderers de movimiento
        if (spriteRendererUp != null) spriteRendererUp.gameObject.SetActive(false);
        if (spriteRendererDown != null) spriteRendererDown.gameObject.SetActive(false);
        if (spriteRendererLeft != null) spriteRendererLeft.gameObject.SetActive(false);
        if (spriteRendererRight != null) spriteRendererRight.gameObject.SetActive(false);
        
        // Activar solo el de muerte
        if (spriteRendererDeath != null) {
            spriteRendererDeath.gameObject.SetActive(true);
            spriteRendererDeath.idle = false;
            spriteRendererDeath.RestartAnimation();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        
        // Comprobar por tag o por layer para ser más robustos
        if (other.CompareTag("Explosion") || other.gameObject.layer == LayerMask.NameToLayer("Explosion")) {
            Debug.Log($"[COLLISION] {gameObject.name} hit by explosion!");
            PlayerData data = GetComponent<PlayerData>();
            if (data != null) {
                data.TakeDamageServerRpc();
            }
        }
    }
}
