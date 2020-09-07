using Lab.EventSourcing.Core;
using System;

namespace Lab.EventSourcing.Inventory
{
    public class ProductAdded : ModelEventBase
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        public ProductAdded(Guid modelId, int modelVersion, Guid productId, int quantity)
            : base(modelId, modelVersion) =>
            (ProductId, Quantity) = (productId, quantity);
                
    }
}