using Amazon;
using Amazon.Runtime;

namespace AwsTools.Models
{
    /// <summary>
    /// Service Base Setting.
    /// </summary>
    public class SettingBase
    {
        /// <summary>
        /// The AWS Account Credentials.
        /// </summary>
        public AwsCredentials Credentials { get; private set; }

        /// <summary>
        /// The AWS service region.
        /// </summary>
        public string Region { get; private set; }

        /// <summary>
        /// Service Base
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        public SettingBase(AwsCredentials credentials, string region)
        {
            Credentials = credentials;
            Region = region;
        }

        /// <summary>
        /// Retrieves the AWS Account Credentials.
        /// </summary>
        /// <returns>The AWS Account Credentials.</returns>
        internal AWSCredentials GetCredentials()
        {
            return new BasicAWSCredentials(Credentials.AccessKeyId, Credentials.SecretAccessKey);
        }

        /// <summary>
        /// Retrieves the AWS service region endpoint.
        /// </summary>
        /// <returns>The AWS service region endpoint.</returns>
        internal RegionEndpoint GetRegionEndpoint()
        {
            return RegionEndpoint.GetBySystemName(Region);
        }
    }
}
