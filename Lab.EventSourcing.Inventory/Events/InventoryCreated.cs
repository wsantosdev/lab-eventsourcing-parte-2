using Lab.EventSourcing.Core;
using System;

namespace Lab.EventSourcing.Inventory
{
    public class InventoryCreated : ModelEventBase
    {
        public InventoryCreated(Guid modelId) : base(modelId, 1) { }
    }
}
