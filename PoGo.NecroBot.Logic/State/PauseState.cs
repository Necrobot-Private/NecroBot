#region using directives

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Media;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.Forms;
using System.Net.Http;

#endregion

namespace PoGo.NecroBot.Logic.State
{
    public class PauseState : IState
    {
        public static bool IsRunning;

        public async Task<IState> Execute(ISession session, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(200).ConfigureAwait(false);

            if (IsRunning)
            {
                Logger.Write("Starting Bot...");
                return new VersionCheckState();
            }
            while (!IsRunning)
            {
                Logger.Write("The Bot is Currently Paused, Click 'Play Bot' to Resume", LogLevel.Info);
                await Task.Delay(1000).ConfigureAwait(false);
            }
            return new VersionCheckState();
        }
    }
}
