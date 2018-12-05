using MongoDB.Driver;

namespace ThriveChurchOfficialAPI.Repositories
{
    internal interface IMongoConnectionBase
    {
        // convert this to a generic type?
        //IMongoCollection<> GetCollectionFromDB(string connectionSTring);
    }
}