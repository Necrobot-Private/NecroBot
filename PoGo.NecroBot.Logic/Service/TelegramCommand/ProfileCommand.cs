using System;
using System.Linq;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.State;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    // TODO I18N
    public class ProfileCommand : CommandMessage
    {
        public override string Command => "/profile";
        public override bool StopProcess => true;
        public override TranslationString DescriptionI18NKey => TranslationString.TelegramCommandProfileDescription;
        public override TranslationString MsgHeadI18NKey => TranslationString.TelegramCommandProfileMsgHead;

        public ProfileCommand(TelegramUtils telegramUtils) : base(telegramUtils)
        {
        }

        // jjskuld - Ignore CS1998 warning for now.
        #pragma warning disable 1998
        public override async Task<bool> OnCommand(ISession session, string cmd, Action<string> callback)
        {
            var playerStats = (session.Inventory.GetPlayerStats()).FirstOrDefault();
            if (cmd.ToLower() != Command || playerStats == null)
            {
                return false;
            }

            var answerTextmessage = GetMsgHead(session, session.Profile.PlayerData.Username) + "\r\n\r\n";
            var pokemonInBag = session.Inventory.GetPokemons().ToList().Count;
            answerTextmessage += session.Translation.GetTranslation(
                TranslationString.TelegramCommandProfileMsgBody,
                session.Profile.PlayerData.Username,
                playerStats.Level,
                playerStats.Experience,
                playerStats.NextLevelXp - playerStats.Experience,
                playerStats.PokemonsCaptured,
                pokemonInBag - playerStats.PokemonsCaptured,
                pokemonInBag,
                playerStats.Evolutions,
                playerStats.PokeStopVisits,
                session.Inventory.GetTotalItemCount(),
                session.Inventory.GetStarDust(),
                playerStats.EggsHatched,
                playerStats.UniquePokedexEntries,
                playerStats.KmWalked
            );

            callback(answerTextmessage);
            return true;
        }
        #pragma warning restore 1998
    }
}