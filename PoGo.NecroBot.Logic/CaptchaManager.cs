using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PoGo.NecroBot.Logic.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic
{
    public class CaptchaManager
    {
        public static object ManualResolveCaptcha(ISession session, string url)
        {
            try
            {
                IWebDriver driverOne = new ChromeDriver(@"C:\Users\Khoaimap\Downloads\chromedriver_win32");
                driverOne.Navigate().GoToUrl(url);

                var ele = driverOne.FindElement(By.Id("g-recaptcha-response"));
                string token = ele.GetAttribute("value");
                session.Client.SetCaptchaToken(token);
                var verified = session.Client.Player.VerifyChallenge(token).Result;
            }
            catch (Exception ex)
            {

            }

            return true;
        }
    }
}
