using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface ISermonsService
    {
        /// <summary>
        /// returns a list of all Sermon Series'
        /// </summary>
        Task<SystemResponse<AllSermonsSummaryResponse>> GetAllSermons(bool highResImg = false);

        /// <summary>
        /// Creates a new Sermon Series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonSeriesResponse>> CreateNewSermonSeries(CreateSermonSeriesRequest request);

        /// <summary>
        /// Return the information about a live sermon going on now - if it's live
        /// </summary>
        /// <returns></returns>
        Task<LiveStreamingResponse> GetLiveSermons();

        /// <summary>
        /// Updates the LiveSermons Object and updates mongo
        /// </summary>
        /// <returns></returns>
        Task<SystemResponse<LiveStreamingResponse>> GoLive(LiveSermonsUpdateRequest request);

        /// <summary>
        /// Updates the LiveSermon for a special event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<LiveStreamingResponse>> UpdateLiveForSpecialEvents(LiveSermonsSpecialEventUpdateRequest request);

        /// <summary>
        /// Return information about a currently active stream
        /// </summary>
        /// <returns></returns>
        Task<LiveSermonsPollingResponse> PollForLiveEventData();

        /// <summary>
        /// Updates a sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonSeries>> ModifySermonSeries(string SeriesId, SermonSeriesUpdateRequest request);
        
        /// <summary>
        /// Gets a sermon series for its Id
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonSeriesResponse>> GetSeriesForId(string seriesId);

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="highResImg"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber, bool highResImg = false);

        /// <summary>
        /// Reset the LiveSermons object back to it's origional state & stop async timer
        /// </summary>
        /// <returns></returns>
        Task<SystemResponse<LiveSermons>> UpdateLiveSermonsInactive();

        /// <summary>
        /// Updates a sermon series to add a list of sermon messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonSeriesResponse>> AddMessageToSermonSeries(string SeriesId, AddMessagesToSeriesRequest request);

        /// <summary>
        /// Update a message within a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonMessage>> UpdateMessageInSermonSeries(string SeriesId, UpdateMessagesInSermonSeriesRequest request);

        /// <summary>
        /// Schedule a livestream to occur regularly
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        string ScheduleLiveStream(LiveSermonsSchedulingRequest request);

        /// <summary>
        /// Gets sermon statistics
        /// </summary>
        /// <returns></returns>
        Task<SystemResponse<SermonStatsResponse>> GetSermonStats();

        /// <summary>
        /// Updates the playcount for a sermon message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonMessage>> MarkMessagePlayed(string messageId);

        /// <summary>
        /// Returns a series of data for display in a chart
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="chartType"></param>
        /// <param name="displayType"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonStatsChartResponse>> GetSermonsStatsChartData(DateTime? startDate, DateTime? endDate, StatsChartType chartType, StatsAggregateDisplayType displayType);

        /// <summary>
        /// Uploads an audio file to S3 and returns the public URL
        /// </summary>
        /// <param name="request">The file stream to upload</param>
        /// <returns>SystemResponse containing the S3 URL or error message</returns>
        Task<SystemResponse<string>> UploadAudioFileAsync(HttpRequest request);

        /// <summary>
        /// Search for messages or series by tags
        /// </summary>
        /// <param name="request">Tag search request</param>
        /// <returns>Matching messages or series</returns>
        Task<SystemResponse<SearchResponse>> Search(SearchRequest request);

        /// <summary>
        /// Gets all unique speaker names
        /// </summary>
        /// <returns>Collection of unique speaker names</returns>
        Task<SystemResponse<IEnumerable<string>>> GetUniqueSpeakers();
    }
}