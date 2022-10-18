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
