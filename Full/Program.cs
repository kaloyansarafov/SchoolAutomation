using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Full
{
    class Program
    {
        static NLog.Logger logger;

        static async Task Main(string[] args)
        {
            SetupLogger();
            CancellationTokenSource source = new();
            LangConfig config = await GetConfig();

            Classroom classroom = new Classroom(config, source.Token);
            Meet meet = new Meet(config);
            classroom.OnGreetingReceived += meet.EnterMeet;
            try
            {
                Task crTask = classroom.Start();
                Console.ReadLine();
                source.Cancel();
                Task.WaitAll(crTask);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TaskCanceledException)
                    logger.Info("Successfully canceled");
                else logger.Error(ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                classroom.Dispose();
                meet.Dispose();
            }
        }
        private static void SetupLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            logconsole.Layout = new NLog.Layouts.SimpleLayout(
                "[${date:format=HH\\:mm\\:ss}][${level}]${logger:shortName=true}: ${message}"
            );
            logconsole.Encoding = System.Text.Encoding.UTF8;

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logconsole, "*", final: true);

            // Apply config           
            NLog.LogManager.Configuration = config;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        private static async Task<LangConfig> GetConfig()
        {
            return JsonConvert.DeserializeObject<LangConfig>(await System.IO.File.ReadAllTextAsync("config.json"));
        }
    }
}
