using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Model;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service
{
    public class TwoCaptchaService
    {
        private readonly string CaptchaKey = "## Your Captcha Key ##";
        private readonly string CaptchaIn = "http://2captcha.com/in.php?";
        private readonly string CaptchaOut = "http://2captcha.com/res.php?";
        public async Task<string> SolveCaptcha(ISession session, string url)
        {
            session.EventDispatcher.Send(new NoticeEvent { Message = "Getting Site Key for Captcha" });
            var siteResponse = await GetCaptchaSiteResponse(url);
            var m = Regex.Match(siteResponse, "data-sitekey=\"(.*)\"");
            var siteKey = m.Groups[1];
            session.EventDispatcher.Send(new NoticeEvent { Message = "Sending Captcha Solve Request to 2Captcha" });
            MethodResult result = await SendCaptchaSolveRequest(url, siteKey.Value);
            if (result.Success)
            {
                var response = await GetSolvedCaptchaResult(session, result.CaptchaId);
                if (response.Success)
                {
                    return response.CaptchaResponse;
                }
            }
            return null;
        }

        private async Task<string> GetCaptchaSiteResponse(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 10_0 like Mac OS X) AppleWebKit/602.1.38 (KHTML, like Gecko) Version/10.0 Mobile/14A300 Safari/602.1");
                var content = await client.GetStringAsync(url);
                return content;
            }
        }

        private async Task<MethodResult> GetSolvedCaptchaResult(ISession session, string captchaId)
        {
            var methodResult = new MethodResult();
            var postData = $"key={CaptchaKey}&action=get&id={captchaId}";

            try
            {
                session.EventDispatcher.Send(new NoticeEvent { Message = "Trying to get solve Captcha from 2Captcha" });
                string result = await SendRecaptchav2RequestTask(CaptchaOut, postData);
                while (result.Contains("CAPCHA_NOT_READY"))
                {
                    session.EventDispatcher.Send(new NoticeEvent { Message = "Captcha not ready yet" });
                    await Task.Delay(3000);
                    result = await SendRecaptchav2RequestTask(CaptchaOut, postData);
                }

                if (result.Contains("OK|"))
                {
                    session.EventDispatcher.Send(new NoticeEvent { Message = "Captcha Solved. Getting back response" });
                    methodResult.CaptchaResponse = result.Substring(3, result.Length - 3);
                    methodResult.Success = true;
                }
                else
                {
                    methodResult.Error = new Exception(result);
                    methodResult.Success = false;
                }
            }
            catch (Exception ex)
            {
                methodResult.Error = ex;
                methodResult.Success = false;
            }

            return methodResult;
        }

        private async Task<MethodResult> SendCaptchaSolveRequest(string url, string siteKey)
        {
            var methodResult = new MethodResult();
            var postData =
                    $"key={CaptchaKey}&method=userrecaptcha&googlekey={siteKey}&pageurl={url}";
            try
            {
                string result = await SendRecaptchav2RequestTask(CaptchaIn, postData);

                if (result.Contains("OK|"))
                {
                    methodResult.CaptchaId = result.Substring(3, result.Length - 3);
                    methodResult.Success = true;
                }
                else
                {
                    methodResult.Error = new Exception(result);
                    methodResult.Success = false;
                }
            }
            catch (Exception ex)
            {
                methodResult.Error = ex;
                methodResult.Success = false;
            }

            return methodResult;
        }

        private static async Task<string> SendRecaptchav2RequestTask(string url, string post)
        {
            //POST

            return await Task.Run(() =>
            {
                ServicePointManager.Expect100Continue = false;
                var request = (HttpWebRequest)WebRequest.Create(url + post);
                request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 10_0 like Mac OS X) AppleWebKit/602.1.38 (KHTML, like Gecko) Version/10.0 Mobile/14A300 Safari/602.1";
                var data = Encoding.ASCII.GetBytes(post);

                request.Method = "POST";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    return responseString;
                }
            });
        }
    }
}
