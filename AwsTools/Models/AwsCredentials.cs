namespace AwsTools.Models
{
    public class AwsCredentials
    {
        public string AccessKeyId { get; private set; }
        public string SecretAccessKey { get; private set; }

        public AwsCredentials(string accessKeyId, string secretAccessKey)
        {
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
        }
    }
}
