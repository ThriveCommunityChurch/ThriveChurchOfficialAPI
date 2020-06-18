using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class ConfigKeyRequest
    {
        /// <summary>
        /// A collection of configuration keys
        /// </summary>
        public IEnumerable<string> Keys { get; set; }

        public static ValidationResponse Validate(ConfigKeyRequest request)
        {
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            if (request.Keys == null || !request.Keys.Any())
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, nameof(Keys)));
            }

            return new ValidationResponse("Success!");
        }
    }
}