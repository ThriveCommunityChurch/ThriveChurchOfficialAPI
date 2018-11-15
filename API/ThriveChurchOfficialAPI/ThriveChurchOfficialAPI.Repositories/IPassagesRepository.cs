using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Repositories
{
    public interface IPassagesRepository
    {
        Task<string> GetAllPassages(string apiKey);
    }
}
