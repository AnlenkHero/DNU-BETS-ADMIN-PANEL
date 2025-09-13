using Libs.Repositories;
using UnityEngine;


public class MatchManager : MonoBehaviour
{
   [SerializeField] private MatchButton matchPrefab;
   [SerializeField] private Transform matchPrefabParent;

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
