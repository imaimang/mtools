using MTools.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTools.Http
{
    public enum RequestMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    public class HttpProxy
    {
        private string baseAddress;

        public HttpProxy(string ip, int port)
        {
            this.baseAddress = String.Format("http://{0}:{1}/", ip, port);
        }

        public  Task<MResult<T>> PostMessage<T>(string url, object? message)
        {
            return  Request<T>(url, message, RequestMethod.Post);
        }

        public async Task<MResult<T>> PutMessage<T>(string url, object? message)
        {
            return await Request<T>(url, message, RequestMethod.Put);
        }

        public async Task<MResult<T>> GetMessage<T>(string url, object? message)
        {
            return await Request<T>(url, message, RequestMethod.Get);
        }

        public async Task<MResult<T>> DeleteMessage<T>(string url, object? message)
        {
            return await Request<T>(url, message, RequestMethod.Delete);
        }

        private async Task<MResult<T>> Request<T>(string url,object? message,RequestMethod requestMethod)
        {
            MResult<T> mResult = new MResult<T>();
            HttpClient client = new HttpClient();
            try
            {
                HttpContent? httpContent = null;
                HttpResponseMessage? httpResponseMessage = null;
                if (url.FirstOrDefault() == '/')
                {
                   url= url.Substring(1);
                }
                switch (requestMethod)
                {
                    case RequestMethod.Delete:
                    case RequestMethod.Get:
                        if (message != null)
                        {
                            url += "?";
                            var type = message.GetType();
                            var properties = type.GetProperties();
                            foreach (var item in properties)
                            {
                                url += item.Name + "=" + item.GetValue(message) + "&";
                            }

                            url = url.Substring(0, url.Length - 1);
                        }
                        break;
                    case RequestMethod.Put:
                    case RequestMethod.Post:
                        if (message != null)
                        {
                            httpContent = ConvertHttpContent(message);
                        }
                        break;
                }

                switch (requestMethod)
                {
                    case RequestMethod.Delete:
                        httpResponseMessage = await client.DeleteAsync(baseAddress + url);
                        break;
                    case RequestMethod.Get:
                        httpResponseMessage = await client.GetAsync(baseAddress + url);
                        break;
                    case RequestMethod.Put:
                        httpResponseMessage = await client.PutAsync(baseAddress + url, httpContent);
                        break;
                    case RequestMethod.Post:
                        httpResponseMessage = await client.PostAsync(baseAddress + url, httpContent);
                        break;
                }

                mResult.StatusCode = httpResponseMessage!.StatusCode;

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    byte[] responseResult = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                    mResult.Content = ConvertResult<T>(responseResult);
                    mResult.IsSuccess = true;
                }
                else
                {
                    
                    mResult.IsSuccess = false;
                    switch (httpResponseMessage.StatusCode)
                    {
                        case System.Net.HttpStatusCode.NotFound:
                            mResult.Error = "Not Found";
                            break;
                        default:
                            string responseResult = await httpResponseMessage.Content.ReadAsStringAsync();
                            mResult.Error = responseResult;
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                mResult.IsSuccess = false;
                mResult.Error = ex.Message;
            }
            finally
            {
                client.Dispose();
            }
            return mResult;
        }

        private T? ConvertResult<T>(byte[] datas)
        {
            var type = typeof(T);

            if (type == typeof(string))
            {
                return (T)Convert.ChangeType(Encoding.UTF8.GetString(datas), typeof(T));
            }
            else if (type == typeof(byte[]))
            {
                return (T)Convert.ChangeType(datas, typeof(T));
            }
            else if (type == typeof(bool))
            {
                if (datas.Length == 0)
                {
                    return (T)Convert.ChangeType(true, typeof(T));
                }
                else
                {
                    return (T)Convert.ChangeType(datas[0], typeof(T));
                }
            }
            else if (type == typeof(int))
            {
                return (T)Convert.ChangeType(int.Parse(Encoding.UTF8.GetString(datas)), typeof(T));
            }
            else
            {
                JsonSerializerSettings setting = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(datas), setting);
            }
        }

        private HttpContent ConvertHttpContent<T>(T message)
        {
            if (message is string msgStr)
                return new StringContent(msgStr);
            else if (message is byte[] msgBuf)
                return new ByteArrayContent(msgBuf);
            else if (message is HttpContent msgHttp)
                return msgHttp;
            else if (message is IEnumerable<KeyValuePair<string, string>> msgDictionary)
                return new FormUrlEncodedContent(msgDictionary);
            else if (message is ValueType value)
                return new StringContent(value.ToString());
            else
                return new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
        }
    }
}
