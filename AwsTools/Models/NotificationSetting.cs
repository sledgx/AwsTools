namespace AwsTools.Models
{
    /// <summary>
    /// Amazon Simple Notification Service Setting.
    /// </summary>
    public class NotificationSetting : SettingBase
    {
        /// <summary>
        /// The SNS topic ARN.
        /// </summary>
        public string TopicArn { get; private set; }

        /// <summary>
        /// Amazon Simple Notification Service.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="topicArn">The SNS topic ARN.</param>
        public NotificationSetting(AwsCredentials credentials, string region, string topicArn)
            : base(credentials, region)
        {
            TopicArn = topicArn;
        }
    }
}
