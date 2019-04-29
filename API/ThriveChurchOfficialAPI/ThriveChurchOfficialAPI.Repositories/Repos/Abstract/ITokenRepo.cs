using System;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface ITokenRepo
    {
        /// <summary>
        /// Validate API Keys
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        ValidationResponse ValidateToken(string apiKey);
    }
}
