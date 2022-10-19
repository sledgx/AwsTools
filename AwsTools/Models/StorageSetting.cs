namespace SledGX.Tools.AWS.Models
{
    /// <summary>
    /// Amazon Simple Storage Service Setting.
    /// </summary>
    public class StorageSetting : SettingBase
    {
        /// <summary>
        /// The S3 bucket name.
        /// </summary>
        public string BucketName { get; private set; }

        /// <summary>
        /// Amazon Simple Storage Service.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="bucketName">The S3 bucket name.</param>
        public StorageSetting(AwsCredentials credentials, string region, string bucketName)
            : base(credentials, region)
        {
            BucketName = bucketName;
        }
    }
}
