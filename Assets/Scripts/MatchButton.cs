using System;
using Libs.Helpers;
using TMPro;
using UnityEngine;
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
        TextureLoader.LoadTexture(this,imageUrl,texture2D =>
        {
            if (texture2D != null)
            {
                image.texture = texture2D; 
            }
            else
            {
                Debug.LogError("Texture failed to load.");
            }
        });
    }
    private void MoveToEdit()
    {
        if(String.IsNullOrEmpty(id)!=true)
            MatchesCache.selectedMatchID = id;
        SceneManager.LoadScene("EditScene");
    }
    
}
