using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GBot;
using GCRBot;
using GCRBot.Data;
using MeetGBot;

namespace Full
{
    public class Meet : IDisposable
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly LangConfig config;
        private MeetBot meetBot;
        private bool languageClass;

        public Meet(LangConfig config)
        {
            meetBot = new MeetBot(config);
            this.config = config;
        }
        public async void EnterMeet(object sender, DataEventArgs<Message> eventArgs)
        {
            switch (meetBot.State)
            {
                case MeetState.InCall:
                    logger.Warn("In call");
                    //TODO FOR NOW
                    return;
                case MeetState.OutsideMeet:
                    logger.Debug("Entering meet...");
                    break;
                case MeetState.NotLoggedIn:
                    Login();
                    break;
                default:
                    throw new Exception("Invalid state: " + meetBot.State);
            }
            //GLOBAL VARIABLE
            languageClass = Utils.IsLangClass(eventArgs.Data);

            string link = Utils.GetMeetLink(eventArgs.Data.Information);
            EnterOverview(link);

            bool entered = await Task.Run(() =>
                //infinite
                Utils.Retry(() =>
                {
                    bool entered = TryEnterMeet();
                    if (!entered) Task.Delay(new TimeSpan(0, 0, 20)).Wait();
                    return entered;
                })
            );
            if (!entered)
            {
                logger.Error("Couldn't enter meet: {0}", link);
            }
            await ExitMeet();
        }
        private async Task ExitMeet()
        {
            if (meetBot.State != MeetState.InCall) throw new Exception("Not in call");
            int startingPeople = meetBot.PeopleInMeet();
            await Task.Delay(new TimeSpan(0, minutes: 15, 0));
            logger.Debug("Starting exit loop...");
            while (true)
            {
                int peopleInCall = meetBot.PeopleInMeet();
                int peopleNeeded = GetPeopleNeeded();
                if (peopleInCall < startingPeople)
                {
                    break;
                }
                await Task.Delay(new TimeSpan(0, 0, seconds: 30));
            }
            logger.Info("Leaving meet");
            meetBot.LeaveMeet();
        }

        private int GetPeopleNeeded()
        {
            int peopleNeeded;
            //TODO REPLACE CONSTANTS
            if (languageClass)
            {
                peopleNeeded = 4;
            }
            else
            {
                peopleNeeded = 14;
            }

            return peopleNeeded;
        }

        private bool TryEnterMeet()
        {
            if (meetBot.State != MeetState.InOverview)
            {
                throw new Exception("Not in overview");
            }
            int people = meetBot.PeopleInMeetOverview();
            logger.Debug("People in meet: {0}", people);
            if (people >= GetPeopleNeeded())
            {
                logger.Info("Entering meet...");
                meetBot.EnterMeet();
                return true;
            }
            logger.Warn("Not enough people to enter");
            return false;
        }

        private void EnterOverview(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                link = config.DefaultMeetLink;
            }
            if (meetBot.State != MeetState.OutsideMeet)
                throw new Exception("Meetbot with invalid state: " + meetBot.State);

            logger.Debug("Joining {0}", link);
            try
            {
                meetBot.EnterMeetOverview(link);
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException ex)
            {
                // Most likely
                if (link.Contains("/lookup/"))
                {
                    logger.Error(ex, "No meet in lookup link");
                }
                else throw;
            }
        }
        private void Login()
        {
            if (meetBot.State != MeetState.NotLoggedIn) throw new Exception("Wanting to login while logged");
            bool loggedIn = Utils.Retry(meetBot.Login, 3);
            if (!loggedIn)
            {
                throw new Exception("Not logged in");
            }
            logger.Debug("Logged in");
        }

        public void Dispose()
        {
            ((IDisposable)meetBot).Dispose();
        }

    }
}