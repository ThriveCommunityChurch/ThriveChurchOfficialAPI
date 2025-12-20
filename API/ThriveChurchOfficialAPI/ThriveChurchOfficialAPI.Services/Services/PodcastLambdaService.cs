using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Serilog;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Services
{
    /// <summary>
    /// Service for invoking AWS Lambda functions for sermon/podcast processing pipeline
    /// </summary>
    public class PodcastLambdaService : IPodcastLambdaService
    {
        private const string TranscriptionFunctionName = "transcription-processor-prod";
        private const string PodcastFunctionName = "podcast-rss-generator-prod";
        private const string Region = "us-east-2";

        private readonly IAmazonLambda _lambdaClient;

        /// <summary>
        /// Constructor for production use - creates Lambda client
        /// </summary>
        public PodcastLambdaService()
        {
        }

        /// <summary>
        /// Constructor for testing - allows injecting a mock Lambda client
        /// </summary>
        /// <param name="lambdaClient">Optional Lambda client (for testing). If null, creates a real client.</param>
        public PodcastLambdaService(IAmazonLambda lambdaClient)
        {
            if (lambdaClient != null)
            {
                _lambdaClient = lambdaClient;
            }
            else
            {
                // When running in AWS (App Runner), the SDK automatically uses the instance role
                // No explicit credentials needed - IAM role handles authentication
                var region = RegionEndpoint.GetBySystemName(Region);
                _lambdaClient = new AmazonLambdaClient(region);
            }

            Log.Information("PodcastLambdaService initialized - Transcription: {TranscriptionFunc}, Podcast: {PodcastFunc}",
                TranscriptionFunctionName, PodcastFunctionName);
        }

        /// <inheritdoc/>
        public async Task<bool> RebuildFeedAsync()
        {
            return await InvokeLambdaAsync(PodcastFunctionName, new { action = "rebuild" });
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertEpisodeAsync(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                Log.Warning("UpsertEpisodeAsync called with null or empty messageId");
                return false;
            }

            // Invoke Transcription Lambda - it will fetch metadata from MongoDB,
            // transcribe the audio, then invoke Sermon and Podcast Lambdas
            return await InvokeLambdaAsync(TranscriptionFunctionName, new { messageId });
        }

        private async Task<bool> InvokeLambdaAsync(string functionName, object payload)
        {
            try
            {
                var request = new InvokeRequest
                {
                    FunctionName = functionName,
                    InvocationType = InvocationType.Event, // Fire-and-forget (async)
                    Payload = JsonSerializer.Serialize(payload)
                };

                Log.Information("Invoking Lambda {FunctionName} with payload: {Payload}",
                    functionName, request.Payload);

                var response = await _lambdaClient.InvokeAsync(request);

                // 202 = accepted for async invocation
                if (response.StatusCode == 202)
                {
                    Log.Information("Lambda {FunctionName} invoked successfully", functionName);
                    return true;
                }

                Log.Warning("Lambda {FunctionName} returned unexpected status code: {StatusCode}",
                    functionName, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to invoke Lambda {FunctionName}", functionName);
                return false;
            }
        }
    }
}

