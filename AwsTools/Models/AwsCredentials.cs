namespace SledGX.Tools.AWS.Models
{
    /// <summary>
    /// AWS Account Credentials Setting.
    /// </summary>
    public class AwsCredentials
    {
        /// <summary>
        /// Your AWS account access key id.
        /// </summary>
        public string AccessKeyId { get; private set; }

        /// <summary>
        /// Your AWS account secret access key.
        /// </summary>
        public string SecretAccessKey { get; private set; }

        /// <summary>
        /// AWS Account Credentials.
        /// </summary>
        /// <param name="accessKeyId">Your AWS account access key id.</param>
        /// <param name="secretAccessKey">Your AWS account secret access key.</param>
        public AwsCredentials(string accessKeyId, string secretAccessKey)
        {
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
        }
    }
}
