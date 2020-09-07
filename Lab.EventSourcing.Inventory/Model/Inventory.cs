using Lab.EventSourcing.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lab.EventSourcing.Inventory
{
    public class Inventory : EventSourcingModel
    {
        private readonly ConcurrentDictionary<Guid, int> _stock = new ConcurrentDictionary<Guid, int>();
        
        protected Inventory(IEnumerable<ModelEventBase> events) : base(events) {}

        public static Inventory Create()
        {
            var inventory = new Inventory(Enumerable.Empty<ModelEventBase>());
            inventory.RaiseEvent(new InventoryCreated(Guid.NewGuid()));

            return inventory;
        }

        public void AddProduct(Guid id, int quantity)
        {
            if (quantity == 0)
                throw new InvalidOperationException("The quantity must be greater than zero.");
            
            RaiseEvent(new ProductAdded(Id, NextVersion, id, quantity));
        }

        public void RemoveProduct(Guid id, int quantity)
        {
            if (!_stock.ContainsKey(id))
                throw new InvalidOperationException("Product not found.");

            if (_stock[id] < quantity)
                throw new InvalidOperationException($"The requested quantity is unavailable. Current quantity: {_stock[id]}.");
                
            RaiseEvent(new ProductRemoved(Id, NextVersion, id, quantity));
        }

        public int GetProductCount(Guid productId)
        {
            return _stock.TryGetValue(productId, out int quantity) 
                ? quantity
                : 0;
        }

        protected override void Apply(IEvent pendingEvent)
        {
            switch(pendingEvent)
            {
                case InventoryCreated created:
                    Apply(created);
                    break;
                case ProductAdded added:
                    Apply(added);
                    break;
                case ProductRemoved removed:
                    Apply(removed);
                    break;
                default:
                    throw new ArgumentException($"Invalid event type: {pendingEvent.GetType()}.");
            }
        }

        protected void Apply(InventoryCreated pending) =>
            Id = pending.ModelId;

        protected void Apply(ProductAdded pending) =>
            _stock.AddOrUpdate(pending.ProductId, pending.Quantity,
                               (productId, currentQuantity) => currentQuantity += pending.Quantity);

        protected void Apply(ProductRemoved pending) =>
            _stock[pending.ProductId] -= pending.Quantity;
    }
}
