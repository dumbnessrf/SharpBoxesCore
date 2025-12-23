namespace SharpBoxesCore.Helpers;

/// <summary>
/// 用于处理HTTP请求的帮助类
/// </summary>
public static class HTTPHelper
{
   static HttpClient _client = new HttpClient();
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static HttpContent Get(string url)
    {
        
        var response = _client.GetAsync(url).Result;
        return response.Content;
    }

    /// <summary>
    /// 发送GET请求并反序列化为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public static T Get<T>(string url)
    {
        
        var response = _client.GetAsync(url).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<T>(content);
    }

    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static HttpContent Post(string url, string data, Encoding encoding = null)
    {
        
        var content = new StringContent(data, encoding, "application/json");
        var response = _client.PostAsync(url, content).Result;
        return response.Content;
    }

    /// <summary>
    /// 发送POST请求并反序列化为指定类型
    /// </summary>
    public static T Post<T>(string url, string data, Encoding encoding = null)
    {
        
        var content = new StringContent(data, encoding, "application/json");
        var response = _client.PostAsync(url, content).Result;
        var contentString = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<T>(contentString);
    }

    public static Task<HttpContent> GetAsync(string url)
    {
        
        return _client.GetAsync(url).ContinueWith(task => task.Result.Content);
    }

    /// <summary>
    /// <inheritdoc cref="Get{T}(string)" />
    /// </summary>
    public static Task<T> GetAsync<T>(string url)
    {
        
        return _client
            .GetAsync(url)
            .ContinueWith(task =>
            {
                var content = task.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(content);
            });
    }

    /// <summary>
    /// <inheritdoc cref="Post(string, string, Encoding)" />
    /// </summary>
    public static Task<HttpContent> PostAsync(string url, string data, Encoding encoding = null)
    {
        
        var content = new StringContent(data, encoding, "application/json");
        return _client.PostAsync(url, content).ContinueWith(task => task.Result.Content);
    }

    /// <summary>
    /// <inheritdoc cref="Post{T}(string, string, Encoding)" />
    /// </summary>
    public static Task<T> PostAsync<T>(string url, string data, Encoding encoding = null)
    {
        
        var content = new StringContent(data, encoding, "application/json");
        return _client
            .PostAsync(url, content)
            .ContinueWith(task =>
            {
                var contentString = task.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(contentString);
            });
    }
}
