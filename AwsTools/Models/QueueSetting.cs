namespace AwsTools.Models
{
    public class QueueSetting : SettingBase
    {
        public string QueueName { get; private set; }

        public QueueSetting(AwsCredentials credentials, string region, string queueName)
            : base(credentials, region)
        {
            QueueName = queueName;
        }
    }
}
