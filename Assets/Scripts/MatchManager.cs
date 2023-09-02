using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
   private List<GameObject> _matchList = new ();
   [SerializeField] private GameObject matchPrefab;
   [SerializeField] private Transform matchPrefabParent;
   [SerializeField] private GameObject emptyMatchPrefab;

   private void Start()
   {
      for (int i = 0; i < 3; i++)
      {
        var tempMatchPrefab = Instantiate(matchPrefab, matchPrefabParent);
         _matchList.Add(tempMatchPrefab);
      }

      var tempEmptyMatch = Instantiate(emptyMatchPrefab, matchPrefabParent);
      _matchList.Add(tempEmptyMatch);
   }
}
