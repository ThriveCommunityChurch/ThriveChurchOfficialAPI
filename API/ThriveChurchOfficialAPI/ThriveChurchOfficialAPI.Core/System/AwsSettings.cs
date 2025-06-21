namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// AWS configuration settings for S3 and other AWS services
    /// </summary>
    public class AwsSettings
    {
        /// <summary>
        /// S3 bucket name for storing audio files
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// AWS access key for S3 operations
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// AWS secret key for S3 operations
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// AWS region for S3 bucket
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Base URL for constructing S3 file URLs
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Maximum file size allowed for uploads in MB
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 50;

        /// <summary>
        /// Allowed audio file extensions
        /// </summary>
        public string[] AllowedExtensions { get; set; } = { ".mp3" };
    }
}
