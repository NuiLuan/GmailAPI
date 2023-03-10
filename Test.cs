using System.Collections.Generic;
using NavigosAT.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Threading;
using System;



using NavigosAT.WrapperFactory;

namespace Test
{

    public class CheckMail
    {
        private string getMsgList_uri = "https://gmail.googleapis.com/gmail/v1/users/me/messages";
        private string token_uri = "https://oauth2.googleapis.com/token";
        private string client_id = "1093416153554-i7u92jlsjdi6udpqecflag8083des7d2.apps.googleusercontent.com";
        private string client_secret = "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";
        private string refresh_token = "1//0e1gCCjIJbsfDCgYIARAAGA4SNwF-L9IrveRgy89qdAc56oakfi0xZ2A-Q_KCmRgdDqMKPOjIqLF4eoJuvtcGaM7MnRb3UMaqrvA";
        private string grant_type = "refresh_token";
        private string scope = "https://mail.google.com/";
        private string access_token = null;

        [Test, Category("CheckEmail")]   
        public void CheckMailForFiveMinutes(){
            AssertWrapper.True(IsReceicedMailForFiveMinutes(), "DID NOT RECEIVED NEW MAIL!");
        }
        //contructor
        public CheckMail(){
            this.access_token = "Bearer " + GetGmailAccessToken();         
            Logger.Log(string.Format("access_token: {0}", access_token));
        }
        private string GetGmailAccessToken()
        {
            Options options = new Options(token_uri, APIMethod.POST);
            options.AddQueryParam("client_id", client_id);
            options.AddQueryParam("client_secret", client_secret);
            options.AddQueryParam("refresh_token", refresh_token);
            options.AddQueryParam("grant_type", grant_type);
            options.AddQueryParam("scope", scope);
            var response = APIHelper.Request(options).Content;
            string token = "";
            Utility.TryGetValueFromJSON(JObject.Parse(response)["access_token"], out token);
            return token;
        }  
        private string GetFirstIdFromMsgListBySubject(string subjectGmail="")
        {
            Options options = new Options(string.Format(getMsgList_uri + "?q=subject:{0}", subjectGmail), APIMethod.GET);
            options.AddHeader("Authorization", access_token);
            var response = APIHelper.Request(options).Content;
            string result = "";
            Utility.TryGetValueFromJSON(JObject.Parse(response)["messages"][0]["id"], out result);
            return result;
        }

        public bool IsReceicedMailForFiveMinutes(){
            // return IsReceicedMailBySubjectForInputtedMinutes(5);
            return IsReceicedMailForInputtedMinutesBySubject(5, "Hello");
        }

        //Method below still can be used without subject
        public bool IsReceicedMailForInputtedMinutesBySubject(int timeoutInMinutes, string subjectGmail="")
        {
            bool isReceivedEmail = false;
            int timeoutInSeconds = timeoutInMinutes*60;
            string idToCheck = GetFirstIdFromMsgListBySubject(subjectGmail);
            string idNewest;
            long expectedTime = GetCurrentUnixTime() + (long)timeoutInSeconds;
            long actualTime;
            do{
                Logger.Log("Checking...!");
                idNewest = GetFirstIdFromMsgListBySubject(subjectGmail);
                if(idNewest!=idToCheck){
                    Logger.Log("Received a new Gmail!");
                    PrintGmailContentByID(idNewest);
                    isReceivedEmail = true;
                    break;
                }
                actualTime = GetCurrentUnixTime();
                Thread.Sleep(10000);
            }while(expectedTime >= actualTime);
            return isReceivedEmail;
        }
        public void PrintGmailContentByID(string idGmail){
            Options options = new Options(string.Format(getMsgList_uri + "/{0}", idGmail), APIMethod.GET);
            options.AddHeader("Authorization", access_token);
            var response = APIHelper.Request(options).Content;
            string result = "";
            Utility.TryGetValueFromJSON(JObject.Parse(response)["snippet"], out result);
            Logger.Log(string.Format("The mail id-{0} with content: {1}", idGmail, result));
        }

        // update to Util (if needed)
        public long GetCurrentUnixTime()
        {
            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            return unixTime;
        }
    }
}
