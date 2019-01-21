using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Core.DTOs;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface ISermonsRepository
    {
        Task<AllSermonsResponse> GetAllSermons();

        Task<SermonSeries> CreateNewSermonSeries(SermonSeries request);

        Task<SermonSeries> GetSermonSeriesForId(string SeriesId);

        Task<LiveSermons> GetLiveSermons();

        Task<LiveSermons> UpdateLiveSermons(LiveSermons request);

        Task<LiveSermons> UpdateLiveSermonsInactive();

        Task<SermonSeries> UpdateSermonSeries(SermonSeries request);

        Task<SermonSeries> GetSermonSeriesForSlug(string slug);

        Task<SermonMessage> GetMessageForId(string messageId);

        Task<IEnumerable<RecentMessage>> GetRecentlyWatched(string userId);
        
        Task<SermonsSummaryPagedResponse> GetPagedSermons(int pageNumber);
    }
}