using AwsTools.Models;
using System.Text.Json;

namespace AwsTools.Services
{
    public class QueuePollingService<T>
    {
        private readonly int sleep;
        private readonly int idleAfter;
        private readonly int idleSleep;
        private readonly int killAfter;
        private readonly bool autoClose;
        private readonly QueueService service;
        private readonly Thread thread;

        private bool listenerEnable = false;

        public delegate void OnMessageReceivedAction(T? data);
        public event OnMessageReceivedAction? OnMessageReceived;

        public delegate void OnErrorAction(Exception ex);
        public event OnErrorAction? OnError;

        public QueuePollingService(QueuePollingSetting setting)
        {
            sleep = setting.Sleep;
            idleSleep = setting.IdleSleep;
            idleAfter = setting.IdleAfter;
            killAfter = setting.KillAfter;
            autoClose = setting.AutoClose;

            service = new QueueService(setting);
            thread = new Thread(MessageListener);
        }

        public void Start()
        {
            listenerEnable = true;
            thread.Start();
        }

        public void StartAndWait()
        {
            Start();
            thread.Join();
        }

        public void Stop()
        {
            listenerEnable = false;
            thread.Join();
        }

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
                        service.DeleteMessage(message.Value.receiptHandle);
                        errorCounter = 0;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex);
                        errorCounter++;
                    }
                }

                if (autoClose && idleCounter >= idleAfter)
                    break;

                if (killAfter > 0 && errorCounter >= killAfter)
                    break;

                Thread.Sleep(idleCounter >= idleAfter ? idleSleep : sleep);
                idleCounter++;
            }
        }
    }
}
