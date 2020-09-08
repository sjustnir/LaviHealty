using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;

namespace LaviHealty
{
    public partial class Default : Page
    {
        private static readonly Dictionary<string, Parent> Parents =
        new Dictionary<string, Parent>
        { 
            {
                "Nir",
                new Parent
                {
                    FirstName = "ניר", LastName = "צבר", Id = "200479400", PhoneNumber = "0506714186",
                    Email = "sjustnir@gmail.com"
                }
            },
            {
                "Dana",
                new Parent
                {
                    FirstName = "דנה", LastName = "צבר", Id = "200635167", PhoneNumber = "0542145402",
                    Email = "danas04@gmail.com"
                }
            }
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }
            SendAsDropDown.Items.Clear();
            SendAsDropDown.Items.AddRange(Parents.Select(p=> new ListItem(p.Key, p.Key)).ToArray());
            SendAsDropDown.SelectedIndex = 0;
        }

        protected void SendConfirmationButton_OnClick(object sender, EventArgs e)
        {
            var result = GetResultText();
            var isSuccess = false;
            if (!result.IsNullOrWhiteSpace())
            {
                var regex = new Regex(".*\"statusCode\":(\\d+),.*");
                var match = regex.Match(result);
                if (match.Success && int.Parse(match.Groups[1].Value) == 0)
                {
                    var regex2 = new Regex(".*\\\\\"Code\\\\\":\\\\\"(\\d+)\\\\\",.*");
                    var match2 = regex2.Match(result);
                    isSuccess = (int.Parse(match2.Groups[1].Value) == 0);
                }
            }
            
            ResultLabel.Text = isSuccess ? "Confirmation was sent successfully" : $"Failure: {result ?? "Unknown issue"}";
            DisableSubmissionControls();
            ResultLabel.Visible = true;
            LinkToSiteLabel.Visible = !isSuccess;
            SitePanel.Update();
        }

        private void DisableSubmissionControls()
        {
            SendConfirmationButton.Text = "Sent!";
            SendConfirmationButton.OnClientClick = null;
            SendConfirmationButton.Enabled = false;
            SendConfirmationButton.CssClass = "btn-primary gray";
            SendAsDropDown.Enabled = false;
        }

        private string GetResultText()
        {
            var uuid = GetGetResult();
            return uuid == null ? "Failed to get requestID" : GetPostRequestResult(uuid);
        }

        private static string GetGetResult()
        {
            string getRequestResult;
            try
            {
                const string url = "https://govforms.gov.il/mw/forms/ChildHealthDeclaration@molsa.gov.il";

                var request = (HttpWebRequest) WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (var response = (HttpWebResponse) request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    getRequestResult = reader.ReadToEnd();
                }
                var regex = new Regex(".*\"requestID\":\"([0-9a-z\\-]+)\".*");
                var match = regex.Match(getRequestResult);
                if (match.Success) return match.Groups[1].Value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return getRequestResult;
        }

        private string GetPostRequestResult(string uuid)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create("https://govforms.gov.il/MW/Process/Data/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var todayDate = DateTime.Now.ToString("dd/MM/yyyy");
            var parent = Parents[SendAsDropDown.SelectedItem.Value];
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = "{\"requestID\":\"" + uuid + "\",\"processID\":null," +
                           "\"formData\":{\"declarationProperties\":{\"childInformation\":{\"idNum\":\"226745099\"," +
                           "\"lastName\":\"צבר\",\"firstName\":\"לביא\"},\"parentInformation\":{\"idNum\":\"" + parent.Id + "\"," +
                           "\"lastName\":\"" + parent.LastName + "\",\"firstName\":\"" + parent.FirstName + "\"},\"childBirthDate\":\"20/03/2019\"," +
                           "\"daycareManager\":\"טלי\",\"dayCareCity\":{\"dataCode\":\"4000\"," +
                           "\"dataText\":\"חיפה\"},\"dayCareName\":{\"dataCode\":\"2127\"," +
                           "\"dataText\":\"שם מסגרת: מת\\\"מ-ב', סמל מעון: 2127, כתובת: מרכז תעשיות מדע\"}," +
                           "\"parentMobile\":\"" + parent.PhoneNumber + "\",\"parentEmail\":\"" + parent.Email + "\"," +
                           "\"parentFirstDeclaration\":true,\"parentSecondDeclaration\":true,\"parentDeclaration3\":true," +
                           "\"declarationDate\":\"" + todayDate +
                           "\",\"name\":\"declarationProperties\",\"state\":\"completed\"," +
                           "\"next\":\"\",\"prev\":\"\",\"isClosed\":true},\"containersViewModel\":{\"showPrintButton\":false," +
                           "\"currentContainerName\":\"declarationProperties\",\"validatedStatus\":true}," +
                           "\"formInformation\":{\"isFormSent\":false,\"loadingDate\":\"" + todayDate + "\"," +
                           "\"firstLoadingDate\":\"\",\"isMobile\":false,\"language\":\"hebrew\"}},\"language\":\"he\",\"attachments\":[]}";
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            string result = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new EvaluateException()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private new class Parent
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Id { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
        }
    }
}