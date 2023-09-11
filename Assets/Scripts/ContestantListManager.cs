using System.Collections.Generic;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ContestantListManager : MonoBehaviour
{
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private TextMeshProUGUI contenderCounter;
    [FormerlySerializedAs("contenderEditorPrefab")]
    [SerializeField] private ContestantFormView contestantEditorPrefab;
    [FormerlySerializedAs("contenderEditorPrefabParent")]
    [SerializeField] private Transform contestantEditorPrefabParent;
    
    private List<ContestantFormView> _contenderList = new ();

    private void Awake()
    {
        decreaseButton.onClick.AddListener(Decrease);
        increaseButton.onClick.AddListener(Increase);
    }

    private void Start()
    {
        if (MatchesCache.selectedMatchID == null)
        {
            for (int i = 0; i < 2; i++)
            {
                var tempContenderPrefab = Instantiate(contestantEditorPrefab, contestantEditorPrefabParent);
                _contenderList.Add(tempContenderPrefab);
            }

            decreaseButton.interactable = false;
            contenderCounter.text = _contenderList.Count.ToString();
        }
    }

    public void SetData(Match match)
    {
        foreach (var contestant in match.Contestants)
        {
            var tempContenderPrefab = Instantiate(contestantEditorPrefab, contestantEditorPrefabParent);
            _contenderList.Add(tempContenderPrefab);
            tempContenderPrefab.SetData(contestant);
        }
        contenderCounter.text = _contenderList.Count.ToString();
        if(_contenderList.Count <= 2)
            decreaseButton.interactable = false;
        else if (_contenderList.Count >= 6)
            increaseButton.interactable = false;
    }

    private void Decrease()
    {
        if (_contenderList.Count <= 2) 
        {
            decreaseButton.interactable = false;
            return;
        }
        var tempContenderPrefab = _contenderList[^1];
        _contenderList.Remove(tempContenderPrefab);
        Destroy(tempContenderPrefab.gameObject);
        increaseButton.interactable = true;
        contenderCounter.text = _contenderList.Count.ToString();
    }

    private void Increase()
    {     
        if (_contenderList.Count >= 6) 
        { 
            increaseButton.interactable = false;
            return;
        }
        var tempContenderPrefab = Instantiate(contestantEditorPrefab,contestantEditorPrefabParent);
        _contenderList.Add(tempContenderPrefab);
        decreaseButton.interactable = true;
        contenderCounter.text = _contenderList.Count.ToString();
    }
}