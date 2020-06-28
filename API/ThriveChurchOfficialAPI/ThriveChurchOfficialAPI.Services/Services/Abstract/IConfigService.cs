using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    public interface IConfigService
    {
        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task<SystemResponse<ConfigurationResponse>> GetConfigValue(string setting);  
        
        /// <summary>
        /// Get a value for a config setting
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<ConfigurationCollectionResponse>> GetConfigValues(IEnumerable<string> Keys);

        /// <summary>
        /// Set values for config settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SystemResponse<string>> SetConfigValues(SetConfigRequest request);

        /// <summary>
        /// Set values for config settings from a CSV
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        Task<SystemResponse<string>> SetConfigValuesFromCSV(string csv);
    }
}