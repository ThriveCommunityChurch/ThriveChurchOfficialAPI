using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface ISermonsService
    {
        /// <summary>
        /// returns a list of all Sermon Series'
        /// </summary>
        Task<SystemResponse<AllSermonsSummaryResponse>> GetAllSermons();

        /// <summary>
        /// Creates a new Sermon Series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonSeries>> CreateNewSermonSeries(CreateSermonSeriesRequest request);

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
        Task<SystemResponse<SermonSeries>> GetSeriesForId(string seriesId);

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber);

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
        Task<SystemResponse<SermonSeries>> AddMessageToSermonSeries(string SeriesId, AddMessagesToSeriesRequest request);

        /// <summary>
        /// Update a message within a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonMessage>> UpdateMessageInSermonSeries(string SeriesId, UpdateMessagesInSermonSeriesRequest request);
    }
}
