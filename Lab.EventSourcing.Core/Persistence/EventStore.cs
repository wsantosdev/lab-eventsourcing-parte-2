using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lab.EventSourcing.Core
{
    public class EventStore
    {
        private readonly EventStoreDbContext _eventStoreContext;

        public static EventStore Create() =>
            new EventStore();

        private EventStore()
        {
            _eventStoreContext = new EventStoreDbContext(new DbContextOptionsBuilder<EventStoreDbContext>()
                                                                .UseInMemoryDatabase(databaseName: "EventStore")
                                                                .EnableSensitiveDataLogging()
                                                                .Options);
        }

        public void Commit<TModel>(TModel model) where TModel : EventSourcingModel
        {
            var events = model.PendingEvents.Select(e => PersistentEvent.Create(model.Id,
                                                                                ((ModelEventBase)e).ModelVersion,
                                                                                ((ModelEventBase)e).When,
                                                                                e.GetType().AssemblyQualifiedName,
                                                                                JsonConvert.SerializeObject(e)));
            
            _eventStoreContext.Events.AddRange(events);
            _eventStoreContext.SaveChanges();
            model.Commit();
        }

        public TModel GetById<TModel>(Guid id) where TModel : EventSourcingModel =>
            LoadModel<TModel>(e => e.ModelId == id);

        public TModel GetByVersion<TModel>(Guid id, int version) where TModel : EventSourcingModel =>
            LoadModel<TModel>(e => e.ModelId == id && e.ModelVersion <= version);

        public TModel GetByTime<TModel>(Guid id, DateTime until) where TModel : EventSourcingModel =>
            LoadModel<TModel>(e => e.ModelId == id && e.When <= until);

        private TModel LoadModel<TModel>(Expression<Func<PersistentEvent, bool>> expression) where TModel : EventSourcingModel
        {
            var events = _eventStoreContext.Events.Where(expression)
                                  .OrderBy(e => e.ModelVersion)
                                  .Select(e => JsonConvert.DeserializeObject(e.Data, Type.GetType(e.EventType)))
                                  .Cast<ModelEventBase>();

            return (TModel)Activator.CreateInstance(typeof(TModel), 
                                                    BindingFlags.NonPublic | BindingFlags.Instance, 
                                                    null, 
                                                    new[] { events } , 
                                                    CultureInfo.InvariantCulture);
        }

        private class EventStoreDbContext : DbContext
        {
            public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options) { }

            public DbSet<PersistentEvent> Events { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) =>
                modelBuilder.Entity<PersistentEvent>().HasKey(k => new { k.ModelId, k.ModelVersion });
        }
    }
}
