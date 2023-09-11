using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchButton : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button button;
    [SerializeField] private RawImage image;
    private void Awake()
    {
        button.onClick.AddListener(MoveToEdit);
    }

    public void SetInfo(string text,string imageUrl,string matchId)
    {
        buttonText.text = text;
        id = matchId;
        StartCoroutine(LoadImage(imageUrl));
    }
    private void MoveToEdit()
    {
        if(String.IsNullOrEmpty(id)!=true)
            MatchesCache.selectedMatchID = id;
        SceneManager.LoadScene("EditScene");
    }
    IEnumerator LoadImage(string path)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
        {
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Failed to get image from file browser {uwr.error}");
            }
            else
            {
                var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                image.texture = uwrTexture;
            }
        }
    }
}
