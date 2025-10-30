using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Request object for importing sermon series and message data
    /// </summary>
    public class ImportSermonDataRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ImportSermonDataRequest()
        {
            Series = new List<SermonSeriesResponse>();
        }

        /// <summary>
        /// Collection of sermon series to import with their nested messages. Each series should include all properties.
        /// </summary>
        public IEnumerable<SermonSeriesResponse> Series { get; set; }

        /// <summary>
        /// Validates the import request object
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>ValidationResponse indicating success or failure</returns>
        public static ValidationResponse ValidateRequest(ImportSermonDataRequest request)
        {
            // Validate request is not null
            if (request == null)
            {
                return new ValidationResponse(true, SystemMessages.EmptyRequest);
            }

            // Validate Series array is not null
            if (request.Series == null)
            {
                return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Series"));
            }

            // Validate Series array is not empty
            var seriesList = request.Series.ToList();
            if (!seriesList.Any())
            {
                return new ValidationResponse(true, "Series array cannot be empty");
            }

            // Validate each series
            foreach (var series in seriesList)
            {
                // Validate series ID is valid MongoDB ObjectId format
                if (string.IsNullOrEmpty(series.Id))
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Series.Id"));
                }

                if (!ObjectId.TryParse(series.Id, out ObjectId _))
                {
                    return new ValidationResponse(true, string.Format("Series Id '{0}' is not a valid MongoDB ObjectId format", series.Id));
                }

                // Validate required series fields
                if (string.IsNullOrEmpty(series.Name))
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Series.Name"));
                }

                if (series.StartDate == null)
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Series.StartDate"));
                }

                if (string.IsNullOrEmpty(series.Slug))
                {
                    return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Series.Slug"));
                }

                // Validate dates are chronological if EndDate is provided
                if (series.EndDate != null && series.StartDate > series.EndDate)
                {
                    return new ValidationResponse(true, SystemMessages.EndDateMustBeAfterStartDate);
                }

                // Validate tags don't contain Unknown
                if (series.Tags != null && series.Tags.Any(t => t == MessageTag.Unknown))
                {
                    return new ValidationResponse(true, "Unknown tag type is not supported for series");
                }

                // Validate each message in the series
                if (series.Messages != null)
                {
                    foreach (var message in series.Messages)
                    {
                        // Validate message ID is valid MongoDB ObjectId format
                        if (string.IsNullOrEmpty(message.MessageId))
                        {
                            return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "MessageId"));
                        }

                        if (!ObjectId.TryParse(message.MessageId, out ObjectId _))
                        {
                            return new ValidationResponse(true, string.Format("MessageId '{0}' is not a valid MongoDB ObjectId format", message.MessageId));
                        }

                        // Validate required message fields
                        if (string.IsNullOrEmpty(message.Speaker))
                        {
                            return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Messages.Speaker"));
                        }

                        if (string.IsNullOrEmpty(message.Title))
                        {
                            return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Messages.Title"));
                        }

                        if (message.Date == null)
                        {
                            return new ValidationResponse(true, string.Format(SystemMessages.NullProperty, "Messages.Date"));
                        }

                        // Validate AudioDuration if provided
                        if (message.AudioDuration.HasValue && message.AudioDuration <= 0)
                        {
                            return new ValidationResponse(true, SystemMessages.AudioDurationTooShort);
                        }

                        // Validate tags don't contain Unknown
                        if (message.Tags != null && message.Tags.Any(t => t == MessageTag.Unknown))
                        {
                            return new ValidationResponse(true, "Unknown tag type is not supported for messages");
                        }
                    }
                }
            }

            return new ValidationResponse("Success!");
        }
    }
}

