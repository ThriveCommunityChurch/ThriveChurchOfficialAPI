using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Services
{
    public interface IPassagesService
    {
        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        Task<string> GetAllPassages();
    }
}
