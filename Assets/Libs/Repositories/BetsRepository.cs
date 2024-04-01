using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Config;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using Libs.Models;
using Libs.Models.RequestModels;

namespace Libs.Repositories
{
    public static class BetsRepository
    {
        private const string FirebaseDbUrl = "https://wwe-bets-default-rtdb.europe-west1.firebasedatabase.app/";

        private static readonly ApiSettings APISettings = ConfigManager.Settings.ApiSettings;
        
        public static IPromise<string> SaveBet(BetRequest betRequest)
        {
            var promise = new Promise<string>();

            string validationMessage = ValidateBet(betRequest);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            RestClient.Post($"{FirebaseDbUrl}bets.json", betRequest).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newBetId))
                {
                    promise.Resolve(newBetId);
                }
                else
                {
                    promise.Reject(new Exception("Bet id is not returned"));
                }
            }).Catch(error => { promise.Reject(error); });

            return promise;
        }

        public static IPromise<ResponseHelper> UpdateBet(string betId, BetRequest betToUpdate)
        {
            string url = $"{APISettings.Url}/bets/{betId}";
            
            var promise = new Promise<ResponseHelper>();
            
            string validationMessage = ValidateBet(betToUpdate);

            if (validationMessage != null)
            {
                promise.Reject(new Exception(validationMessage));
                return promise;
            }

            return RestClient.Put(url, betToUpdate);
        }

        public static IPromise<ResponseHelper> DeleteBet(string betId)
        {
            if (betId == null)
            {
                var promise = new Promise<ResponseHelper>();
                promise.Reject(new Exception("Id is null"));
                return promise;
            }

            string url = $"{FirebaseDbUrl}bets/{betId}.json";
            return RestClient.Delete(url);
        }

        public static Promise<Bet> GetBetById(string betId)
        {
            return new Promise<Bet>((resolve, reject) =>
            {
                RestClient.Get($"{FirebaseDbUrl}bets/{betId}.json").Then(response =>
                {
                    Bet bet = JsonConvert.DeserializeObject<Bet>(response.Text);

                    if (bet == null)
                    {
                        reject(new Exception("Bet not found"));
                        return;
                    }

                    resolve(bet);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving bet by ID: {error.Message}"));
                });
            });
        }

        public static Promise<List<Bet>> GetAllBetsByUserId(string userId)
        {
            throw new NotImplementedException();
        }
        
        public static Promise<List<Bet>> GetAllBetsByMatchId(int matchId)
        {
            return new Promise<List<Bet>>((resolve, reject) =>
            {
                string url = $"{APISettings.Url}/api/bets?matchId={matchId}";

                RestClient.Get(url).Then(response =>
                {
                    var bets = JsonConvert.DeserializeObject<List<Bet>>(response.Text);

                    resolve(bets);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving bets by match ID: {error.Message}"));
                });
            });
        }

        private static string ValidateBet(BetRequest bet)
        {
            if (bet.MatchId <= 0)
                return "Match ID cannot be empty.";
            if (bet.ContestantId <= 0)
                return "Contestant ID cannot be empty.";
            if (bet.BetAmount <= 0)
                return "Bet amount should be greater than 0.";
            if (string.IsNullOrEmpty(bet.UserId))
                return "User ID cannot be empty.";

            return null;
        }
    }
}
