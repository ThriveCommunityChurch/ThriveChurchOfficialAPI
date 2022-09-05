using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface ISermonsRepository
    {
        Task<IEnumerable<SermonSeries>> GetAllSermons(bool sorted = true);

        /// <summary>
        /// Get all sermon series with matching slug
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SermonSeries>> GetSermonsBySlug(string slug);

        /// <summary>
        /// Get the currently active series
        /// </summary>
        /// <returns></returns>
        Task<SermonSeries> GetActiveSeries();

        Task<SystemResponse<SermonSeries>> CreateNewSermonSeries(SermonSeries request);

        Task<SystemResponse<SermonSeries>> GetSermonSeriesForId(string SeriesId);

        Task<LiveSermons> GetLiveSermons();

        Task<LiveSermons> GoLive(LiveSermonsUpdateRequest request);

        Task<SystemResponse<LiveSermons>> UpdateLiveSermons(LiveSermons request);

        Task<SystemResponse<LiveSermons>> UpdateLiveSermonsInactive(DateTime? nextLive = null);

        Task<SystemResponse<SermonSeries>> UpdateSermonSeries(SermonSeries request);

        /// <summary>
        /// Returns a paged collection of summarized sermon data
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        Task<SystemResponse<SermonsSummaryPagedResponse>> GetPagedSermons(int pageNumber);
    }
}
