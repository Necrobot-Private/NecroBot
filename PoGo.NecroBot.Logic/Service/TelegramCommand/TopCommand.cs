using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.PoGoUtils;
using PoGo.NecroBot.Logic.State;
using POGOProtos.Data;

namespace PoGo.NecroBot.Logic.Service.TelegramCommand
{
    public class TopCommand : CommandMessage
    {
        private const int DeafultTopEntries = 10;

        // TODO Add additional parameter info [n]
        public override string Command => "/top";
        public override bool StopProcess => true;
        public override TranslationString DescriptionI18NKey => TranslationString.TelegramCommandTopDescription;
        public override TranslationString MsgHeadI18NKey => TranslationString.TelegramCommandTopMsgHead;

        public TopCommand(TelegramUtils telegramUtils) : base(telegramUtils)
        {
        }

        public override async Task<bool> OnCommand(ISession session, string cmd, Action<string> callback)
        {
            string[] messagetext = cmd.Split(' ');
            string answerTextmessage = "";

            if (messagetext[0].ToLower() == Command)
            {
                var times = DeafultTopEntries;
                var sortby = "cp";

                if (messagetext.Length >= 2)
                {
                    sortby = messagetext[1];
                }
                if (messagetext.Length == 3)
                {
                    try
                    {
                        times = Convert.ToInt32(messagetext[2]);
                    }
                    catch (FormatException)
                    {
                        answerTextmessage =
                            session.Translation.GetTranslation(TranslationString.UsageHelp, "/top [cp/iv] [amount]");
                    }
                }
                else if (messagetext.Length > 3)
                {
                    answerTextmessage =
                        session.Translation.GetTranslation(TranslationString.UsageHelp, "/top [cp/iv] [amount]");
                }

                IEnumerable<PokemonData> topPokemons = null;
                if (sortby.Equals("iv"))
                {
                    topPokemons = await session.Inventory.GetHighestsPerfect(times);
                }
                else if (sortby.Equals("cp"))
                {
                    topPokemons = await session.Inventory.GetHighestsCp(times);
                }
                else
                {
                    answerTextmessage =
                        session.Translation.GetTranslation(TranslationString.UsageHelp, "/top [cp/iv] [amount]");
                }

                if (topPokemons == null)
                {
                    return true;
                }

                foreach (var pokemon in topPokemons)
                {
                    answerTextmessage += session.Translation.GetTranslation(TranslationString.ShowPokeSkillTemplate,
                        pokemon.Cp, PokemonInfo.CalculatePokemonPerfection(pokemon).ToString("0.00"),
                        session.Translation.GetPokemonMovesetTranslation(PokemonInfo.GetPokemonMove1(pokemon)),
                        session.Translation.GetPokemonMovesetTranslation(PokemonInfo.GetPokemonMove2(pokemon)),
                        session.Translation.GetPokemonTranslation(pokemon.PokemonId));

                    if (answerTextmessage.Length > 3800)
                    {
                        callback(answerTextmessage);
                        answerTextmessage = "";
                    }
                }

                callback(answerTextmessage);
                return true;
            }
            return false;
        }
    }
}