using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI
{
    public abstract class BaseService
    {
        /// <summary>
        /// Global Cache options for IMemoryCache
        /// </summary>
        public MemoryCacheEntryOptions CacheEntryOptions { get
            {
                // set a reusible cache options object
                return new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            }
        }

        /// <summary>
        /// Get a substring between 2 other substrings
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public string GetBetween(string strSource, string strStart, string strEnd)
        {
            if (string.IsNullOrEmpty(strSource))
            {
                return "";
            }

            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0, StringComparison.InvariantCultureIgnoreCase) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start, StringComparison.InvariantCultureIgnoreCase);

                if (End == -1 || Start == -1)
                {
                    return "";
                }

                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Remove the footer tags from the response from ESV
        /// </summary>
        /// <param name="passage"></param>
        /// <returns></returns>
        public string RemoveFooterFromResponse(string passage)
        {
            var response = "";

            var footnotes = GetBetween(passage, "Footnotes", "(ESV)");
            if (!string.IsNullOrEmpty(footnotes))
            {
                response = passage.Replace(footnotes, "").Replace("\n\nFootnotes(ESV)", "").TrimEnd('\n').TrimEnd();
            }
            else
            {
                response = passage.Replace(" (ESV)", "").TrimEnd('\n').TrimEnd();
            }

            return response;
        }
    }
}
