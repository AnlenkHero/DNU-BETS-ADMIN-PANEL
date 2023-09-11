using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform contestantListParent;
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TMP_InputField matchTitle;
    [SerializeField] private Toggle bettingAvailableToggle;
    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ContestantListManager contestantListManager;
    private bool imageUpdated;
    
    private static readonly CultureInfo DefaultDateCulture = CultureInfo.InvariantCulture;

    private void OnEnable()
    {
        FileManager.OnImageSelected += () => imageUpdated = true;
    }

    private void Awake()
    {
        if(MatchesCache.selectedMatchID!=null)
            SetData(MatchesCache.selectedMatchID);
        CheckDeleteButtonConditions();
        backButton.onClick.AddListener(BackToMatchChooseScene);
        saveButton.onClick.AddListener(SaveMatch);
        deleteButton.onClick.AddListener(DeleteMatch);
    }

    private void SetData(string id)
    {
        MatchesRepository.GetMatchById(id).Then(match =>
        {
            contestantListManager.SetData(match);
            matchTitle.text = match.MatchTitle;
            bettingAvailableToggle.isOn = match.IsBettingAvailable;
            StartCoroutine(LoadImage(match.ImageUrl));
        } );
    }
    private void SaveMatch()
    {
        saveButton.interactable = false;
        
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

                infoPanel.ShowPanel(Color.green, "Match saved successfully!","Edit",$"Match ID: {newMatchId}");

                MatchesCache.matches.Add(GetMatchModel(newMatchId, matchToCreate));
                
            }).Catch(error =>
            {
                infoPanel.ShowPanel(Color.red, "Match was not created!","Try again",error.Message);
            }).Finally(() =>
            {
                saveButton.interactable = true;
                deleteButton.interactable = true;
            });
        }
        else
        {
            MatchesRepository.UpdateMatch(MatchesCache.selectedMatchID,matchToCreate,imageUpdated? matchImage.texture as Texture2D : null ).Then(newMatchId =>
            {
                var matchModel = GetMatchModel(MatchesCache.selectedMatchID, matchToCreate);

                infoPanel.ShowPanel(Color.green, "Match was edited successfully!","Edit again",$"Edited match ID: {MatchesCache.selectedMatchID}");

                MatchesCache.matches.Remove(MatchesCache.matches.First(x => x.Id == MatchesCache.selectedMatchID));
                MatchesCache.matches.Add(matchModel);
                //TODO LOW LVL OPTIMIZE!!!
            }).Catch(error =>
            {
                infoPanel.ShowPanel(Color.red, "Match was not edited!","Try again",error.Message);
            }).Finally(() => saveButton.interactable = true);
        }
    }

    private Match GetMatchModel(string newMatchId, MatchRequest createdMatchToCreate)
    {
        Match newMatch = new Match
        {
            Id = newMatchId,
            ImageUrl = createdMatchToCreate.ImageUrl,
            MatchTitle = createdMatchToCreate.MatchTitle,
            IsBettingAvailable = createdMatchToCreate.IsBettingAvailable,
            FinishedDateUtc = createdMatchToCreate.FinishedDateUtc,
            Contestants = new List<Contestant>()
        };

        for (int i = 0; i < createdMatchToCreate.Contestants.Count; i++)
        {
            Contestant newContestant = new Contestant
            {
                Id = i.ToString(),
                Name = createdMatchToCreate.Contestants[i].Name,
                Coefficient = createdMatchToCreate.Contestants[i].Coefficient,
                Winner = createdMatchToCreate.Contestants[i].Winner
            };
            newMatch.Contestants.Add(newContestant);
        }

        return newMatch;
    }

    private List<ContestantRequest> GetContestants(IEnumerable<ContestantFormView> views)
    {
        var contestants = new List<ContestantRequest>();
        
        foreach (var form in views)
        {
            ContestantRequest contestant = new ContestantRequest
            {
                Name = form.Name,
                Coefficient = form.Coefficient,
                Winner = form.IsWinner,
            };
            contestants.Add(contestant);
        }
        
        return contestants;
    }

    private void DeleteMatch()
    {
        MatchesRepository.DeleteMatch(MatchesCache.selectedMatchID).Then(_ =>
        {
            infoPanel.ShowPanel(Color.green, "Match deleted successfully!", "Back to match chooser",
                $"Deleted match ID: {MatchesCache.selectedMatchID}",BackToMatchChooseScene);
            MatchesCache.selectedMatchID = null;
        }).Catch(error =>
            infoPanel.ShowPanel(Color.red, "Error!!Match was not deleted!",
                "Try again", error.Message)); ////TODO MOVE TO OTHER SCENE .Then(MatchesCache.selectedMatchID = null;).Catch();
    }

    private void CheckDeleteButtonConditions()
    {
        if (MatchesCache.selectedMatchID == null)
            deleteButton.interactable = false;
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
                matchImage.texture = uwrTexture;
            }
        }
    }

    private void BackToMatchChooseScene()
    {
        SceneManager.LoadScene("MatchChooseScene");
        MatchesCache.selectedMatchID = null;
    }
}