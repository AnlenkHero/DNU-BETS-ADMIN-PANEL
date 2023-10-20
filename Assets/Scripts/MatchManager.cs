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
   [SerializeField] private MatchButton matchPrefab;
   [SerializeField] private Transform matchPrefabParent;
   [SerializeField] private GameObject emptyMatchPrefab;

   private void Start()
   {
      MatchesRepository.GetNotFinishedMatches().Then(matches =>
         {
            
            MatchesCache.matches = matches;
            foreach (var match in matches)
            {
               var tempMatchPrefab = Instantiate(matchPrefab, matchPrefabParent);
               tempMatchPrefab.SetInfo(match.MatchTitle, match.ImageUrl,match.Id);
            }
         })
         .Catch(error =>
         {
            Debug.LogError($"Error getting matches: {error.Message}");
         });
   }
}
