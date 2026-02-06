using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI.Controllers
{
    /// <summary>
    /// Sermons Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SermonsController : ControllerBase
    {
        private readonly ISermonsService _sermonsService;
        private readonly ITranscriptService _transcriptService;

        /// <summary>
        /// Sermons Controller C'tor
        /// </summary>
        /// <param name="sermonsService"></param>
        /// <param name="transcriptService"></param>
        public SermonsController(ISermonsService sermonsService, ITranscriptService transcriptService)
        {
            // delay the init of the repo for when we go to the service, we will grab the connection
            // string from the IConfiguration object there instead of init-ing the repo here
            _sermonsService = sermonsService;
            _transcriptService = transcriptService;
        }

        /// <summary>
        /// Get a summary of every sermon series
        /// </summary>
        /// <returns>SermonsSummary object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AllSermonsSummaryResponse>> GetAllSermons(bool highResImg = false)
        {
            var response = await _sermonsService.GetAllSermons(highResImg);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Recieve Sermon Series in a paged format
        /// </summary>
        /// <remarks>
        /// This will return the sermon series' in a paged format. 
        /// <br />
        /// NOTE: 
        /// <br />
        /// &#8901; The first page will contain the 5 first messagess.
        /// &#8901; Every subsequent page will contain up to 10 messages.
        /// &#8901; The response will contain the total number of pages.
        /// </remarks>
        /// <param name="PageNumber"></param>
        /// <param name="highResImg"></param>
        /// <returns>Paged Sermon Data</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("paged")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SermonsSummaryPagedResponse>> GetPagedSermons([BindRequired] int PageNumber, bool highResImg = false)
        {
            var response = await _sermonsService.GetPagedSermons(PageNumber, highResImg);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Start a new sermon series
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("series")]
        [ProducesResponseType(401)]
        public async Task<ActionResult<SermonSeriesResponse>> CreateNewSermonSeries([FromBody] CreateSermonSeriesRequest request)
        {
            var response = await _sermonsService.CreateNewSermonSeries(request);

            if (response == null)
            {
                return StatusCode(400);
            }

            if (response.SuccessMessage == "202")
            {
                // Return a 202 here because this is valid, however there is something else active and nothing was done
                // "The request has been received but not yet acted upon" is what I would expect to be a correct response
                // More on that here https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/202
                return StatusCode(202, response.Result);
            }

            return response.Result;
        }

        /// <summary>
        /// Get a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <returns>SermonSeries object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("series/{SeriesId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SermonSeriesResponse>> GetSeriesForId([BindRequired] string SeriesId)
        {
            var response = await _sermonsService.GetSeriesForId(SeriesId);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Update a sermon series including messages
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns>SermonSeries object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPut("series/{SeriesId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<SermonSeries>> ModifySermonSeries([BindRequired] string SeriesId, [FromBody] SermonSeriesUpdateRequest request)
        {
            var response = await _sermonsService.ModifySermonSeries(SeriesId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Add a message to a sermon series
        /// </summary>
        /// <param name="SeriesId"></param>
        /// <param name="request"></param>
        /// <returns>SermonSeries Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("series/{SeriesId}/message")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<SermonSeriesResponse>> AddMessagesToSermonSeries([BindRequired] string SeriesId, [FromBody] AddMessagesToSeriesRequest request)
        {
            var response = await _sermonsService.AddMessageToSermonSeries(SeriesId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Modify a sermon message
        /// </summary>
        /// <param name="MessageId"></param>
        /// <param name="request"></param>
        /// <returns>An updated sermon message</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPut("series/message/{MessageId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<SermonMessage>> UpdateMessagesInSermonSeries([BindRequired] string MessageId, [FromBody] UpdateMessagesInSermonSeriesRequest request)
        {
            var response = await _sermonsService.UpdateMessageInSermonSeries(MessageId, request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Mark a message as played by a user
        /// </summary>
        /// <remarks>
        /// Request body is ignored on this request.
        /// </remarks>
        /// <param name="MessageId"></param>
        /// <returns>SermonSeries Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("series/message/{MessageId}/played")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SermonMessage>> MarkMessagePlayed([BindRequired] string MessageId)
        {
            var response = await _sermonsService.MarkMessagePlayed(MessageId);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get the waveform data for a message
        /// </summary>
        /// <param name="MessageId"></param>
        /// <returns>Waveform Data</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("series/message/{MessageId}/waveforms")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<double>>> GetMessageWaveformData([BindRequired] string MessageId) 
        {
            var response = await _sermonsService.GetMessageWaveformData(MessageId);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get the full transcript for a sermon message
        /// </summary>
        /// <remarks>
        /// Returns the full transcript text along with metadata (title, speaker, word count).
        /// Also includes sermon notes and study guide if available.
        /// Transcripts are stored in Azure Blob Storage and generated via AI transcription.
        /// </remarks>
        /// <param name="MessageId">The unique identifier of the message</param>
        /// <returns>Transcript response containing full text, metadata, and optional notes/study guide</returns>
        /// <response code="200">OK - Transcript found</response>
        /// <response code="400">Bad Request - Invalid message ID</response>
        /// <response code="404">Not Found - Transcript not available for this message</response>
        [Produces("application/json")]
        [HttpGet("series/message/{MessageId}/transcript")]
        [ProducesResponseType(typeof(TranscriptResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TranscriptResponse>> GetMessageTranscript([BindRequired] string MessageId)
        {
            var response = await _transcriptService.GetTranscriptAsync(MessageId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage.Contains("not found"))
                {
                    return StatusCode(404, response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get AI-generated sermon notes for a message
        /// </summary>
        /// <remarks>
        /// Returns structured sermon notes including key points, scripture references, quotes, and application points.
        /// Notes are AI-generated from the sermon transcript.
        /// </remarks>
        /// <param name="MessageId">The unique identifier of the message</param>
        /// <returns>Sermon notes response with structured content</returns>
        /// <response code="200">OK - Sermon notes found</response>
        /// <response code="400">Bad Request - Invalid message ID or service not configured</response>
        /// <response code="404">Not Found - Sermon notes not yet generated for this message</response>
        [Produces("application/json")]
        [HttpGet("series/message/{MessageId}/notes")]
        [ProducesResponseType(typeof(SermonNotesResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SermonNotesResponse>> GetSermonNotes([BindRequired] string MessageId)
        {
            var response = await _transcriptService.GetSermonNotesAsync(MessageId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage.Contains("not found") || response.ErrorMessage.Contains("not yet generated"))
                {
                    return StatusCode(404, response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get AI-generated study guide for a message
        /// </summary>
        /// <remarks>
        /// Returns a structured study guide suitable for small group discussion.
        /// Includes discussion questions, scripture references, illustrations, prayer prompts, and take-home challenges.
        /// Study guides are AI-generated from the sermon transcript.
        /// </remarks>
        /// <param name="MessageId">The unique identifier of the message</param>
        /// <returns>Study guide response with discussion material</returns>
        /// <response code="200">OK - Study guide found</response>
        /// <response code="400">Bad Request - Invalid message ID or service not configured</response>
        /// <response code="404">Not Found - Study guide not yet generated for this message</response>
        [Produces("application/json")]
        [HttpGet("series/message/{MessageId}/study-guide")]
        [ProducesResponseType(typeof(StudyGuideResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<StudyGuideResponse>> GetStudyGuide([BindRequired] string MessageId)
        {
            var response = await _transcriptService.GetStudyGuideAsync(MessageId);

            if (response.HasErrors)
            {
                if (response.ErrorMessage.Contains("not found") || response.ErrorMessage.Contains("not yet generated"))
                {
                    return StatusCode(404, response.ErrorMessage);
                }
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Rebuild the Sermon messages RSS feed
        /// </summary>
        /// <returns>response message</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("feed/rebuild")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<string>> RebuildSermonRSSFeed()
        {
            var response = await _sermonsService.RebuildSermonRSSFeed();

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Gets all podcast messages from DB
        /// </summary>
        /// <returns>response message</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpGet("feed/messages")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<PodcastMessage>>> GetPodcastMessages()
        {
            var response = await _sermonsService.GetPodcastMessages();
            return response;
        }

        /// <summary>
        /// Updates a podcast message
        /// </summary>
        /// <returns>response message</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("feed/message/{MessageId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<PodcastMessage>> UpdatePodcastMessage([BindRequired] string MessageId, [FromBody] PodcastMessageRequest request)
        {
            var response = await _sermonsService.UpdatePodcastMessage(MessageId, request);
            return response;
        }

        /// <summary>
        /// Get Livestreaming information
        /// </summary>
        /// <returns>Live Streaming info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LiveStreamingResponse>> GetLiveSermons()
        {
            var response = await _sermonsService.GetLiveSermons();

            if (response == null)
            {
                return StatusCode(400);
            }

            var value = new ActionResult<LiveStreamingResponse>(response).Value;

            return value;
        }

        /// <summary>
        /// Set and activate the livestream
        /// </summary>
        /// <param name="request"></param>
        /// <returns>LiveSermon Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LiveStreamingResponse>> GoLive([FromBody] LiveSermonsUpdateRequest request)
        {
            var response = await _sermonsService.GoLive(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Set an active livestream for a special event [DEPRECATED]
        /// </summary>
        /// <param name="request"></param>
        /// <returns>LiveSermon Object</returns>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [HttpPut("live/special")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LiveStreamingResponse>> UpdateLiveForSpecialEvents([FromBody] LiveSermonsSpecialEventUpdateRequest request)
        {
            var response = await _sermonsService.UpdateLiveForSpecialEvents(request);

            if (response.HasErrors)
            {
                return StatusCode(400);
            }

            return response.Result;
        }

        /// <summary>
        /// Get active livstream data
        /// </summary>
        /// <remarks>
        /// This is a polling route, meaning that you can call this in a loop if you wish. However, please keep in mind the Rate Limits. <br />
        /// <br />
        /// This response includes an end time, so you may not need to call this in a loop.
        /// </remarks>
        /// <returns>LiveSermon info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("live/poll")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LiveSermonsPollingResponse>> PollForLiveEventData()
        {
            var response = await _sermonsService.PollForLiveEventData();

            if (response == null)
            {
                return StatusCode(400, "");
            }

            return response;
        }

        /// <summary>
        /// End a livestream
        /// </summary>
        /// <returns>LiveSermon Object</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpDelete("live")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LiveSermons>> UpdateLiveSermonsInactive()
        {
            var response = await _sermonsService.UpdateLiveSermonsInactive();

            if (response.HasErrors)
            {
                return StatusCode(400);
            }

            return response.Result;
        }

        /// <summary>
        /// Get Stats for all sermons
        /// </summary>
        /// <returns>Live Streaming info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("stats")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SermonStatsResponse>> GetSermonsStats()
        {
            var response = await _sermonsService.GetSermonStats();

            if (response.HasErrors)
            {
                return StatusCode(400);
            }

            return response.Result;
        }

        /// <summary>
        /// Get Stats for all sermons
        /// </summary>
        /// <returns>Live Streaming info</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("stats/chart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SermonStatsChartResponse>> GetSermonsStatsChartData(DateTime? startDate = null, 
            DateTime? endDate = null, 
            StatsChartType chartType = StatsChartType.Unknown, 
            StatsAggregateDisplayType displayType = StatsAggregateDisplayType.Monthly)
        {
            var response = await _sermonsService.GetSermonsStatsChartData(startDate, endDate, chartType, displayType);

            if (response.HasErrors)
            {
                return StatusCode(400);
            }

            return response.Result;
        }

        /// <summary>
        /// Upload an audio file to S3 storage
        /// </summary>
        /// <returns>JSON response with the S3 URL</returns>
        /// <response code="200">OK - Returns the S3 URL</response>
        /// <response code="400">Bad Request - File validation failed or upload error</response>
        /// <response code="401">Unauthorized</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("audio/upload")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<string>> UploadAudioFile()
        {
            var response = await _sermonsService.UploadAudioFileAsync(Request);
            if (response.HasErrors)
            {
                return StatusCode(400);
            }

            return response.Result;
        }

        /// <summary>
        /// Search for messages or series
        /// </summary>
        /// <param name="request">search request</param>
        /// <returns>Matching messages or series</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpPost("search")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SearchResponse>> Search([FromBody] SearchRequest request)
        {
            var response = await _sermonsService.Search(request);

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }

        /// <summary>
        /// Get all unique speaker names
        /// </summary>
        /// <returns>List of unique speaker names</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        [Produces("application/json")]
        [HttpGet("speakers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<string>>> GetUniqueSpeakers()
        {
            var response = await _sermonsService.GetUniqueSpeakers();

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Export all sermon series and message data as JSON for backup purposes
        /// </summary>
        /// <returns>Export data containing all series and messages with metadata</returns>
        /// <response code="200">OK - Export successful</response>
        /// <response code="401">Unauthorized - JWT authentication required</response>
        /// <response code="500">Internal Server Error - Export operation failed</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("export")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ExportSermonDataResponse>> ExportAllSermonData()
        {
            var response = await _sermonsService.ExportAllSermonData();
            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Import sermon series and message data from JSON for restore purposes
        /// </summary>
        /// <param name="request">Import request containing series and messages to update</param>
        /// <returns>Import statistics including updated and skipped items</returns>
        /// <response code="200">OK - Import successful</response>
        /// <response code="400">Bad Request - Validation failed</response>
        /// <response code="401">Unauthorized - JWT authentication required</response>
        /// <response code="500">Internal Server Error - Import operation failed</response>
        [Authorize]
        [Produces("application/json")]
        [HttpPost("import")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportSermonDataResponse>> ImportSermonData([FromBody] ImportSermonDataRequest request)
        {
            var response = await _sermonsService.ImportSermonData(request);
            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return Ok(response.Result);
        }

        /// <summary>
        /// Get minimal sermon data for sitemap generation
        /// </summary>
        /// <remarks>
        /// Returns all sermon series and their messages with only IDs and dates.
        /// Designed for efficient sitemap generation without requiring multiple API calls.
        /// Response is cached for 2 hours.
        /// </remarks>
        /// <returns>Series and message IDs with dates</returns>
        /// <response code="200">OK - Sitemap data retrieved</response>
        /// <response code="400">Bad Request - Failed to retrieve data</response>
        [Produces("application/json")]
        [HttpGet("sitemap")]
        [ProducesResponseType(typeof(SitemapDataResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SitemapDataResponse>> GetSitemapData()
        {
            var response = await _sermonsService.GetSitemapData();

            if (response.HasErrors)
            {
                return StatusCode(400, response.ErrorMessage);
            }

            return response.Result;
        }
    }
}