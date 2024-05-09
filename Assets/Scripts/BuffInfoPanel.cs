using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuffInfoPanel : MonoBehaviour
{
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI profileName;
    [SerializeField] private TextMeshProUGUI buffPurchasedText;
    [SerializeField] private TextMeshProUGUI buffProcessedText;
    [SerializeField] private Button processButton;
    [SerializeField] private RawImage backgroundImageScroller;
    private InfoPanel infoPanel;

    public void SetData(User user, InfoPanel infoPanelObject)
    {
        var unprocessedBuffs = user.buffPurchases.Count(x => !x.isProcessed);
        infoPanel = infoPanelObject;
        profileName.text = user.userName;
        buffPurchasedText.text = $"Buff purchased:{user.buffPurchases.Count.ToString()}";
        buffProcessedText.text = $"Buff unprocessed:{unprocessedBuffs.ToString()}";
        TextureLoader.LoadTexture(this, user.imageUrl, texture2D =>
        {
            if (texture2D != null)
            {
                profileImage.texture = texture2D;
                backgroundImageScroller.texture = texture2D;
            }
            else
            {
                Debug.Log("Texture failed to load.");
            }
        });
        processButton.onClick.AddListener(() => ProcessBuff(user));
    }

    private void ProcessBuff(User user)
    {
        UserRepository.ProcessAllUserBuffs(user.id, user.buffPurchases.Count).Then(checkUser =>
        {
            buffProcessedText.text = $"Buffs unprocessed:0";
            infoPanel.ShowPanel(ColorHelper.LightGreen, "Success!!!",
                $"Buffs were processed for {user.userName}");
        }).Catch(e =>
        {
            var requestException = e as RequestException;

            if (requestException.StatusCode is StatusCodes.BadRequestStatusCode or StatusCodes.NotFoundStatusCode)
            {
                infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!!",
                    $"{requestException.StatusCode}: {requestException.Response}",
                    () => infoPanel.AddButton("Refresh scene",
                        () => SceneManager.LoadScene(SceneManager.GetActiveScene().name)));
            }
            else
            {
                infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!!",
                    $"Buffs were not processed.\n{requestException.Message}");
            }
        });
    }
}