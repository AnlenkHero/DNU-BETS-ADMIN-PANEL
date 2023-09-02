using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchListManager : MonoBehaviour
{
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private TextMeshProUGUI contenderCounter;
    [SerializeField] private GameObject contenderEditorPrefab;
    [SerializeField] private Transform contenderEditorPrefabParent;
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
            var tempContenderPrefab = Instantiate(contenderEditorPrefab,contenderEditorPrefabParent);
            _contenderList.Add(tempContenderPrefab);
        }
        DisableEnableButton(decreaseButton,false);
        UpdateContenderCounter();
    }

    private void Decrease()
    {
        if (_contenderList.Count <= 2) 
        {
            DisableEnableButton(decreaseButton,false);
            return;
        }
        var tempContenderPrefab = _contenderList[^1];
        _contenderList.Remove(tempContenderPrefab);
        Destroy(tempContenderPrefab);
        DisableEnableButton(increaseButton,true);
        UpdateContenderCounter();
    }

    private void Increase()
    {     
        if (_contenderList.Count >= 6) 
        { 
            DisableEnableButton(increaseButton,false);
            return;
        }
        var tempContenderPrefab = Instantiate(contenderEditorPrefab,contenderEditorPrefabParent);
        _contenderList.Add(tempContenderPrefab);
        DisableEnableButton(decreaseButton,true);
        UpdateContenderCounter();
    }
    private void UpdateContenderCounter()
    {
        contenderCounter.text = _contenderList.Count.ToString();
    }

    private void DisableEnableButton(Button button, bool state)
    {
        button.interactable = state;
    }
    
}
