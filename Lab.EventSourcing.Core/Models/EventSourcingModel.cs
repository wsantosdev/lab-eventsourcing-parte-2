using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.EventSourcing.Core
{
    public abstract class EventSourcingModel
    {
        private Queue<IEvent> _pendingEvents = new Queue<IEvent>();
        public IEnumerable<IEvent> PendingEvents { get => _pendingEvents.AsEnumerable(); }
        public Guid Id { get; protected set; }
        public int Version { get; protected set; } = 0;
        protected int NextVersion { get => Version + 1; }
        
        protected EventSourcingModel(IEnumerable<ModelEventBase> persisted)
        {
            foreach (var e in persisted)
            {
                Apply(e);
                Version = e.ModelVersion;
            }
        }

        protected void RaiseEvent<TEvent>(TEvent pendingEvent) where TEvent: ModelEventBase
        {
            _pendingEvents.Enqueue(pendingEvent);
            Apply(pendingEvent);
            Version = pendingEvent.ModelVersion;
        }

        protected abstract void Apply(IEvent pendingEvent);

        public void Commit() =>
            _pendingEvents.Clear();
    }
}