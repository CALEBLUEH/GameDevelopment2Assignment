using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleSheetsSender : MonoBehaviour
{
    public string apiURL = "https://script.google.com/macros/s/AKfycbw2L1kRn8jDso2ousEgJT_FCjEA9gHEfiujtpKVWDpw0InM7ZwQUc0dNfPN4rZfkQc1HQ/exec";

    public void SendScore(string playerName, int score)
    {
        StartCoroutine(Post(playerName, score));
    }

    IEnumerator Post(string playerName, int score)
    {
        PlayerData data = new PlayerData(playerName, score);
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request =
            new UnityWebRequest(apiURL, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Sent to Google Sheet");
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int score;

        public PlayerData(string name, int s)
        {
            playerName = name;
            score = s;
        }
    }
}