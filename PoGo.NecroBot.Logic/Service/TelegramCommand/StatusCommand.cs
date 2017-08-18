using System;
using System.Reflection;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    // TODO I18N
    public class StatusCommand : CommandMessage
    {
        public override string Command => "/status";
        public override bool StopProcess => true;
        public override TranslationString DescriptionI18NKey => TranslationString.TelegramCommandStatusDescription;
        public override TranslationString MsgHeadI18NKey => TranslationString.TelegramCommandStatusMsgHead;

        public StatusCommand(TelegramUtils telegramUtils) : base(telegramUtils)
        {
        }

        #pragma warning disable 1998 // added to get rid of compiler warning. Remove this if async code is used below.
        public override async Task<bool> OnCommand(ISession session, string cmd, Action<string> callback)
        #pragma warning restore 1998
        {
            if (cmd.ToLower() == Command)
            {
                var NecroBotVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                var NecroBotStatistics = session.RuntimeStatistics;
                var NecroBotStats = await NecroBotStatistics.GetCurrentInfo(session, session.Inventory).ConfigureAwait(false);

                var answerCatchLimit = "diabled";
                var answerPokestopLimit = "disabled";

                if (session.LogicSettings.UseCatchLimit)
                {
                    answerCatchLimit = string.Format(
                        "{0} / {1}",
                        session.Stats.GetNumPokemonsInLast24Hours(),
                        session.LogicSettings.CatchPokemonLimit
                    );
                }

                if (session.LogicSettings.UsePokeStopLimit)
                {
                    answerPokestopLimit = string.Format(
                        "{0} / {1}",
                        session.Stats.GetNumPokestopsInLast24Hours(),
                        session.LogicSettings.PokeStopLimit
                    );
                }

                var answerTextmessage = GetMsgHead(session, session.Profile.PlayerData.Username) + "\r\n\r\n";

                answerTextmessage += session.Translation.GetTranslation(
                    TranslationString.TelegramCommandStatusMsgBody,
                    NecroBotVersion,
                    session.Profile.PlayerData.Username,
                    NecroBotStatistics.FormatRuntime(),
                    NecroBotStats.Level,
                    NecroBotStats.HoursUntilLvl,
                    NecroBotStats.MinutesUntilLevel,
                    NecroBotStats.LevelupXp - NecroBotStats.CurrentXp,
                    NecroBotStatistics.TotalExperience/NecroBotStatistics.GetRuntime(),
                    NecroBotStatistics.TotalPokemons/NecroBotStatistics.GetRuntime(),
                    NecroBotStatistics.TotalStardust/NecroBotStatistics.GetRuntime(),
                    NecroBotStatistics.TotalPokemonTransferred,
                    NecroBotStatistics.TotalPokemonEvolved,
                    NecroBotStatistics.TotalItemsRemoved,
                    answerPokestopLimit,
                    answerCatchLimit,
                    session.Profile.PlayerData.Currencies[1].Amount,
                    session.Profile.PlayerData.Currencies[0].Amount
                    );

                callback(answerTextmessage);
                return true;
            }
            return false;
        }
    }
}