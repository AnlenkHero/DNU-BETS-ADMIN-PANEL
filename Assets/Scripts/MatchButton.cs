using Libs.Helpers;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchButton : MonoBehaviour
{
    private const string EditSceneName = "EditScene";
    
    [SerializeField] private int id;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button button;
    [SerializeField] private RawImage image;
    
    private void Awake()
    {
        button.onClick.AddListener(MoveToEdit);
    }

    public void SetInfo(string text, string imageUrl, int matchId)
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
        if (id > 0)
        {
            MatchesRepository.GetMatchById(id).Then(match =>
            {
                MatchesCache.SelectedMatch = match; 
                SceneManager.LoadScene(EditSceneName);
            });
            
            return;
        }
        
        SceneManager.LoadScene(EditSceneName);
    }
    
}
