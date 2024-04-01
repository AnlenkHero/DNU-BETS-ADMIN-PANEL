using System.Collections.Generic;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContestantListManager : MonoBehaviour
{
    private const int MinContestants = 2;
    private const int MaxContestants = 6;

    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private TextMeshProUGUI contestantCountText;
    [SerializeField] private ContestantFormView contestantEditorPrefab;
    [SerializeField] private Transform contestantEditorParent;
    [SerializeField] private ToggleGroup winnerToggleGroup;
    [SerializeField] private InfoPanel infoPanel;

    private readonly List<ContestantFormView> contestantViews = new();

    private void Awake()
    {
        RegisterButtonEvents();
    }

    private void Start()
    {
        if (MatchesCache.SelectedMatch == null)
        {
            InitializeDefaultContestants();
            UpdateButtonInteractivity();
        }
    }

    public void SetData(Match match)
    {
        foreach (var contestant in match.Contestants)
        {
            AddContestantView(contestant);
        }

        UpdateButtonInteractivity();
    }

    private void RegisterButtonEvents()
    {
        decreaseButton.onClick.AddListener(RemoveLastContestantView);
        increaseButton.onClick.AddListener(AddNewContestantView);
    }

    private void InitializeDefaultContestants()
    {
        for (int i = 0; i < MinContestants; i++)
        {
            AddNewContestantView();
        }
    }

    private void UpdateButtonInteractivity()
    {
        decreaseButton.interactable = contestantViews.Count > MinContestants;
        increaseButton.interactable = contestantViews.Count < MaxContestants;
        contestantCountText.text = contestantViews.Count.ToString();
    }

    private void AddContestantView(Contestant contestant = null)
    {
        var newContestantView = Instantiate(contestantEditorPrefab, contestantEditorParent);
        newContestantView.WinnerToggleGroup = winnerToggleGroup;

        if (contestant != null)
        {
            newContestantView.SetData(contestant);
        }

        newContestantView.AddConfirmation(() => MatchWinnerConfirmation(newContestantView));
        contestantViews.Add(newContestantView);
        UpdateButtonInteractivity();
    }

    private void RemoveLastContestantView()
    {
        if (contestantViews.Count > MinContestants)
        {
            var lastContestantView = contestantViews[^1];
            contestantViews.Remove(lastContestantView);
            Destroy(lastContestantView.gameObject);
            UpdateButtonInteractivity();
        }
    }

    private void AddNewContestantView()
    {
        if (contestantViews.Count < MaxContestants)
        {
            AddContestantView();
        }
    }

    private void MatchWinnerConfirmation(ContestantFormView contestantFormView)
    {
        if (!contestantFormView.IsWinner)
            return;

        infoPanel.ShowPanel(ColorHelper.PaleYellow, "Confirm match winner",
            $"Is <color={ColorHelper.LightGreenString}>{contestantFormView.Name}</color> winner?",
            () =>
            {
                infoPanel.AddButton($"Yes, {contestantFormView.Name} is winner", () =>
                {
                    infoPanel.HidePanel();
                    contestantFormView.IsWinner = true;
                }, ColorHelper.LightGreenString);
                infoPanel.AddButton("No, discard", () =>
                {
                    infoPanel.HidePanel();
                    contestantFormView.IsWinner = false;
                }, ColorHelper.HotPinkString);
            });
    }
}