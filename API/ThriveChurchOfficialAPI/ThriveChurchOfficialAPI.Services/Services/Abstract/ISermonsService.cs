﻿using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface ISermonsService
    {
        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        Task<AllSermonsResponse> GetAllSermons();

        /// <summary>
        /// Return the information about a live sermon going on now - if it's live
        /// </summary>
        /// <returns></returns>
        Task<LiveSermons> GetLiveSermons();
    }
}