using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface IConfigRepository
    {
        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task<SystemResponse<ConfigSettings>> GetConfigValue(string setting);

        /// <summary>
        /// Get values for a collection of config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<ConfigSettings>>> GetConfigValues(ConfigKeyRequest request);

        /// <summary>
        /// Get values for a collection of config keys
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<ConfigSettings>>> GetConfigValues(IEnumerable<string> request);

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<string>>> SetConfigValues(SetConfigRequest request);
    }
}