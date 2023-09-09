using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Libs.Models;
using Libs.Models.RequestModels;
using Libs.Repositories;
using Newtonsoft.Json;
using TMPro;
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
    
    private const string FirebaseURL = "https://wwe-bets-default-rtdb.europe-west1.firebasedatabase.app/";
    private static readonly CultureInfo DefaultDateCulture = CultureInfo.InvariantCulture;
    
    private void Awake()
    {
        saveButton.onClick.AddListener(SaveMatch);
        deleteButton.onClick.AddListener(DeleteMatch);
    }
        
    private void SaveMatch()
    {
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
        
        MatchesRepository.Save(matchToCreate, matchImage.texture as Texture2D).Then(response =>
        {
            var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

            if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newMatchId))
            {
                Debug.Log($"Match saved successfully with ID: {newMatchId}");

                Match newMatch = new Match
                {
                    Id = newMatchId,
                    ImageUrl = matchToCreate.ImageUrl,
                    MatchTitle = matchToCreate.MatchTitle,
                    IsBettingAvailable = matchToCreate.IsBettingAvailable,
                    FinishedDateUtc = matchToCreate.FinishedDateUtc, 
                    Contestants = new List<Contestant>()
                };

                for (int i = 0; i < matchToCreate.Contestants.Count; i++)
                {
                    Contestant newContestant = new Contestant
                    {
                        Id = i.ToString(),
                        Name = matchToCreate.Contestants[i].Name,
                        Coefficient = matchToCreate.Contestants[i].Coefficient,
                        Winner = matchToCreate.Contestants[i].Winner
                    };
                    newMatch.Contestants.Add(newContestant);
                }

                MatchesCache.Matches.Add(newMatch);
            }
            else
            {
                Debug.LogWarning("Could not retrieve the Firebase ID for the newly created match.");
            }

        }).Catch(error =>
        {
            Debug.LogError($"Failed to save match: {error.Message}");
            //TODO show poup
        });
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
        //TODO
    }
}