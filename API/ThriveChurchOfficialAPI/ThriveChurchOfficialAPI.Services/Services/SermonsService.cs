using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService
    {
        private readonly ISermonsRepository _sermonsRepository;
        private readonly IMemoryCache _cache;
        private Timer _timer;

        public SermonsService(ISermonsRepository sermonsRepo, IMemoryCache cache)
        {
            // init the repo with the connection string via DI
            _sermonsRepository = sermonsRepo;
            _cache = cache;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<AllSermonsSummaryResponse> GetAllSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetAllSermons();

            // we need to convert everything to the right response pattern
            var sortedSeries = getAllSermonsResponse.Sermons.OrderByDescending(i => i.StartDate);

            // for each one add only the properties we want to the list
            var responseList = new List<SermonSeriesSummary>();
            foreach (var series in sortedSeries)
            {
                var elemToAdd = new SermonSeriesSummary
                {
                    ArtUrl = series.ArtUrl,
                    Id = series.Id,
                    StartDate = series.StartDate.Value,
                    Title = series.Name
                };

                responseList.Add(elemToAdd);
            }

            var response = new AllSermonsSummaryResponse
            {
                Summaries = responseList
            };

            return response;
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public async Task<SermonsSummaryPagedResponse> GetPagedSermons(int pageNumber)
        {
            // Page num canonot be 0, and neg page numbers make no sense
            if (pageNumber <= 0)
            {
                return null;
            }

            // since this is going to get called a ton of times we should cache this

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetPagedSermons, pageNumber), out SermonsSummaryPagedResponse pagedSermonsResponse))
            {
                // Key not in cache, so get data.
                pagedSermonsResponse = await _sermonsRepository.GetPagedSermons(pageNumber);

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetPagedSermons, pageNumber), pagedSermonsResponse, CacheEntryOptions);
            }

            return pagedSermonsResponse;
        }

        /// <summary>
        /// returns a list of all SermonSeries Objets
        /// </summary>
        public async Task<SermonSeries> CreateNewSermonSeries(SermonSeries request)
        {
            var validRequest = SermonSeries.ValidateRequest(request);

            if (!validRequest)
            {
                return null;
            }

            // the Slug on the series should be unique, so if we already have one with this slug
            // return an error - because we want to avoid having bad data in our database
            var allSermonSries = await _sermonsRepository.GetAllSermons();

            if (allSermonSries == null || allSermonSries == default(AllSermonsResponse))
            {
                return null;
            }

            var seriesWithSameSlug = allSermonSries.Sermons.Where(i => string.Equals(i.Slug, request.Slug, StringComparison.InvariantCultureIgnoreCase));
            if (seriesWithSameSlug.Any())
            {
                // there is already a sermon series with this slug, respond with one of those
                return seriesWithSameSlug.FirstOrDefault();
            }

            // if any of the sermon series' currently have a null
            if (request.EndDate == null)
            {
                var currentlyActiveSeries = allSermonSries.Sermons.Where(i => i.EndDate == null);

                if (currentlyActiveSeries.Any())
                {
                    return currentlyActiveSeries.FirstOrDefault();
                }
            }
            else
            {
                request.EndDate = request.EndDate.Value.ToUniversalTime().Date;
            }

            // sanitise the start dates
            request.StartDate = request.StartDate.Value.ToUniversalTime().Date;

            foreach (var message in request.Messages)
            {
                // sanitise the message dates and get rid of the times
                message.Date = message.Date.Value.Date.ToUniversalTime().Date;
                message.MessageId = Guid.NewGuid().ToString();
            }

            var getAllSermonsResponse = await _sermonsRepository.CreateNewSermonSeries(request);

            return getAllSermonsResponse;
        }

        /// <summary>
        /// Adds a new spoken message to a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonSeries> AddMessageToSermonSeries(string SeriesId, AddMessagesToSeriesRequest request)
        {
            var validRequest = AddMessagesToSeriesRequest.ValidateRequest(request);
            if (!validRequest)
            {
                return null;
            }

            if (string.IsNullOrEmpty(SeriesId))
            {
                return null;
            }

            // if we can't find it then the Id is invalid
            var getSermonSeriesResponse = await _sermonsRepository.GetSermonSeriesForId(SeriesId);
            if (getSermonSeriesResponse == null)
            {
                // didn't find it
                return null;
            }

            // add the sermon message to the response object and re-update the Mongo doc
            var currentMessages = getSermonSeriesResponse.Messages.ToList();

            // add the Guid to the requested messages then add the messages
            foreach (var message in request.MessagesToAdd)
            {
                // sanitise the message dates and get rid of the times
                message.Date = message.Date.Value.Date.ToUniversalTime().Date;
                message.MessageId = Guid.NewGuid().ToString();
            }

            currentMessages.AddRange(request.MessagesToAdd);

            // readd the messages back to the object, This is important (see SO for  Deep Copy vs shallow copy)
            getSermonSeriesResponse.Messages = currentMessages;

            // find and replace the one with the updated object
            var updateResponse = await _sermonsRepository.UpdateSermonSeries(getSermonSeriesResponse);
            if(updateResponse == null)
            {
                return null;
            }

            return getSermonSeriesResponse;
        }

        /// <summary>
        /// Updates a sermon message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonMessage> UpdateMessageInSermonSeries(string messageId, UpdateMessagesInSermonSeriesRequest request)
        {
            var validRequest = UpdateMessagesInSermonSeriesRequest.ValidateRequest(request);
            if (!validRequest)
            {
                return null;
            }

            var validGuid = Guid.TryParse(messageId, out Guid messageGuid);
            if (!validGuid)
            {
                return null;
            }

            var messageResponse = await _sermonsRepository.GetMessageForId(messageId);
            if (messageResponse == null)
            {
                return null;
            }

            return messageResponse;
        }

        /// <summary>
        /// Gets a sermon series for its Id
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task<SermonSeries> GetSeriesForId(string seriesId)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                return null;
            }

            var seriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
            if (seriesResponse == null)
            {
                // the series Id that was requested is invalid
                return null;
            }

            var orderedMessages = seriesResponse.Messages.OrderByDescending(i => i.Date.Value);
            seriesResponse.Messages = orderedMessages;

            return seriesResponse;
        }

        /// <summary>
        /// Updates a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonSeries> ModifySermonSeries(string seriesId, SermonSeriesUpdateRequest request)
        {
            var validRequest = SermonSeriesUpdateRequest.ValidateRequest(request);
            if (!validRequest)
            {
                return null;
            }

            if (string.IsNullOrEmpty(seriesId))
            {
                return null;
            }

            var getSermonSeriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
            if (getSermonSeriesResponse == null)
            {
                return null;
            }

            // make sure that no one can update the slug to something that already exists
            // this is not allowed
            if (getSermonSeriesResponse.Slug != request.Slug)
            {
                // cannot change the slug -> make sure a slug is set when you create the series.
                return null;
            }

            getSermonSeriesResponse.Name = request.Name;
            getSermonSeriesResponse.EndDate = request.EndDate.ToUniversalTime().Date;
            getSermonSeriesResponse.StartDate = request.StartDate.ToUniversalTime().Date;
            getSermonSeriesResponse.Thumbnail = request.Thumbnail;
            getSermonSeriesResponse.ArtUrl = request.ArtUrl;
            getSermonSeriesResponse.Slug = request.Slug;

            var updateResponse = await _sermonsRepository.UpdateSermonSeries(getSermonSeriesResponse);

            return updateResponse;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<LiveStreamingResponse> GetLiveSermons()
        {
            var getLiveSermonsResponse = await _sermonsRepository.GetLiveSermons();

            // we are not streaming so there's no need to include anything
            var response = new LiveStreamingResponse
            {
                IsLive = false,
                ExpirationTime = getLiveSermonsResponse.ExpirationTime
            };

            // if we are currently streaming then we will need to add the slug to the middle of the Facebook link
            if (getLiveSermonsResponse.IsLive)
            {
                // do the business logic here friend
                response.IsLive = true;
                response.IsSpecialEvent = getLiveSermonsResponse.SpecialEventTimes != null ? true : false;
                response.SpecialEventTimes = getLiveSermonsResponse.SpecialEventTimes ?? null;
            }

            return response;
        }

        /// <summary>
        /// Update the LiveSermons object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveStreamingResponse> UpdateLiveSermons(LiveSermonsUpdateRequest request)
        {
            // validate the request
            var validRequest = LiveSermonsUpdateRequest.ValidateRequest(request);

            if (!validRequest)
            {
                // an error ocurred here
                return default(LiveStreamingResponse);
            }

            // Update this object for the requested fields
            var updated = new LiveSermons
            {
                ExpirationTime = new DateTime(1990, 01, 01, 11, 20, 0, 0), // reset this on this update & give ourselves a little buffer (5 min)
                IsLive = true, 
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = null,
                Id = request.Id
            };

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(updated);
            if (updateLiveSermonsResponse == null)
            {
                // something bad happened here
                return default(LiveStreamingResponse);
            }

            // times have already been converted to UTC
            var response = new LiveStreamingResponse
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime,
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = updateLiveSermonsResponse.SpecialEventTimes != null ? true : false,
                SpecialEventTimes = updateLiveSermonsResponse.SpecialEventTimes ?? null
            };

            // we are updating this so we should watch for when it expires, when it does we will need to update Mongo
            DetermineIfStreamIsInactive();

            return response;
        }

        /// <summary>
        /// Updates the LiveSermon to be a special event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveStreamingResponse> UpdateLiveForSpecialEvents(LiveSermonsSpecialEventUpdateRequest request)
        {
            // validate the request
            var validRequest = LiveSermonsSpecialEventUpdateRequest.ValidateRequest(request);

            if (!validRequest)
            {
                // an error ocurred here
                return default(LiveStreamingResponse);
            }
            
            // Update this object for the requested fields
            var updated = new LiveSermons
            {
                ExpirationTime = request.SpecialEventTimes.End ?? new DateTime(1990, 01, 01, 11, 15, 0, 0),
                IsLive = true,
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = request.SpecialEventTimes
            };

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(updated);
            if (updateLiveSermonsResponse == null)
            {
                // something bad happened here
                return default(LiveStreamingResponse);
            }

            var response = new LiveStreamingResponse
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime.ToUniversalTime(),
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = true,
                SpecialEventTimes = request.SpecialEventTimes
            };

            // we are updating this so we should watch for when it expires, when it does we will need to update Mongo
            DetermineIfStreamIsInactive();

            return response;
        }

        /// <summary>
        /// Returns info about an acive stream
        /// </summary>
        /// <returns></returns>
        public async Task<LiveSermonsPollingResponse> PollForLiveEventData()
        {

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(CacheKeys.GetSermons, out LiveSermons liveSermons))
            {
                // Key not in cache, so get data.
                liveSermons = await _sermonsRepository.GetLiveSermons();

                // Save data in cache.
                _cache.Set(CacheKeys.GetSermons, liveSermons, CacheEntryOptions);
            }

            // if we are not live then we should remove the timer and stop looking
            if (!liveSermons.IsLive)
            {
                if (_timer != null)
                {
                    _timer?.Dispose();
                }

                var pastTimeResponse = new LiveSermonsPollingResponse
                {
                    IsLive = false
                    // we cannot set the ExpireTime because it will have been null here
                };

                return pastTimeResponse;
            }

            // generate response
            var response = new LiveSermonsPollingResponse
            {
                IsLive = liveSermons.IsLive,
                StreamExpirationTime = liveSermons.ExpirationTime.ToUniversalTime()
            };

            return response;
        }

        /// <summary>
        /// Async function that will determine if the stream is currently active
        /// (Every 10 seconds, check the cache)
        /// </summary>
        /// <returns></returns>
        private void DetermineIfStreamIsInactive()
        {
            // we'll look every 10 seconds to see if the stream has expired
            _timer = new Timer(CheckStreamingStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Fire and forget this
        /// </summary>
        /// <param name="state"></param>
        private async void CheckStreamingStatus(object state)
        {
            // look in mongo or cache this response until the time in the DB is finished
            // we just need to make sure we update the value in the database automaticaly when this is past the expiration time
            var pollingResponse = await PollForLiveEventData();

            // IT's now later than what the time is in the database
            if (DateTime.UtcNow.TimeOfDay > pollingResponse.StreamExpirationTime.ToUniversalTime().TimeOfDay)
            { 
                // update mongo to reflect that the sermon is inactive
                await _sermonsRepository.UpdateLiveSermonsInactive();

                // when it's done kill the timer
                _timer?.Dispose();
            }
        }

        /// <summary>
        /// Reset the LiveSermons object back to it's origional state & stop async timer
        /// </summary>
        /// <returns></returns>
        public async Task<LiveSermons> UpdateLiveSermonsInactive()
        {
            var liveStreamCompletedResponse = await _sermonsRepository.UpdateLiveSermonsInactive();

            // we want to stop all async tasks because this will for sure lead to a memory leak
            if (_timer != null)
            {
                _timer?.Dispose();
            }

            return liveStreamCompletedResponse;
        }
    }

    /// <summary>
    /// Globally used Caching keys for O(1) lookups
    /// </summary>
    public static class CacheKeys
    {
        public static string GetSermons { get { return "LiveSermonsCache"; } }

        public static string GetPagedSermons { get { return "PagedSermonsCache:{0}"; } }
    }
}
