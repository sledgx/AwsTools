# AwsTools
AWS Services Wrapper for .NET

- [Notification](#notification-service): implementation of the Amazon Simple Notification Service (SNS) API.
- [Queue Polling](#queue-polling-service): implementation of the message listing service from the Amazon Simple Queue Service (SQS).
- [Queue](#queue-service): implementation of the Amazon Simple Queue Service (SQS) API.
- [Storage](#storage-service): implementation of the Amazon Simple Storage Service (S3) API.
- [Streamer](#streamer-service): implementation of the Amazon Kinesis Data Firehose API.

## How to use
Eeach service requires an AWS account to authenticate, so first you need to instantiate the `AwsCredentials` object to define the account details.

Example.
```csharp
var credetials = new AwsCredentials("<your access key id>", "<your secret access key>");
```

### Notification Service
To initialize this service it's necessary to use the `NotificationService` object by passing in the AWS credentials, the region where the service was created and the ARN of the SNS topic.

Example.
```csharp
var setting = new NotificationSetting(credetials, "<region>", "<topic arn>");
var service = new NotificationService(setting);
```

Below is a list of the available methods:

| Method           | Description                                                                                     |
|------------------|-------------------------------------------------------------------------------------------------|
| PushMessage      | Sends a text message to an Amazon SNS topic.                                                    |
| PushObject\<T\>  | Sends an object to an Amazon SNS topic. The object is automatically transformed into json.      |
| PushMessages     | Sends multiple text messages to an Amazon SNS topic.                                            |
| PushObjects\<T\> | Sends multiple objects to an Amazon SNS topic. Objects are automatically transformed into json. |

### Queue Polling Service
To initialize this service it's necessary to use the `QueuePollingService\<T\>` object by passing in the AWS credentials, the region where the service was created and the url of the SQS queue.
There are further settings that can be optionally defined:

| Setting   | Description                                                            |
|-----------|------------------------------------------------------------------------|
| Sleep     | Duration of the pause in milliseconds between message retrievals.      |
| IdleSleep | Duration of the long pause in milliseconds between message retrievals. |
| IdleAfter | Slows down the listing after the number of empty messages.             |
| KillAfter | Stops the listing after the number of failed processes.                |
| AutoStop  | Stops the listing after `IdleAfter` empty messages.                    |

Example.
```csharp
var setting = new QueuePollingSetting(credetials, "<region>", "<queue url>")
{
    Sleep = 5000
};
var service = new QueuePollingService<Data>(setting);
service.OnMessageReceived += Queue_OnMessageReceived;

// ...

void Queue_OnMessageReceived(Data? data)
{
    // ...
}
```

Below is a list of the available methods:

| Method                 | Description                                                                                      |
|------------------------|--------------------------------------------------------------------------------------------------|
| Start                  | Starts listening to the Amazon SQS queue.                                                        |
| StartAndWait           | Starts listening to the Amazon SQS queue and waits.                                              |
| Stop                   | Stops listening to the Amazon SQS queue.                                                         |
| OnMessageReceived\<T\> | Action to take when receiving a message from the Amazon SQS queue.                               |
| OnError                | Action to take when an error is received while processing the message from the Amazon SQS queue. |

### Queue Service
To initialize this service it's necessary to use the `QueueService` object by passing in the AWS credentials, the region where the service was created and the url of the SQS queue.

Example.
```csharp
var setting = new QueueSetting(credetials, "<region>", "<queue url>");
var service = new QueueService(setting);
```

Below is a list of the available methods:

| Method           | Description                                                                                     |
|------------------|-------------------------------------------------------------------------------------------------|
| CountMessages    | Counts the number of messages on an Amazon SQS queue.                                           |
| PushMessage      | Sends a text message to an Amazon SQS queue.                                                    |
| PushObject\<T\>  | Sends an object to an Amazon SQS queue. The object is automatically transformed into json.      |
| PushMessages     | Sends multiple text messages to an Amazon SQS queue.                                            |
| PushObjects\<T\> | Sends multiple objects to an Amazon SQS queue. Objects are automatically transformed into json. |
| ReceiveMessage   | Receives a message from the Amazon SQS queue.                                                   |
| DeleteMessage    | Deletes a message from the Amazon SQS queue.                                                    |

### Storage Service
To initialize this service it's necessary to use the `StorageService` object by passing in the AWS credentials, the region where the service was created and the name of the S3 bucket.

Example.
```csharp
var setting = new StorageSetting(credetials, "<region>", "<bucket name>");
var service = new StorageService(setting);
```

Below is a list of the available methods:

| Method            | Description                                                                                   |
|-------------------|-----------------------------------------------------------------------------------------------|
| ObjectExists      | Checks if an object is present inside the Amazon S3 bucket.                                   |
| UploadText        | Writes a text into an Amazon S3 bucket.                                                       |
| UploadObject\<T\> | Writes an object into an Amazon S3 bucket. The object is automatically transformed into json. |
| UploadFile        | Writes a file to an Amazon S3 bucket.                                                         |
| GetText           | Retrieves a text from Amazon S3 bucket.                                                       |
| GetObject\<T\>    | Retrieves an object from Amazon S3 bucket.                                                    |
| DownloadFile      | Downloads the file from Amazon S3 bucket.                                                     |
| DeleteObject      | Deletes an object from the Amazon S3 bucket.                                                  |

### Streamer Service
To initialize this service it's necessary to use the `StreamerService` object by passing in the AWS credentials, the region where the service was created and the name of Kinesis delivery stream.

Example.
```csharp
var setting = new StreamerSetting(credetials, "<region>", "<stream name>");
var service = new StreamerService(setting);
```

Below is a list of the available methods:

| Method          | Description                                                                        |
|-----------------|------------------------------------------------------------------------------------|
| PutRecord\<T\>  | Writes a single data record into an Amazon Kinesis Data Firehose delivery stream.  |
| PutRecords\<T\> | Writes multiple data records into an Amazon Kinesis Data Firehose delivery stream. |

## License
Released under the [MIT License](https://github.com/sledgx/AwsTools/blob/master/LICENSE).

## Copyright
[SledGX](https://github.com/sledgx)
