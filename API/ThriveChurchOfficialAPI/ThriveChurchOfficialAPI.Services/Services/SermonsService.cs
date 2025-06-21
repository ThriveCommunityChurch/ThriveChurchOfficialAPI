using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService, IDisposable
    {
        private readonly ISermonsRepository _sermonsRepository;
        private readonly IMessagesRepository _messagesRepository;
        private readonly IMemoryCache _cache;
        private readonly IS3Repository _s3Repository;
        private Timer _timer;

        CultureInfo culture = new CultureInfo("en-US");

        /// <summary>
        /// Sermons Service
        /// </summary>
        /// <param name="sermonsRepo"></param>
        /// <param name="messagesRepository"></param>
        /// <param name="cache"></param>
        /// <param name="s3Repository"></param>
        public SermonsService(ISermonsRepository sermonsRepo,
            IMessagesRepository messagesRepository,
            IMemoryCache cache,
            IS3Repository s3Repository)
        {
            // init the repo with the connection string via DI
            _sermonsRepository = sermonsRepo;
            _messagesRepository = messagesRepository;
            _cache = cache;
            _s3Repository = s3Repository;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<SystemResponse<AllSermonsSummaryResponse>> GetAllSermons(bool highResImg = false)
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
                    ArtUrl = highResImg ? series.ArtUrl : series.Thumbnail,
                    Id = series.Id,
                    StartDate = series.StartDate,
                    Title = series.Name,
                    MessageCount = messageCountBySeries.ContainsKey(series.Id) ? messageCountBySeries[series.Id] : 0,
                    EndDate = series.EndDate,
                    LastUpdated = series.LastUpdated
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
        /// <param name="highResImg"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber, bool highResImg = false)
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
                pagedSermonsResponse = await _sermonsRepository.GetPagedSermons(pageNumber, highResImg);

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

            // now that this message was added let's make sure we capture that there was a change to the series
            await _sermonsRepository.UpdateSeriesLastUpdated(seriesId);

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

            // clear cache when update was successful
            _cache.Remove(string.Format(CacheKeys.GetSermonSeries, messageResult.SeriesId));

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
                response.IsSpecialEvent = getLiveSermonsResponse.SpecialEventTimes != null;
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
                IsSpecialEvent = updateLiveSermonsResponse.SpecialEventTimes != null,
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
        /// Returns a series of data for display in a chart
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="chartType"></param>
        /// <param name="displayType"></param>
        /// <returns></returns>
        public async Task<SystemResponse<SermonStatsChartResponse>> GetSermonsStatsChartData(DateTime? startDate, DateTime? endDate, StatsChartType chartType, StatsAggregateDisplayType displayType)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                // this case is both have values and start is after end
                return new SystemResponse<SermonStatsChartResponse>(true, SystemMessages.EndDateMustBeAfterStartDate);
            }
            else if ((startDate.HasValue && !endDate.HasValue) || (!startDate.HasValue && endDate.HasValue))
            {
                // this case is one has a value and the other doesn't
                return new SystemResponse<SermonStatsChartResponse>(true, SystemMessages.StartDateAndEnddateMustBothHaveValues);
            }

            var response = new SermonStatsChartResponse();

            switch (chartType)
            {
                case StatsChartType.AudioDuration:
                    response = await GenerateAudioDurationData(startDate, endDate, displayType);
                    break;

                case StatsChartType.TotAudioFileSize:
                    response = await GenerateAggregateDataForProperty<SermonMessage>(startDate, endDate, nameof(SermonMessage.AudioFileSize), displayType);
                    break;

                case StatsChartType.TotAudioDuration:
                    response = await GenerateAggregateDataForProperty<SermonMessage>(startDate, endDate, nameof(SermonMessage.AudioDuration), displayType);
                    break;

                default:
                    break;
            }

            return new SystemResponse<SermonStatsChartResponse>(response, "Success!");
        }

        /// <summary>
        /// Values generated are averages for each display type between the requested dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="displayType"></param>
        /// <returns></returns>
        private async Task<SermonStatsChartResponse> GenerateAggregateDataForProperty<T>(DateTime? startDate, DateTime? endDate, string property, StatsAggregateDisplayType displayType)
        {
            /*
             * IMPORTANT NOTE:
             * Overall, both requested dates act as INCLUSIVE in the graph...
             * 
             * This method is also dynamic, meaning you can request nearly any property and it will just work.
             * 
             * This means that data at the start of the requested period, will contain an aggregation of all the messages prior to that start or end point.
             * If a message happens on that date, we're going to include it in the results.
             * 
             * Here's an example.
             * 
             * We have 10 messages that occurred "before" relative to the requested range. 
             * The last message occurs on the same date as is in the request for the start range.
             *
             * So, the way this would actually work is:
             *     1) We include that first data item in the graph, even though it happened on the same day
             *     2) Every message within that range, 5 items lets say would add up to the 150Mb total
             *     3) In the end the graph will display 100Mb -> 150Mb where each message bumps the aggreate by 10Mb since that's its size.
             *  
             *  Hope that makes this make much more sense.
             *  This is probably going to look a little confusing.
             * 
             */ 

            var response = new SermonStatsChartResponse();
            var dataCollection = new List<SermonStatsChartData>();

            // If we get all the data and we can evaluate it all at once
            var messages = await _messagesRepository.GetMessageByDateRange();

            List<SermonMessage> messagesToEvaluate = new List<SermonMessage>();

            double? rollingTotal = null;
            if (startDate.HasValue)
            {
                double? previousSize = null;

                // it's better to do 1 loop and do all the logic we need in here rather than doing it in 3 LINQ querries
                foreach (var message in messages)
                {
                    if (message.Date < startDate)
                    {
                        previousSize = CalculateRollingTotal(previousSize, GetPropertyValue(message, property));
                    }
                    else if (message.Date >= startDate && message.Date <= endDate)
                    {
                        messagesToEvaluate.Add(message);
                    }
                }

                // Anything that happened BEFORE the requested date range, we're using that as our placeholder point. The graph won't actually start here.
                rollingTotal = previousSize == 0.0 ? null : previousSize;
            }
            else
            {
                // we're just using the full range
                messagesToEvaluate = messages.ToList();
            }

            if (messagesToEvaluate.Any())
            {
                switch (displayType)
                {
                    case StatsAggregateDisplayType.Daily:

                        foreach (var message in messagesToEvaluate.OrderBy(i => i.Date)) 
                        {
                            // since we're showing an aggregated total, we need to append the current file to that
                            rollingTotal = CalculateRollingTotal(rollingTotal, GetPropertyValue(message, property));

                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = message.Date.Value,
                                Value = rollingTotal
                            });
                        }

                        response.Data = dataCollection;
                        break;

                    case StatsAggregateDisplayType.Weekly:
                        var weeklyData = messagesToEvaluate.GroupBy(i => new { i.Date.Value.Year, Week = GetWeekOfYear(i.Date.Value) });

                        foreach (var weekPerYear in weeklyData.OrderBy(i => i.Key.Year).ThenBy(i => i.Key.Week))
                        {
                            DateTime weekOf = FirstDateOfWeek(weekPerYear.Key.Year, weekPerYear.Key.Week);

                            // grab the total for the whole week
                            foreach (var message in weekPerYear)
                            {
                                // since we're showing an aggregated total, we need to append the current file to that
                                rollingTotal = CalculateRollingTotal(rollingTotal, GetPropertyValue(message, property));
                            }

                            // append the new data for the week rather than each single message
                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = weekOf,
                                Value = rollingTotal
                            });
                        }

                        response.Data = dataCollection;
                        break;

                    case StatsAggregateDisplayType.Monthly:
                        var monthlyData = messagesToEvaluate.GroupBy(i => new { i.Date.Value.Year, i.Date.Value.Month });

                        foreach (var monthPerYear in monthlyData.OrderBy(i => i.Key.Year).ThenBy(i => i.Key.Month))
                        {
                            DateTime monthOf = new DateTime(monthPerYear.Key.Year, monthPerYear.Key.Month, 1);

                            // grab the total for the whole month
                            foreach (var message in monthPerYear)
                            {
                                // since we're showing an aggregated total, we need to append the current file to that
                                rollingTotal = CalculateRollingTotal(rollingTotal, GetPropertyValue(message, property));
                            }

                            // append the new data for the month rather than each single message
                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = monthOf,
                                Value = rollingTotal
                            });
                        }

                        response.Data = dataCollection;
                        break;

                    case StatsAggregateDisplayType.Yearly:
                        var yearlyData = messagesToEvaluate.GroupBy(i => new { i.Date.Value.Year });

                        foreach (var messagesPerYear in yearlyData.OrderBy(i => i.Key.Year))
                        {
                            DateTime yearOf = new DateTime(messagesPerYear.Key.Year, 1, 1);

                            // grab the total for the whole year
                            foreach (var message in messagesPerYear)
                            {
                                // since we're showing an aggregated total, we need to append the current file to that
                                rollingTotal = CalculateRollingTotal(rollingTotal, GetPropertyValue(message, property));
                            }

                            // append the new data for the year rather than each single message
                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = yearOf,
                                Value = rollingTotal
                            });
                        }

                        response.Data = dataCollection;
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Get the value for the requested property at runtime.
        /// We're using dynamic here because the type could be anything.
        /// 
        /// It's up to the caller to know what type the response should be.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static dynamic GetPropertyValue<T>(T sourceObject, string propertyName)
        {
            Type type = sourceObject.GetType();

            // Very important to use binding flags here, because otherwise we won't be able to find the property we're looking for.
            // This can be an issue for some properties. Since we're doing Public, we also need the other 2.
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            return property.GetValue(sourceObject, null);
        }


        /// <summary>
        /// Calculates a rolling total based on the new value passed
        /// </summary>
        /// <param name="total"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private double? CalculateRollingTotal(double? total, double? newValue)
        {
            if (!total.HasValue)
            {
                total = newValue;
            }
            else
            {
                // since we're showing an aggregated total, we need to append the current file to that
                total += newValue ?? 0.0;
            }

            return total;
        }

        /// <summary>
        /// Values generated are averages for each display type between the requested dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="displayType"></param>
        /// <returns></returns>
        private async Task<SermonStatsChartResponse> GenerateAudioDurationData(DateTime? startDate, DateTime? endDate, StatsAggregateDisplayType displayType)
        {
            var response = new SermonStatsChartResponse();
            var dataCollection = new List<SermonStatsChartData>();

            var messages = await _messagesRepository.GetMessageByDateRange(startDate, endDate);
            if (messages.Any())
            {
                switch(displayType)
                {
                    case StatsAggregateDisplayType.Daily:
                        response.Data = messages.Select(i => new SermonStatsChartData 
                        { 
                            Date = i.Date.Value, 
                            Value = i.AudioDuration
                        }).OrderBy(i => i.Date);
                        break;

                    case StatsAggregateDisplayType.Weekly:
                        var weeklyData = messages.GroupBy(i => new { i.Date.Value.Year, Week = GetWeekOfYear(i.Date.Value) });
                        foreach (var weekPerYear in weeklyData)
                        {
                            DateTime weekOf = FirstDateOfWeek(weekPerYear.Key.Year, weekPerYear.Key.Week);
                            int countForWeek = 0;
                            double? totDuration = null;

                            // need to calculate the averages
                            foreach (var messageThisWeek in weekPerYear)
                            {
                                if (messageThisWeek.AudioDuration.HasValue)
                                {
                                    // the average only counts if there's a value
                                    countForWeek++;

                                    if (totDuration == null)
                                    {
                                        totDuration = messageThisWeek.AudioDuration;
                                    }
                                    else
                                    {
                                        totDuration += messageThisWeek.AudioDuration;
                                    }
                                }
                            }

                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = weekOf,
                                Value = totDuration.HasValue ? totDuration.Value / countForWeek : null
                            });
                        }
                        response.Data = dataCollection.OrderBy(i => i.Date); ;
                        break;

                    case StatsAggregateDisplayType.Monthly:
                        var monthlyData = messages.GroupBy(i => new { i.Date.Value.Year, i.Date.Value.Month });
                        foreach (var monthPerYear in monthlyData)
                        {
                            DateTime monthOf = new DateTime(monthPerYear.Key.Year, monthPerYear.Key.Month, 1);
                            int countForMonth = 0;
                            double? totDuration = null;

                            // need to calculate the averages
                            foreach (var messageThisMonth in monthPerYear)
                            {
                                if (messageThisMonth.AudioDuration.HasValue)
                                {
                                    // the average only counts if there's a value
                                    countForMonth++;

                                    if (totDuration == null)
                                    {
                                        totDuration = messageThisMonth.AudioDuration;
                                    }
                                    else
                                    {
                                        totDuration += messageThisMonth.AudioDuration;
                                    }
                                }
                            }

                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = monthOf,
                                Value = totDuration.HasValue ? totDuration.Value / countForMonth : null
                            });
                        }
                        response.Data = dataCollection.OrderBy(i => i.Date); ;
                        break;

                    case StatsAggregateDisplayType.Yearly:
                        var yearlyData = messages.GroupBy(i => new { i.Date.Value.Year});
                        foreach (var messagesPerYear in yearlyData)
                        {
                            DateTime yearOf = new DateTime(messagesPerYear.Key.Year, 1, 1);
                            int countForYear = 0;
                            double? totDuration = null;

                            // need to calculate the averages
                            foreach (var messageThisYear in messagesPerYear)
                            {
                                if (messageThisYear.AudioDuration.HasValue)
                                {
                                    // the average only counts if there's a value
                                    countForYear++;

                                    if (totDuration == null)
                                    {
                                        totDuration = messageThisYear.AudioDuration;
                                    }
                                    else
                                    {
                                        totDuration += messageThisYear.AudioDuration;
                                    }
                                }
                            }

                            dataCollection.Add(new SermonStatsChartData
                            {
                                Date = yearOf,
                                Value = totDuration.HasValue ? totDuration.Value / countForYear : null
                            });
                        }
                        response.Data = dataCollection.OrderBy(i => i.Date);
                        break;
                }
            }

            return response;
        }

        private DateTime FirstDateOfWeek(int year, int weekNum)
        {
            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Sunday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);

            Calendar calendar = culture.Calendar;
            int firstWeek = calendar.GetWeekOfYear(firstMonday, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstMonday.AddDays(weekNum * 7);

            return result;
        }

        /// <summary>
        /// Returns the week that the date is on
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private int GetWeekOfYear(DateTime date)
        {
            // Gets the Calendar instance associated with a CultureInfo.
            Calendar calendar = culture.Calendar;

            int week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            return week;
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
            Dictionary<string, SermonSeries> seriesById = new Dictionary<string, SermonSeries>();
            SermonMessageSummary longestMessage = null;

            foreach (var seriesItem in seriesList)
            {
                seriesLength.Add(seriesItem.Id, 0);
                seriesById[seriesItem.Id] = seriesItem;
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
                if (message.AudioDuration != null && message.AudioDuration > 0)
                {
                    // assuming there's no long message (default) let's just set it to the current one
                    if (longestMessage == null)
                    {
                        longestMessage = new SermonMessageSummary
                        {
                            AudioDuration = message.AudioDuration.Value,
                            Date = message.Date.Value,
                            Speaker = message.Speaker,
                            Title = message.Title,
                            SeriesArt = seriesById[message.SeriesId].Thumbnail,
                            SeriesName = seriesById[message.SeriesId].Name
                        };
                    }
                    // If this message is longer than the current one selected then let's change it
                    else if (message.AudioDuration > longestMessage.AudioDuration)
                    {
                        longestMessage = new SermonMessageSummary
                        {
                            AudioDuration = message.AudioDuration.Value,
                            Date = message.Date.Value,
                            Speaker = message.Speaker,
                            Title = message.Title,
                            SeriesArt = seriesById[message.SeriesId].Thumbnail,
                            SeriesName = seriesById[message.SeriesId].Name
                        };
                    }

                    if (!speakerLength.ContainsKey(message.Speaker))
                    {
                        // assuming there's no speaker yet, we can init them with the current duration
                        speakerLength[message.Speaker] = message.AudioDuration.Value;
                    }
                    else
                    {
                        // just append to what we have otherwise, but if its null - we just add nothing
                        speakerLength[message.Speaker] += message.AudioDuration.Value;
                    }
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

            if (longestSeries != null)
            {
                response.LongestSeries = new LongestSermonSeriesSummary
                {
                    ArtUrl = longestSeries.Thumbnail,
                    Id = longestSeries.Id,
                    SeriesLength = seriesLength[longestSeries.Id],
                    StartDate = longestSeries.StartDate,
                    Title = longestSeries.Name
                };
            }

            if (longestMessage != null)
            {
                response.LongestMessage = longestMessage;
            }

            return new SystemResponse<SermonStatsResponse>(response, "Success!");
        }

        /// <summary>
        /// Uploads an audio file to S3 and returns the public URL
        /// </summary>
        /// <param name="request">The file stream to upload</param>
        /// <returns>SystemResponse containing the S3 URL or error message</returns>
        public async Task<SystemResponse<string>> UploadAudioFileAsync(HttpRequest request)
        {
            try
            {
                return await _s3Repository.UploadAudioFileAsync(request);
            }
            catch (Exception ex)
            {
                return new SystemResponse<string>(true, $"Upload failed: {ex.Message}");
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