using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service for retrieving sermon transcripts, notes, and study guides from Azure Blob Storage
    /// </summary>
    public class TranscriptService : ITranscriptService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ICacheService _cache;

        /// <summary>
        /// Cache expiration for transcript data (365 days - transcripts are immutable once created)
        /// </summary>
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(365);

        /// <summary>
        /// Constructor for TranscriptService
        /// </summary>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="containerName">Name of the blob container storing transcripts</param>
        /// <param name="cache">Cache service instance</param>
        public TranscriptService(string connectionString, string containerName, ICacheService cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Warning("TranscriptService initialized with empty connection string");
                _containerClient = null;
            }
            else
            {
                _containerClient = new BlobContainerClient(connectionString, containerName);
                Log.Information("TranscriptService initialized with container: {ContainerName}", containerName);
            }
        }

        /// <summary>
        /// Constructor for testing - allows injecting a mock container client
        /// </summary>
        /// <param name="containerClient">Mock blob container client for testing</param>
        /// <param name="cache">Cache service instance</param>
        public TranscriptService(BlobContainerClient containerClient, ICacheService cache)
        {
            _containerClient = containerClient;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Log.Information("TranscriptService initialized with injected container client");
        }

        /// <inheritdoc/>
        public async Task<SystemResponse<TranscriptResponse>> GetTranscriptAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return new SystemResponse<TranscriptResponse>(true, "Message ID is required.");
            }

            if (_containerClient == null)
            {
                return new SystemResponse<TranscriptResponse>(true,
                    "Transcript service is not configured. Azure Storage connection string is missing.");
            }

            try
            {
                var blob = await DownloadBlobAsync(messageId);
                if (blob == null)
                {
                    return new SystemResponse<TranscriptResponse>(true,
                        $"Transcript not found for message ID: {messageId}");
                }

                // Map blob DTO to API response (PascalCase)
                var transcript = new TranscriptResponse
                {
                    MessageId = messageId,
                    Title = blob.Title,
                    Speaker = blob.Speaker,
                    FullText = blob.Transcript,
                    WordCount = blob.WordCount,
                    Notes = MapNotesToResponse(blob.Notes),
                    StudyGuide = MapStudyGuideToResponse(blob.StudyGuide)
                };

                Log.Information("Successfully retrieved transcript for message: {MessageId}", messageId);
                return new SystemResponse<TranscriptResponse>(transcript, "Success!");
            }
            catch (Azure.RequestFailedException ex)
            {
                Log.Error(ex, "Azure request failed while retrieving transcript for message: {MessageId}", messageId);
                return new SystemResponse<TranscriptResponse>(true,
                    $"Failed to retrieve transcript: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to parse transcript JSON for message: {MessageId}", messageId);
                return new SystemResponse<TranscriptResponse>(true,
                    "Failed to parse transcript data.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error retrieving transcript for message: {MessageId}", messageId);
                return new SystemResponse<TranscriptResponse>(true,
                    $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<SystemResponse<SermonNotesResponse>> GetSermonNotesAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return new SystemResponse<SermonNotesResponse>(true, "Message ID is required.");
            }

            if (_containerClient == null)
            {
                return new SystemResponse<SermonNotesResponse>(true,
                    "Transcript service is not configured. Azure Storage connection string is missing.");
            }

            try
            {
                var blob = await DownloadBlobAsync(messageId);
                if (blob == null)
                {
                    return new SystemResponse<SermonNotesResponse>(true,
                        $"Transcript not found for message ID: {messageId}");
                }

                if (blob.Notes == null)
                {
                    return new SystemResponse<SermonNotesResponse>(true,
                        $"Sermon notes not yet generated for message ID: {messageId}");
                }

                var response = MapNotesToResponse(blob.Notes);
                Log.Information("Successfully retrieved sermon notes for message: {MessageId}", messageId);
                return new SystemResponse<SermonNotesResponse>(response, "Success!");
            }
            catch (Azure.RequestFailedException ex)
            {
                Log.Error(ex, "Azure request failed while retrieving sermon notes for message: {MessageId}", messageId);
                return new SystemResponse<SermonNotesResponse>(true,
                    $"Failed to retrieve sermon notes: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to parse transcript JSON for message: {MessageId}", messageId);
                return new SystemResponse<SermonNotesResponse>(true,
                    "Failed to parse sermon notes data.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error retrieving sermon notes for message: {MessageId}", messageId);
                return new SystemResponse<SermonNotesResponse>(true,
                    $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<SystemResponse<StudyGuideResponse>> GetStudyGuideAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                return new SystemResponse<StudyGuideResponse>(true, "Message ID is required.");
            }

            if (_containerClient == null)
            {
                return new SystemResponse<StudyGuideResponse>(true,
                    "Transcript service is not configured. Azure Storage connection string is missing.");
            }

            try
            {
                var blob = await DownloadBlobAsync(messageId);
                if (blob == null)
                {
                    return new SystemResponse<StudyGuideResponse>(true,
                        $"Transcript not found for message ID: {messageId}");
                }

                if (blob.StudyGuide == null)
                {
                    return new SystemResponse<StudyGuideResponse>(true,
                        $"Study guide not yet generated for message ID: {messageId}");
                }

                var response = MapStudyGuideToResponse(blob.StudyGuide);
                Log.Information("Successfully retrieved study guide for message: {MessageId}", messageId);
                return new SystemResponse<StudyGuideResponse>(response, "Success!");
            }
            catch (Azure.RequestFailedException ex)
            {
                Log.Error(ex, "Azure request failed while retrieving study guide for message: {MessageId}", messageId);
                return new SystemResponse<StudyGuideResponse>(true,
                    $"Failed to retrieve study guide: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Failed to parse transcript JSON for message: {MessageId}", messageId);
                return new SystemResponse<StudyGuideResponse>(true,
                    "Failed to parse study guide data.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error retrieving study guide for message: {MessageId}", messageId);
                return new SystemResponse<StudyGuideResponse>(true,
                    $"An unexpected error occurred: {ex.Message}");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Downloads and parses the transcript blob with caching
        /// </summary>
        private async Task<TranscriptBlob> DownloadBlobAsync(string messageId)
        {
            // Check cache first
            var cacheKey = string.Format(CacheKeys.TranscriptBlob, messageId.ToLowerInvariant());
            var cachedBlob = _cache.ReadFromCache<TranscriptBlob>(cacheKey);
            if (cachedBlob != null)
            {
                Log.Debug("Cache hit for transcript blob: {MessageId}", messageId);
                return cachedBlob;
            }

            // Cache miss - download from Azure Blob Storage
            var blobName = $"{messageId}.json";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync();
            if (!exists.Value)
            {
                return null;
            }

            var downloadResponse = await blobClient.DownloadContentAsync();
            var content = downloadResponse.Value.Content.ToString();
            var blob = JsonConvert.DeserializeObject<TranscriptBlob>(content);

            // Cache the result
            if (blob != null)
            {
                _cache.InsertIntoCache(cacheKey, blob, CacheExpiration);
                Log.Debug("Cached transcript blob for message: {MessageId}", messageId);
            }

            return blob;
        }

        /// <summary>
        /// Maps SermonNotesBlob to SermonNotesResponse
        /// </summary>
        private SermonNotesResponse MapNotesToResponse(SermonNotesBlob blob)
        {
            if (blob == null) return null;

            return new SermonNotesResponse
            {
                Title = blob.Title,
                Speaker = blob.Speaker,
                Date = blob.Date,
                MainScripture = blob.MainScripture,
                Summary = blob.Summary,
                KeyPoints = blob.KeyPoints?.Select(MapKeyPointToResponse).ToList(),
                Quotes = blob.Quotes?.Select(q => new QuoteResponse { Text = q.Text, Context = q.Context }).ToList(),
                ApplicationPoints = blob.ApplicationPoints,
                GeneratedAt = blob.GeneratedAt,
                ModelUsed = blob.ModelUsed,
                WordCount = blob.WordCount
            };
        }

        /// <summary>
        /// Maps StudyGuideBlob to StudyGuideResponse
        /// </summary>
        private StudyGuideResponse MapStudyGuideToResponse(StudyGuideBlob blob)
        {
            if (blob == null) return null;

            return new StudyGuideResponse
            {
                Title = blob.Title,
                Speaker = blob.Speaker,
                Date = blob.Date,
                MainScripture = blob.MainScripture,
                Summary = blob.Summary,
                KeyPoints = blob.KeyPoints?.Select(MapKeyPointToResponse).ToList(),
                ScriptureReferences = blob.ScriptureReferences?.Select(s => new ScriptureReferenceResponse
                {
                    Reference = s.Reference,
                    Context = s.Context,
                    DirectlyQuoted = s.DirectlyQuoted
                }).ToList(),
                DiscussionQuestions = blob.DiscussionQuestions != null ? new DiscussionQuestionsResponse
                {
                    Icebreaker = blob.DiscussionQuestions.Icebreaker,
                    Reflection = blob.DiscussionQuestions.Reflection,
                    Application = blob.DiscussionQuestions.Application,
                    ForLeaders = blob.DiscussionQuestions.ForLeaders
                } : null,
                Illustrations = blob.Illustrations?.Select(i => new IllustrationResponse
                {
                    Summary = i.Summary,
                    Point = i.Point
                }).ToList(),
                PrayerPrompts = blob.PrayerPrompts,
                TakeHomeChallenges = blob.TakeHomeChallenges,
                Devotional = blob.Devotional,
                AdditionalStudy = blob.AdditionalStudy?.Select(a => new AdditionalStudyResponse
                {
                    Topic = a.Topic,
                    Scriptures = a.Scriptures,
                    Note = a.Note
                }).ToList(),
                EstimatedStudyTime = blob.EstimatedStudyTime,
                GeneratedAt = blob.GeneratedAt,
                ModelUsed = blob.ModelUsed,
                Confidence = blob.Confidence != null ? new ConfidenceResponse
                {
                    ScriptureAccuracy = blob.Confidence.ScriptureAccuracy,
                    ContentCoverage = blob.Confidence.ContentCoverage
                } : null
            };
        }

        /// <summary>
        /// Maps KeyPointBlob to KeyPointResponse
        /// </summary>
        private KeyPointResponse MapKeyPointToResponse(KeyPointBlob blob)
        {
            return new KeyPointResponse
            {
                Point = blob.Point,
                Scripture = blob.Scripture,
                Detail = blob.Detail,
                TheologicalContext = blob.TheologicalContext,
                DirectlyQuoted = blob.DirectlyQuoted
            };
        }

        #endregion
    }
}

