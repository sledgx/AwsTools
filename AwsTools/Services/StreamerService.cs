using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using SledGX.Tools.AWS.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SledGX.Tools.AWS
{
    /// <summary>
    /// Wrapper for accessing Amazon Kinesis Data Firehose.
    /// </summary>
    public class StreamerService
    {
        private readonly string streamName;
        private readonly AmazonKinesisFirehoseClient client;

        /// <summary>
        /// Streamer service initialization.
        /// </summary>
        /// <param name="setting">Streamer service setting.</param>
        public StreamerService(StreamerSetting setting)
        {
            streamName = setting.StreamName;
            client = new AmazonKinesisFirehoseClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        /// <summary>
        /// Writes a single data record into an Amazon Kinesis Data Firehose delivery stream.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="record">The record you want to send.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PutRecord<T>(T record, string encoder = "UTF-8")
        {
            var request = new PutRecordRequest
            {
                DeliveryStreamName = streamName,
                Record = BuildRecord(record, encoder)
            };

            var response = client.PutRecordAsync(request).Result;
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Writes multiple data records into an Amazon Kinesis Data Firehose delivery stream.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="records">The list of objects you want to send.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <param name="chunkSize">The quantity of records to be sent at the same time.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool PutRecords<T>(List<T> records, string encoder = "UTF-8", int chunkSize = 500)
        {
            var result = new List<HttpStatusCode>();
            foreach (var chunk in records.Chunk(chunkSize))
            {
                var request = new PutRecordBatchRequest
                {
                    DeliveryStreamName = streamName,
                    Records = chunk.
                        Select(x => BuildRecord(x, encoder))
                        .ToList()
                };

                var response = client.PutRecordBatchAsync(request).Result;
                result.Add(response.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }

        /// <summary>
        /// Transforms the object into a data record in json format.
        /// </summary>
        /// <typeparam name="T">The generic type of the object.</typeparam>
        /// <param name="record">The object you want to send.</param>
        /// <param name="encoder">The encoding of the content.</param>
        /// <returns>The data record.</returns>
        private static Record BuildRecord<T>(T record, string encoder = "UTF-8")
        {
            string json = JsonSerializer.Serialize(record);
            var encoding = Encoding.GetEncoding(encoder);
            byte[] bytes = encoding.GetBytes(json);

            using var stream = new MemoryStream(bytes);
            return new Record
            {
                Data = stream
            };
        }
    }
}
