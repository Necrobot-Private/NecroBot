using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Service;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Tasks
{
    public static class SolveCaptchaTask
    {
        public static async Task<bool> Execute(ISession session, CancellationToken cancellationToken, string url)
        {
            TwoCaptchaService service = new TwoCaptchaService(session);
            var tokenResponse = await service.SolveCaptcha(url);
            if (tokenResponse != null)
            {
                session.Client.SetCaptchaToken(tokenResponse);
                session.EventDispatcher.Send(new NoticeEvent { Message = "Sending Captcha Response to Pokemon GO" });
                var result = await session.Client.Player.VerifyChallenge(tokenResponse);
                return result.Success;
            }
            return false;
        }
    }
}
