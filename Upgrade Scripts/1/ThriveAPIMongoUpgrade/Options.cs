using CommandLine;

namespace ThriveAPIMongoUpgrade
{
    internal class Options
    {
        [Option("ConnectionString", Required = true, HelpText = "Mongo dababase connection string.")]
        public string ConnectionString { get; set; }
    }
}