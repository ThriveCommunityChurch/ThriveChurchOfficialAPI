using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using System.Linq;

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
        public async Task<AllSermonsResponse> GetAllSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetAllSermons();

            // do the business logic here friend

            return getAllSermonsResponse;
        }

        /// <summary>
        /// returns a list of all Passage Objets
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
                return currentlyActiveSeries.FirstOrDefault();
            }
            else
            {
                request.EndDate = request.StartDate.Value.Date.ToUniversalTime();
            }

            // sanitise the start dates
            request.StartDate = request.StartDate.Value.Date.ToUniversalTime();

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
                message.Date = message.Date.Value.Date.ToUniversalTime();
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
            var seriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
            if (seriesResponse == null)
            {
                // the series Id that was requested is invalid
            }

            return seriesResponse;
        }

        /// <summary>
        /// Updates a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SermonSeries> ModifySermonSeries(string SeriesId, SermonSeriesUpdateRequest request)
        {
            var validRequest = SermonSeriesUpdateRequest.ValidateRequest(request);
            if (!validRequest)
            {
                return null;
            }

            if (string.IsNullOrEmpty(SeriesId))
            {
                return null;
            }

            var getSermonSeriesResponse = await _sermonsRepository.GetSermonSeriesForId(request.SermonId);
            if (getSermonSeriesResponse == null)
            {
                return null;
            }

            // make sure that no one can update the slug to something that already exists
            // this is not allowed
            var validateSlugResponse = await _sermonsRepository.GetSermonSeriesForSlug(request.Slug);
            if (validateSlugResponse != null)
            {
                // cannot edit this series to contain the same response
                return null;
            }

            getSermonSeriesResponse.Name = request.Name;
            getSermonSeriesResponse.EndDate = request.EndDate;
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
                var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/",
                    getLiveSermonsResponse.VideoUrlSlug);

                // do the business logic here friend
                response.IsLive = true;
                response.Title = getLiveSermonsResponse.Title;
                response.VideoUrl = videoUrl;
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
                ExpirationTime = new DateTime(1990, 01, 01, 12, 20, 0, 0), // reset this on this update & give ourselves a little buffer (5 min)
                IsLive = true, 
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = null,
                Title = request.Title,
                VideoUrlSlug = request.Slug,
                Id = request.Id
            };

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(updated);
            if (updateLiveSermonsResponse == null)
            {
                // something bad happened here
                return default(LiveStreamingResponse);
            }

            var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/",
                    updateLiveSermonsResponse.VideoUrlSlug);

            // times have already been converted to UTC
            var response = new LiveStreamingResponse
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime,
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = updateLiveSermonsResponse.SpecialEventTimes != null ? true : false,
                SpecialEventTimes = updateLiveSermonsResponse.SpecialEventTimes ?? null,
                Title = updateLiveSermonsResponse.Title,
                VideoUrl = videoUrl
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

            // generate the updated object so we can update everything at once in the repo
            var getAllSermonsResponse = await _sermonsRepository.GetLiveSermons();

            // Update this object for the requested fields
            var updated = new LiveSermons
            {
                ExpirationTime = request.SpecialEventTimes.End ?? new DateTime(1990, 01, 01, 11, 15, 0, 0),
                IsLive = true,
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = request.SpecialEventTimes,
                Title = request.Title,
                VideoUrlSlug = request.Slug
            };

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(updated);
            if (updateLiveSermonsResponse == null)
            {
                // something bad happened here
                return default(LiveStreamingResponse);
            }

            var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/",
                    updateLiveSermonsResponse.VideoUrlSlug);

            var response = new LiveStreamingResponse
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime.ToUniversalTime(),
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = true,
                SpecialEventTimes = request.SpecialEventTimes,
                Title = updateLiveSermonsResponse.Title,
                VideoUrl = videoUrl
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
            LiveSermons liveSermons = new LiveSermons();
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(CacheKeys.GetSermons, out liveSermons))
            {
                // Key not in cache, so get data.
                liveSermons = await _sermonsRepository.GetLiveSermons();

                // Set cache options.
                cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

                // Save data in cache.
                _cache.Set(CacheKeys.GetSermons, liveSermons, cacheEntryOptions);
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
                var liveStreamCompletedResponse = await _sermonsRepository.UpdateLiveSermonsInactive();

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

        /// <summary>
        /// Gets a collection of recently watched sermon messages
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<RecentlyWatchedMessagesResponse> GetRecentlyWatched(string userId)
        {
            var validGuid = Guid.TryParse(userId, out Guid userGuid);
            if (!validGuid)
            {
                return null;
            }

            var recentlyWatchedResult = await _sermonsRepository.GetRecentlyWatched(userId);
            if (recentlyWatchedResult == null)
            {
                return null;
            }

            var response = new RecentlyWatchedMessagesResponse
            {
                RecentMessages = recentlyWatchedResult
            };

            return response;
        }
    }

    /// <summary>
    /// Globally used Caching keys for O(1) lookups
    /// </summary>
    public static class CacheKeys
    {
        public static string GetSermons { get { return "LiveSermonsCache"; } }
    }
}