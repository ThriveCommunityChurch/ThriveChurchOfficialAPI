using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service for retrieving sermon transcripts from Azure Blob Storage
    /// </summary>
    public class TranscriptService : ITranscriptService
    {
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Constructor for TranscriptService
        /// </summary>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="containerName">Name of the blob container storing transcripts</param>
        public TranscriptService(string connectionString, string containerName)
        {
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
        public TranscriptService(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
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
                // Transcripts are stored as {messageId}.json
                var blobName = $"{messageId}.json";
                var blobClient = _containerClient.GetBlobClient(blobName);

                // Check if the blob exists
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    return new SystemResponse<TranscriptResponse>(true, 
                        $"Transcript not found for message ID: {messageId}");
                }

                // Download the blob content
                var downloadResponse = await blobClient.DownloadContentAsync();
                var content = downloadResponse.Value.Content.ToString();

                // Parse the JSON content
                var transcript = JsonConvert.DeserializeObject<TranscriptResponse>(content);
                
                if (transcript == null)
                {
                    return new SystemResponse<TranscriptResponse>(true, 
                        "Failed to parse transcript data.");
                }

                // Ensure the MessageId is set
                transcript.MessageId = messageId;

                // Calculate word count if not present
                if (transcript.WordCount == 0 && !string.IsNullOrEmpty(transcript.FullText))
                {
                    transcript.WordCount = transcript.FullText.Split(
                        new[] { ' ', '\t', '\n', '\r' }, 
                        StringSplitOptions.RemoveEmptyEntries).Length;
                }

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
    }
}

