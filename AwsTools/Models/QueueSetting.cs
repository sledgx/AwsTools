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
        public string QueueUrl { get; private set; }

        /// <summary>
        /// Amazon Simple Queue Service.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="queueUrl">The SQS queue url.</param>
        public QueueSetting(AwsCredentials credentials, string region, string queueUrl)
            : base(credentials, region)
        {
            QueueUrl = queueUrl;
        }
    }
}
