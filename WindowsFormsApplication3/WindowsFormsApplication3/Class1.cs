﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace WindowsFormsApplication3
{
    public static class WebRequestUtil
    {

        private static bool hasInit = false;

        private static void Init()
        {
            ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, ssl) => true; // ignore ssl as we're querying localhost
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            hasInit = true;
        }

        public static async Task<string> GetResponse(string url)
        {
            if (!hasInit) Init();
            string json = "";
            
            WebRequest request = HttpWebRequest.Create(url);
            


            try
            {
                using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                    return json;
                }
            }
            catch (WebException e)
            {
                throw new WebException("Error retrieving " + url, e);
            }
        }

        public async static Task<bool> IsLive(string url)
        {
            if (!hasInit) Init();
            WebRequest request = HttpWebRequest.Create(url);
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine("FAIL1" +e);
                return false;
            }
        }
    }
}

