using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using O2Html;
using O2Html.Dom;
using ReferenceLoopHandling = O2Html.ReferenceLoopHandling;

namespace SharpBoxesCore.O2Html;

/// <summary>
/// 渲染 HTML 时使用的 CSS 主题。
/// </summary>
public enum O2HtmlTheme
{
    /// <summary>浅色主题（默认）。</summary>
    Light,

    /// <summary>深色主题。</summary>
    Dark,
}

public static class O2HtmlHelper
{
    /// <summary>
    /// 用默认选项序列化并渲染。
    /// </summary>
    public static void Render(
        System.Windows.Controls.WebBrowser browser,
        object? obj,
        O2HtmlTheme theme = O2HtmlTheme.Light
    )
    {
        Render(
            browser,
            obj,
            new HtmlSerializerOptions
            {
                ReferenceLoopHandling = ReferenceLoopHandling.IgnoreAndSerializeCyclicReference,
                DoNotSerializeNonRootEmptyCollections = true,
            },
            theme
        );
    }

    /// <summary>
    /// 用指定选项序列化对象为 HTML，并在 WebBrowser 中显示。
    /// 实现概览：Serialize 返回 Dom.Node -> ToHtml() 得到片段 -> 套上完整 HTML 文档和配套 CSS -> 写临时文件 -> 导航。
    /// </summary>
    public static void Render(
        System.Windows.Controls.WebBrowser browser,
        object? obj,
        HtmlSerializerOptions options,
        O2HtmlTheme theme = O2HtmlTheme.Light
    )
    {
        string bodyHtml;
        try
        {
            // 核心调用：O2Html 把对象序列化成 DOM 节点，再转 HTML 字符串
            Node node = HtmlSerializer.Serialize(obj, options);
            bodyHtml = node.ToHtml();
        }
        catch (Exception ex)
        {
            bodyHtml = $"<pre style='color:#b00'>序列化抛异常：\n{WebEscape(ex.ToString())}</pre>";
        }

        string fullHtml = WrapWithStyle(bodyHtml, theme);
        RenderHtml(browser, fullHtml);
    }

    /// <summary>
    /// 直接把一段 inner-HTML 渲染到 WebBrowser。
    /// </summary>
    public static void RenderHtml(System.Windows.Controls.WebBrowser browser, string bodyHtml)
    {
        string path = Path.Combine(Path.GetTempPath(), "o2html_demo.html");
        File.WriteAllText(path, bodyHtml, Encoding.UTF8);
        browser.Navigate(path);
    }

    /// <summary>
    /// 把 O2Html 生成的 HTML 片段套进完整文档，并附上与 O2Html CSS class 对应的样式。
    /// 作用：让 table/property-name/property-value/null 等节点有可读的视觉效果。
    /// </summary>
    /// <param name="bodyHtml">O2Html 生成的 HTML 片段。</param>
    /// <param name="theme">使用的 CSS 主题，仅支持 <see cref="O2HtmlTheme.Light"/> 与 <see cref="O2HtmlTheme.Dark"/>。</param>
    public static string WrapWithStyle(string bodyHtml, O2HtmlTheme theme = O2HtmlTheme.Light)
    {
        return @"<!DOCTYPE html>
<html lang='zh-CN'>
<head>
<meta charset='UTF-8'>"
            + GetStyleCss(theme)
            + @"
</head>
<body>"
            + bodyHtml
            + @"
</body>
</html>";
    }

    private static string GetStyleCss(O2HtmlTheme theme)
    {
        return theme switch
        {
            O2HtmlTheme.Dark => @"<style>
  body { font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif; font-size: 13px; margin: 12px; color: #e6e6e6; background: #1e1e1e; }
  table { border-collapse: collapse; margin: 4px 0; }
  th, td { border: 1px solid #3c3c3c; padding: 3px 8px; vertical-align: top; text-align: left; }
  /* O2Html 表头：类型名行 */
  tr.table-info-header th { background: #16202b; color: #fff; font-weight: 600; }
  /* O2Html 集合的数据列头 */
  tr.table-data-header th { background: #2a2a2a; font-weight: 600; }
  /* 属性名列 */
  th.property-name { background: #242424; color: #c9d1d9; font-weight: 600; white-space: nowrap; }
  /* 属性值列 */
  td.property-value { background: #1e1e1e; }
  /* null 值 */
  .null { color: #8b949e; font-style: italic; }
  /* 空集合 */
  .empty-collection { color: #8b949e; font-style: italic; }
  /* 循环引用标记 */
  .cyclic-reference { color: #ff7b72; font-style: italic; }
  /* 最大深度截断 */
  .max-depth-reached { color: #d29922; font-style: italic; }
  h3 { color: #c9d1d9; }
</style>",
            _ => @"<style>
  body { font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif; font-size: 13px; margin: 12px; color: #222; background: #fff; }
  table { border-collapse: collapse; margin: 4px 0; }
  th, td { border: 1px solid #d0d0d0; padding: 3px 8px; vertical-align: top; text-align: left; }
  /* O2Html 表头：类型名行 */
  tr.table-info-header th { background: #2c3e50; color: #fff; font-weight: 600; }
  /* O2Html 集合的数据列头 */
  tr.table-data-header th { background: #ecf0f1; font-weight: 600; }
  /* 属性名列 */
  th.property-name { background: #f7f9fb; color: #34495e; font-weight: 600; white-space: nowrap; }
  /* 属性值列 */
  td.property-value { background: #fff; }
  /* null 值 */
  .null { color: #999; font-style: italic; }
  /* 空集合 */
  .empty-collection { color: #999; font-style: italic; }
  /* 循环引用标记 */
  .cyclic-reference { color: #c0392b; font-style: italic; }
  /* 最大深度截断 */
  .max-depth-reached { color: #e67e22; font-style: italic; }
  h3 { color: #2c3e50; }
</style>",
        };
    }

    public static string WebEscape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    // ============ WebBrowser 引擎升级 ============

    /// <summary>
    /// 让 WPF WebBrowser 使用 Edge (Chromium) 而非默认 IE7。
    /// 作用：表格、中文、CSS 才能正确渲染。实现概览：写注册表FEATURE_BROWSER_EMULATION，进程名 -> 11001(Edge)。
    /// </summary>
    private static void SetBrowserFeature()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"
        );
        string exe = AppDomain.CurrentDomain.FriendlyName;
        // 11001 = Edge 模式（IE11 仿真，net4.8 WebBrowser 能用的最高档）
        key?.SetValue(exe, 11001, Microsoft.Win32.RegistryValueKind.DWord);
    }
}
