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
        Task<SystemResponse<ConfigSetting>> GetConfigValue(string setting);

        /// <summary>
        /// Get values for a collection of config keys
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<ConfigSetting>>> GetConfigValues(IEnumerable<string> request);

        /// <summary>
        /// Get all config settings
        /// </summary>
        /// <returns></returns>
        Task<SystemResponse<IEnumerable<ConfigSetting>>> GetAllConfigs();

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<string>> SetConfigValues(SetConfigRequest request);

        /// <summary>
        /// Delete a config setting by key
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task<SystemResponse<string>> DeleteConfig(string setting);
    }
}