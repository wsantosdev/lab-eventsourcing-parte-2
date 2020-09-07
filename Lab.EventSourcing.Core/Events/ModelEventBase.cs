using System;

namespace Lab.EventSourcing.Core
{
    public abstract class ModelEventBase : IEvent
    {
        public Guid ModelId { get; private set; }
        public int ModelVersion { get; private set; }
        public DateTime When { get; private set; }
        
        public ModelEventBase(Guid modelId, int modelVersion) =>
            (ModelId, ModelVersion, When) = (modelId, modelVersion, DateTime.Now);
    }
}