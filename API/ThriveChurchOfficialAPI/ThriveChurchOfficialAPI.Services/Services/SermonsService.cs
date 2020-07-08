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
using MongoDB.Bson;
using Hangfire;
using ThriveChurchOfficialAPI.Core.System;
using NCrontab;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService, IDisposable
    {
        private readonly ISermonsRepository _sermonsRepository;
        private readonly IMemoryCache _cache;
        private Timer _timer;

        public SermonsService(ISermonsRepository sermonsRepo, 
            IMemoryCache cache)
        {
            // init the repo with the connection string via DI
            _sermonsRepository = sermonsRepo;
            _cache = cache;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<SystemResponse<AllSermonsSummaryResponse>> GetAllSermons()
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

            return new SystemResponse<AllSermonsSummaryResponse>(response, "Success!");
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber)
        {
            // Page num canonot be 0, and neg page numbers make no sense
            if (pageNumber <= 0 || pageNumber >= 1474836500)
            {
                return new SystemResponse<SermonsSummaryPagedResponse>(true, string.Format(SystemMessages.IllogicalPagingNumber, pageNumber));
            }

            // since this is going to get called a ton of times we should cache this

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetPagedSermons, pageNumber), out SystemResponse<SermonsSummaryPagedResponse> pagedSermonsResponse))
            {
                // Key not in cache, so get data.
                pagedSermonsResponse = await _sermonsRepository.GetPagedSermons(pageNumber);

                if (pagedSermonsResponse.HasErrors)
                {
                    return new SystemResponse<SermonsSummaryPagedResponse>(true, pagedSermonsResponse.ErrorMessage);
                }

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetPagedSermons, pageNumber), pagedSermonsResponse, CacheEntryOptions);
            }

            return pagedSermonsResponse;
        }

        /// <summary>
        /// returns a list of all SermonSeries Objets
        /// </summary>
        public async Task<SystemResponse<SermonSeries>> CreateNewSermonSeries(CreateSermonSeriesRequest request)
        {
            var validRequest = CreateSermonSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, validRequest.ErrorMessage);
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
                var foundSeries = seriesWithSameSlug.FirstOrDefault();

                // there is already a sermon series with this slug, respond with one of those
                return new SystemResponse<SermonSeries>(foundSeries, "202");
            }

            // if any of the sermon series' are currently active
            if (request.EndDate == null)
            {
                var currentlyActiveSeries = allSermonSries.Sermons.Where(i => i.EndDate == null);
                if (currentlyActiveSeries.Any())
                {
                    // one of the series' is already active
                    var currentlyActive = currentlyActiveSeries.FirstOrDefault();
                    return new SystemResponse<SermonSeries>(currentlyActive, "202");
                }
            }
            else
            {
                request.EndDate = request.EndDate.Value.ToUniversalTime().Date;
            }

            // sanitise the start dates
            request.StartDate = request.StartDate.Value.ToUniversalTime().Date;

            var messageList = new List<SermonMessage>();

            foreach (var message in request.Messages)
            {
                messageList.Add(new SermonMessage
                {
                    AudioFileSize = message.AudioFileSize,
                    AudioDuration = message.AudioDuration,
                    // sanitise the message dates and get rid of the times
                    Date = message.Date.Value.Date.ToUniversalTime().Date,
                    MessageId = Guid.NewGuid().ToString(),
                    AudioUrl = message.AudioUrl,
                    PassageRef = message.PassageRef,
                    Speaker = message.Speaker,
                    Title = message.Title,
                    VideoUrl = message.VideoUrl
                });
            }

            var sermonSeries = new SermonSeries
            {
                ArtUrl = request.ArtUrl,
                EndDate = request.EndDate,
                Messages = messageList,
                Name = request.Name,
                Slug = request.Slug,
                StartDate = request.StartDate,
                Thumbnail = request.Thumbnail,
                Year = request.Year
            };

            var creationResponse = await _sermonsRepository.CreateNewSermonSeries(sermonSeries);
            if (creationResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, creationResponse.ErrorMessage); 
            }

            // Save data in cache.
            _cache.Set(string.Format(CacheKeys.GetSermonSeries, creationResponse.Result.Id), creationResponse.Result, PersistentCacheEntryOptions);

            return new SystemResponse<SermonSeries>(creationResponse.Result, "Success!");
        }

        /// <summary>
        /// Adds a new spoken message to a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> AddMessageToSermonSeries(string SeriesId, AddMessagesToSeriesRequest request)
        {
            var validRequest = AddMessagesToSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, validRequest.ErrorMessage);
            }

            if (string.IsNullOrEmpty(SeriesId))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.NullProperty, "SeriesId"));
            }

            // if we can't find it then the Id is invalid
            var getSermonSeriesResponse = await _sermonsRepository.GetSermonSeriesForId(SeriesId);
            if (getSermonSeriesResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, getSermonSeriesResponse.ErrorMessage);
            }

            var series = getSermonSeriesResponse.Result;

            // add the sermon message to the response object and re-update the Mongo doc
            var currentMessages = series.Messages.ToList();

            var messageList = new List<SermonMessage>();

            foreach (var message in request.MessagesToAdd)
            {
                messageList.Add(new SermonMessage
                {
                    AudioFileSize = message.AudioFileSize,
                    AudioDuration = message.AudioDuration,
                    // sanitise the message dates and get rid of the times
                    Date = message.Date.Value.Date.ToUniversalTime().Date,
                    MessageId = Guid.NewGuid().ToString(),
                    AudioUrl = message.AudioUrl,
                    PassageRef = message.PassageRef,
                    Speaker = message.Speaker,
                    Title = message.Title,
                    VideoUrl = message.VideoUrl
                });
            }

            currentMessages.AddRange(messageList);

            // readd the messages back to the object, This is important (see SO for  Deep Copy vs shallow copy)
            series.Messages = currentMessages;

            // find and replace the one with the updated object
            var updateResponse = await _sermonsRepository.UpdateSermonSeries(series);
            if(updateResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, updateResponse.ErrorMessage);
            }

            // Save data in cache.
            _cache.Set(string.Format(CacheKeys.GetSermonSeries, updateResponse.Result.Id), updateResponse.Result, PersistentCacheEntryOptions);

            return new SystemResponse<SermonSeries>(updateResponse.Result, "Success!");
        }

        /// <summary>
        /// Updates a sermon message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonMessage>> UpdateMessageInSermonSeries(string messageId, UpdateMessagesInSermonSeriesRequest request)
        {
            // TODO: Fix this

            var validRequest = UpdateMessagesInSermonSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonMessage>(true, validRequest.ErrorMessage);
            }

            var validGuid = Guid.TryParse(messageId, out Guid messageGuid);
            if (!validGuid)
            {
                return new SystemResponse<SermonMessage>(true, string.Format(SystemMessages.InvalidPropertyType, "messageId", "Guid"));
            }

            var messageResponse = await _sermonsRepository.GetMessageForId(messageId);
            if (messageResponse == null || messageResponse == default(SermonMessage))
            {
                return new SystemResponse<SermonMessage>(true, string.Format(SystemMessages.UnableToFindSermonMessageWithId, messageId));
            }

            return new SystemResponse<SermonMessage>(messageResponse, "Success!");
        }

        /// <summary>
        /// Schedule a livestream to occur regularly
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string ScheduleLiveStream(LiveSermonsSchedulingRequest request)
        {
            var hangfireCreationResponse = CreateLiveStreamStartHangfire(request);
            var hangfireEndingResponse = CreateLiveStreamEndHangfire(request);

            var hangfireResponse = $"Start Job ID: {hangfireCreationResponse}\nEnd Job ID: {hangfireEndingResponse}";
            return hangfireResponse;
        }

        private string CreateLiveStreamStartHangfire(LiveSermonsSchedulingRequest request)
        {
            var jobId = request.StartSchedule;

            // Upserts the recurring job data
            RecurringJob.AddOrUpdate(jobId, () =>
                GoLiveHangfire(request),
                request.StartSchedule,
                TimeZoneInfo.Local
            );

            return jobId;
        }

        private string CreateLiveStreamEndHangfire(LiveSermonsSchedulingRequest request)
        {
            var jobId = request.EndSchedule;

            // Upserts the recurring job data
            RecurringJob.AddOrUpdate(jobId, () =>
                EndLiveHangfire(request),
                request.EndSchedule,
                TimeZoneInfo.Local
            );

            return jobId;
        }

        /// <summary>
        /// Gets a sermon series for its Id
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> GetSeriesForId(string seriesId)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.NullProperty, "seriesId"));
            }

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetSermonSeries, seriesId), out SermonSeries series))
            {
                // Key not in cache, so get data.
                var seriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
                if (seriesResponse.HasErrors)
                {
                    return new SystemResponse<SermonSeries>(true, seriesResponse.ErrorMessage);
                }

                series = seriesResponse.Result;

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetSermonSeries, seriesId), series, PersistentCacheEntryOptions);
            }

            var orderedMessages = series.Messages.OrderByDescending(i => i.Date.Value);
            series.Messages = orderedMessages;

            return new SystemResponse<SermonSeries>(series, "Success!");
        }

        /// <summary>
        /// Updates a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeries>> ModifySermonSeries(string seriesId, SermonSeriesUpdateRequest request)
        {
            var validRequest = SermonSeriesUpdateRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, validRequest.ErrorMessage);
            }

            if (string.IsNullOrEmpty(seriesId))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.NullProperty, "SeriesId"));
            }

            var invalidId = ObjectId.TryParse(seriesId, out ObjectId id);
            if (!invalidId)
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.InvalidPropertyType, "SeriesId", "ObjectId"));
            }

            var getSermonSeriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
            if (getSermonSeriesResponse == null)
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.ErrorOcurredUpdatingDocumentForKey, seriesId));
            }

            var series = getSermonSeriesResponse.Result;

            // make sure that no one can update the slug to something that already exists
            // this is not allowed
            if (series.Slug != request.Slug)
            {
                // cannot change the slug -> make sure a slug is set when you create the series.
                return new SystemResponse<SermonSeries>(true, SystemMessages.UnableToModifySlugForExistingSermonSeries);
            }

            series.Name = request.Name;
            series.EndDate = request.EndDate.ToUniversalTime().Date;
            series.StartDate = request.StartDate.ToUniversalTime().Date;
            series.Thumbnail = request.Thumbnail;
            series.ArtUrl = request.ArtUrl;
            series.Slug = request.Slug;

            var updateResponse = await _sermonsRepository.UpdateSermonSeries(series);
            if (updateResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, updateResponse.ErrorMessage);
            }

            var response = updateResponse.Result;

            // Save data in cache.
            _cache.Set(string.Format(CacheKeys.GetSermonSeries, seriesId), response, PersistentCacheEntryOptions);

            return new SystemResponse<SermonSeries>(response, "Success!");
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

        public Task GoLiveHangfire(LiveSermonsSchedulingRequest request)
        {
            var jobId = request.StartSchedule;

            CrontabSchedule schedule = CrontabSchedule.Parse(request.EndSchedule);
            DateTime endTime = schedule.GetNextOccurrence(DateTime.Now);

            var liveStreamUpdate = new LiveSermonsUpdateRequest
            {
                ExpirationTime = endTime
            };

            return GoLive(liveStreamUpdate);
        }

        public async Task EndLiveHangfire(LiveSermonsSchedulingRequest request)
        {
            var liveStreamCompletedResponse = await _sermonsRepository.UpdateLiveSermonsInactive();

            return;
        }

        /// <summary>
        /// Update the LiveSermons object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<LiveStreamingResponse>> GoLive(LiveSermonsUpdateRequest request)
        {
            // validate the request
            var validRequest = LiveSermonsUpdateRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                // an error ocurred here
                return new SystemResponse<LiveStreamingResponse>(true, validRequest.ErrorMessage);
            }

            var updateLiveSermonsResponse = await _sermonsRepository.GoLive(request);
            if (updateLiveSermonsResponse == null)
            {
                return new SystemResponse<LiveStreamingResponse>(true, SystemMessages.UnableToFindLiveSermon);
            }

            // times have already been converted to UTC
            var response = new LiveStreamingResponse
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime,
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = updateLiveSermonsResponse.SpecialEventTimes != null ? true : false,
                SpecialEventTimes = updateLiveSermonsResponse.SpecialEventTimes ?? null
            };

            return new SystemResponse<LiveStreamingResponse>(response, "Success!");
        }

        /// <summary>
        /// Updates the LiveSermon to be a special event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<LiveStreamingResponse>> UpdateLiveForSpecialEvents(LiveSermonsSpecialEventUpdateRequest request)
        {
            // validate the request
            var validationResponse = request.ValidateRequest();
            if (validationResponse.HasErrors)
            {
                return new SystemResponse<LiveStreamingResponse>(true, validationResponse.ErrorMessage);
            }
            
            // Update this object for the requested fields
            var updated = new LiveSermons
            {
                ExpirationTime = request.SpecialEventTimes.End ?? new DateTime(1990, 01, 01, 11, 20, 0, 0),
                IsLive = true,
                LastUpdated = DateTime.UtcNow,
                SpecialEventTimes = request.SpecialEventTimes
            };

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(updated);
            if (updateLiveSermonsResponse.HasErrors)
            {
                return new SystemResponse<LiveStreamingResponse>(true, updateLiveSermonsResponse.ErrorMessage);
            }

            var liveResponse = updateLiveSermonsResponse.Result;

            var response = new LiveStreamingResponse
            {
                ExpirationTime = liveResponse.ExpirationTime.ToUniversalTime(),
                IsLive = liveResponse.IsLive,
                IsSpecialEvent = true,
                SpecialEventTimes = request.SpecialEventTimes
            };

            // we are updating this so we should watch for when it expires, when it does we will need to update Mongo
            DetermineIfStreamIsInactive();

            return new SystemResponse<LiveStreamingResponse>(response, "Success!");
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
                    Dispose();
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
            // we'll look every 15 seconds to see if the stream has expired
            _timer = new Timer(CheckStreamingStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
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
                Dispose();
            }
        }

        /// <summary>
        /// Reset the LiveSermons object back to it's origional state & stop async timer
        /// </summary>
        /// <returns></returns>
        public async Task<SystemResponse<LiveSermons>> UpdateLiveSermonsInactive()
        {
            var liveStreamCompletedResponse = await _sermonsRepository.UpdateLiveSermonsInactive();

            // we want to stop all async tasks because this will for sure lead to a memory leak
            if (_timer != null)
            {
                Dispose();
            }

            return liveStreamCompletedResponse;
        }

        public void Dispose()
        {
            // if the timer has been initialized, dispose of it
            if (_timer != null)
            {
                _timer.Dispose();
            }
        }
    }

    /// <summary>
    /// Globally used Caching keys for O(1) lookups
    /// </summary>
    public static class CacheKeys
    {
        public static string GetSermons { get { return "LiveSermonsCache"; } }

        public static string GetPagedSermons { get { return "PagedSermonsCache:{0}"; } }

        public static string GetSermonSeries { get { return "SermonSeriesCache:{0}"; } }

        public static string GetConfig { get { return "SystemConfiguration:{0}"; } }
    }
}
