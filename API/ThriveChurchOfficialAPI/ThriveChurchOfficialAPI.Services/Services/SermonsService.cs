using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class SermonsService : BaseService, ISermonsService
    {
        private readonly ISermonsRepository _sermonsRepository;

        // the controller cannot have multiple inheritance so we must push it to the service layer
        public SermonsService(IConfiguration Configuration) 
            : base(Configuration)
        {
            // init the repo with the connection string
            _sermonsRepository = new SermonsRepository(Configuration);
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<AllSermonsResponse> GetAllSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetAllSermons();

            // do the business logic here friend

            return getAllSermonsResponse;
        }

        /// <summary>
        /// returns a list of all Passage Objets
        /// </summary>
        public async Task<LiveStreamingResponse> GetLiveSermons()
        {
            var getAllSermonsResponse = await _sermonsRepository.GetLiveSermons();

            LiveStreamingResponse response;

            // if we are currently streaming then we will need to add the slug to the middle of the Facebook link
            if (getAllSermonsResponse.IsLive)
            {
                var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/", 
                    getAllSermonsResponse.VideoUrlSlug);

                // do the business logic here friend
                response = new LiveStreamingResponse()
                {
                    IsLive = true,
                    Title = getAllSermonsResponse.Title,
                    VideoUrl = videoUrl,
                    ExpirationTime = new DateTime(1990, 01, 01, 11, 15, 0, 0), //TODO: are we sure this is right?
                    IsSpecialEvent = getAllSermonsResponse.SpecialEventTimes != null ? true : false,
                    SpecialEventTimes = getAllSermonsResponse.SpecialEventTimes ?? null
                };
            }
            else
            {
                // we are not streaming so there's no need to include anything
                response = new LiveStreamingResponse()
                {
                    IsLive = false
                };
            }

            return response;
        }

        /// <summary>
        /// Update the LiveSermons object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LiveStreamingResponse> UpdateLiveSermons(LiveSermons request)
        {
            // validate the request
            var validRequest = new LiveSermons().ValidateRequest(request);

            if (!validRequest)
            {
                // an error ocurred here
                return default(LiveStreamingResponse);
            }

            var updateLiveSermonsResponse = await _sermonsRepository.UpdateLiveSermons(request);
            if (updateLiveSermonsResponse == null)
            {
                // something really bad happened here
            }

            var videoUrl = string.Format("https://facebook.com/thriveFL/videos/{0}/",
                    updateLiveSermonsResponse.VideoUrlSlug);

            var response = new LiveStreamingResponse()
            {
                ExpirationTime = updateLiveSermonsResponse.ExpirationTime,
                IsLive = updateLiveSermonsResponse.IsLive,
                IsSpecialEvent = updateLiveSermonsResponse.SpecialEventTimes != null ? true : false,
                SpecialEventTimes = updateLiveSermonsResponse.SpecialEventTimes ?? null,
                Title = updateLiveSermonsResponse.Title,
                VideoUrl = videoUrl
            };

            return response;
        }
    }
}
