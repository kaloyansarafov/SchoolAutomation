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
        public event EventHandler<DataEventArgs<Message>> OnMessage;
        public event EventHandler<DataEventArgs<Post>> OnPost;

        private readonly Config config;
        private CancellationTokenSource source = new();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
                loopBot.Add(LatestMSG);
                loopBot.Add(LatestPost);
                foreach (Task t in loopBot.Tasks)
                {
                    await t;
                }
            }
            finally
            {
                loopBot?.Dispose();
            }

        }
        private async Task LatestPost(ClassroomBot bot, CancellationToken token)
        {
            Post lastPost = null;
            try
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    Post latestPost = bot.GetPost(0);
                    // To avoid reference comparison
                    if ((Post)latestPost != lastPost)
                    {
                        OnPost?.Invoke(bot, new DataEventArgs<Post>(latestPost));
                    }
                    lastPost = latestPost;

                    await Task.Delay(new TimeSpan(0, minutes: 1, 0), token);
                }
            }
            catch (TaskCanceledException)
            {
                logger.Info($"{nameof(LatestPost)} canceled successfuly");
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(LatestPost)} caught", ex.Message);
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
                        OnMessage?.Invoke(bot, new DataEventArgs<Message>(latestMsg));
                    }
                    lastMessage = latestMsg;

                    await Task.Delay(new TimeSpan(0, minutes: 1, 0), token);
                }
            }
            catch (TaskCanceledException)
            {
                logger.Info($"{nameof(LatestMSG)} canceled successfully");
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(LatestMSG)} caught", ex.Message);
            }
            // Console.WriteLine($"{nameof(LatestMSG)} is done.");
        }
    }
}