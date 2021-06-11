using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Xunfei
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "itrans.xfyun.cn";
            string url = string.Format("https://{0}/v2/its",host);
            // 原文
            string q = "amyotrophic lateral sclerosis";
            q = "今天天气怎么样？";
            //q = "中华人民共和国于1949年成立";
            //q = "";
            // 源语言
            string from = "cn";
            // 目标语言
            string to = "en";

            string apikey = "yourkey";
            string apisecret = "yoursecret";
            string appid = "yourid";

            string requestJson = BuildXunFeiRequestJson(appid, from, to, q);

            string Digest = "SHA-256=" + ComputeHash256_base64(requestJson, new SHA256CryptoServiceProvider());

            DateTime dateTime = DateTime.UtcNow;
            string dateStr = dateTime.ToString("r");
            Console.WriteLine(dateStr);

            string signature = string.Format("host: {0}\ndate: {1}\nPOST /v2/its HTTP/1.1\ndigest: {2}", host, dateStr, Digest);
            Console.WriteLine(signature);

            string signature_sha = hmacsha256(signature, apisecret);
            Console.WriteLine(signature_sha);

            string Authorization = string.Format("api_key=\"{0}\", algorithm=\"hmac-sha256\", headers=\"host date request-line digest\", signature=\"{1}\"", apikey, signature_sha);
            Console.WriteLine(Authorization);

            //post
            byte[] byteArray = Encoding.UTF8.GetBytes(requestJson);
            HttpWebRequest httpWebRequest;
            //HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                // 这里设置了协议类型。
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
                httpWebRequest.KeepAlive = false;
                ServicePointManager.CheckCertificateRevocationList = true;
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.Expect100Continue = false;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = byteArray.Length;
            httpWebRequest.Accept = "application/json";

            httpWebRequest.Date = dateTime;
            httpWebRequest.Host = host;
            httpWebRequest.Headers.Add("Digest", Digest);
            httpWebRequest.Headers.Add("Authorization", Authorization);

            httpWebRequest.KeepAlive = false;

            int respondCode = 0;
            string translation = string.Empty;
            string respondStr = string.Empty;

            try
            {
                using (Stream reqStream = httpWebRequest.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    respondCode = (int)webResponse.StatusCode;
                    if (respondCode == 200)
                    {
                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            respondStr = sr.ReadToEnd();
                        }

                        //sw.WriteLine("result");
                        //translation = GetTranslationFromKingsoftJSON(respondStr);

                        translation = respondStr;

                        Console.WriteLine(translation);

                        dynamic TempResult = JsonConvert.DeserializeObject(respondStr);
                        respondStr = Convert.ToString(TempResult["data"]["trans_result"]["dst"]);

                        Console.WriteLine(respondStr);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        protected static string ComputeHash256_base64(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

            return Convert.ToBase64String(hashedBytes);
        }

        private static string BuildXunFeiRequestJson(string appid, string from, string to, string text)
        {
            common c = new common();
            c.app_id = appid;

            business b = new business();
            b.from = from;
            b.to = to;

            data d = new data();
            d.text = EncryptBase64(text);

            XunFeiRequestClass xunFeiRequestClass = new XunFeiRequestClass();

            xunFeiRequestClass.c = c;
            xunFeiRequestClass.b = b;
            xunFeiRequestClass.d = d;

            return JsonConvert.SerializeObject(xunFeiRequestClass);

        }

        private static string EncryptBase64(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        private static string hmacsha256(string text, string secret)
        {
            string signRet = string.Empty;
            using (HMACSHA256 mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = mac.ComputeHash(Encoding.UTF8.GetBytes(text));
                signRet = Convert.ToBase64String(hash);
            }
            return signRet;

        }

        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

    }
}
