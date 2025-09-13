using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
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
        var unprocessedBuffs = user.buffPurchase.Count(x => x.isProcessed == false);
        infoPanel = infoPanelObject;
        profileName.text = user.userName;
        buffPurchasedText.text = $"Buff purchased:{user.buffPurchase.Count.ToString()}";
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
        foreach (var buffPurchase in user.buffPurchase.Where(x => x.isProcessed == false))
        {
            buffPurchase.isProcessed = true;
        }

        UserRepository.GetUserByUserId(user.userId).Then((checkUser =>
        {
            if (checkUser.buffPurchase.Count != user.buffPurchase.Count)
            {
                infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!!",
                    $"User has bought more buffs since you have opened this scene.",
                    () => infoPanel.AddButton("Refresh scene",
                        () => SceneManager.LoadScene(SceneManager.GetActiveScene().name)));
            }
            else
            {
                UserRepository.UpdateUserInfo(user).Then(_ =>
                {
                    buffProcessedText.text = $"Buffs unprocessed:0";
                    infoPanel.ShowPanel(ColorHelper.LightGreen, "Success!!!",
                        $"Buffs were processed for {user.userName}");
                }).Catch(exception =>
                {
                    infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!!",
                        $"Buffs were not processed.\n{exception.Message}");
                });
            }
        }));
    }
}