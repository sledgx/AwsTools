namespace AwsTools.Models
{
    /// <summary>
    /// Amazon Simple Storage Service Polling Setting.
    /// </summary>
    public class QueuePollingSetting : QueueSetting
    {
        /// <summary>
        /// Duration of the pause in milliseconds between message retrievals.
        /// </summary>
        public int Sleep { get; set; } = 1000;

        /// <summary>
        /// Duration of the long pause in milliseconds between message retrievals.
        /// </summary>
        public int IdleSleep { get; set; } = 30000;

        /// <summary>
        /// Slows down the listing after the number of empty messages.
        /// </summary>
        public int IdleAfter { get; set; } = 10;

        /// <summary>
        /// Stops the listing after the number of failed processes.
        /// </summary>
        public int KillAfter { get; set; } = -1;

        /// <summary>
        /// Stops the listing after <see cref="IdleAfter">IdleAfter</see> empty messages.
        /// </summary>
        public bool AutoStop { get; set; } = false;

        /// <summary>
        /// Amazon Simple Storage Service Polling.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="queueUrl">The SQS queue url.</param>
        public QueuePollingSetting(AwsCredentials credentials, string region, string queueUrl)
            : base(credentials, region, queueUrl) { }
    }
}
