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

        public override async Task<bool> OnCommand(ISession session, string cmd, Action<string> callback)
        {
            var playerStats = session.Inventory.GetPlayerStats().Result.FirstOrDefault();

            if (cmd.ToLower() != Command || playerStats == null)
            {
                return false;
            }

            var answerTextmessage = GetMsgHead(session, session.Profile.PlayerData.Username) + "\r\n\r\n";
            answerTextmessage += string.Format(
                "Account: {0}\n" +
                "Level: {1}\n" +
                "Total XP: {2}\n" +
                "XP until level up: {3}\n" +
                "Pokemon caught: {4}\n" +
                "Pokemon sent: {5}\n" +
                "Pokemon in bag: {6}\n" +
                "Pokemon evolved: {7}\n" +
                "Pokestops visited: {8}\n" +
                "Items in bag: {9}\n" +
                "Stardust: {10}\n" +
                "Eggs hatched: {11}\n" +
                "Pokedex entries: {12}\n" +
                "KM walked: {13}",
                session.Profile.PlayerData.Username,
                playerStats.Level,
                playerStats.Experience,
                playerStats.NextLevelXp - playerStats.Experience,
                playerStats.PokemonsCaptured,
                playerStats.PokemonDeployed,
                (await session.Inventory.GetPokemons()).ToList().Count,
                playerStats.Evolutions,
                playerStats.PokeStopVisits,
                (await session.Inventory.GetTotalItemCount()),
                session.Inventory.GetStarDust(),
                playerStats.EggsHatched,
                playerStats.UniquePokedexEntries,
                playerStats.KmWalked
            );

            callback(answerTextmessage);
            return true;
        }
    }
}