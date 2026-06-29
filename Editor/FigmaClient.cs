using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;

namespace FigmaClient
{
    public class Client
    {
        private static string ImagesUrl = "https://api.figma.com/v1/images/{0}?ids={1}&format=png&scale=1";
        private static string GetFileNodes  = "https://api.figma.com/v1/files/{0}/nodes?ids={1}"; // 0: file key; 1: ids

        private static HttpClient client = new HttpClient()
        {
            Timeout = System.TimeSpan.FromMinutes(5) // 5 minute timeout for large image downloads
        };

        private static readonly Regex ImagesJsonRegex = new Regex("\"([^\"]+)\":\"(https://[^\"]+)\"", RegexOptions.Compiled);

        public static async Task<string> GetNodeDataAsync(string fileKey, string nodeId,  string token)
        {
            string apiUrl = string.Format(GetFileNodes, fileKey, nodeId);
            Debug.Log($"start get node info {apiUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("X-Figma-Token", token);

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string nodeData = await response.Content.ReadAsStringAsync();
                Debug.Log($"get node info {apiUrl} success: {nodeData}");
                return nodeData;
            }
            else
            {
                Debug.LogError($"get node info {apiUrl} fail: {response.ReasonPhrase}");
                return null;
            }
        }

        public static async Task<Dictionary<string, string>> GetImageDownloadUrls(string fileKey, IEnumerable<string> nodeIds, string token)
        {
            string ids = string.Join(",", nodeIds);
            string url = string.Format(ImagesUrl, fileKey, ids);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Figma-Token", token);
            
            Debug.Log($"start get batch image download urls, {string.Join(",", nodeIds)} ids, with token {token.Substring(0,6)}...");
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var result = new Dictionary<string, string>();
                var matches = ImagesJsonRegex.Matches(content);
                foreach (Match match in matches)
                {
                    result[match.Groups[1].Value] = match.Groups[2].Value;
                }
                if (result.Count > 0)
                {
                    Debug.Log($"get batch image download urls success, got {result.Count} urls");
                    return result;
                }
                Debug.LogError($"get batch image download urls FAIL: no images in response\n{content}");
                return null;
            }
            else
            {
                Debug.LogError($"get batch image download urls FAIL {response.ReasonPhrase}");
                return null;
            }
        }
        
        public static async Task<byte[]> DownloadImageDataAsync(string nodeImageUrl, string nodeId)
        {
            if (string.IsNullOrEmpty(nodeImageUrl)) return null;

            Debug.Log($"start download node image {nodeId} : {nodeImageUrl}");
            try
            {
                HttpResponseMessage response = await client.GetAsync(nodeImageUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    Debug.Log($"download node image {nodeId} success");
                    return data;
                }
                else
                {
                    Debug.LogError($"download node image {nodeId} fail: {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"download node image {nodeId} exception: {e.Message}");
                return null;
            }
        }
    }
}