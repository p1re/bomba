using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Script para validar la conexión con el servidor Node.js mediante UnityWebRequest.
/// Este script realiza una petición HTTP GET al endpoint /status del servidor.
/// </summary>
public class ServerStatusChecker : MonoBehaviour
{
    [Header("Configuración del Servidor")]
    [Tooltip("La URL completa del endpoint de estado del servidor Node.js")]
    public string statusUrl = "http://localhost:8080/status";

    [Header("Ajustes de comprobación")]
    [Tooltip("¿Comprobar el estado automáticamente al iniciar el juego?")]
    public bool checkOnStart = true;

    void Start()
    {
        if (checkOnStart)
        {
            Debug.Log("<color=cyan>[WEB] Iniciando comprobación de estado del servidor...</color>");
            StartCoroutine(GetServerStatus());
        }
    }

    /// <summary>
    /// Corrutina que realiza la petición web al servidor.
    /// </summary>
    public IEnumerator GetServerStatus()
    {
        // 1. Creamos la petición GET hacia la URL configurada
        using (UnityWebRequest webRequest = UnityWebRequest.Get(statusUrl))
        {
            // 2. Enviamos la petición y esperamos a que el servidor responda (sin bloquear el juego)
            yield return webRequest.SendWebRequest();

            // 3. Verificamos el resultado de la conexión
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("[WEB] Error de conexión: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("[WEB] Error de protocolo (HTTP): " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    // 4. Si la conexión es exitosa, procesamos el JSON recibido
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log("<color=green>[WEB] ¡Conexión HTTP Exitosa!</color>");
                    Debug.Log("<color=white>[WEB] Respuesta JSON: " + jsonResponse + "</color>");
                    
                    // Ejemplo de parseo del JSON para mostrar datos específicos
                    ServerData data = JsonUtility.FromJson<ServerData>(jsonResponse);
                    Debug.Log($"<color=yellow>[SERVER INFO] Online: {data.online} | Jugadores: {data.playerCount} | Bombas: {data.bombCount}</color>");
                    break;
            }
        }
    }

    /// <summary>
    /// Método público para forzar una comprobación manual (útil para botones de UI).
    /// </summary>
    public void ManualCheck()
    {
        StartCoroutine(GetServerStatus());
    }
}

/// <summary>
/// Estructura de datos para mapear la respuesta JSON del servidor Node.js.
/// </summary>
[System.Serializable]
public class ServerData
{
    public bool online;
    public int playerCount;
    public int bombCount;
    public float uptime;
}
