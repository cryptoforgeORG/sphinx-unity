//
//  http://playentertainment.company
//  


using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using UnityEngine;
using UnityEngine.Networking;

namespace PlayEntertainment.Sphinx
{

    public class API
    {
        public string url = string.Empty;
        public string tokenKey = string.Empty;
        public string tokenValue = string.Empty;
        public Action resetIPCallbackurl;

        public MonoBehaviour executor;

        public API(string url, string tokenKey, string tokenValue, MonoBehaviour executor, Action resetIPCallback)
        {
            this.url = url;
            this.tokenKey = tokenKey;
            this.tokenValue = tokenValue;
            this.resetIPCallbackurl = resetIPCallback;
            this.executor = executor;
        }

        public Action<string, object, string, Action<string>> AddMethod(string method, string rootUrl)
        {
            return delegate (string url, object data, string encoding, Action<string> callback)
            {
                // data null check
                bool skip = this.isPublic(url);

                if (!string.IsNullOrEmpty(this.tokenKey) && string.IsNullOrEmpty(this.tokenValue) && !skip)
                {
                    return;
                }

                Dictionary<string, string> headers = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(this.tokenKey) && !string.IsNullOrEmpty(this.tokenValue))
                {
                    headers.Add(this.tokenKey, this.tokenValue);
                }

                Dictionary<string, object> opts = new Dictionary<string, object>();
                opts.Add("mode", "cors");

                if (method == "POST" || method == "PUT")
                {
                    if (!string.IsNullOrEmpty(encoding))
                    {
                        headers.Add("Content-Type", encoding);

                        if (encoding == "application/x-www-form-urlencoded")
                        {
                            opts.Add("body", data);
                            // opts.Add("body", this.makeSearchParams(data));
                        }
                        else
                        {
                            opts.Add("body", data);
                        }
                    }
                    else
                    {
                        headers.Add("Content-Type", "application/json");
                        opts.Add("body", Json.Serialize(data));
                    }
                }

                if (method == "UPLOAD")
                {
                    headers.Add("Content-Type", "multipart/form-data");
                    opts.Add("body", data);
                }

                // opts.headers = new Headers(headers)

                opts.Add("method", method == "UPLOAD" ? "POST" : method);

                if (method == "BLOB")
                    opts["method"] = "GET";


                if (opts["method"].ToString() == "GET")
                {
                    this.executor.StartCoroutine(GetRequest(rootUrl + url, headers, callback));
                }

                if (opts["method"].ToString() == "POST")
                {
                    this.executor.StartCoroutine(PostRequest(rootUrl + url, headers, opts, callback));
                }
            };
        }

        bool isPublic(string url)
        {
            return url.EndsWith("login");
        }

        string makeSearchParams(object data)
        {

            Dictionary<string, object> parameters = data as Dictionary<string, object>;
            // function makeSearchParams(params) {
            //     return Object.keys(params)
            //         .map(key => {
            //         return encodeURIComponent(key) + '=' + encodeURIComponent(params[key])
            //         })
            //         .join('&')
            //     }
            return HttpUtility.UrlEncode(string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))));
        }

        IEnumerator GetRequest(string uri, Dictionary<string, string> headers, Action<string> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    webRequest.SetRequestHeader(entry.Key, entry.Value);
                }
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        callback(webRequest.downloadHandler.text);
                        break;
                }
            }
        }

        IEnumerator PostRequest(string uri, Dictionary<string, string> headers, Dictionary<string, object> opts, Action<string> callback)
        {
            string bodyJsonString = opts["body"].ToString();

            var webRequest = new UnityWebRequest(uri, "POST");

            if (headers["Content-Type"] == "application/json")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                // webRequest.SetRequestHeader("Content-Type", "application/json"); // Pre 'Verify Signature Issue' Line
            }
            else
            {
                WWWForm form = new WWWForm();

                Dictionary<string, object> body = opts["body"] as Dictionary<string, object>;

                foreach (KeyValuePair<string, object> entry in body)
                {
                    form.AddField(entry.Key, entry.Value.ToString());
                }

                webRequest = UnityWebRequest.Post(uri, form);
            }

            foreach (KeyValuePair<string, string> entry in headers)
            {
                webRequest.SetRequestHeader(entry.Key, entry.Value);
            }

            yield return webRequest.SendWebRequest();
            Debug.Log("Status Code: " + webRequest.responseCode);

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    callback(webRequest.downloadHandler.text);
                    break;
            }
        }
    }

}
