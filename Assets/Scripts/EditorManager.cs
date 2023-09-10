using System;
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
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform contestantListParent;
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TMP_InputField matchTitle;
    [SerializeField] private Toggle bettingAvailableToggle;
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private TextMeshProUGUI failText;//low lvl lazy TODO REFACTOR POP UP.
    private bool imageUpdated;
    
    private static readonly CultureInfo DefaultDateCulture = CultureInfo.InvariantCulture;

    private void OnEnable()
    {
        FileManager.OnImageSelected += () => imageUpdated = true;
    }

    private void Awake()
    {
        CheckDeleteButtonConditions();
        saveButton.onClick.AddListener(SaveMatch);
        deleteButton.onClick.AddListener(DeleteMatch);
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

                successPanel.SetActive(true);

                MatchesCache.matches.Add(GetMatchModel(newMatchId, matchToCreate));
                
            }).Catch(error =>
            {
                failText.text = error.Message;
                failPanel.SetActive(true);
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

                successPanel.SetActive(true);

                MatchesCache.matches.Remove(MatchesCache.matches.First(x => x.Id == MatchesCache.selectedMatchID));
                MatchesCache.matches.Add(matchModel);
                //TODO LOW LVL OPTIMIZE!!!
            }).Catch(error =>
            {
                failText.text = error.Message;
                failPanel.SetActive(true);
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
        MatchesRepository.DeleteMatch(MatchesCache.selectedMatchID);////TODO MOVE TO OTHER SCENE .Then(MatchesCache.selectedMatchID = null;).Catch();
    }

    private void CheckDeleteButtonConditions()
    {
        if (MatchesCache.selectedMatchID == null)
            deleteButton.interactable = false;
    }
}