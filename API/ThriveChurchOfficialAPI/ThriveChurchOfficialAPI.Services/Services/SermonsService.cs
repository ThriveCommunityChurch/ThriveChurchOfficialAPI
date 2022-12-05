using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using Hangfire;
using NCrontab;
using Hangfire.Storage;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService, IDisposable
    {
        private readonly ISermonsRepository _sermonsRepository;
        private readonly IMessagesRepository _messagesRepository;
        private readonly IMemoryCache _cache;
        private Timer _timer;

        /// <summary>
        /// Sermons Service
        /// </summary>
        /// <param name="sermonsRepo"></param>
        /// <param name="messagesRepository"></param>
        /// <param name="cache"></param>
        public SermonsService(ISermonsRepository sermonsRepo, 
            IMessagesRepository messagesRepository,
            IMemoryCache cache)
        {
            // init the repo with the connection string via DI
            _sermonsRepository = sermonsRepo;
            _messagesRepository = messagesRepository;
            _cache = cache;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<SystemResponse<AllSermonsSummaryResponse>> GetAllSermons()
        {
            var getAllSermonsTask = _sermonsRepository.GetAllSermons();
            var getAllMessagesTask = _messagesRepository.GetAllMessages();

            await Task.WhenAll(getAllSermonsTask, getAllMessagesTask);

            var getAllMessagesResponse = getAllMessagesTask.Result;
            var getAllSermonsResponse = getAllSermonsTask.Result;

            Dictionary<string, int> messageCountBySeries = getAllMessagesResponse.GroupBy(i => i.SeriesId).ToDictionary(Key => Key.Key, Value => Value.Count());

            // we need to convert everything to the right response pattern
            var responseList = new List<AllSermonSeriesSummary>();
            foreach (var series in getAllSermonsResponse)
            {
                // for each one add only the properties we want to the list
                var elemToAdd = new AllSermonSeriesSummary
                {
                    ArtUrl = series.ArtUrl,
                    Id = series.Id,
                    StartDate = series.StartDate,
                    Title = series.Name,
                    MessageCount = messageCountBySeries.ContainsKey(series.Id) ? messageCountBySeries[series.Id] : 0,
                    EndDate = series.EndDate
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
        public async Task<SystemResponse<SermonSeriesResponse>> CreateNewSermonSeries(CreateSermonSeriesRequest request)
        {
            var validRequest = CreateSermonSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, validRequest.ErrorMessage);
            }

            // the Slug on the series should be unique, so if we already have one with this slug
            // return an error - because we want to avoid having bad data in our database
            var sermonsWithSlug = await _sermonsRepository.GetSermonsBySlug(request.Slug);

            if (sermonsWithSlug.Any())
            {
                SermonSeries foundSeries = sermonsWithSlug.FirstOrDefault();

                var found = new SermonSeriesResponse
                {
                    ArtUrl = foundSeries.ArtUrl,
                    LastUpdated = foundSeries.LastUpdated,
                    EndDate = foundSeries.EndDate,
                    Id = foundSeries.Id,
                    Name = foundSeries.Name,
                    Slug = foundSeries.Slug,
                    StartDate = foundSeries.StartDate,
                    Thumbnail = foundSeries.Thumbnail,
                    Year = $"{foundSeries.StartDate.Year}"
                };

                var messages = await _messagesRepository.GetMessagesBySeriesId(foundSeries.Id);

                // there is already a sermon series with this slug, respond with one of those
                return new SystemResponse<SermonSeriesResponse>(found, "202");
            }

            // Check if any of the sermon series' are currently active
            // Because we can only have 1 active series
            if (request.EndDate == null)
            {
                var activeSeries = await _sermonsRepository.GetActiveSeries();

                // if there's a value then there's an active series
                if (activeSeries != null)
                {
                    // one of the series' is already active
                    var currentlyActive = new SermonSeriesResponse
                    {
                        ArtUrl = activeSeries.ArtUrl,
                        EndDate = activeSeries.EndDate,
                        Id = activeSeries.Id,
                        LastUpdated = activeSeries.LastUpdated,
                        Name = activeSeries.Name,
                        Slug = activeSeries.Slug,
                        StartDate = activeSeries.StartDate,
                        Thumbnail = activeSeries.Thumbnail,
                        Year = $"{activeSeries.StartDate.Year}"
                    };

                    var messages = await _messagesRepository.GetMessagesBySeriesId(activeSeries.Id);
                    if (messages != null && messages.Any())
                    {
                        currentlyActive.Messages = SermonMessage.ConvertToResponseList(messages);
                    }

                    return new SystemResponse<SermonSeriesResponse>(currentlyActive, "202");
                }
            }
            else
            {
                request.EndDate = request.EndDate.Value.ToUniversalTime().Date;
            }

            // sanitise the start dates
            request.StartDate = request.StartDate.ToUniversalTime().Date;

            var sermonSeries = new SermonSeries
            {
                ArtUrl = request.ArtUrl,
                EndDate = request.EndDate,
                Name = request.Name,
                Slug = request.Slug,
                StartDate = request.StartDate,
                Thumbnail = request.Thumbnail
            };

            var seriesCreatedResponse = await _sermonsRepository.CreateNewSermonSeries(sermonSeries);
            if (seriesCreatedResponse.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, seriesCreatedResponse.ErrorMessage); 
            }

            var messageList = new List<SermonMessage>();
            var createdSeries = seriesCreatedResponse.Result;

            foreach (var message in request.Messages)
            {
                messageList.Add(new SermonMessage
                {
                    AudioFileSize = message.AudioFileSize,
                    AudioDuration = message.AudioDuration,
                    // sanitise the message dates and get rid of the times
                    Date = message.Date.Date.ToUniversalTime().Date,
                    AudioUrl = message.AudioUrl,
                    PassageRef = message.PassageRef,
                    Speaker = message.Speaker,
                    Title = message.Title,
                    VideoUrl = message.VideoUrl,
                    SeriesId = createdSeries.Id
                });
            }

            var newMessagesResponse = await _messagesRepository.CreateNewMessages(messageList);
            if (newMessagesResponse.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, newMessagesResponse.ErrorMessage);
            }

            var response = new SermonSeriesResponse
            {
                ArtUrl = createdSeries.ArtUrl,
                EndDate = createdSeries.EndDate,
                Id = createdSeries.Id,
                LastUpdated = createdSeries.LastUpdated,
                Messages = SermonMessage.ConvertToResponseList(newMessagesResponse.Result),
                Name = createdSeries.Name,
                Slug = createdSeries.Slug,
                StartDate = createdSeries.StartDate,
                Thumbnail = createdSeries.Thumbnail,
                Year = $"{createdSeries.StartDate.Year}"
            };

            // Save data in cache.
            _cache.Set(string.Format(CacheKeys.GetSermonSeries, response.Id), response, PersistentCacheEntryOptions);

            return new SystemResponse<SermonSeriesResponse>(response, "Success!");
        }

        private async Task<IEnumerable<SermonMessage>> GetMessagesBySeriesId(string seriesId)
        {
            var messages = await _messagesRepository.GetMessagesBySeriesId(seriesId);

            return messages;
        }

        /// <summary>
        /// Adds a new spoken message to a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeriesResponse>> AddMessageToSermonSeries(string seriesId, AddMessagesToSeriesRequest request)
        {
            var validRequest = AddMessagesToSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, validRequest.ErrorMessage);
            }

            var getSermonSeriesDelegate = GetSeriesById(seriesId);
            var messagesDelegate = _messagesRepository.GetMessagesBySeriesId(seriesId);

            await Task.WhenAll(getSermonSeriesDelegate, messagesDelegate);

            var getSermonSeriesResponse = getSermonSeriesDelegate.Result;
            var messages = messagesDelegate.Result.ToList();

            if (getSermonSeriesResponse.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, getSermonSeriesResponse.ErrorMessage);
            }

            SermonSeries series = getSermonSeriesResponse.Result;

            // add the sermon message to mongo as 1 list
            var newMessages = new List<SermonMessage>();

            foreach (var message in request.MessagesToAdd)
            {
                newMessages.Add(new SermonMessage
                {
                    AudioFileSize = message.AudioFileSize,
                    AudioDuration = message.AudioDuration,
                    // sanitise the message dates and get rid of the times
                    Date = message.Date.Date.ToUniversalTime().Date,
                    AudioUrl = message.AudioUrl,
                    PassageRef = message.PassageRef,
                    Speaker = message.Speaker,
                    Title = message.Title,
                    VideoUrl = message.VideoUrl,
                    SeriesId = seriesId
                });
            }

            var updateResponse = await _messagesRepository.CreateNewMessages(newMessages);
            if (updateResponse.HasErrors)
            {
                return new SystemResponse<SermonSeriesResponse>(true, updateResponse.ErrorMessage);
            }

            messages.AddRange(newMessages);

            var response = new SermonSeriesResponse
            {
                Id = series.Id,
                ArtUrl = series.ArtUrl,
                EndDate = series.EndDate,
                LastUpdated = series.LastUpdated,
                Messages = SermonMessage.ConvertToResponseList(messages.OrderByDescending(i => i.Date)),
                Name = series.Name,
                Slug = series.Slug,
                StartDate = series.StartDate,
                Thumbnail = series.Thumbnail,
                Year = $"{series.StartDate.Year}"
            };

            // Save data in cache.
            _cache.Set(string.Format(CacheKeys.GetSermonSeries, series.Id), response, PersistentCacheEntryOptions);

            return new SystemResponse<SermonSeriesResponse>(response, "Success!");
        }

        /// <summary>
        /// Updates a sermon message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonMessage>> UpdateMessageInSermonSeries(string messageId, UpdateMessagesInSermonSeriesRequest request)
        {
            var validRequest = UpdateMessagesInSermonSeriesRequest.ValidateRequest(request);
            if (validRequest.HasErrors)
            {
                return new SystemResponse<SermonMessage>(true, validRequest.ErrorMessage);
            }

            var messageResponse = await _messagesRepository.GetMessageById(messageId);
            if (messageResponse.HasErrors)
            {
                return new SystemResponse<SermonMessage>(true, messageResponse.ErrorMessage);
            }

            var messageResult = await _messagesRepository.UpdateMessageById(messageId, request.Message);

            return new SystemResponse<SermonMessage>(messageResult, "Success!");
        }

        /// <summary>
        /// Gets a sermon series for its Id
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonSeriesResponse>> GetSeriesForId(string seriesId)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                return new SystemResponse<SermonSeriesResponse>(true, string.Format(SystemMessages.NullProperty, "seriesId"));
            }

            // check the cache first -> if there's a value there grab it
            if (!_cache.TryGetValue(string.Format(CacheKeys.GetSermonSeries, seriesId), out SermonSeriesResponse series))
            {
                // Key not in cache, so get data.
                var seriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
                if (seriesResponse.HasErrors)
                {
                    return new SystemResponse<SermonSeriesResponse>(true, seriesResponse.ErrorMessage);
                }

                var seriesResult = seriesResponse.Result;

                var aeriesResponse = new SermonSeriesResponse
                {
                    ArtUrl = seriesResult.ArtUrl,
                    EndDate = seriesResult.EndDate,
                    Id = seriesResult.Id,
                    LastUpdated = seriesResult.LastUpdated,
                    Name = seriesResult.Name,
                    Slug = seriesResult.Slug,
                    StartDate = seriesResult.StartDate,
                    Thumbnail = seriesResult.Thumbnail,
                    Year = $"{seriesResult.StartDate.Year}"
                };

                var messagesResponse = await _messagesRepository.GetMessagesBySeriesId(seriesId);
                if (messagesResponse != null && messagesResponse.Any())
                {
                    aeriesResponse.Messages = SermonMessage.ConvertToResponseList(messagesResponse);
                }

                // Save data in cache.
                _cache.Set(string.Format(CacheKeys.GetSermonSeries, seriesId), aeriesResponse, PersistentCacheEntryOptions);
                return new SystemResponse<SermonSeriesResponse>(aeriesResponse, "Success!");
            }         

            return new SystemResponse<SermonSeriesResponse>(series, "Success!");
        }

        private async Task<SystemResponse<SermonSeries>> GetSeriesById(string seriesId)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.NullProperty, "SeriesId"));
            }

            var invalidId = ObjectId.TryParse(seriesId, out ObjectId _);
            if (!invalidId)
            {
                return new SystemResponse<SermonSeries>(true, string.Format(SystemMessages.InvalidPropertyType, "SeriesId", "ObjectId"));
            }

            var seriesResponse = await _sermonsRepository.GetSermonSeriesForId(seriesId);
            if (seriesResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, seriesResponse.ErrorMessage);
            }

            return new SystemResponse<SermonSeries>(seriesResponse.Result, "Success!");
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

            var getSermonSeriesResponse = await GetSeriesById(seriesId);
            if (getSermonSeriesResponse.HasErrors)
            {
                return new SystemResponse<SermonSeries>(true, getSermonSeriesResponse.ErrorMessage);
            }

            SermonSeries series = getSermonSeriesResponse.Result;

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

        #region Live

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
            else
            {
                response.NextLive = getLiveSermonsResponse.NextLive;
            }

            return response;
        }

        public Task GoLiveHangfire(LiveSermonsSchedulingRequest request)
        {
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
            CrontabSchedule schedule = CrontabSchedule.Parse(request.StartSchedule);
            DateTime nextLocal = schedule.GetNextOccurrence(DateTime.Now);

            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var nextJobExecTime = recurringJobs.OrderBy(i => i.NextExecution.Value).First().NextExecution.Value;

            // make sure that we're using UTC
            DateTime nextLive = nextJobExecTime.ToUniversalTime();

            var liveStreamCompletedResponse = await _sermonsRepository.UpdateLiveSermonsInactive(nextLive);

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
                return new SystemResponse<LiveStreamingResponse>(true, string.Join(SystemMessages.UnableToFind, "LiveSermon"));
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

        #endregion

        /// <summary>
        /// Updates the playcount for a sermon message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonMessage>> MarkMessagePlayed(string messageId)
        {
            var updateResponse = await _messagesRepository.UpdateMessagePlayCount(messageId);
            if (updateResponse == null)
            {
                return new SystemResponse<SermonMessage>(true, string.Format(SystemMessages.UnableToFindPropertyForId, "message", messageId));
            }

            return new SystemResponse<SermonMessage>(updateResponse, "Success!");
        }

        /// <summary>
        /// Gets sermon statistics
        /// </summary>
        /// <returns></returns>
        public async Task<SystemResponse<SermonStatsResponse>> GetSermonStats()
        {
            var allSermonsDelegate = _sermonsRepository.GetAllSermons(sorted: false);
            var allMessagesDelegate = _messagesRepository.GetAllMessages();

            await Task.WhenAll(allMessagesDelegate, allSermonsDelegate);

            var allSermonsResponse = allSermonsDelegate.Result;
            var allMessagesResponse = allMessagesDelegate.Result;

            if (allSermonsResponse == null || allSermonsResponse == null || !allSermonsResponse.Any())
            {
                return new SystemResponse<SermonStatsResponse>(true, string.Join(SystemMessages.UnableToFind, "all sermons"));
            }

            if (allMessagesResponse == null || allMessagesResponse == null || !allMessagesResponse.Any())
            {
                return new SystemResponse<SermonStatsResponse>(true, string.Join(SystemMessages.UnableToFind, "all sermons"));
            }

            var seriesList = allSermonsResponse.ToList();

            var response = new SermonStatsResponse
            {
                TotalSeriesNum = seriesList.Count
            };

            Dictionary<string, SpeakerStats> speakerStats = new Dictionary<string, SpeakerStats>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, double> speakerLength = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> seriesLength = new Dictionary<string, int>();
            SermonMessage longestMessage = null;

            foreach (var seriesItem in seriesList)
            {
                seriesLength.Add(seriesItem.Id, 0);
            }

            // iterate through each message and calculate what we need to
            // this should be O(n*m) where n = # of series and m = # messages each series 
            foreach (SermonMessage message in allMessagesResponse)
            {
                response.TotalMessageNum++;
                response.TotalAudioLength += message.AudioDuration ?? 0;
                response.TotalFileSize += message.AudioFileSize ?? 0;

                seriesLength[message.SeriesId]++;

                // we haven't seen this speaker yet, so add them to the list
                if (!speakerStats.ContainsKey(message.Speaker))
                {
                    speakerStats[message.Speaker] = new SpeakerStats
                    {
                        MessageCount = 1
                    };
                }
                else
                {
                    // we've seen this speaker, so just increment
                    speakerStats[message.Speaker].MessageCount++;
                }

                // if there's a duration then we can track that
                if (message.AudioDuration != null && message.AudioDuration > 0 && !speakerLength.ContainsKey(message.Speaker))
                {
                    // assuming there's no long message (default) let's just set it to the current one
                    if (longestMessage == null)
                    {
                        longestMessage = message;
                    }
                    // If this message is longer than the current one selected then let's change it
                    else if (message.AudioDuration > longestMessage.AudioDuration)
                    {
                        longestMessage = message;
                    }

                    // assuming there's no speaker yet, we can init them with the current duration
                    speakerLength[message.Speaker] = (double)message.AudioDuration;
                }
                else
                {
                    // just append to what we have otherwise, but if its null - we just add nothing
                    speakerLength[message.Speaker] += message.AudioDuration ?? 0;
                }
            }

            var finalSpeakerList = new List<SpeakerStats>();

            // now we handle the speakers
            foreach (var speaker in speakerStats)
            {
                double duration = 0.0;

                if (speakerLength.ContainsKey(speaker.Key))
                {
                    duration = speakerLength[speaker.Key];
                }

                finalSpeakerList.Add(new SpeakerStats
                {
                    AvgLength = duration / speaker.Value.MessageCount,
                    MessageCount = speaker.Value.MessageCount,
                    Name = speaker.Key 
                });
            }

            // get all the averages now that we've iterated over everything
            response.AvgMessagesPerSeries = (double)response.TotalMessageNum / response.TotalSeriesNum;
            response.AvgAudioLength = response.TotalAudioLength / response.TotalMessageNum;
            response.AvgFileSize = response.TotalFileSize / response.TotalMessageNum;
            response.SpeakerStats = finalSpeakerList.OrderByDescending(i => i.MessageCount);
            SermonSeries longestSeries = seriesLength.Any() ? seriesList.First(i => i.Id == seriesLength.OrderByDescending(j => j.Value).First().Key) : null;

            return new SystemResponse<SermonStatsResponse>(response, "Success!");
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