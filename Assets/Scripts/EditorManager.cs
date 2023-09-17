using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform contestantListParent;
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TMP_InputField matchTitle;
    [SerializeField] private Toggle bettingAvailableToggle;
    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ContestantListManager contestantListManager;

    #endregion

    #region Private Fields

    private bool imageUpdated;
    private static readonly CultureInfo DefaultDateCulture = CultureInfo.InvariantCulture;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        FileManager.OnImageSelected += () => imageUpdated = true;
    }

    private void Awake()
    {
        InitializeData();
        InitializeListeners();
        CheckDeleteButtonConditions();
    }

    #endregion

    #region Initializers

    private void InitializeData()
    {
        if (MatchesCache.selectedMatchID != null)
            SetData(MatchesCache.selectedMatchID);
    }

    private void InitializeListeners()
    {
        backButton.onClick.AddListener(BackToMatchChooseScene);
        saveButton.onClick.AddListener(SaveMatch);
        deleteButton.onClick.AddListener(DeleteMatch);
    }

    #endregion

    #region Helper Methods

    private void SetData(string id)
    {
        var match = MatchesCache.matches.First(match => match.Id == id);
        contestantListManager.SetData(match);
        matchTitle.text = match.MatchTitle;
        bettingAvailableToggle.isOn = match.IsBettingAvailable;
        StartCoroutine(LoadImage(match.ImageUrl));
    }

    private string GetImageUrl(string id)
    {
        var match = MatchesCache.matches.First(match => match.Id == id);
        return match.ImageUrl;
    }

    private void CheckDeleteButtonConditions()
    {
        if (MatchesCache.selectedMatchID == null)
            deleteButton.interactable = false;
    }

    private List<ContestantRequest> GetContestants(IEnumerable<ContestantFormView> views)
    {
        return views.Select(form => new ContestantRequest
        {
            Name = form.Name,
            Coefficient = form.Coefficient,
            Winner = form.IsWinner,
        }).ToList();
    }

    private Match GetMatchModel(string newMatchId, MatchRequest createdMatchToCreate)
    {
        return new Match
        {
            Id = newMatchId,
            ImageUrl = createdMatchToCreate.ImageUrl,
            MatchTitle = createdMatchToCreate.MatchTitle,
            IsBettingAvailable = createdMatchToCreate.IsBettingAvailable,
            FinishedDateUtc = createdMatchToCreate.FinishedDateUtc,
            Contestants = createdMatchToCreate.Contestants.Select((contestant, index) => new Contestant
            {
                Id = index.ToString(),
                Name = contestant.Name,
                Coefficient = contestant.Coefficient,
                Winner = contestant.Winner
            }).ToList()
        };
    }

    #endregion

    #region Coroutine Methods

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"Failed to get image from file browser {uwr.error}");
            }
            else
            {
                var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                matchImage.texture = uwrTexture;
            }
        }
    }

    #endregion

    #region UI Event Handlers

    private void SaveMatch()
    {
        saveButton.interactable = false;
        backButton.interactable = false;

        ContestantFormView[] contestantViews = contestantListParent.GetComponentsInChildren<ContestantFormView>();
        var matchToCreate = new MatchRequest()
        {
            IsBettingAvailable = bettingAvailableToggle.isOn,
            MatchTitle = matchTitle.text,
            Contestants = GetContestants(contestantViews)
        };

        if (matchToCreate.Contestants.Any(x => x.Winner))
        {
            matchToCreate.FinishedDateUtc = DateTime.UtcNow.ToString(DefaultDateCulture);
        }

        if (MatchesCache.selectedMatchID == null)
        {
            MatchesRepository.Save(matchToCreate, matchImage.texture as Texture2D).Then(newMatchId =>
                {
                    MatchesCache.selectedMatchID = newMatchId;

                    infoPanel.ShowPanel(Color.green, "Match saved successfully!", "Edit", $"Match ID: {newMatchId}");

                    MatchesCache.matches.Add(GetMatchModel(newMatchId, matchToCreate));
                }).Catch(error =>
                {
                    infoPanel.ShowPanel(Color.red, "Match was not created!", "Try again", error.Message);
                })
                .Finally(() =>
                {
                    backButton.interactable = true;
                    saveButton.interactable = true;
                    deleteButton.interactable = true;
                });
        }
        else
        {
            MatchesRepository.UpdateMatch(MatchesCache.selectedMatchID, matchToCreate,
                imageUpdated ? matchImage.texture as Texture2D : null, GetImageUrl(MatchesCache.selectedMatchID)).Then(
                _ =>
                {
                    var matchModel = GetMatchModel(MatchesCache.selectedMatchID, matchToCreate);

                    infoPanel.ShowPanel(Color.green, "Match was edited successfully!", "Edit again",
                        $"Edited match ID: {MatchesCache.selectedMatchID}");

                    MatchesCache.matches.Remove(MatchesCache.matches.First(x => x.Id == MatchesCache.selectedMatchID));
                    MatchesCache.matches.Add(matchModel);
                    //TODO LOW LVL OPTIMIZE!!!
                }).Catch(error =>
            {
                infoPanel.ShowPanel(Color.red, "Match was not edited!", "Try again", error.Message);
            }).Finally(() =>
            {
                saveButton.interactable = true;
                backButton.interactable = true;
            });
        }
    }

    private void DeleteMatch()
    {
        backButton.interactable = false;
        MatchesRepository.DeleteMatch(MatchesCache.selectedMatchID).Then(_ =>
        {
            var match = MatchesCache.matches.First(match => match.Id == MatchesCache.selectedMatchID);
            MatchesRepository.DeleteImage(match.ImageUrl);
            infoPanel.ShowPanel(Color.green, "Match deleted successfully!", "Back to match chooser",
                $"Deleted match ID: {MatchesCache.selectedMatchID}", BackToMatchChooseScene);
            MatchesCache.selectedMatchID = null;
        }).Catch(error =>
            infoPanel.ShowPanel(Color.red, "Error!!Match was not deleted!",
                "Try again", error.Message))
            .Finally(() => backButton.interactable = true);
    }

    private void BackToMatchChooseScene()
    {
        SceneManager.LoadScene("MatchChooseScene");
        MatchesCache.selectedMatchID = null;
    }

    #endregion
}