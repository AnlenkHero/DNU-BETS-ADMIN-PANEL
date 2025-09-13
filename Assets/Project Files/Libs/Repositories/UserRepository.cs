using System;
using System.Collections.Generic;
using System.Linq;
using Libs.Models;
using Libs.Models.RequestModels;
using Newtonsoft.Json;
using Project_Files.Libs;
using Proyecto26;
using RSG;

namespace Libs.Repositories
{
    public static class UserRepository
    {
        
        public static Promise<User> GetUserByUserId(string userId)
        {
            return new Promise<User>((resolve, reject) =>
            {
                string queryUrl = $"{Config.FirebaseDbUrl}users.json?orderBy=\"userId\"&equalTo=\"{userId}\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawUsers =
                        JsonConvert.DeserializeObject<Dictionary<string, UserRequest>>(response.Text);
                    if (rawUsers == null || !rawUsers.Any())
                    {
                        reject(new Exception("User not found for provided UserID"));
                        return;
                    }

                    var firstUserKey = rawUsers.Keys.First();
                    var rawUser = rawUsers[firstUserKey];

                    User user = new User
                    {
                        id = firstUserKey,
                        userId = rawUser.userId,
                        userName = rawUser.userName,
                        balance = rawUser.balance,
                        imageUrl = rawUser.imageUrl,
                        buffPurchase = rawUser.buffPurchase
                    };

                    resolve(user);
                }).Catch(error => { reject(new Exception($"Error retrieving user by UserID: {error.Message}")); });
            });
        }

        public static IPromise<string> SaveUser(UserRequest user)
        {
            var promise = new Promise<string>();

            if (string.IsNullOrEmpty(user.userId) || string.IsNullOrEmpty(user.userName))
            {
                promise.Reject(new Exception("UserID or UserName is null or empty"));
                return promise;
            }

            RestClient.Post($"{Config.FirebaseDbUrl}users.json", user).Then(response =>
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Text);

                if (jsonResponse != null && jsonResponse.TryGetValue("name", out string newUserId))
                {
                    promise.Resolve(newUserId);
                }
                else
                {
                    promise.Reject(new Exception("User ID not returned from Firebase"));
                }
            }).Catch(error => { promise.Reject(error); });


            return promise;
        }

        public static IPromise<ResponseHelper> UpdateUserInfo(User user)
        {
            var userRequest = new UserRequest {userName = user.userName, balance = user.balance, userId = user.userId, imageUrl = user.imageUrl, buffPurchase = user.buffPurchase};
            string keyUrlPart = $"{Config.FirebaseDbUrl}users/{user.id}.json";
            return RestClient.Put(keyUrlPart, userRequest);
        }


        public static Promise<double> GetUserBalanceById(string userId)
        {
            return new Promise<double>((resolve, reject) =>
            {
                string queryUrl = $"{Config.FirebaseDbUrl}users.json?orderBy=\"userId\"&equalTo=\"{userId}\"";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawUsers =
                        JsonConvert.DeserializeObject<Dictionary<string, UserRequest>>(response.Text);
                    if (rawUsers == null || !rawUsers.Any())
                    {
                        reject(new Exception("User not found for provided UserID"));
                        return;
                    }

                    var firstUserKey = rawUsers.Keys.First();
                    var rawUser = rawUsers[firstUserKey];

                    double balance = rawUser.balance;
                    resolve(balance);
                }).Catch(error =>
                {
                    reject(new Exception($"Error retrieving user balance by UserID: {error.Message}"));
                });
            });
        }

        public static Promise<List<User>> GetAllUsers()
        {
            return new Promise<List<User>>((resolve, reject) =>
            {
                string queryUrl = $"{Config.FirebaseDbUrl}users.json";

                RestClient.Get(queryUrl).Then(response =>
                {
                    var rawUsers = JsonConvert.DeserializeObject<Dictionary<string, UserRequest>>(response.Text);
                    if (rawUsers == null || !rawUsers.Any())
                    {
                        reject(new Exception("No users found"));
                        return;
                    }

                    var users = rawUsers.Select(kvp =>
                    {
                        var user = new User
                        {
                            userId = kvp.Value.userId, userName = kvp.Value.userName, balance = kvp.Value.balance,
                            imageUrl = kvp.Value.imageUrl, buffPurchase = kvp.Value.buffPurchase,
                            id = kvp.Key
                        };
                        return user;
                    }).ToList();

                    resolve(users);
                }).Catch(error => { reject(new Exception($"Error retrieving all users: {error.Message}")); });
            });
        }
    }
}