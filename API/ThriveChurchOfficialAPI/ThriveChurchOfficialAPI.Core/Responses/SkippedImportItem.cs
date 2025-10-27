namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Represents an item (series or message) that was skipped during import
    /// </summary>
    public class SkippedImportItem
    {
        /// <summary>
        /// The ID of the item that was skipped (SeriesId or MessageId)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The type of item that was skipped ('Series' or 'Message')
        /// </summary>
        public string Type { get; set; }
    }
}
