using MTools.Crypto;
using MTools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MTools.Auth
{
    public static class DigestAuth
    {
        public static async Task<MResult<HttpClient>> Login(string ip, int port, string url, string userName, string userPwd,string method)
        {
            MResult<HttpClient> mResult = new MResult<HttpClient>();
            string realm = string.Empty;
            string qop = string.Empty;
            string nonce = string.Empty;
            string opaque = string.Empty;
            //string method = method;
            string path = string.Format("http://{0}:{1}{2}", ip, port, url);
         
            var handler = new HttpClientHandler() { UseCookies = true};
            HttpClient client = new HttpClient(handler);
            client.MaxResponseContentBufferSize = 256000;
     
            var response = await client.GetAsync(path);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                foreach (var item in response.Headers.WwwAuthenticate)
                {
                    if (item.Scheme.ToUpper() == "DIGEST")
                    {
                        if (!string.IsNullOrEmpty(item.Parameter))
                        {
                            var items1 = item.Parameter.Split(",");
                            foreach (var item1 in items1)
                            {
                                var index1 = item1.IndexOf('"');
                                var index2 = item1.LastIndexOf('"');
                                var key = item1.Substring(0, index1 - 1).Trim();
                                var value = item1.Substring(index1 + 1, index2 - index1 - 1).Trim();
                                switch (key)
                                {
                                    case "realm":
                                        realm = value;
                                        break;
                                    case "nonce":
                                        nonce = value;
                                        break;
                                    case "qop":
                                        qop = value;
                                        break;
                                    case "opaque":
                                        opaque = value;
                                        break;
                                }
                            }
                        }
                        break;
                    }
                }

                string nc = DateTime.Now.ToString("yyMMddHHmmss");
                string cnonce = Guid.NewGuid().ToString();

                string ha1 = MD5Helper.Encrypt32(userName + ":" + realm + ":" + userPwd);
                string ha2 = MD5Helper.Encrypt32(method + ":" + url);
                string encryptResult = MD5Helper.Encrypt32(ha1 + ":" + nonce + ":" + nc + ":" + cnonce + ":" + qop + ":" + ha2);

                StringBuilder sb = new StringBuilder();
                sb.Append("Digest  username=\"" + userName + "\", realm=\"" + realm + "\", nonce=\"" + nonce + "\", uri=\"" + url + "\", ");
                sb.Append("qop=\"" + qop + "\", nc=\"" + nc + "\", cnonce=\"" + cnonce + "\", response=\"" + encryptResult + "\", opaque=\"" + opaque + "\"");
                client.DefaultRequestHeaders.Add("Authorization", sb.ToString());
                switch (method)
                {
                    case "POST":
                        response = await client.PostAsync(path, null);
                        break;
                    case "PUT":
                        response = await client.PutAsync(path, null);
                        break;
                    case "GET":
                        response = await client.GetAsync(path);
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(path);
                        break;
                }

                if (response.IsSuccessStatusCode)
                {
                    mResult.IsSuccess = true;
                    mResult.Content = client;
                }
            }
            else if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                mResult.IsSuccess = false;
                mResult.Error = await response.Content.ReadAsStringAsync();
            }
            return mResult;
        }
    }
}
