namespace SledGX.Tools.AWS.Models
{
    /// <summary>
    /// Amazon Lambda Service Setting.
    /// </summary>
    public class FunctionSetting : SettingBase
    {
        /// <summary>
        /// The Lambda function name.
        /// </summary>
        public string FunctionName { get; private set; }

        /// <summary>
        /// Amazon Lambda Service.
        /// </summary>
        /// <param name="credentials">The AWS Account Credentials.</param>
        /// <param name="region">The AWS service region.</param>
        /// <param name="functionName">The Lambda function name.</param>
        public FunctionSetting(AwsCredentials credentials, string region, string functionName)
            : base(credentials, region)
        {
            FunctionName = functionName;
        }
    }
}
