using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using System.Linq;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Sermons Repo
    /// </summary>
    public class SermonsRepository : RepositoryBase, ISermonsRepository
    {
        private readonly IMongoDatabase db;

        /// <summary>
        /// Sermons Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public SermonsRepository(IConfiguration Configuration, ITokenRepo tokenRepo)
            : base(Configuration, tokenRepo)
        {
            db = Client.GetDatabase("SermonSeries");
        }

        /// <summary>
        /// Returns all Sermon Series' from MongoDB - including active sermon series'
        /// </summary>
        /// <returns></returns>
        public async Task<AllSermonsResponse> GetAllSermons()
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");
            var documents = await collection.Find(_ => true).ToListAsync();

            var allSermonsResponse = new AllSermonsResponse
            {
                Sermons = documents.OrderBy(i => i.StartDate)
            };

            return allSermonsResponse;
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber)
        {
            // determine which number of sermon series to request & which ones to return
            var responseCount = 10;
            if (pageNumber < 3)
            {
                responseCount = 5;
            }

            // what indexes should we use as the beginning? However, if larger than this we'll need to do more cheeckyness
            var beginningCount = pageNumber == 2 ? 5 : 0;

            if (pageNumber >= 3)
            {
                beginningCount = (pageNumber * 10) - 20; // we subtract 20 because the first 10 pages we only return 5
            }

            var totalPageNumber = 1;

            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");
            var documents = await collection.Find(_ => true)
                .SortByDescending(i => i.StartDate)
                .Skip(beginningCount)
                .Limit(responseCount)
                .ToListAsync();

            // use this response in the total Record count
            var totalRecordDelegate = GetAllSermons();

            // get the count from the async task above
            var totalRecord = await totalRecordDelegate;

            // converting this to an array first will allow us to not have to enumerate the collection multiple times
            // ToArray() just converts the DataType via reflection through a Buffer -> O(n) BUT length reads are O(1) after this
            var totalRecordCount = totalRecord.Sermons.ToArray().Length;

            // since we know how many there are, in this method we can use that to indicate the total Pages in this method
            // Remember that pages 1 & 2 return a response of only 5
            // take out the first 2 sets of 5, and as long as the number isn't neg before we get there then we have 2 pages
            var remainingRecords = totalRecordCount - 10;

            if (remainingRecords <= 0)
            {
                totalPageNumber = 2;
            }

            // we already know that there are only 2 pages, so if there's only those 2 then continue,
            // otherwise calculate the leftovers
            if (totalRecordCount > 10)
            {
                // we know that there are at least 2 pages if we are here
                totalPageNumber = 2;

                // Now we divide the total count by the calc and if we have a remainder then we round up to the next highest whole number
                double remainingPages = remainingRecords / 10.0;

                // this technichally is similiar to modulo, except we want the remainder and the integer,
                // so that we can add whole pages that are included as the int and any leftovers that are not quite a full page yet in the remainder
                long intPart = (long)remainingPages;
                double fractionalPart = remainingPages - intPart;

                var value = (int)intPart;
                if (value > 0)
                {
                    // Append whatever number of records are left based on our paging scheme
                    totalPageNumber += value;
                }

                // We don't have another page
                if (fractionalPart <= 1 && fractionalPart > 0)
                {
                    totalPageNumber++;
                }
            }

            if (pageNumber > totalPageNumber)
            {
                return new SystemResponse<SermonsSummaryPagedResponse>(true, string.Format(SystemMessages.InvalidPagingNumber, pageNumber, totalPageNumber));
            }

            // construct the list of summaries
            var summariesList = new List<SermonSeriesSummary>();
            foreach (var series in documents)
            {
                var summary = new SermonSeriesSummary
                {
                    // Use the thumbnail URL for these summaries, 
                    // because we will be loading many of them at once
                    ArtUrl = series.Thumbnail,
                    Id = series.Id,
                    StartDate = series.StartDate.Value,
                    Title = series.Name
                };
                summariesList.Add(summary);
            }

            // construct the final response
            var response = new SermonsSummaryPagedResponse
            {
                Summaries = summariesList,
                PagingInfo = new PageInfo
                {
                    PageNumber = pageNumber,
                    TotalPageCount = totalPageNumber,
                    TotalRecordCount = totalRecordCount
                }
            };

            return new SystemResponse<SermonsSummaryPagedResponse>(response, "Success!");
        }

        /// <summary>
        /// Adds a new SermonSeries to the Sermons Collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> CreateNewSermonSeries(SermonSeries request)
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // updated time is now
            request.LastUpdated = DateTime.UtcNow;

            await collection.InsertOneAsync(request);

            // respond with the inserted object
            var inserted = await collection.FindAsync(
                    Builders<SermonSeries>.Filter.Eq(l => l.Slug, request.Slug));

            var response = inserted.FirstOrDefault();
            if (response == default(SermonSeries))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.ErrorOcurredInsertingIntoCollection, "Sermons"));
            }

            return new SystemResponse<SermonSeries>(response, "Success!");
        }

        /// <summary>
        /// Finds and replaces the old object for the replacement
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> UpdateSermonSeries(SermonSeries request)
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // updated time is now
            request.LastUpdated = DateTime.UtcNow;

            var singleSeries = await collection.FindOneAndReplaceAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Id, request.Id), request);

            if (singleSeries == null)
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.UnableToFindValueInCollection, request.Id, "Sermons"));
            }

            return new SystemResponse<SermonSeries>(request, "Success!");
        }

        /// <summary>
        /// Gets a series object for the specified Id
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <returns></returns>
        public async Task<SermonSeries> GetSermonSeriesForId(string SeriesId)
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            var singleSeries = await collection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Id, SeriesId));

            var response = singleSeries.FirstOrDefault();

            return response;
        }

        /// <summary>
        /// Used to find a series for a particular unique slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> GetSermonSeriesForSlug(string slug)
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            var singleSeries = await collection.FindAsync(
                   Builders<SermonSeries>.Filter.Eq(s => s.Slug, slug));

            var response = singleSeries.FirstOrDefault();
            if (response == default(SermonSeries))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.UnableToFindSermonWithSlug, slug));
            }

            return new SystemResponse<SermonSeries>(response, "Success!");
        }

        /// <summary>
        /// Gets a sermon message for its Id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SermonMessage> GetMessageForId(string messageId)
        {
            IMongoCollection<SermonSeries> collection = db.GetCollection<SermonSeries>("Sermons");

            // use a filter since we are looking for an Id which is a value in an array with n elements
            var filter = Builders<SermonSeries>.Filter.ElemMatch(x => x.Messages, x => x.MessageId == messageId);

            var seriesResponse = await collection.FindAsync(filter);
            var series = seriesResponse.FirstOrDefault();
            var response = series.Messages.Where(i => i.MessageId == messageId).FirstOrDefault();

            return response;
        }

        /// <summary>
        /// Returns all Sermon Series' from MongoDB - including active sermon series'
        /// </summary>
        /// <returns></returns>
        public async Task<LiveSermons> GetLiveSermons()
        {
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");
            var documents = await collection.Find(_ => true).FirstOrDefaultAsync();

            return documents;
        }

        /// <summary>
        /// Updates the LiveStreaming object in Mongo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermons(LiveSermons request)
        {
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");

            var document = await collection.FindOneAndUpdateAsync(
                    Builders<LiveSermons>.Filter
                        .Eq(l => l.Id, request.Id),
                    Builders<LiveSermons>.Update
                    .Set(l => l.LastUpdated, DateTime.UtcNow)
                    .Set(l => l.IsLive, request.IsLive)
                    .Set(l => l.SpecialEventTimes, request.SpecialEventTimes)
                    .Set(l => l.ExpirationTime, request.ExpirationTime.ToUniversalTime())
                );

            if (document == null || document == default(LiveSermons))
            {
                // something bad happened here
                return null;
            }

            // get the object again because it's not updated in memory
            var updatedDocument = await collection.FindAsync(
                    Builders<LiveSermons>.Filter.Eq(l => l.Id, request.Id));

            var response = updatedDocument.FirstOrDefault();

            if (response == null || response == default(LiveSermons))
            {
                // something bad happened here
                return null;
            }

            return response;
        }
        
        /// <summary>
        /// Updates the LiveStreaming object in Mongo
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveSermons> GoLive(LiveSermonsUpdateRequest request)
        {
            IMongoCollection<LiveSermons> collection = db.GetCollection<LiveSermons>("Livestream");

            // get the live sermon, we will only ever have 1 object in this collection
            var liveObject = await GetLiveSermons();

            var document = await collection.FindOneAndUpdateAsync(
                    Builders<LiveSermons>.Filter
                        .Eq(l => l.Id, liveObject.Id),
                    Builders<LiveSermons>.Update
                    .Set(l => l.LastUpdated, DateTime.UtcNow)
                    .Set(l => l.IsLive, true)
                    .Set(l => l.SpecialEventTimes, null)
                    .Set(l => l.ExpirationTime, request.ExpirationTime.Value.ToUniversalTime())
                );

            if (document == null || document == default(LiveSermons))
            {
                // something bad happened here
                return null;
            }

            // get the object again because it's not updated in memory
            var updatedDocument = await collection.FindAsync(
                    Builders<LiveSermons>.Filter.Eq(l => l.Id, liveObject.Id));

            var response = updatedDocument.FirstOrDefault();

            if (response == null || response == default(LiveSermons))
            {
                // something bad happened here
                return null;
            }

            return response;
        }

        /// <summary>
        /// Update LiveSermons to inactive once the stream has concluded
        /// </summary>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermonsInactive()
        {
            var liveSermonsResponse = await GetLiveSermons();
            if (liveSermonsResponse == null || liveSermonsResponse == default(LiveSermons))
            {
                // something bad happened here
                return default(LiveSermons);
            }

            // make the change to reflect that this sermon was just updated
            liveSermonsResponse.IsLive = false;
            liveSermonsResponse.SpecialEventTimes = null;

            var updatedLiveSermon = await UpdateLiveSermons(liveSermonsResponse);
            if (updatedLiveSermon == null || updatedLiveSermon == default(LiveSermons))
            {
                // something bad happened here
                return default(LiveSermons);
            }

            return updatedLiveSermon;
        }
    }
}
