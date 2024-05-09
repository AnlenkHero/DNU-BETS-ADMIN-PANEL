using System;

namespace Libs.Models
{
    [Serializable]
    public class BuffPurchase
    {
        public User User;
        public int userId;
        public double price;
        public bool isProcessed;
        public int quantity;
        public string date;
    }
}