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
        protected void Page_Load(object sender, EventArgs e)
        {
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
            DisableSubmissionButton();
            ResultLabel.Visible = true;
            LinkToSiteLabel.Visible = !isSuccess;
            SitePanel.Update();
        }

        private void DisableSubmissionButton()
        {
            SendConfirmationButton.Text = "Sent!";
            SendConfirmationButton.OnClientClick = null;
            SendConfirmationButton.Enabled = false;
            SendConfirmationButton.CssClass = "btn-primary gray";
        }

        private static string GetResultText()
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

        private static string GetPostRequestResult(string uuid)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create("https://govforms.gov.il/MW/Process/Data/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var todayDate = DateTime.Now.ToString("dd/MM/yyyy"); ;
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = "{\"requestID\":\"" + uuid + "\",\"processID\":null," +
                           "\"formData\":{\"declarationProperties\":{\"childInformation\":{\"idNum\":\"226745099\"," +
                           "\"lastName\":\"צבר\",\"firstName\":\"לביא\"},\"parentInformation\":{\"idNum\":\"200635167\"," +
                           "\"lastName\":\"צבר\",\"firstName\":\"דנה\"},\"childBirthDate\":\"20/03/2019\"," +
                           "\"daycareManager\":\"טלי\",\"dayCareCity\":{\"dataCode\":\"4000\"," +
                           "\"dataText\":\"חיפה\"},\"dayCareName\":{\"dataCode\":\"2127\"," +
                           "\"dataText\":\"שם מסגרת: מת\\\"מ-ב', סמל מעון: 2127, כתובת: מרכז תעשיות מדע\"}," +
                           "\"parentMobile\":\"0542145402\",\"parentEmail\":\"danas04@gmail.com\"," +
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
    }
}