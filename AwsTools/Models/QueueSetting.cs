namespace AwsTools.Models
{
    /// <summary>
    /// Amazon Simple Queue Service Setting.
    /// </summary>
    public class QueueSetting : SettingBase
    {
        /// <summary>
        /// The SQS queue name.
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Amazon Simple Queue Service.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="queueName">The SQS queue name.</param>
        public QueueSetting(AwsCredentials credentials, string region, string queueName)
            : base(credentials, region)
        {
            QueueName = queueName;
        }
    }
}
