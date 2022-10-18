using Amazon;
using Amazon.Runtime;

namespace AwsTools.Models
{
    public class SettingBase
    {
        public AwsCredentials Credentials { get; private set; } 
        public string Region { get; private set; }

        public SettingBase(AwsCredentials credentials, string region)
        {
            Credentials = credentials;
            Region = region;
        }

        internal AWSCredentials GetCredentials()
        {
            return new BasicAWSCredentials(Credentials.AccessKeyId, Credentials.SecretAccessKey);
        }

        internal RegionEndpoint GetRegionEndpoint()
        {
            return RegionEndpoint.GetBySystemName(Region);
        }
    }
}
