namespace AwsTools.Models
{
    public class StorageSetting : SettingBase
    {
        public string BucketName { get; private set; }

        public StorageSetting(AwsCredentials credentials, string region, string bucketName)
            : base(credentials, region)
        {
            BucketName = bucketName;
        }
    }
}
