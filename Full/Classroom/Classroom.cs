using System;
using System.Threading;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;

namespace Full
{
    public class Classroom : IDisposable
    {
        public event EventHandler<DataEventArgs<Message>> OnMessageReceived;
        public event EventHandler<DataEventArgs<Message>> OnGreetingReceived;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private CancellationToken token;
        private ClassroomBot crBot;
        private readonly LangConfig config;

        public Classroom(LangConfig config)
        {
            config.Driver.Headless = true;
            crBot = new ClassroomBot(config);
            OnMessageReceived += Greet;
            this.config = config;
        }
        public Classroom(LangConfig config, CancellationToken token) : this(config)
        {
            this.token = token;
            token.Register(() => logger.Debug("Cancellation requested for classroom"));
        }

        public async Task Start()
        {
            Login();
            await GetMessageLoop();
        }
        public async Task GetMessageLoop()
        {
            Message last = null;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                Message latest = crBot.GetMessage(0);
                logger.Trace("Received message from {0}", latest.Teacher);
                if ((Message)latest != last)
                {
                    logger.Debug("Received message from " + latest.Teacher);
                    OnMessageReceived?.Invoke(crBot, new DataEventArgs<Message>(latest, last));
                    last = latest;
                }
                await Task.Delay(new TimeSpan(0, minutes: 3, 0), token);
            }
        }
        private void Login()
        {
            bool loggedIn = Utils.Retry(crBot.Login, times: 3);
            if (!loggedIn)
            {
                throw new Exception("Couldn't login");
            }
            logger.Debug("Logged in.");
        }
        private void Greet(object sender, DataEventArgs<Message> eventArgs)
        {
            ClassroomBot bot = (ClassroomBot)sender;
            Message latest = eventArgs.Data;
            Message previous = eventArgs.PreviousData;
            if (latest.Information.ContainsGreeting()
                || latest.Information.HasMeetLink())
            {
                if (!Utils.IsLangClass(latest) && !Utils.IsLangClass(previous))
                {
                    latest = LangGroupFilter(bot, latest);
                    logger.Info("Saying hello to {0}", eventArgs.Data.Teacher);

                    if (!bot.WrittenCommentOn(latest))
                        bot.SendOnMessage(eventArgs.Data, "Добър ден.");

                    OnGreetingReceived?.Invoke(bot, eventArgs);
                }
            }
        }
        private Message LangGroupFilter(ClassroomBot bot, Message latest)
        {
            if (config.NemskaGrupa && latest.Teacher.Contains("Чапанова"))
            {
                Message msgAfter = bot.GetMessageAfter(latest);
                if (msgAfter.Teacher.Contains("Вихрогонова"))
                {
                    logger.Trace("Nemska grupa found teacher");
                    latest = msgAfter;
                }
                else
                {
                    latest = null;
                }
            }
            else if (!config.NemskaGrupa && latest.Teacher.Contains("Вихрогонова"))
            {
                Message msgAfter = bot.GetMessageAfter(latest);
                if (msgAfter.Teacher.Contains("Чапанова"))
                {
                    logger.Trace("Ruska grupa found teacher");
                    latest = msgAfter;
                }
                else
                {
                    latest = null;
                }
            }

            return latest;
        }

        public void Dispose()
        {
            ((IDisposable)crBot).Dispose();
        }
    }
}