using System.Web.Mvc;
using TextMe.Models;

namespace TextMe.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Xml.Linq;

    using Twilio;
    using Twilio.Clients;
    using Twilio.Exceptions;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public class HomeController : Controller
    {
        // Set our Account SID and Auth Token
        private const string accountSid = "YOUR_TWILIO_ACCOUNT_SID";
        private const string authToken = "YOUR_TWILIO_AUTH_TOKEN";
        public PhoneNumber To;
        public static readonly PhoneNumber From = new PhoneNumber("YOUR_TWILIO_NUMBER");

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: Mobile
        public ActionResult Mobile()
        {
            return View();
        }

        public ActionResult SetEvent(EventModel model)
        {
            /* curl 'https://api.twilio.com/2010-04-01/Accounts/AC6211becd387f897d31b97fee2bf6d713/Messages.json' -X POST \
            --data-urlencode 'To=4807790007' \
            --data-urlencode 'From=+16237772199' \
            --data-urlencode 'Body=stuff' \
            -u AC6211becd387f897d31b97fee2bf6d713:2f704ce1c6ec3c68977c320e3507dc21 */
            var response = string.Empty;
            if (ModelState.IsValid)
            {
                try
                {
                    To = new PhoneNumber(model.Phone);

                    if (model.Action.Equals("Text"))
                        response = SendText(model);
                    else if (model.Action.Equals("Call"))
                        response = MakeCall(model);
                }
                catch (Exception e)
                {
                    return Json(e.Message);
                }

                return Json(response);
            }

            return Json("invalid input");
        }

        private string SendText(EventModel model)
        {
            // let the thread run until it is time to send the message!
            while (DateTime.UtcNow < model.Time)
            {
                Thread.Sleep(60000);
            }

            // Initialize the Twilio client
            TwilioClient.Init(accountSid, authToken);
            
            var message = MessageResource.Create(
                To,
                from: From,
                body: model.Message);

            // return ("UtcNow: " + DateTime.UtcNow.ToString() + ", model.Time: " + model.Time.ToString() + ", " + DateTime.Now.ToString());
            return "Message sent!";
        }

        private string MakeCall(EventModel model)
        {
            // Initiate a new outbound call
            var filename = $"{model.Title.Replace(" ", "").ToLower()}{model.Phone.Replace("+", "")}.xml";
            var sitepath = Server.MapPath("~");
            var filepath = $"{sitepath}content\\voice\\{filename}";
                
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            var xml = new XElement("Response",
                new XElement("Say",
                    new XAttribute("voice", "alice"), model.Message));

            xml.Save(filepath);

            //return "sitepath: " + sitepath +
            //       "\nfilepath: " + filepath +
            //       "\ncurrent site: " + Server.MapPath(".") +
            //       "\ncurrent drectory: " + Environment.CurrentDirectory +
            //       "\nwebsite: " + Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + "/content/voice/" + filename;


            var url = new Uri($"{Request.Url.Scheme}{Uri.SchemeDelimiter}{Request.Url.Host}/content/voice/{filename}");


            // let the thread run until it is time to send the message!
            while (DateTime.UtcNow < model.Time)
            {
                Thread.Sleep(60000);
            }

            // Initialize the Twilio client
            TwilioClient.Init(accountSid, authToken);

            var call = CallResource.Create(To, From, url: url, method: "GET");

            return filepath;
        }
    }
}
