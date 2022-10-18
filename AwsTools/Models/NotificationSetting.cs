namespace AwsTools.Models
{
    public class NotificationSetting : SettingBase
    {
        public string TopicName { get; private set; }

        public NotificationSetting(AwsCredentials credentials, string region, string topicName)
            : base(credentials, region)
        {
            TopicName = topicName;
        }
    }
}
