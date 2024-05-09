using System;

namespace Libs.Models.RequestModels
{
    [Serializable]
    public class BuffPurchaseFilterModel
    {
        public int? UserId;
        public bool? IsProcessed;

        public BuffPurchaseFilterModel() { ; }

        public BuffPurchaseFilterModel(int? userId, bool? isProcessed)
        {
            this.UserId = userId;
            this.IsProcessed = isProcessed;
        }

    }
}