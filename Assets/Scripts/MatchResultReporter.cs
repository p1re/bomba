using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

// Clase auxiliar para ignorar errores de certificado SSL en desarrollo
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Retornamos siempre true para aceptar cualquier certificado (solo para desarrollo)
        return true;
    }
}

public class MatchResultReporter : MonoBehaviour
{
    public string apiEndpoint = "https://your-api.com/results";

    public void ReportResult(string winner, float p1Area, float p2Area)
    {
        StartCoroutine(PostResult(winner, p1Area, p2Area));
    }

    private IEnumerator PostResult(string winner, float p1Area, float p2Area)
    {
        WWWForm form = new WWWForm();
        form.AddField("winner", winner);
        form.AddField("p1Area", p1Area.ToString());
        form.AddField("p2Area", p2Area.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(apiEndpoint, form))
        {
            // Añadimos el bypass para evitar errores de SSL en desarrollo
            www.certificateHandler = new BypassCertificate();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error reporting results: " + www.error);
            }
            else
            {
                Debug.Log("Result reported successfully!");
            }
        }
    }
}
