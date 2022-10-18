namespace AwsTools.Models
{
    /// <summary>
    /// Amazon Kinesis Data Firehose Setting.
    /// </summary>
    public class StreamerSetting : SettingBase
    {
        /// <summary>
        /// The Kinesis Firehose delivery stream name.
        /// </summary>
        public string StreamName { get; private set; }

        /// <summary>
        /// Amazon Kinesis Data Firehose.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="streamName">The Kinesis Firehose delivery stream name.</param>
        public StreamerSetting(AwsCredentials credentials, string region, string streamName)
            : base(credentials, region)
        {
            StreamName = streamName;
        }
    }
}
