using Amazon.SQS;
using Amazon.SQS.Model;
using AwsTools.Models;
using System.Net;
using System.Text.Json;

namespace AwsTools.Services
{
    /// <summary>
    /// Wrapper for accessing Amazon Simple Queue Service (SQS).
    /// </summary>
    public class QueueService
    {
        private readonly string queueName;
        private readonly AmazonSQSClient client;

        /// <summary>
        /// Queue service initialization.
        /// </summary>
        /// <param name="setting">Queue service setting.</param>
        public QueueService(QueueSetting setting)
        {
            queueName = setting.QueueName;
            client = new AmazonSQSClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        /// <summary>
        /// Counts the number of messages on an Amazon SQS queue.
        /// </summary>
        /// <returns>The total message counter.</returns>
        public int CountMessages()
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = queueName,
                AttributeNames = new List<string>
                {
                    "ApproximateNumberOfMessages"
                }
            };

            var response = client.GetQueueAttributesAsync(request).Result;
            return response.ApproximateNumberOfMessages;
        }

        /// <summary>
        /// Sends a text message to an Amazon SQS queue.
        /// </summary>
        /// <param name="message">The message you want to send.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushMessage(string message)
        {
            var request = new SendMessageRequest
            {
                QueueUrl = queueName,
                MessageBody = message
            };

            var response = client.SendMessageAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Sends an object to an Amazon SQS queue. The object is automatically transformed into json.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="data">The object you want to send.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushObject<T>(T data)
        {
            string json = JsonSerializer.Serialize(data);
            return PushMessage(json);
        }

        /// <summary>
        /// Sends multiple text messages to an Amazon SQS queue.
        /// </summary>
        /// <param name="messages">The list of messages you want to send.</param>
        /// <param name="chunkSize">The quantity of messages to be sent at the same time.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushMessages(List<string> messages, int chunkSize = 500)
        {
            var result = new List<HttpStatusCode>();
            foreach (var chunk in messages.Chunk(chunkSize))
            {
                var request = new SendMessageBatchRequest
                {
                    QueueUrl = queueName,
                    Entries = messages
                        .Select(x => new SendMessageBatchRequestEntry
                        {
                            MessageBody = x
                        })
                        .ToList()
                };

                var response = client.SendMessageBatchAsync(request).Result;
                result.Add(response.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }

        /// <summary>
        /// Sends multiple objects to an Amazon SQS queue. Objects are automatically transformed into json.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="data">The list of objects you want to send.</param>
        /// <param name="chunkSize">The quantity of messages to be sent at the same time.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PushObjects<T>(List<T> data, int chunkSize = 500)
        {
            var messages = data
                .Select(x => JsonSerializer.Serialize(x))
                .ToList();

            return PushMessages(messages, chunkSize);
        }

        /// <summary>
        /// Receives a message from the Amazon SQS queue.
        /// </summary>
        /// <returns>The message retrieved as a id and body pair.</returns>
        public (string id, string body)? ReceiveMessage()
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueName,
                WaitTimeSeconds = 1,
                MaxNumberOfMessages = 1
            };

            var response = client.ReceiveMessageAsync(request).Result;

            if (response?.Messages != null && response.Messages.Count > 0)
                return (response.Messages[0].ReceiptHandle, response.Messages[0].Body);

            return null;
        }

        /// <summary>
        /// Deletes a message from the Amazon SQS queue.
        /// </summary>
        /// <param name="id">The id of the message to be deleted.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool DeleteMessage(string id)
        {
            var request = new DeleteMessageRequest
            {
                QueueUrl = queueName,
                ReceiptHandle = id
            };

            var response = client.DeleteMessageAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }
    }
}
