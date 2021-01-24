using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;
using Newtonsoft.Json;

namespace Full
{
    internal class ClassroomBotter
    {
        public event EventHandler<MessageEventArgs> OnMessage;

        private readonly Config config;
        private CancellationTokenSource source = new();

        public ClassroomBotter(Config config)
        {
            this.config = config;
        }

        public void Stop()
        {
            source.Cancel();
        }
        public async Task Start()
        {
            if (config == null) return;

            var loopBot = new LoopBot(config, source.Token);
            try
            {
                loopBot.Login();
                loopBot.Start(LatestMSG);
                // Console.WriteLine("Awaiting task...");
                await loopBot.Task;
            }
            finally
            {
                loopBot?.Dispose();
            }

        }
        private async Task LatestMSG(ClassroomBot bot, CancellationToken token)
        {
            Message lastMessage = null;
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    Message latestMsg = bot.GetMessage(0);
                    if (latestMsg != lastMessage)
                    {
                        OnMessage.Invoke(bot, new MessageEventArgs(latestMsg));
                    }
                    lastMessage = latestMsg;

                    await Task.Delay(new TimeSpan(0, minutes: 1, 0), token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Task canceled successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(LatestMSG)} caught: " + ex.Message);
            }
            // Console.WriteLine($"{nameof(LatestMSG)} is done.");
        }
    }
}