using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class Consts
{
    public const string DataSheetFolderPath = "Assets/DataSheets";
    public const string DataSheetSymbolALLKEY = "ALL; KEY";
    public const string DataSheetSymbolALL = "ALL";
    public const string DataSheetSymbolDESIGN = "DESIGN";
}

public static class DataUtility_EX
{
    public static void ReadAllExcelFiles()
    {
        string[] filePaths = Directory.GetFiles(Consts.DataSheetFolderPath, "*Data*", SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            ExcelToDataSet(filePath);
        }
    }

    public static DataSet ExcelToDataSet(string filePath)
    {
        DataSet dataSet = new DataSet(Path.GetFileName(filePath));

        if (!filePath.EndsWith(".xlsx") && !filePath.EndsWith(".xls"))
        {
            throw new Exception("지원되지 않는 파일 형식입니다. .xlsx 또는 xls 파일만 테이블화 가능합니다.");
        }

        IWorkbook workbook;
        using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            workbook = filePath.EndsWith(".xlsx") ? (IWorkbook)new XSSFWorkbook(file) : new HSSFWorkbook(file);
        }

        // Load All Sheets
        Dictionary<ISheet, string> activeSheets = new Dictionary<ISheet, string>();

        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            if (!workbook.IsSheetHidden(i))
            {
                activeSheets.Add(workbook.GetSheetAt(i), workbook.GetSheetName(i));
            }
        }

        foreach (var sheet in activeSheets.Keys)
        {
            DataTable dataTable = new DataTable(sheet.SheetName);

            IRow headerRow = sheet.GetRow(0);           // Header
            IRow dataTypeRow = sheet.GetRow(1);         // DataType
            IRow variableNameRow = sheet.GetRow(2);     // VariableName

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var headerValue = headerRow.GetCell(i).StringCellValue;

                if (headerValue != Consts.DataSheetSymbolALLKEY &&
                    headerValue != Consts.DataSheetSymbolALL &&
                    headerValue != Consts.DataSheetSymbolDESIGN)
                {
                    throw new InvalidOperationException($"헤더 값이 정의된 상수 값이 아닙니다. \n입력한 값 : {headerValue}");
                }

                if (headerValue == Consts.DataSheetSymbolDESIGN)
                {
                    continue;
                }

                Type targetType = Type.GetType(dataTypeRow.GetCell(i).StringCellValue);
                
                if (targetType == null)
                {
                    throw new InvalidOperationException($"자료형을 올바르게 작성하신 것 맞나요? 해당 DataType으로 변경이 불가합니다. \n입력한 값 : {dataTypeRow.GetCell(i).StringCellValue}");
                }

                dataTable.Columns.Add(variableNameRow.GetCell(i).StringCellValue, Type.GetType(dataTypeRow.GetCell(i).StringCellValue));
            }

            for (int rowIndex = 3; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow currentRow = sheet.GetRow(rowIndex);
                if (currentRow == null) continue;

                DataRow dataRow = dataTable.NewRow();

                for (int colIndex = 0; colIndex < currentRow.LastCellNum; colIndex++)
                {
                    ICell currentCell = currentRow.GetCell(colIndex);
                    string currentCellValue = currentCell?.ToString() ?? string.Empty;

                    var headerValue = headerRow.GetCell(colIndex).StringCellValue;

                    // 유일한 키 체크
                    if (headerValue == Consts.DataSheetSymbolALLKEY)
                    {
                        if (dataTable.AsEnumerable().Any(row => row[colIndex].ToString() == currentCellValue))
                        {
                            throw new InvalidOperationException($"중복된 키 값 발견: {currentCellValue} 열 번호: {colIndex + 1}");
                        }
                    }

                    // Nullable 타입 체크
                    Type targetType = dataTable.Columns[colIndex].DataType;
                    bool isNullableType = targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);

                    if (string.IsNullOrEmpty(currentCellValue))
                    {
                        if (isNullableType)
                        {
                            dataRow[colIndex] = DBNull.Value;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Null 또는 기본값이 허용되지 않는 타입입니다. 열 번호: {colIndex + 1}");
                        }
                    }
                    else
                    {
                        // 적절한 타입 변환을 가정하고 데이터 할당
                        dataRow[colIndex] = Convert.ChangeType(currentCellValue, Nullable.GetUnderlyingType(targetType) ?? targetType);
                    }
                }

                dataTable.Rows.Add(dataRow);
            }

            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }

    public static void CreateScriptableFromDataTable<T>(DataSet dataSet, string dataPath) where T : IBaseData, new()
    {
        foreach (DataTable table in dataSet.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                T instance = new T();
                instance.LoadData(row);
            }
            
            //string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/{table.TableName}.asset");
            //AssetDatabase.CreateAsset(instance, assetPath);
        }
        
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
    }
}

public class GameDatas
{
    public static GameDatas Instance; 
    
    public Dictionary<int, MyBaseData> DTMyBaseDatas = new Dictionary<int, MyBaseData>();
    
}

public class MyBaseData : IBaseData
{
    public int int1;
    public float float1;
    public string string1;

    public override void LoadData(DataRow row)
    {
        base.LoadData(row);
        
        int1 = Convert.ToInt32(row["Int1ColumnName"]);
        float1 = Convert.ToSingle(row["Float1ColumnName"]);
        string1 = row["String1ColumnName"].ToString();
        GameDatas.Instance.DTMyBaseDatas.Add(int1, this);
    }
}

public class IBaseData
{
    public virtual void LoadData(DataRow row)
    {
    }
}
public class ReadOnlyInInspectorAttribute : PropertyAttribute
{
}

[CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}