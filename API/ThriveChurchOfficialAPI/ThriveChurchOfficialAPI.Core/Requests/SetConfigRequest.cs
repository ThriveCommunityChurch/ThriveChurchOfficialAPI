using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class SetConfigRequest
    {
        /// <summary>
        /// A collection of configurations to set
        /// </summary>
        public IEnumerable<ConfigurationMap> Configurations { get; set; }

        /// <summary>
        /// Validate the request object
        /// </summary>
        /// <returns></returns>
        public static ValidationResponse Validate(SetConfigRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            if (request.Configurations == null || !request.Configurations.Any())
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Configurations)));
            }

            return new ValidationResponse("Success!");
        }
    }
}