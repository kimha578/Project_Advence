using UnityEngine;
using UnityEditor;

public class ExcelReaderWindow : EditorWindow
{
    [MenuItem("Excel/Excel Reader")]
    public static void ShowWindow()
    {
        GetWindow<ExcelReaderWindow>("Excel Reader");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Read All Excel Files in DataSheets Folder"))
        {
            ReadAllExcelFilesWithProgress();
        }
    }

    private void ReadAllExcelFilesWithProgress()
    {
        try
        {
            string[] filePaths = System.IO.Directory.GetFiles(Consts.DataSheetFolderPath, "*.xls", System.IO.SearchOption.AllDirectories);
            int totalFiles = filePaths.Length;
            for (int i = 0; i < totalFiles; i++)
            {
                string file = filePaths[i];
                float progress = (float)i / totalFiles;
                EditorUtility.DisplayProgressBar("Reading Excel Files", $"Processing {file}...", progress);

                //DataUtility_EX.ReadExcel(file);

                // 프로세스 중단 확인
                if (EditorUtility.DisplayCancelableProgressBar("Reading Excel Files", $"Processing {file}...", progress))
                {
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
