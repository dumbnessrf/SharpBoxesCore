using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
namespace SharpBoxesCore.Office.Excel;
public interface IExcelExporter
{
    /// <summary>
    /// 导出数据到Excel文件
    /// <example>
    /// <code>
    /// <![CDATA[
    /// var values = new List<Dictionary<string, object>>()
    /// {
    ///     new Dictionary<string,object>{{ "Column1", "MiniExcel" }, { "Column2", 1 } },
    ///     new Dictionary<string,object>{{ "Column1", "Github" }, { "Column2", 2 } }
    /// };
    /// xxxx.SaveAs(path, values)
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    Task ExportToExcelAsync(string filePath, List<Dictionary<string, object>> datas);

    Task<List<T>> ConvertToEntities<T>(string path) where T:class, new();

    Task ExportToExcelAsync<T>(List<T> entities, string filePath) where T:class, new();

    Task<DataTable> ConvertToDataTable(string path);
    Task<List<Dictionary<string, object>>> ConvertToDictionary(string path);


}