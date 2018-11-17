using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface IPassagesService
    {
        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        Task<PassagesResponse> GetPassagesForSearch(string searchCriteria);
    }
}
