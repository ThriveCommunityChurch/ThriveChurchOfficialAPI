using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface IPassagesRepository
    {
        /// <summary>
        /// Go to the ESV API and get the passage
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<PassageTextInfo> GetPassagesForSearch(string searchCriteria);

        /// <summary>
        /// Search for a bible passage in the cache
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<SystemResponse<BiblePassage>> GetPassageFromCache(string searchCriteria);

        /// <summary>
        /// Insert a new cache value for a specified passage
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<BiblePassage> SetPassageForCache(BiblePassage passage);
    }
}
