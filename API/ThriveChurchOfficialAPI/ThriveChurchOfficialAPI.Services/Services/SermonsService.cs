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
        private IMemoryCache _cache;
        private Timer _timer;

        // the controller cannot have multiple inheritance so we must push it to the service layer
        public SermonsService(IConfiguration Configuration,
            IMemoryCache memoryCache)
            : base(Configuration)
        {
            // init the repo with the connection string
            _sermonsRepository = new SermonsRepository(Configuration);
            _cache = memoryCache;
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
            var validRequest = new SermonSeries().ValidateRequest(request);

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

            var getAllSermonsResponse = await _sermonsRepository.CreateNewSermonSeries(request);

            // do the business logic here friend

            return getAllSermonsResponse;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<LiveStreamingResponse> GetLiveSermons()
        {
            var getLiveSermonsResponse = await _sermonsRepository.GetLiveSermons();

            LiveStreamingResponse response;

            // if we are currently streaming then we will need to add the slug to the middle of the Facebook link
            if (getLiveSermonsResponse.IsLive)
            {
                var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/",
                    getLiveSermonsResponse.VideoUrlSlug);

                // do the business logic here friend
                response = new LiveStreamingResponse()
                {
                    IsLive = true,
                    Title = getLiveSermonsResponse.Title,
                    VideoUrl = videoUrl,
                    ExpirationTime = getLiveSermonsResponse.ExpirationTime,
                    IsSpecialEvent = getLiveSermonsResponse.SpecialEventTimes != null ? true : false,
                    SpecialEventTimes = getLiveSermonsResponse.SpecialEventTimes ?? null
                };
            }
            else
            {
                // we are not streaming so there's no need to include anything
                response = new LiveStreamingResponse()
                {
                    IsLive = false,
                    ExpirationTime = getLiveSermonsResponse.ExpirationTime
                };
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
            var validRequest = new LiveSermonsUpdateRequest().ValidateRequest(request);

            if (!validRequest)
            {
                // an error ocurred here
                return default(LiveStreamingResponse);
            }

            // generate the updated object so we can update everything at once in the repo
            var getLiveSermonsResponse = await _sermonsRepository.GetLiveSermons();

            // Update this object for the requested fields
            var updated = new LiveSermons()
            {
                ExpirationTime = new DateTime(1990, 01, 01, 12, 20, 0, 0), // reset this on this update & give ourselves a little buffer (5 min)
                IsLive = true, 
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = null,
                Title = request.Title,
                VideoUrlSlug = request.Slug,
                Id = getLiveSermonsResponse.Id
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
            var response = new LiveStreamingResponse()
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
            var validRequest = new LiveSermonsSpecialEventUpdateRequest().ValidateRequest(request);

            if (!validRequest)
            {
                // an error ocurred here
                return default(LiveStreamingResponse);
            }

            // generate the updated object so we can update everything at once in the repo
            var getAllSermonsResponse = await _sermonsRepository.GetLiveSermons();

            // Update this object for the requested fields
            var updated = new LiveSermons()
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

            var response = new LiveStreamingResponse()
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

                var pastTimeResponse = new LiveSermonsPollingResponse()
                {
                    IsLive = false
                    // we cannot set the ExpireTime because it will have been null here
                };

                return pastTimeResponse;
            }

            // generate response
            var response = new LiveSermonsPollingResponse()
            {
                IsLive = liveSermons.IsLive,
                StreamExpirationTime = liveSermons.ExpirationTime.ToUniversalTime()
            };

            return response;

            // otherwise go to mongo and grab the object and return it
            // once we have it here store it in the cache
            // and then return it

            // if an error ocurs then return with null
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void DetermineIfStreamIsInactive()
        {
            // we'll look every 10 seconds to see if the stream has expired
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// Do the things
        /// </summary>
        /// <param name="state"></param>
        private async void DoWork(object state)
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
    }

    /// <summary>
    /// Globally used Caching keys for O(1) lookups
    /// </summary>
    public static class CacheKeys
    {
        public static string GetSermons { get { return "LiveSermonsCache"; } }
    }
}