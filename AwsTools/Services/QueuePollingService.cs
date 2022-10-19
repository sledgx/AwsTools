using SledGX.Tools.AWS.Models;
using System.Text.Json;

namespace SledGX.Tools.AWS
{
    /// <summary>
    /// Wrapper for listening to an Amazon Simple Queue Service (SQS) queue.
    /// </summary>
    /// <typeparam name="T">The generic type of the object.</typeparam>
    public class QueuePollingService<T>
    {
        private readonly int sleep;
        private readonly int idleAfter;
        private readonly int idleSleep;
        private readonly int killAfter;
        private readonly bool autoStop;
        private readonly QueueService service;
        private readonly Thread thread;

        private bool listenerEnable = false;

        /// <summary>
        /// Receives a message from the Amazon SQS queue.
        /// </summary>
        /// <param name="data">The object received from the queue.</param>
        public delegate void OnMessageReceivedAction(T? data);

        /// <summary>
        /// Action to take when receiving a message from the Amazon SQS queue.
        /// </summary>
        public event OnMessageReceivedAction? OnMessageReceived;

        /// <summary>
        /// Receives an error while processing the message from the Amazon SQS queue.
        /// </summary>
        /// <param name="ex">The exception error.</param>
        public delegate void OnErrorAction(Exception ex);

        /// <summary>
        /// Action to take when an error is received while processing the message from the Amazon SQS queue.
        /// </summary>
        public event OnErrorAction? OnError;

        /// <summary>
        /// Queue polling service initialization.
        /// </summary>
        /// <param name="setting">Queue polling service setting.</param>
        public QueuePollingService(QueuePollingSetting setting)
        {
            sleep = setting.Sleep;
            idleSleep = setting.IdleSleep;
            idleAfter = setting.IdleAfter;
            killAfter = setting.KillAfter;
            autoStop = setting.AutoStop;

            service = new QueueService(setting);
            thread = new Thread(MessageListener);
        }

        /// <summary>
        /// Starts listening to the Amazon SQS queue.
        /// </summary>
        public void Start()
        {
            listenerEnable = true;
            thread.Start();
        }

        /// <summary>
        /// Starts listening to the Amazon SQS queue and waits.
        /// </summary>
        public void StartAndWait()
        {
            Start();
            thread.Join();
        }

        /// <summary>
        /// Stops listening to the Amazon SQS queue.
        /// </summary>
        public void Stop()
        {
            listenerEnable = false;
            thread.Join();
        }

        /// <summary>
        /// Background worker to listen to the Amazon SQS queue.
        /// </summary>
        private void MessageListener()
        {
            uint idleCounter = 0;
            uint errorCounter = 0;

            while (listenerEnable)
            {
                var message = service.ReceiveMessage();

                if (message.HasValue)
                {
                    idleCounter = 0;

                    try
                    {
                        var data = JsonSerializer.Deserialize<T>(message.Value.body);
                        OnMessageReceived?.Invoke(data);
                        service.DeleteMessage(message.Value.id);
                        errorCounter = 0;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex);
                        errorCounter++;
                    }
                }

                if (autoStop && idleCounter >= idleAfter)
                    break;

                if (killAfter > 0 && errorCounter >= killAfter)
                    break;

                Thread.Sleep(idleCounter >= idleAfter ? idleSleep : sleep);
                idleCounter++;
            }
        }
    }
}
