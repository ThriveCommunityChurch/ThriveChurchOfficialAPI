using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;

namespace ThriveChurchOfficialAPI.Repositories
{
    /// <summary>
    /// Repository for handling S3 file operations
    /// </summary>
    public class S3Repository : IS3Repository
    {
        private readonly AwsSettings _awsSettings;
        private readonly IAmazonS3 _s3Client;

        private readonly string regexPattern = @"^(\d{4})-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])-(.+)$";

        public S3Repository(IOptions<AwsSettings> awsSettings)
        {
            _awsSettings = awsSettings.Value;

            if (_awsSettings == null)
            {
                throw new ArgumentNullException(nameof(_awsSettings), "The S3 settings are not properly configured");
            }

            // Initialize S3 client with credentials
            _s3Client = new AmazonS3Client(_awsSettings.AccessKey, _awsSettings.SecretKey, Amazon.RegionEndpoint.GetBySystemName(_awsSettings.Region));
        }

        /// <summary>
        /// Uploads an audio file to S3 bucket
        /// </summary>
        /// <param name="request">The file stream to upload</param>
        /// <returns>SystemResponse containing the S3 URL or error message</returns>
        public async Task<SystemResponse<string>> UploadAudioFileAsync(HttpRequest request)
        {
            try
            {
                // Validations
                if (request == null || request.Body == null || // null body for post
                    request.ContentLength == null || request.ContentLength == 0 || // file sise of 0 bytes
                    request.Form.Files == null || !request.Form.Files.Any()) // no attached files for multi-part request
                {
                    return new SystemResponse<string>(true, SystemMessages.EmptyRequest);
                }

                var file = request.Form.Files[0]; // Get the first uploaded file
                var fileName = file.FileName;

                // Validate file
                if (!IsValidAudioFile(fileName))
                {
                    return new SystemResponse<string>(true, "Invalid file name format. Please use: YYYY-MM-DD-Recording.mp3");
                }

                if (!IsValidFileSize(request.ContentLength.Value))
                {
                    return new SystemResponse<string>(true, $"File size exceeds maximum allowed size of {_awsSettings.MaxFileSizeMB}MB.");
                }

                // Upload file
                using (var stream = file.OpenReadStream())
                {
                    // Generate unique filename
                    var fileYear = GetFileYear(fileName);
                    var uniqueFileName = $"{fileYear}/{fileName}";

                    // Create upload request
                    var s3Request = new PutObjectRequest
                    {
                        BucketName = _awsSettings.BucketName,
                        Key = uniqueFileName,
                        InputStream = stream,
                        ContentType = "audio/mpeg",
                        ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                    };

                    // Upload file
                    var response = await _s3Client.PutObjectAsync(s3Request);

                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Construct public URL
                        var fileUrl = $"{_awsSettings.BaseUrl}/{uniqueFileName}";
                        return new SystemResponse<string>(fileUrl, "Upload successful");
                    }
                    else
                    {
                        return new SystemResponse<string>(true, $"Failed to upload file. Status: {response.HttpStatusCode}");
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                return new SystemResponse<string>(true, $"AWS S3 Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new SystemResponse<string>(true, $"Upload failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if the file extension is allowed for audio uploads
        /// </summary>
        /// <param name="fileName">The filename to validate</param>
        /// <returns>True if the extension is allowed, false otherwise</returns>
        private bool IsValidAudioFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (!IsValidFileNameFormat(fileName))
            {
                return false;
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _awsSettings.AllowedExtensions.Contains(extension);
        }

        /// <summary>
        /// Validates if the file size is within allowed limits
        /// </summary>
        /// <param name="fileSizeBytes">File size in bytes</param>
        /// <returns>True if the size is within limits, false otherwise</returns>
        private bool IsValidFileSize(long fileSizeBytes)
        {
            var maxSizeBytes = _awsSettings.MaxFileSizeMB * 1024 * 1024;
            return fileSizeBytes <= maxSizeBytes;
        }

        /// <summary>
        /// Removes .mp3 from the end of the file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string RemoveFileExtention(string fileName)
        {
            // Remove the .mp3 extension for validation
            return fileName.Replace(".mp3", "", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the year from the file name to use as the folder name in S3
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private int GetFileYear(string fileName)
        {
            // Remove the .mp3
            var nameWithoutExtension = RemoveFileExtention(fileName);

            var regex = new Regex(regexPattern);

            var match = regex.Match(nameWithoutExtension);
            return int.Parse(match.Groups[1].Value);
        }

        /// <summary>
        /// Validates if the filename follows the required format: YYYY-MM-DD-Description.mp3
        /// </summary>
        /// <param name="fileName">The filename to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool IsValidFileNameFormat(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            // Remove the .mp3 extension for validation
            var nameWithoutExtension = RemoveFileExtention(fileName);

            var regex = new Regex(regexPattern);

            var match = regex.Match(nameWithoutExtension);
            if (!match.Success)
            {
                return false;
            }

            try
            {
                var year = int.Parse(match.Groups[1].Value);
                var month = int.Parse(match.Groups[2].Value);
                var day = int.Parse(match.Groups[3].Value);
                var description = match.Groups[4].Value;

                _ = new DateTime(year, month, day);
                
                return !string.IsNullOrWhiteSpace(description);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            _s3Client?.Dispose();
        }
    }
}
