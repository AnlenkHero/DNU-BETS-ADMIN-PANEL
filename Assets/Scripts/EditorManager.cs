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
    private static readonly CultureInfo DefaultDateCulture = CultureInfo.InvariantCulture;
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
        if (MatchesCache.selectedMatchID != null)
            SetData(MatchesCache.selectedMatchID);
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
            Contestants = GetContestants(contestantViews)
        };

        if (matchToCreate.Contestants.Any(x => x.Winner) || matchToCreate.IsMatchCanceled)
        {
            matchToCreate.FinishedDateUtc = DateTime.UtcNow.ToString(DefaultDateCulture);
        }

        return matchToCreate;
    }

    private void SetData(string id)
    {
        var match = MatchesCache.matches.First(match => match.Id == id);
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
            IsMatchCanceled = createdMatchToCreate.IsMatchCanceled,
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

    #region UI Event Handlers

    private void SaveMatch()
    {
        saveButton.interactable = false;
        backButton.interactable = false;

        var matchToCreate = PrepareMatchRequest();

        if (MatchesCache.selectedMatchID == null)
        {
            CreateNewMatch(matchToCreate);
        }
        else
        {
            UpdateExistingMatch(matchToCreate);
        }
    }

    private void CreateNewMatch(MatchRequest matchToCreate)
    {
        MatchesRepository.Save(matchToCreate, matchImage.texture as Texture2D).Then(newMatchId =>
        {
            HandleMatchSavedSuccessfully(newMatchId, matchToCreate);
        }).Catch(HandleMatchCreationFailure).Finally(HandleMatchCreationCompletion);
    }

    private void UpdateExistingMatch(MatchRequest matchToCreate)
    {
        MatchesRepository.UpdateMatch(MatchesCache.selectedMatchID, matchToCreate,
                imageUpdated ? matchImage.texture as Texture2D : null, GetImageUrl(MatchesCache.selectedMatchID)).Then(
                _ => { HandleMatchUpdatedSuccessfully(matchToCreate); })
            .Catch(HandleMatchUpdateFailure).Finally(HandleMatchUpdateCompletion);
    }

    private void HandleMatchSavedSuccessfully(string newMatchId, MatchRequest createdMatchToCreate)
    {
        MatchesCache.selectedMatchID = newMatchId;
        infoPanel.ShowPanel(ColorHelper.LightGreen, "Match saved successfully!", $"Match ID: {newMatchId}");

        MatchesCache.matches.Add(GetMatchModel(newMatchId, createdMatchToCreate));
    }

    private void HandleMatchCreationFailure(Exception error)
    {
        infoPanel.ShowPanel(ColorHelper.HotPink, "Match was not created!", error.Message);
    }

    private void HandleMatchCreationCompletion()
    {
        backButton.interactable = true;
        saveButton.interactable = true;
        if (MatchesCache.selectedMatchID != null)
            deleteButton.interactable = true;
    }

    private void CheckMatchForBets()
    {
        StartCoroutine(ClearExistingBets());
        emptyBetsGameObject.SetActive(false);

        BetsRepository.GetAllBetsByMatchId(MatchesCache.selectedMatchID).Then(bets =>
        {
            var matchModel = MatchesCache.matches.First(match => match.Id == MatchesCache.selectedMatchID);
            foreach (var contestant in matchModel.Contestants)
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


    private void HandleMatchUpdatedSuccessfully(MatchRequest matchToCreate)
    {
        var matchModel = GetMatchModel(MatchesCache.selectedMatchID, matchToCreate);

        BetsRepository.GetAllBetsByMatchId(MatchesCache.selectedMatchID).Then(
            bets =>
            {
                if (matchModel.Contestants.Any(x => x.Winner) && bets != null)
                {
                    var contestant = matchModel.Contestants.First(x => x.Winner);
                    foreach (var bet in bets.Where(bet => bet.ContestantId == contestant.Id && bet.IsActive))
                    {
                        double winnings = bet.BetAmount * contestant.Coefficient;
                        UserRepository.GetUserByUserId(bet.UserId).Then(user =>
                        {
                            user.balance += winnings;
                            UserRepository.UpdateUserInfo(user).Catch(exception =>
                                Debug.Log($"Failed to update user balance {exception.Message}"));
                        }).Catch(exception => Debug.Log($"Failed to get user by id {exception.Message}"));
                    }

                    foreach (var bet in bets.Where(bet => bet.IsActive))
                    {
                        bet.IsActive = false;
                        BetRequest newBetRequest = new BetRequest
                        {
                            BetAmount = bet.BetAmount, ContestantId = bet.ContestantId, MatchId = bet.MatchId,
                            UserId = bet.UserId, IsActive = bet.IsActive
                        };
                        BetsRepository.UpdateBet(bet.BetId, newBetRequest);
                    }
                }
                else if (matchModel.IsMatchCanceled && bets != null)
                {
                    foreach (var bet in bets.Where((bet => bet.IsActive)))
                    {
                        bet.IsActive = false;
                        BetRequest newBetRequest = new BetRequest
                        {
                            BetAmount = bet.BetAmount, ContestantId = bet.ContestantId, MatchId = bet.MatchId,
                            UserId = bet.UserId, IsActive = bet.IsActive
                        };
                        BetsRepository.UpdateBet(bet.BetId, newBetRequest);

                        UserRepository.GetUserByUserId(bet.UserId).Then(user =>
                        {
                            user.balance += bet.BetAmount;
                            UserRepository.UpdateUserInfo(user).Catch(exception =>
                                Debug.Log($"Failed to update user balance {exception.Message}"));
                        }).Catch(exception => Debug.Log($"Failed to get user by id {exception.Message}"));
                    }
                }
            }).Catch(exception => { Debug.Log(exception.Message); });

        infoPanel.ShowPanel(ColorHelper.LightGreen, "Match was edited successfully!",
            $"Edited match ID: {MatchesCache.selectedMatchID}");

        MatchesCache.matches.Remove(MatchesCache.matches.First(x => x.Id == MatchesCache.selectedMatchID));
        MatchesCache.matches.Add(matchModel);
        //TODO LOW LVL OPTIMIZE!!!
    }


    private void HandleMatchUpdateFailure(Exception error)
    {
        infoPanel.ShowPanel(ColorHelper.HotPink, "Match was not edited!", error.Message);
    }

    private void HandleMatchUpdateCompletion()
    {
        saveButton.interactable = true;
        backButton.interactable = true;
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
        backButton.interactable = false;
        BetsRepository.GetAllBetsByMatchId(MatchesCache.selectedMatchID).Then(bets =>
        {
            if (bets == null) return;

            foreach (var bet in bets.Where((bet => bet.IsActive)))
            {
                bet.IsActive = false;
                BetRequest newBetRequest = new BetRequest
                {
                    BetAmount = bet.BetAmount, ContestantId = bet.ContestantId, MatchId = bet.MatchId,
                    UserId = bet.UserId, IsActive = bet.IsActive
                };
                BetsRepository.UpdateBet(bet.BetId, newBetRequest).Then(helper => BetsRepository.DeleteBet(bet.BetId));

                UserRepository.GetUserByUserId(bet.UserId).Then(user =>
                {
                    user.balance += bet.BetAmount;
                    UserRepository.UpdateUserInfo(user).Catch(exception =>
                        Debug.Log($"Failed to update user balance {exception.Message}"));
                }).Catch(exception => Debug.Log($"Failed to get user by id {exception.Message}"));
            }
        }).Catch(exception => { Debug.Log(exception.Message); }).Finally(() =>
        {
            MatchesRepository.DeleteMatch(MatchesCache.selectedMatchID).Then(_ => { HandleMatchDeletedSuccessfully(); })
                .Catch(HandleMatchDeletionFailure).Finally(() => backButton.interactable = true);
        });
    }

    private void HandleMatchDeletedSuccessfully()
    {
        var match = MatchesCache.matches.First(match => match.Id == MatchesCache.selectedMatchID);
        MatchesRepository.DeleteImage(match.ImageUrl);
        infoPanel.ShowPanel(ColorHelper.LightGreen, "Match deleted successfully!",
            $"Deleted match ID: {MatchesCache.selectedMatchID}",
            () => infoPanel.AddButton("Back to match choose", BackToMatchChooseScene));
        MatchesCache.selectedMatchID = null;
    }

    private void HandleMatchDeletionFailure(Exception error)
    {
        infoPanel.ShowPanel(ColorHelper.HotPink, "Error!!Match was not deleted!", error.Message);
    }

    private void BackToMatchChooseScene()
    {
        SceneManager.LoadScene(MatchChooseSceneName);
        MatchesCache.selectedMatchID = null;
    }

    #endregion
}