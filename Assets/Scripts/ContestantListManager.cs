using System.Collections.Generic;
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
    [SerializeField] private GameObject contestantEditorPrefab;
    [FormerlySerializedAs("contenderEditorPrefabParent")]
    [SerializeField] private Transform contestantEditorPrefabParent;
    
    private List<GameObject> _contenderList = new ();

    private void Awake()
    {
        decreaseButton.onClick.AddListener(Decrease);
        increaseButton.onClick.AddListener(Increase);
    }

    private void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            var tempContenderPrefab = Instantiate(contestantEditorPrefab,contestantEditorPrefabParent);
            _contenderList.Add(tempContenderPrefab);
        }
        decreaseButton.interactable = false;
        contenderCounter.text = _contenderList.Count.ToString();
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
        Destroy(tempContenderPrefab);
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