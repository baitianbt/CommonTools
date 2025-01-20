using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace CommonTools.Core.Office;

/// <summary>
/// Excel处理帮助类
/// </summary>
public static class ExcelHelper
{
    #region 读取操作
    /// <summary>
    /// 读取Excel文件内容
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetIndex">工作表索引（默认第一个）</param>
    /// <returns>返回数据列表，每行数据为一个字符串数组</returns>
    public static List<string[]> ReadExcel(string filePath, int sheetIndex = 0)
    {
        var result = new List<string[]>();
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        IWorkbook workbook = Path.GetExtension(filePath).ToLower() == ".xlsx" 
            ? new XSSFWorkbook(fs) 
            : (IWorkbook)new HSSFWorkbook(fs);

        var sheet = workbook.GetSheetAt(sheetIndex);
        for (int i = 0; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) continue;

            var rowData = new string[row.LastCellNum];
            for (int j = 0; j < row.LastCellNum; j++)
            {
                var cell = row.GetCell(j);
                rowData[j] = GetCellValue(cell);
            }
            result.Add(rowData);
        }
        return result;
    }

    /// <summary>
    /// 读取Excel指定列
    /// </summary>
    public static List<string> ReadColumn(string filePath, int columnIndex, int sheetIndex = 0)
    {
        var result = new List<string>();
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        IWorkbook workbook = Path.GetExtension(filePath).ToLower() == ".xlsx"
            ? new XSSFWorkbook(fs)
            : (IWorkbook)new HSSFWorkbook(fs);

        var sheet = workbook.GetSheetAt(sheetIndex);
        for (int i = 0; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) continue;

            var cell = row.GetCell(columnIndex);
            result.Add(GetCellValue(cell));
        }
        return result;
    }
    #endregion

    #region 写入操作
    /// <summary>
    /// 创建Excel文件
    /// </summary>
    /// <param name="data">数据内容</param>
    /// <param name="filePath">保存路径</param>
    /// <param name="sheetName">工作表名称</param>
    public static void CreateExcel(IEnumerable<string[]> data, string filePath, string sheetName = "Sheet1")
    {
        IWorkbook workbook = Path.GetExtension(filePath).ToLower() == ".xlsx" 
            ? new XSSFWorkbook() 
            : (IWorkbook)new HSSFWorkbook();

        var sheet = workbook.CreateSheet(sheetName);
        int rowIndex = 0;

        foreach (var rowData in data)
        {
            var row = sheet.CreateRow(rowIndex++);
            for (int i = 0; i < rowData.Length; i++)
            {
                row.CreateCell(i).SetCellValue(rowData[i]);
            }
        }

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(fs, true);
    }

    /// <summary>
    /// 追加数据到Excel
    /// </summary>
    public static void AppendToExcel(string filePath, IEnumerable<string[]> data, string sheetName = "Sheet1")
    {
        IWorkbook workbook;
        if (File.Exists(filePath))
        {
            using FileStream fs1 = new(filePath, FileMode.Open, FileAccess.Read);
            workbook = Path.GetExtension(filePath).ToLower() == ".xlsx"
                ? new XSSFWorkbook(fs1)
                : (IWorkbook)new HSSFWorkbook(fs1);
        }
        else
        {
            workbook = Path.GetExtension(filePath).ToLower() == ".xlsx"
                ? new XSSFWorkbook()
                : (IWorkbook)new HSSFWorkbook();
        }

        var sheet = workbook.GetSheet(sheetName) ?? workbook.CreateSheet(sheetName);
        int rowIndex = sheet.LastRowNum + 1;

        foreach (var rowData in data)
        {
            var row = sheet.CreateRow(rowIndex++);
            for (int i = 0; i < rowData.Length; i++)
            {
                row.CreateCell(i).SetCellValue(rowData[i]);
            }
        }

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        workbook.Write(fs, true);
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 获取单元格值
    /// </summary>
    private static string GetCellValue(ICell? cell)
    {
        if (cell == null) return string.Empty;

        switch (cell.CellType)
        {
            case CellType.Numeric:
                return cell.NumericCellValue.ToString();
            case CellType.String:
                return cell.StringCellValue;
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                return cell.CellFormula;
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 合并Excel文件
    /// </summary>
    public static void MergeExcelFiles(string[] sourceFiles, string targetFile, string sheetName = "Sheet1")
    {
        var allData = new List<string[]>();
        foreach (var sourceFile in sourceFiles)
        {
            var data = ReadExcel(sourceFile);
            allData.AddRange(data);
        }

        CreateExcel(allData, targetFile, sheetName);
    }

    /// <summary>
    /// 获取Excel文件的工作表名称列表
    /// </summary>
    public static List<string> GetSheetNames(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        IWorkbook workbook = Path.GetExtension(filePath).ToLower() == ".xlsx"
            ? new XSSFWorkbook(fs)
            : (IWorkbook)new HSSFWorkbook(fs);

        var sheetNames = new List<string>();
        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            sheetNames.Add(workbook.GetSheetName(i));
        }
        return sheetNames;
    }
    #endregion
} 