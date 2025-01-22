namespace SharpBoxesCore.Helpers;

/// <summary>
/// 用于处理HTTP请求的帮助类
/// </summary>
public static class HTTPHelper
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string Get(string url)
    {
        var client = new HttpClient();
        var response = client.GetAsync(url).Result;
        return response.Content.ReadAsStringAsync().Result;
    }

    /// <summary>
    /// 发送GET请求并反序列化为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <returns></returns>
    public static T Get<T>(string url)
    {
        var client = new HttpClient();
        var response = client.GetAsync(url).Result;
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
    public static string Post(string url, string data, Encoding encoding = null)
    {
        var client = new HttpClient();
        var content = new StringContent(data, encoding, "application/json");
        var response = client.PostAsync(url, content).Result;
        return response.Content.ReadAsStringAsync().Result;
    }

    /// <summary>
    /// 发送POST请求并反序列化为指定类型
    /// </summary>
    public static T Post<T>(string url, string data, Encoding encoding = null)
    {
        var client = new HttpClient();
        var content = new StringContent(data, encoding, "application/json");
        var response = client.PostAsync(url, content).Result;
        var contentString = response.Content.ReadAsStringAsync().Result;
        return JsonConvert.DeserializeObject<T>(contentString);
    }

    /// <summary>
    /// <inheritdoc cref="Get(string)" />
    /// </summary>
    public static Task<string> GetAsync(string url)
    {
        var client = new HttpClient();
        return client
            .GetAsync(url)
            .ContinueWith(task => task.Result.Content.ReadAsStringAsync().Result);
    }

    /// <summary>
    /// <inheritdoc cref="Get{T}(string)" />
    /// </summary>
    public static Task<T> GetAsync<T>(string url)
    {
        var client = new HttpClient();
        return client
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
    public static Task<string> PostAsync(string url, string data, Encoding encoding = null)
    {
        var client = new HttpClient();
        var content = new StringContent(data, encoding, "application/json");
        return client
            .PostAsync(url, content)
            .ContinueWith(task => task.Result.Content.ReadAsStringAsync().Result);
    }

    /// <summary>
    /// <inheritdoc cref="Post{T}(string, string, Encoding)" />
    /// </summary>
    public static Task<T> PostAsync<T>(string url, string data, Encoding encoding = null)
    {
        var client = new HttpClient();
        var content = new StringContent(data, encoding, "application/json");
        return client
            .PostAsync(url, content)
            .ContinueWith(task =>
            {
                var contentString = task.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(contentString);
            });
    }
}
