using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Libs.Models;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
   private List<Match> matchList = new ();
   [SerializeField] private MatchButton matchPrefab;
   [SerializeField] private Transform matchPrefabParent;
   [SerializeField] private GameObject emptyMatchPrefab;

   private void Start()
   {
      MatchesRepository.GetNotFinishedMatches().Then(matches =>
         {
            matchList = matches;
            MatchesCache.matches = matchList;
            foreach (var match in matchList)
            {
               var tempMatchPrefab = Instantiate(matchPrefab, matchPrefabParent);
               tempMatchPrefab.SetInfo(match.MatchTitle, match.ImageUrl,match.Id);
            }
         })
         .Catch(error =>
         {
            Debug.LogError($"Error getting matches: {error.Message}");
         });
      /* for (int i = 0; i < ; i++)
       {
         var tempMatchPrefab = Instantiate(matchPrefab, matchPrefabParent);
          _matchList.Add(tempMatchPrefab);
       }
 
       var tempEmptyMatch = Instantiate(emptyMatchPrefab, matchPrefabParent);
       _matchList.Add(tempEmptyMatch);*/
   }
}
