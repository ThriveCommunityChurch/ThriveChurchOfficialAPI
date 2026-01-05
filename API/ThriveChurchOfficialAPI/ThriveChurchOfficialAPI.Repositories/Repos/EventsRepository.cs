using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Repository for Events collection operations
    /// </summary>
    public class EventsRepository : RepositoryBase<Event>, IEventsRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;

        /// <summary>
        /// Events Repository C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public EventsRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _eventsCollection = DB.GetCollection<Event>("Events");

            // Create indexes for better query performance
            CreateIndexesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create indexes for the Events collection
        /// </summary>
        private async Task CreateIndexesAsync()
        {
            var indexModels = new List<CreateIndexModel<Event>>
            {
                // Index on StartTime for date-based queries
                new CreateIndexModel<Event>(
                    Builders<Event>.IndexKeys.Ascending(e => e.StartTime),
                    new CreateIndexOptions { Name = IndexKeys.EventsByStartTimeAsc }),

                // Index on IsActive for filtering active/inactive events
                new CreateIndexModel<Event>(
                    Builders<Event>.IndexKeys.Ascending(e => e.IsActive),
                    new CreateIndexOptions { Name = IndexKeys.EventsByIsActiveAsc }),

                // Index on IsFeatured for featured events queries
                new CreateIndexModel<Event>(
                    Builders<Event>.IndexKeys.Ascending(e => e.IsFeatured),
                    new CreateIndexOptions { Name = IndexKeys.EventsByIsFeaturedAsc }),

                // Compound index for common query pattern: active events sorted by start time
                new CreateIndexModel<Event>(
                    Builders<Event>.IndexKeys
                        .Ascending(e => e.IsActive)
                        .Ascending(e => e.StartTime),
                    new CreateIndexOptions { Name = IndexKeys.EventsByIsActiveStartTimeAsc })
            };

            await _eventsCollection.Indexes.CreateManyAsync(indexModels);
        }

        /// <summary>
        /// Get all events, optionally including inactive events
        /// </summary>
        public async Task<IEnumerable<Event>> GetAllEvents(bool includeInactive = false)
        {
            FilterDefinition<Event> filter;

            if (includeInactive)
            {
                filter = Builders<Event>.Filter.Empty;
            }
            else
            {
                filter = Builders<Event>.Filter.Eq(e => e.IsActive, true);
            }

            var sort = Builders<Event>.Sort.Ascending(e => e.StartTime);

            var cursor = await _eventsCollection.Find(filter).Sort(sort).ToListAsync();

            return cursor;
        }

        /// <summary>
        /// Get upcoming events starting from a specific date
        /// </summary>
        public async Task<IEnumerable<Event>> GetUpcomingEvents(DateTime fromDate, int count = 10)
        {
            var filter = Builders<Event>.Filter.And(
                Builders<Event>.Filter.Eq(e => e.IsActive, true),
                Builders<Event>.Filter.Gte(e => e.StartTime, fromDate)
            );

            var sort = Builders<Event>.Sort.Ascending(e => e.StartTime);

            var cursor = await _eventsCollection
                .Find(filter)
                .Sort(sort)
                .Limit(count)
                .ToListAsync();

            return cursor;
        }

        /// <summary>
        /// Get events within a specific date range
        /// </summary>
        public async Task<IEnumerable<Event>> GetEventsByDateRange(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Event>.Filter.And(
                Builders<Event>.Filter.Eq(e => e.IsActive, true),
                Builders<Event>.Filter.Gte(e => e.StartTime, startDate),
                Builders<Event>.Filter.Lte(e => e.StartTime, endDate)
            );

            var sort = Builders<Event>.Sort.Ascending(e => e.StartTime);

            var cursor = await _eventsCollection.Find(filter).Sort(sort).ToListAsync();

            return cursor;
        }

        /// <summary>
        /// Get a single event by its ID
        /// </summary>
        public async Task<Event> GetEventById(string eventId)
        {
            if (!IsValidObjectId(eventId))
            {
                return null;
            }

            var filter = Builders<Event>.Filter.Eq(e => e.Id, eventId);

            var cursor = await _eventsCollection.FindAsync(filter);

            return await cursor.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get all featured events that are active and upcoming
        /// </summary>
        public async Task<IEnumerable<Event>> GetFeaturedEvents()
        {
            var filter = Builders<Event>.Filter.And(
                Builders<Event>.Filter.Eq(e => e.IsActive, true),
                Builders<Event>.Filter.Eq(e => e.IsFeatured, true),
                Builders<Event>.Filter.Gte(e => e.StartTime, DateTime.UtcNow)
            );

            var sort = Builders<Event>.Sort.Ascending(e => e.StartTime);

            var cursor = await _eventsCollection.Find(filter).Sort(sort).ToListAsync();

            return cursor;
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        public async Task<Event> CreateEvent(Event newEvent)
        {
            // Set timestamps
            newEvent.CreateDate = DateTime.UtcNow;
            newEvent.LastUpdated = DateTime.UtcNow;

            await _eventsCollection.InsertOneAsync(newEvent);

            // Return the inserted event (now has Id assigned by MongoDB)
            return newEvent;
        }

        /// <summary>
        /// Update an existing event
        /// </summary>
        public async Task<Event> UpdateEvent(string eventId, Event updatedEvent)
        {
            if (!IsValidObjectId(eventId))
            {
                return null;
            }

            var filter = Builders<Event>.Filter.Eq(e => e.Id, eventId);

            // Update the LastUpdated timestamp
            updatedEvent.LastUpdated = DateTime.UtcNow;

            // Ensure the Id is set correctly
            updatedEvent.Id = eventId;

            var result = await _eventsCollection.FindOneAndReplaceAsync(
                filter,
                updatedEvent,
                new FindOneAndReplaceOptions<Event>
                {
                    ReturnDocument = ReturnDocument.After
                });

            return result;
        }

        /// <summary>
        /// Permanently delete an event (hard delete)
        /// </summary>
        public async Task<bool> DeleteEvent(string eventId)
        {
            if (!IsValidObjectId(eventId))
            {
                return false;
            }

            var filter = Builders<Event>.Filter.Eq(e => e.Id, eventId);

            var result = await _eventsCollection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Deactivate an event (soft delete)
        /// </summary>
        public async Task<bool> DeactivateEvent(string eventId)
        {
            if (!IsValidObjectId(eventId))
            {
                return false;
            }

            var filter = Builders<Event>.Filter.Eq(e => e.Id, eventId);

            var update = Builders<Event>.Update
                .Set(e => e.IsActive, false)
                .Set(e => e.LastUpdated, DateTime.UtcNow);

            var result = await _eventsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}

