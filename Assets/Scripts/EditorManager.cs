using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button refreshBetsButton;

    [SerializeField] private Transform contestantListParent;
    [SerializeField] private Transform matchBettingInfoParent;

    [SerializeField] private Toggle bettingAvailableToggle;
    [SerializeField] private Toggle matchCanceledToggle;

    [SerializeField] private RawImage matchImage;

    [SerializeField] private TMP_InputField matchTitle;

    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ContestantListManager contestantListManager;
    [SerializeField] private MatchBettingInfo matchBettingInfo;

    [SerializeField] private GameObject emptyBetsGameObject;
    [SerializeField] private MatchBettingInfoTotalBets matchBettingInfoTotalBets;

    #endregion

    #region Private Fields

    private bool imageUpdated;
    private const string MatchChooseSceneName = "MatchChooseScene";

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
        CheckMatchForBets();
        CheckDeleteButtonConditions();
    }

    #endregion

    #region Initializers

    private void InitializeData()
    {
        if (MatchesCache.SelectedMatch != null)
        {
            SetData();
        }
    }

    private void InitializeListeners()
    {
        backButton.onClick.AddListener(BackToMatchChooseScene);
        saveButton.onClick.AddListener(SaveMatch);
        deleteButton.onClick.AddListener(DeleteMatchConfirmation);
        refreshBetsButton.onClick.AddListener(CheckMatchForBets);
        matchCanceledToggle.onValueChanged.AddListener(MatchCancelConfirmation);
    }

    #endregion

    #region Helper Methods

    private IEnumerator ClearExistingBets()
    {
        foreach (Transform child in matchBettingInfoParent)
        {
            if (child.gameObject == emptyBetsGameObject)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        yield return new WaitForEndOfFrame();
    }

    private MatchRequest PrepareMatchRequest()
    {
        ContestantFormView[] contestantViews = contestantListParent.GetComponentsInChildren<ContestantFormView>();
        var matchToCreate = new MatchRequest()
        {
            IsMatchCanceled = matchCanceledToggle.isOn,
            IsBettingAvailable = bettingAvailableToggle.isOn,
            MatchTitle = matchTitle.text,
            Contestants = GetContestants(contestantViews),
            ImageUrl = MatchesCache.SelectedMatch?.ImageUrl
        };

        if (matchToCreate.Contestants.Any(x => x.Winner) || matchToCreate.IsMatchCanceled)
        {
            matchToCreate.FinishedDateUtc = DateTime.UtcNow;
        }

        return matchToCreate;
    }

    private void SetData()
    {
        Match match = MatchesCache.SelectedMatch;
        contestantListManager.SetData(match);
        matchTitle.text = match.MatchTitle;
        bettingAvailableToggle.isOn = match.IsBettingAvailable;
        matchCanceledToggle.isOn = match.IsMatchCanceled;
        TextureLoader.LoadTexture(this, match.ImageUrl, texture2D =>
        {
            if (texture2D != null)
            {
                matchImage.texture = texture2D;
            }
            else
            {
                Debug.LogError("Texture failed to load.");
            }
        });
    }

    private void CheckDeleteButtonConditions()
    {
        if (MatchesCache.SelectedMatch == null)
        {
            deleteButton.interactable = false;
        }
    }

    private List<Contestant> GetContestants(IEnumerable<ContestantFormView> views)
    {
        var contestants = views
            .Select((form, index) => new Contestant
            {
                Id = index,
                Name = form.Name,
                Coefficient = form.Coefficient,
                Winner = form.IsWinner,
            })
            .ToList();

        return contestants;
    }

    #endregion

    #region UI Event Handlers

    private void SaveMatch()
    {
        saveButton.interactable = false;
        backButton.interactable = false;

        MatchRequest matchRequest = PrepareMatchRequest();

        if (MatchesCache.SelectedMatch == null)
        {
            CreateNewMatch(matchRequest);
            return;
        }
        
        UpdateExistingMatch(matchRequest);
    }

    private void CreateNewMatch(MatchRequest matchToCreate)
    {
        MatchesRepository.Create(matchToCreate, matchImage.texture as Texture2D).Then(newMatchId =>
        {
            HandleMatchCreatedSuccessfully(newMatchId, matchToCreate);
        }).Catch(HandleMatchCreationFailure).Finally(HandleMatchCreationCompletion);
    }

    private void UpdateExistingMatch(MatchRequest matchToUpdate)
    {
        Texture2D texture = imageUpdated ? matchImage.texture as Texture2D : null;
        
        MatchesRepository.UpdateMatch(MatchesCache.SelectedMatch.Id, matchToUpdate, texture)
        .Then(_ => {
            infoPanel.ShowPanel(ColorHelper.LightGreen, "Match was edited successfully!",
                $"Edited match ID: {MatchesCache.SelectedMatch.Id}");
        })
        .Catch(x =>
        {
            infoPanel.ShowPanel(ColorHelper.HotPink, "Match was not edited!", x.Message);
        })
        .Finally(() =>
        {
            saveButton.interactable = true;
            backButton.interactable = true;
        });
    }

    private void HandleMatchCreatedSuccessfully(int newMatchId, MatchRequest createdMatch)
    {
        MatchesCache.SelectedMatch = new Match(newMatchId, createdMatch);
        infoPanel.ShowPanel(ColorHelper.LightGreen, "Match created successfully!", $"Match ID: {newMatchId}");

        MatchesCache.matches.Add(new Match(newMatchId, createdMatch));
    }

    private void HandleMatchCreationFailure(Exception error)
    {
        infoPanel.ShowPanel(ColorHelper.HotPink, "Match was not created!", error.Message);
    }

    private void HandleMatchCreationCompletion()
    {
        backButton.interactable = true;
        saveButton.interactable = true;
        
        if (MatchesCache.SelectedMatch != null)
        {
            deleteButton.interactable = true;
        }
    }

    private void CheckMatchForBets()
    {
        StartCoroutine(ClearExistingBets());
        emptyBetsGameObject.SetActive(false);
        
        if (MatchesCache.SelectedMatch != null)
        {
            BetsRepository.GetAllBets(matchId: MatchesCache.SelectedMatch.Id).Then(bets =>
            {
                foreach (var contestant in MatchesCache.SelectedMatch.Contestants)
                {
                    var tempMatchBettingInfo = Instantiate(matchBettingInfo, matchBettingInfoParent);
                    double totalBetAmount =
                        bets.Where((x => x.ContestantId == contestant.Id)).Sum(bet => bet.BetAmount);
                    tempMatchBettingInfo.SetData(contestant.Name, totalBetAmount);
                }

                var totalBets = Instantiate(matchBettingInfoTotalBets, matchBettingInfoParent);
                totalBets.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                totalBets.SetData(bets.Count);
            }).Catch(_ => emptyBetsGameObject.SetActive(true));
        }
    }

    private void MatchCancelConfirmation(bool isOn)
    {
        if (!isOn)
            return;

        infoPanel.ShowPanel(ColorHelper.PaleYellow, "Confirm match cancellation",
            "You want to cancel match?",
            () =>
            {
                infoPanel.AddButton("Cancel match", () =>
                {
                    infoPanel.HidePanel();
                    matchCanceledToggle.isOn = true;
                }, ColorHelper.HotPinkString);
                infoPanel.AddButton("Discard", () =>
                {
                    infoPanel.HidePanel();
                    matchCanceledToggle.isOn = false;
                });
            });
    }

    private void DeleteMatchConfirmation()
    {
        infoPanel.ShowPanel(ColorHelper.PaleYellow, "Confirm deletion",
            "Delete or Cancel?",
            () =>
            {
                infoPanel.AddButton("Delete", DeleteMatch, ColorHelper.HotPinkString);
                infoPanel.AddButton("Cancel", () => infoPanel.HidePanel());
            });
    }

    private void DeleteMatch()
    {
        backButton.interactable = false; //TODO
        
        MatchesRepository.DeleteMatch(MatchesCache.SelectedMatch.Id)
            .Then(_ => { HandleMatchDeletedSuccessfully(); })
            .Catch(HandleMatchDeletionFailure)
            .Finally(() => backButton.interactable = true);
    }

    private void HandleMatchDeletedSuccessfully()
    {
        var match = MatchesCache.SelectedMatch;
        ImageHelper.DeleteImage(match.ImageUrl);
        infoPanel.ShowPanel(ColorHelper.LightGreen, "Match deleted successfully!",
            $"Deleted match ID: {MatchesCache.SelectedMatch.Id}",
            () => infoPanel.AddButton("Back to match choose", BackToMatchChooseScene));
        
        MatchesCache.SelectedMatch = default;
    }

    private void HandleMatchDeletionFailure(Exception error)
    {
        infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!Match was not deleted!", error.Message);
    }

    private void BackToMatchChooseScene()
    {
        MatchesCache.SelectedMatch = null;
        SceneManager.LoadScene(MatchChooseSceneName);
    }

    #endregion
}