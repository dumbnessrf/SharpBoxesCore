using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace SharpBoxesCore.DataStruct;

/// <summary>
/// 通用结果类
/// </summary>
public class Result
{
    /// <summary>
    /// 获取或设置操作是否成功的标志
    /// </summary>
    public bool IsSuccess { get; set; }

    public bool IsFail => !IsSuccess;

    /// <summary>
    /// 获取或设置操作结果的消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 获取或设置操作结果的代码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 初始化 Result 类的新实例
    /// </summary>
    /// <param name="success">操作是否成功的标志</param>
    /// <param name="message">操作结果的消息，默认为空字符串</param>
    /// <param name="code">操作结果的代码，默认为 0</param>
    public Result(bool success, string message = "", int code = 0)
    {
        IsSuccess = success;
        Message = message;
        Code = code;
    }

    /// <summary>
    /// 创建一个表示操作成功的 Result 实例
    /// </summary>
    /// <param name="message">操作结果的消息，默认为空字符串</param>
    /// <param name="code">操作结果的代码，默认为 0</param>
    /// <returns>表示操作成功的 Result 实例</returns>
    public static Result ReturnSuccess(string message = "", int code = 0)
    {
        return new Result(true);
    }

    /// <summary>
    /// 创建一个表示操作失败的 Result 实例
    /// </summary>
    /// <param name="message">操作结果的消息，默认为空字符串</param>
    /// <param name="code">操作结果的代码，默认为 0</param>
    /// <returns>表示操作失败的 Result 实例</returns>
    public static Result ReturnFail(string message = "", int code = 0)
    {
        return new Result(false, message, code);
    }

    /// <summary>
    /// 表示操作成功状态的静态实例
    /// </summary>
    public static Result Success => new(true);

    /// <summary>
    /// 表示操作失败状态的静态实例
    /// </summary>
    public static Result Fail => new(false);

    public static Result AuthorityFail => new(false, "权限不足", 403);

    public static Result NotFoundFail => new(false, "未找到资源", 404);

    public static Result ServerErrorFail => new(false, "服务器错误", 500);

    public static Result ParameterErrorFail => new(false, "参数错误", 400);

    public static Result NotInitializedFail => new(false, "未初始化", 500);

    public static Result NotSupportedFail => new(false, "不支持", 500);
}


public class Result<T>: Result
{
    public T Data { get; set; }

    public Result(bool success, T data, string message = "", int code = 0) : base(success, message, code)
    {
        Data = data;
    }

    public static Result<T> ReturnSuccess(T data, string message = "", int code = 0)
    {
        return new Result<T>(true, data, message, code);
    }

    public static Result<T> ReturnFail(T data, string message = "", int code = 0)
    {
        return new Result<T>(false, data, message, code);
    }

}