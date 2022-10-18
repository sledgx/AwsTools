using Amazon.KinesisFirehose;
using Amazon.KinesisFirehose.Model;
using AwsTools.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AwsTools.Services
{
    public class StreamerService
    {
        private readonly string streamName;
        private readonly AmazonKinesisFirehoseClient client;

        public StreamerService(StreamerSetting setting)
        {
            streamName = setting.StreamName;
            client = new AmazonKinesisFirehoseClient(setting.GetCredentials(), setting.GetRegionEndpoint());
        }

        public bool PutRecord<T>(T record)
        {
            var request = new PutRecordRequest
            {
                DeliveryStreamName = streamName,
                Record = BuildRecord(record)
            };

            var response = client.PutRecordAsync(request).Result;
            return response?.HttpStatusCode == HttpStatusCode.OK;
        }

        public bool PutRecords<T>(List<T> records, int chunkSize = 500)
        {
            var result = new List<HttpStatusCode?>();
            foreach (var chunk in records.Chunk(chunkSize))
            {
                var request = new PutRecordBatchRequest
                {
                    DeliveryStreamName = streamName,
                    Records = chunk.
                        Select(x => BuildRecord(x))
                        .ToList()
                };

                var response = client.PutRecordBatchAsync(request).Result;
                result.Add(response?.HttpStatusCode);
            }

            return result.All(x => x == HttpStatusCode.OK);
        }

        private static Record BuildRecord<T>(T record)
        {
            string json = JsonSerializer.Serialize(record);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            using var stream = new MemoryStream(bytes);
            return new Record
            {
                Data = stream
            };
        }
    }
}
