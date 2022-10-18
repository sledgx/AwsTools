namespace AwsTools.Models
{
    public class QueuePollingSetting : QueueSetting
    {
        public int Sleep { get; set; } = 1000;
        public int IdleSleep { get; set; } = 30000;
        public int IdleAfter { get; set; } = 10;
        public int KillAfter { get; set; } = -1;
        public bool AutoClose { get; set; } = false;

        public QueuePollingSetting(AwsCredentials credentials, string region, string queueName)
            : base(credentials, region, queueName) { }
    }
}
