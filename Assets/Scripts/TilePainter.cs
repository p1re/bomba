using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;
using System.Collections.Generic;

// Estructura para sincronizar cada baldosa por red
public struct TileStatus : INetworkSerializable, System.IEquatable<TileStatus>
{
    public int x, y;
    public int colorIndex; // 0: none, 1: red, 2: blue

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref colorIndex);
    }

    public bool Equals(TileStatus other)
    {
        return x == other.x && y == other.y && colorIndex == other.colorIndex;
    }
}

public class TilePainter : NetworkBehaviour
{
    public static TilePainter Instance { get; private set; }

    public Tilemap floorTilemap;
    public TileBase paintTile;

    // Memoria persistente
    public NetworkList<TileStatus> paintedTilesList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        paintedTilesList = new NetworkList<TileStatus>();
    }

    public override void OnNetworkSpawn()
    {
        paintedTilesList.OnListChanged += OnTileListChanged;
        RepaintAll();
    }

    public override void OnNetworkDespawn()
    {
        if (paintedTilesList != null)
        {
            paintedTilesList.OnListChanged -= OnTileListChanged;
        }
    }

    private void OnTileListChanged(NetworkListEvent<TileStatus> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<TileStatus>.EventType.Add || 
            changeEvent.Type == NetworkListEvent<TileStatus>.EventType.Value)
        {
            ApplyPaintVisual(changeEvent.Value.x, changeEvent.Value.y, changeEvent.Value.colorIndex);
        }
    }

    // MÉTODO PRINCIPAL: Lo llaman las explosiones locales de cada jugador
    public void Paint(Vector3 worldPosition, Color color)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            // Modo Offline
            Vector3Int cell = WorldToCellFixed(worldPosition);
            ApplyPaintVisual(cell.x, cell.y, ColorToIndex(color));
            return;
        }

        // Si somos el cliente, le pedimos al servidor que pinte de forma oficial
        if (!IsServer)
        {
            if (IsSpawned)
            {
                RequestPaintServerRpc(worldPosition, color);
            }
        }
        else
        {
            // Si somos el servidor, pintamos directamente
            ExecutePaint(worldPosition, color);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestPaintServerRpc(Vector3 worldPos, Color color)
    {
        // El servidor recibe la petición del cliente y la ejecuta oficialmente
        ExecutePaint(worldPos, color);
    }

    private void ExecutePaint(Vector3 worldPos, Color color)
    {
        Vector3Int cell = WorldToCellFixed(worldPos);
        int colorIdx = ColorToIndex(color);

        if (colorIdx == 0) {
            return;
        }

        // 1. Guardar en memoria de red (Servidor)
        UpdateTileInList(cell.x, cell.y, colorIdx);

        // 2. Pintar instantáneamente en todos los clientes
        PaintInstantClientRpc(cell.x, cell.y, colorIdx);
    }

    [ClientRpc]
    private void PaintInstantClientRpc(int x, int y, int colorIdx)
    {
        ApplyPaintVisual(x, y, colorIdx);
    }

    private void UpdateTileInList(int x, int y, int colorIdx)
    {
        for (int i = 0; i < paintedTilesList.Count; i++)
        {
            if (paintedTilesList[i].x == x && paintedTilesList[i].y == y)
            {
                paintedTilesList[i] = new TileStatus { x = x, y = y, colorIndex = colorIdx };
                return;
            }
        }
        paintedTilesList.Add(new TileStatus { x = x, y = y, colorIndex = colorIdx });
    }

    private void ApplyPaintVisual(int x, int y, int colorIdx)
    {
        if (floorTilemap == null) {
            return;
        }
        
        Vector3Int cell = new Vector3Int(x, y, 0);
        Color targetColor = IndexToColor(colorIdx);

        if (floorTilemap.GetTile(cell) == null)
        {
            if (paintTile != null) {
                floorTilemap.SetTile(cell, paintTile);
            }
        }

        floorTilemap.SetTileFlags(cell, TileFlags.None);
        floorTilemap.SetColor(cell, targetColor);
    }

    public void RepaintAll()
    {
        if (floorTilemap == null) return;
        foreach (var tile in paintedTilesList)
        {
            ApplyPaintVisual(tile.x, tile.y, tile.colorIndex);
        }
    }

    private Vector3Int WorldToCellFixed(Vector3 worldPos)
    {
        return floorTilemap.WorldToCell(new Vector3(
            Mathf.Floor(worldPos.x) + 0.1f, 
            Mathf.Floor(worldPos.y) + 0.1f, 
            0));
    }

    private int ColorToIndex(Color c)
    {
        if (IsSimilarColor(c, Color.red)) return 1;
        if (IsSimilarColor(c, Color.blue)) return 2;
        if (IsSimilarColor(c, Color.white)) return 3; // Permitir blanco
        return 0;
    }

    private Color IndexToColor(int index)
    {
        if (index == 1) return Color.red;
        if (index == 2) return Color.blue;
        if (index == 3) return Color.white; // Devolver blanco
        return Color.white;
    }

    private bool IsSimilarColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.1f && Mathf.Abs(a.g - b.g) < 0.1f && Mathf.Abs(a.b - b.b) < 0.1f;
    }

    public void GetScores(out float p1Percent, out float p2Percent)
    {
        int p1Count = 0;
        int p2Count = 0;

        foreach (var tile in paintedTilesList)
        {
            if (tile.colorIndex == 1) p1Count++;
            else if (tile.colorIndex == 2) p2Count++;
        }

        int totalTiles = 15 * 13; 
        p1Percent = (float)p1Count / totalTiles * 100f;
        p2Percent = (float)p2Count / totalTiles * 100f;
    }
}
