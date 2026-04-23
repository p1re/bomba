using UnityEngine;
using System;
using System.Collections;
// Note: This script assumes a NativeWebSocket library is added to the project, which is standard for cross-platform.
// If not already in the project, user would need to add it via UPM or as a .unitypackage.
using NativeWebSocket;

public class WebSocketManager : MonoBehaviour
{
    private WebSocket websocket;
    public string serverUrl = "ws://localhost:8080";

    async void Start()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket Connection Open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("WebSocket Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket Connection Closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Handle incoming server messages (e.g., room status updates)
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
        };

        // Keep sending messages at interval
        InvokeRepeating("SendPing", 0.0f, 10.0f);

        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void SendPing()
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText("{\"type\":\"ping\"}");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
