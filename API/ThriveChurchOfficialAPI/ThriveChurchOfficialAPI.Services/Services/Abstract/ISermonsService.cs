using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface ISermonsService
    {
        /// <summary>
        /// returns a list of all Sermon Series'
        /// </summary>
        Task<AllSermonsResponse> GetAllSermons();

        /// <summary>
        /// Creates a new Sermon Series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SermonSeries> CreateNewSermonSeries(SermonSeries request);

        /// <summary>
        /// Return the information about a live sermon going on now - if it's live
        /// </summary>
        /// <returns></returns>
        Task<LiveStreamingResponse> GetLiveSermons();

        /// <summary>
        /// Updates the LiveSermons Object and updates mongo
        /// </summary>
        /// <returns></returns>
        Task<LiveStreamingResponse> UpdateLiveSermons(LiveSermonsUpdateRequest request);

        /// <summary>
        /// Updates the LiveSermon for a special event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<LiveStreamingResponse> UpdateLiveForSpecialEvents(LiveSermonsSpecialEventUpdateRequest request);

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
        Task<SermonSeries> ModifySermonSeries(SermonSeriesUpdateRequest request);

        /// <summary>
        /// Reset the LiveSermons object back to it's origional state & stop async timer
        /// </summary>
        /// <returns></returns>
        Task<LiveSermons> UpdateLiveSermonsInactive();

        /// <summary>
        /// Updates a sermon series to add a list of sermon messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SermonSeries> AddMessageToSermonSeries(AddMessagesToSeriesRequest request);
    }
}
