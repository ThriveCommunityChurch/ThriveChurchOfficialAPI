using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ThriveChurchOfficialAPI.Core
{
    public class TokenHandler
    {
        /// <summary>
        /// ObjectId notation from Mongo
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// String representation of an API key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Timestamp of Key Creation Date
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Email Address
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// Generate a SHA512 hash for the requested string
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static string GenerateHashedKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("ThriveAPIKey cannot hold a null or empty value. Send an email to wyatt@thrive-fl.org for more information.");
            }

            var validId = Guid.TryParse(apiKey, out Guid parseResult);
            if (!validId)
            {
                throw new Exception(string.Format("ThriveAPIKey {0} is not properly formatted.", apiKey));
            }

            StringBuilder Sb = new StringBuilder();

            // createa SHA 512 hash of the key
            using (var hash = SHA512.Create())
            {
                Encoding enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(apiKey));

                foreach (var b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
