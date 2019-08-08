using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Passages Repo
    /// </summary>
    public class PassagesRepository: RepositoryBase, IPassagesRepository
    {
        private readonly IMongoCollection<BiblePassage> _biblePassageCollection;

        /// <summary>
        /// Passages Repo C'tor
        /// </summary>
        /// <param name="Configuration"></param>
        public PassagesRepository(IConfiguration Configuration)
            : base(Configuration)
        {
            _biblePassageCollection = DB.GetCollection<BiblePassage>("BiblePassages");
        }

        /// <summary>
        /// Gets passages from ESV API given some search criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<PassageTextInfo> GetPassagesForSearch(string searchCriteria)
        {
            if (string.IsNullOrEmpty(EsvApiKey))
            {
                throw new ArgumentNullException("'EsvApiKey' is a required within your appsettings.json in order to continue.");
            }

            // setup the request
            var escapedString = Uri.EscapeDataString(searchCriteria);
            var uri = string.Format("https://api.esv.org/v3/passage/text/?q={0}", escapedString);

            var response = await GetPassages(uri, EsvApiKey);

            return response;
        }

        /// <summary>
        /// Send the request to the ESV API with our auth token
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authenticationToken"></param>
        /// <returns></returns>
        public async Task<PassageTextInfo> GetPassages(string uri, string authenticationToken)
        {
            var authToken = string.Format("Token {0}", authenticationToken);

            var response = await GetAsync(uri, authToken);
            PassageTextInfo passageAndInfo = null;

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                passageAndInfo = JsonConvert.DeserializeObject<PassageTextInfo>(jsonString);
            }

            return passageAndInfo;
        }

        /// <summary>
        /// Search for a bible passage in the cache
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<SystemResponse<BiblePassage>> GetPassageFromCache(string searchCriteria)
        {
            // create an equality filter
            FilterDefinition<BiblePassage> filter = Builders<BiblePassage>.Filter.Eq(b => b.PassageRef, searchCriteria);

            // look in the DB
            IAsyncCursor<BiblePassage> result = await _biblePassageCollection.FindAsync(filter);
            BiblePassage passage = result.FirstOrDefault();

            if (passage == null || passage == default(BiblePassage))
            {
                return new SystemResponse<BiblePassage>(true, $"Unable to find passage with PassageRef {searchCriteria}.");
            }

            return new SystemResponse<BiblePassage>(passage, "Success!");
        }

        /// <summary>
        /// Insert a new cache value for a specified passage
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<BiblePassage> SetPassageForCache(BiblePassage passage)
        {
            // create an equality filter
            FilterDefinition<BiblePassage> filter = Builders<BiblePassage>.Filter.Eq(b => b.PassageRef, passage.PassageRef);

            // look in the DB
            IAsyncCursor<BiblePassage> result = await _biblePassageCollection.FindAsync(filter);
            BiblePassage found = result.FirstOrDefault();

            // we should not find one
            if (found == null || found == default(BiblePassage))
            {
                // save the item in the collection, then return once it's been assigned an _id
                await _biblePassageCollection.InsertOneAsync(passage);
                return passage;
            }

            // however if we found one, return that one
            return found;
        }
    }
}
