namespace AwsTools.Models
{
    public class StreamerSetting : SettingBase
    {
        public string StreamName { get; private set; }

        public StreamerSetting(AwsCredentials credentials, string region, string streamName)
            : base(credentials, region)
        {
            StreamName = streamName;
        }
    }
}
