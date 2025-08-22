using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using MiniExcelLibs;

namespace SharpBoxesCore.Office.Excel;

public class ExcelExporter : IExcelExporter
{
    public async Task<DataTable> ConvertToDataTable(string path)
    {
        using var stream = File.OpenRead(path);
        var rows = await stream.QueryAsDataTableAsync();
        return rows;
    }

    public async Task<List<Dictionary<string, object>>> ConvertToDictionary(string path)
    {
        List<Dictionary<string, object>> result = [];
        using var stream = File.OpenRead(path);
        var rows = await stream.QueryAsDataTableAsync();
        foreach (DataRow row in rows.Rows)
        {
            Dictionary<string, object> dict = new();
            foreach (DataColumn column in rows.Columns)
            {
                dict.Add(column.ColumnName, row[column]);
            }
            result.Add(dict);
        }
        return result;
    }

    public async Task<List<T>> ConvertToEntities<T>(string path)
        where T : class, new()
    {
        using var stream = File.OpenRead(path);
        var rows = (await stream.QueryAsync<T>()).ToList();
        return rows;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    public async Task ExportToExcelAsync(string filePath, List<Dictionary<string, object>> datas)
    {
        File.Delete(filePath);
        await MiniExcel.SaveAsAsync(filePath, datas);
    }

    public async Task ExportToExcelAsync<T>(List<T> entities, string filePath)
        where T : class, new()
    {
        File.Delete(filePath);
        await MiniExcel.SaveAsAsync(filePath, entities);
    }


}
