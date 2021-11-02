using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Google.Protobuf;

public class TableExportWindow : EditorWindow
{
    public static readonly Dictionary<string, string> TypeToProtoType = new Dictionary<string, string>()
    {
        { "int", "int32"},
        { "long", "int64"},
        { "uint", "uint32"},
        { "ulong", "uint64"},
        { "string", "string"},
        { "bool", "bool"},
        { "float", "float"},
    };

    public const string ConfigFilePostfix = "Config";

    private TableInfoAsset mTableInfo;
    private List<string> mFiles = new List<string>();
    private bool[] mExportOrNot;
    private Vector2 mScrollPosition;

    [MenuItem("企劃工具/表格輸出面板")]
    public static void OpenWindow()
    {
        var window = GetWindow<TableExportWindow>();
        window.Show();
    }

    [MenuItem("企劃工具/產Info")]
    public static void GenInfoOneStep()
    {
        var window = GetWindow<TableExportWindow>();
        window.SelectAll();
        window.GenInfo();
    }

    [MenuItem("企劃工具/產Code")]
    public static void GenCodeOneStep()
    {
        GenInfoOneStep();
        var window = GetWindow<TableExportWindow>();
        window.SelectAll();
        window.GenClientProto();
        window.GenClientCode();
        window.GenServerProto();
        window.GenServerCode();
    }

    [MenuItem("企劃工具/產資料")]
    public static void GenDataOneStep()
    {
        var window = GetWindow<TableExportWindow>();
        window.SelectAll();
        window.GenClientJsonData();
        window.GenClientBytesData();
        window.GenServerJsonData();
        window.GenServerBytesData();
    }

    [MenuItem("企劃工具/清除")]
    public static void ClearOneStep()
    {
        var window = GetWindow<TableExportWindow>();
        window.SelectAll();
        window.ClearClient();
        window.ClearServer();
    }

    private void Awake()
    {
        Refresh();
    }

    private void OnGUI()
    {
        mTableInfo = (TableInfoAsset)EditorGUILayout.ObjectField(mTableInfo, typeof(TableInfoAsset), false);
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("全選", GUILayout.Width(80), GUILayout.Height(20)))
        {
            SelectAll();
        }
        if (GUILayout.Button("全部取消", GUILayout.Width(80), GUILayout.Height(20)))
        {
            UnselectAll();
        }
        if (GUILayout.Button("Refresh", GUILayout.Width(80), GUILayout.Height(20)))
        {
            Refresh();
        }
        if (GUILayout.Button("GenInfo", GUILayout.Width(80), GUILayout.Height(20)))
            GenInfo();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("全部清除", GUILayout.Width(80), GUILayout.Height(20)))
        {
            ClearOneStep();
        }
        if (GUILayout.Button("清除 Client", GUILayout.Width(80), GUILayout.Height(20)))
        {
            ClearClient();
        }
        if (GUILayout.Button("清除 Server", GUILayout.Width(80), GUILayout.Height(20)))
        {
            ClearServer();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("GenClientCode", GUILayout.Width(120), GUILayout.Height(20)))
        {
            GenInfo();
            GenClientProto();
            GenClientCode();
        }
        if (GUILayout.Button("GenClientData", GUILayout.Width(120), GUILayout.Height(20)))
        {
            GenClientJsonData();
            GenClientBytesData();
        }
        GUILayout.Space(50);
        if (GUILayout.Button("GenClientProto", GUILayout.Width(120), GUILayout.Height(20)))
            GenClientProto();
        if (GUILayout.Button("GenClientCodeOnly", GUILayout.Width(140), GUILayout.Height(20)))
            GenClientCode();
        if (GUILayout.Button("GenClientJson", GUILayout.Width(120), GUILayout.Height(20)))
            GenClientJsonData();
        if (GUILayout.Button("GenClientBytes", GUILayout.Width(120), GUILayout.Height(20)))
            GenClientBytesData();
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("GenServerCode", GUILayout.Width(120), GUILayout.Height(20)))
        {
            GenInfo();
            GenServerProto();
            GenServerCode();
        }
        if (GUILayout.Button("GenServerData", GUILayout.Width(120), GUILayout.Height(20)))
        {
            GenServerJsonData();
            GenServerBytesData();
        }
        GUILayout.Space(50);
        if (GUILayout.Button("GenServerProto", GUILayout.Width(120), GUILayout.Height(20)))
            GenServerProto();
        if (GUILayout.Button("GenServerCodeOnly", GUILayout.Width(140), GUILayout.Height(20)))
            GenServerCode();
        if (GUILayout.Button("GenServerJson", GUILayout.Width(120), GUILayout.Height(20)))
            GenServerJsonData();
        if (GUILayout.Button("GenServerBytes", GUILayout.Width(120), GUILayout.Height(20)))
            GenServerBytesData();
        GUILayout.EndHorizontal();


        //GUILayout.BeginHorizontal("box");
        //if (GUILayout.Button("GenXLuaWrapConfig", GUILayout.Width(120), GUILayout.Height(20)))
        //    GenXLuaWrapConfig();
        //GUILayout.EndHorizontal();

        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
        for (int i = 0; i < mFiles.Count; i++)
        {
            var fileName = Path.GetFileName(mFiles[i]);
            GUILayout.BeginHorizontal("box");
            mExportOrNot[i] = EditorGUILayout.Toggle(mExportOrNot[i], GUILayout.Width(20), GUILayout.Height(20));
            GUILayout.Label($"Name= {fileName}");
            GUILayout.Space(40);
            GUILayout.Label($"Path= {mFiles[i]}");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    public void SelectAll()
    {
        for (int i = 0; i < mExportOrNot.Length; i++)
            mExportOrNot[i] = true;
    }

    public void UnselectAll()
    {
        for (int i = 0; i < mExportOrNot.Length; i++)
            mExportOrNot[i] = false;
    }

    public void ClearServer()
    {
        Clear(false);
    }

    public void ClearClient()
    {
        Clear(true);
    }
    public void Clear(bool client)
    {
        string dataOutputPath = client ? mTableInfo.clientDataOutputPath : mTableInfo.serverDataOutputPath;
        string scriptOutputPath = client ? mTableInfo.clientScriptOutputPath : mTableInfo.serverScriptOutputPath;

        Directory.Delete(dataOutputPath, true);
        Directory.Delete(scriptOutputPath, true);

        AssetDatabase.Refresh();
    }

    public void Refresh()
    {
        var tableInfoAssetPaths = AssetDatabase.FindAssets("t:TableInfoAsset");
        for (int i = 0; i < tableInfoAssetPaths.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(tableInfoAssetPaths[i]);
            mTableInfo = AssetDatabase.LoadAssetAtPath<TableInfoAsset>(path);
            if (mTableInfo != null)
                break;
        }

        if (mTableInfo != null)
        {
            mFiles.Clear();
            var files = Directory.GetFiles(mTableInfo.sourcePath);
            foreach (var filePath in files)
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                if (fileName.StartsWith("~"))
                    continue;
                if (fileName.EndsWith(".xlsx") || fileName.EndsWith("xlsm"))
                    mFiles.Add(filePath);
            }
            mExportOrNot = new bool[mFiles.Count];
        }
    }

    public void GenInfo()
    {
        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;
            var fileName = Path.GetFileNameWithoutExtension(mFiles[i]);
            var table = mTableInfo.tables.Find(t => t.name == fileName);
            var newTableInfo = fileName.EndsWith(ConfigFilePostfix) ? GenConfigTableInfo(mFiles[i]) : GenNewTableInfo(mFiles[i]);
            if (newTableInfo == null)
                continue;
            if (table != null)
            {
                if(CheckDiff(table, newTableInfo))
                {
                    mTableInfo.tables.Remove(table);
                    mTableInfo.tables.Add(newTableInfo);
                    EditorUtility.SetDirty(mTableInfo);
                }
            }
            else
            {
                mTableInfo.tables.Add(newTableInfo);
                EditorUtility.SetDirty(mTableInfo);
            }
        }
    }

    private bool CheckDiff(TableInfoAsset.TableInfo oldTableInfo, TableInfoAsset.TableInfo newTableInfo)
    {
        for (int i = 0; i < oldTableInfo.columns.Count; i++)
        {
            var oldColumn = oldTableInfo.columns[i];
            var newColumnIndex = newTableInfo.columns.FindIndex(nc => nc.name == oldColumn.name);
            if (newColumnIndex >= 0)
            {
                var newColumn = newTableInfo.columns[newColumnIndex];
                if (i != newColumnIndex)
                {
                    // 順序改變
                    return EditorUtility.DisplayDialog("警告", $"表格{oldTableInfo.name} 欄位{oldColumn.name}順序變更，會導致舊資料無法讀取，是否要繼續?", "是", "否");
                }
                if (oldColumn.type != newColumn.type)
                {
                    // 型態改變
                    return EditorUtility.DisplayDialog("警告", $"表格{oldTableInfo.name} 欄位{oldColumn.name}型態改變，會導致舊資料無法讀取，是否要繼續?", "是", "否");
                }
            }
            else
            {
                // 欄位遭刪除
                return EditorUtility.DisplayDialog("警告", $"表格{oldTableInfo.name} 欄位{oldColumn.name}遭刪除，會導致舊資料無法讀取，是否要繼續?", "是", "否");
            }
        }
        return true;
    }

    private TableInfoAsset.TableInfo GenNewTableInfo(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet excel = excelReader.AsDataSet();
        if (excel.Tables.Count < 2)
        {
            UnityEngine.Debug.LogError("表格必須含有第二頁 欄位設定");
            return null;
        }
        var newTableInfo = new TableInfoAsset.TableInfo();
        newTableInfo.name = Path.GetFileNameWithoutExtension(filePath);
        var configSheet = excel.Tables[1];
        for (int row = 1; row < configSheet.Rows.Count; row++)
        {
            var columnInfo = new TableInfoAsset.ColumnInfo();
            bool next = false;
            for (int column = 1; column < configSheet.Columns.Count; column++)
            {
                var value = configSheet.Rows[row][column].ToString();
                value = value.Trim(' ');
                if (column == 1)
                {
                    if (value.StartsWith("#")
                        || string.IsNullOrEmpty(value))
                    {
                        next = true;
                        break;
                    }

                    // 欄位名稱
                    columnInfo.name = value;
                }
                else if (column == 4)
                {
                    // key
                    columnInfo.isKey = !string.IsNullOrEmpty(value);
                }
                else if(column == 5)
                {
                    // 欄位型態
                    value = value.ToLower();
                    if (value == "int64")
                        value = "long";
                    else if (value == "uint64")
                        value = "ulong";
                    columnInfo.type = value;
                }
                else if (column == 6)
                {
                    // 預設值
                    columnInfo.defaultValue = value;
                }
                else if (column == 7)
                {
                    // 最小值
                    columnInfo.min = value;
                }
                else if (column == 8)
                {
                    // 最小值
                    columnInfo.max = value;
                }
                else if (column == 10)
                {
                    // 關聯表格
                    columnInfo.relativeTable = value;
                }
                else if (column == 11)
                {
                    // c/s
                    columnInfo.platform = value.ToLower();
                }
            }
            if(!next)
                newTableInfo.columns.Add(columnInfo);
        }
        return newTableInfo;
    }

    private TableInfoAsset.TableInfo GenConfigTableInfo(string filePath)
    {
        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet excel = excelReader.AsDataSet();
        if (excel.Tables.Count == 0)
        {
            UnityEngine.Debug.LogError("Config表格必須含有一頁");
            return null;
        }
        var newTableInfo = new TableInfoAsset.TableInfo();
        newTableInfo.name = Path.GetFileNameWithoutExtension(filePath);
        var sheet = excel.Tables[0];
        for (int row = 2; row < sheet.Rows.Count; row++)
        {
            TableInfoAsset.ColumnInfo columnInfo = null;
            for (int column = 0; column < 5; column++)
            {
                var value = sheet.Rows[row][column].ToString();
                value = value.Trim(' ');
                if (column == 0)
                {
                    // 是否輸出
                    if (value == "1")
                        columnInfo = new TableInfoAsset.ColumnInfo();
                    else
                        break;
                }
                else if (column == 1)
                {
                    // 名稱
                    columnInfo.name = value;
                }
                else if (column == 2)
                {
                    // 資料型態
                    value = value.ToLower();
                    if (value == "int64")
                        value = "long";
                    else if (value == "uint64")
                        value = "ulong";
                    columnInfo.type = value;
                }
                else if (column == 3)
                {
                    // 值
                    columnInfo.defaultValue = value;
                }
                else if (column == 4)
                {
                    // c/s
                    columnInfo.platform = value.ToLower();
                }
            }
            if (columnInfo != null)
                newTableInfo.columns.Add(columnInfo);
        }
        return newTableInfo;
    }

    public void GenClientProto()
    {
        GenProto(true);
    }

    public void GenServerProto()
    {
        GenProto(false);
    }

    public void GenProto(bool client = true)
    {
        string platform = client ? "c" : "s";
        string dataOutputPath = client ? mTableInfo.clientDataOutputPath : mTableInfo.serverDataOutputPath;
        string package = client ? mTableInfo.clientPackage : mTableInfo.serverPackage;

        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var fileName = Path.GetFileNameWithoutExtension(mFiles[i]);
            var tableInfo = mTableInfo.tables.Find(t => t.name == fileName);
            if (tableInfo == null)
                continue;

            bool isConfig = tableInfo.name.EndsWith(ConfigFilePostfix);
            if (!Directory.Exists(dataOutputPath))
                Directory.CreateDirectory(dataOutputPath);
            FileStream stream = File.Open(dataOutputPath + $"/{tableInfo.name}Table.proto", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("syntax = \"proto3\";");
            writer.WriteLine($"package {package};");

            writer.WriteLine("");

            if(isConfig)
            {
                writer.WriteLine($"message {tableInfo.name}Table");
                writer.WriteLine("{");
                for (int j = 0; j < tableInfo.columns.Count; j++)
                {
                    var column = tableInfo.columns[j];
                    if (column.platform.Contains(platform))
                        writer.WriteLine($"    {ConvertType(column.type)} {column.name} = {j + 1};");
                }
                writer.WriteLine("}");
            }
            else
            {
                writer.WriteLine($"message {tableInfo.name}Table");
                writer.WriteLine("{");
                writer.WriteLine($"    repeated {tableInfo.name} rows = 1;");
                writer.WriteLine("}");
                writer.WriteLine("");

                writer.WriteLine($"message {tableInfo.name}");
                writer.WriteLine("{");
                for (int j = 0; j < tableInfo.columns.Count; j++)
                {
                    var column = tableInfo.columns[j];
                    if (column.platform.Contains(platform))
                        writer.WriteLine($"    {ConvertType(column.type)} {column.name} = {j + 1};");
                }
                writer.WriteLine("}"); // message
            }

            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

        AssetDatabase.Refresh();
    }

    private string ConvertType(string type)
    {
        TypeToProtoType.TryGetValue(type, out var protoType);
        if(string.IsNullOrEmpty(protoType))
        {
            UnityEngine.Debug.Log($"type \"{type}\" can't find matching proto type");
            return "int32";
        }
        return protoType;
    }

    public void GenClientCode()
    {
        GenCode(true);
        GenTableGroup(true);
    }

    public void GenServerCode()
    {
        GenCode(false);
        GenTableGroup(false);
    }

    public void GenCode(bool client = true)
    {
        string dataOutputPath = client ? mTableInfo.clientDataOutputPath : mTableInfo.serverDataOutputPath;
        string scriptOutputPath = client ? mTableInfo.clientScriptOutputPath : mTableInfo.serverScriptOutputPath;
        string package = client ? mTableInfo.clientPackage : mTableInfo.serverPackage;


        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var fileName = Path.GetFileNameWithoutExtension(mFiles[i]);
            var tableInfo = mTableInfo.tables.Find(t => t.name == fileName);
            if (tableInfo == null)
                continue;

            bool isConfig = tableInfo.name.EndsWith(ConfigFilePostfix);
            var protoPath = dataOutputPath + $"/{tableInfo.name}Table.proto";
            var keyColumn = tableInfo.columns.Find(c => c.isKey);

            var dataPath = Application.dataPath;

            ProcessStartInfo process = new ProcessStartInfo();
            process.CreateNoWindow = false;
            process.UseShellExecute = false;
            process.FileName = $"{dataPath}/../protoc.exe";
            process.WindowStyle = ProcessWindowStyle.Hidden;
            process.Arguments = $"{protoPath} --csharp_out={scriptOutputPath} --proto_path={dataOutputPath}";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(process))
                {
                    exeProcess.WaitForExit();
                }

                if (isConfig)
                    continue;

                var exScriptOutputPath = scriptOutputPath + "/Extention";
                if (!Directory.Exists(exScriptOutputPath))
                    Directory.CreateDirectory(exScriptOutputPath);
                var stream = File.Open(exScriptOutputPath + $"/{tableInfo.name}TableEx.cs", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(stream);
                WriteHeader(sw);
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine($"namespace {package}");
                sw.WriteLine("{");


                sw.WriteLine($"    public partial class {tableInfo.name}Table");
                sw.WriteLine("    {");
                sw.WriteLine($"        private Dictionary<{keyColumn.type}, {tableInfo.name}> mDic = null;");
                sw.WriteLine($"        public {tableInfo.name} Get({keyColumn.type} id)");
                sw.WriteLine(@"        {
            if(mDic == null)
            {");
                sw.WriteLine($"                mDic = new Dictionary<{keyColumn.type}, {tableInfo.name}>();");
                sw.WriteLine(@"                foreach(var row in Rows)
                {");
                sw.WriteLine($"                    mDic.Add(row.{keyColumn.name}, row);");
                sw.WriteLine(@"                }
            }
            mDic.TryGetValue(id, out var data);
            return data;
        }"); // Get
                sw.WriteLine("    }");// class 

                sw.WriteLine($"    public partial class {tableInfo.name}");
                sw.WriteLine("    {");
                for (int j = 0; j < tableInfo.columns.Count; j++)
                {
                    var column = tableInfo.columns[j];
                    if (!string.IsNullOrEmpty(column.relativeTable))
                    {
                        sw.WriteLine($"        private {column.relativeTable} m{column.name.FirstCharToUpper()} = null;");
                        sw.WriteLine($"        public {column.relativeTable} {column.name.FirstCharToUpper()}Setting");
                        sw.WriteLine("        {");
                        sw.WriteLine("            get ");
                        sw.WriteLine("            {");
                        sw.Write("                if(");
                        sw.WriteLine($"m{column.name.FirstCharToUpper()} == null)");
                        sw.WriteLine("                {");
                        sw.WriteLine($"                    m{column.name.FirstCharToUpper()} = TableGroup.{column.relativeTable}Table.Get({column.name.FirstCharToUpper()});");
                        sw.WriteLine("                }"); // if
                        sw.WriteLine($"                return m{column.name.FirstCharToUpper()};");
                        sw.WriteLine("            }"); // get
                        sw.WriteLine("        }"); // property
                    }
                }
                sw.WriteLine("    }"); // class
                sw.WriteLine("}"); // namespace
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            catch (System.Exception e)
            {
                // Log error.
                UnityEngine.Debug.LogError(e);
            }
        }

        AssetDatabase.Refresh();
    }

    public void GenClientJsonData()
    {
        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var filePath = mFiles[i];
            GenData(filePath, true, true);
        }
        AssetDatabase.Refresh();
    }

    public void GenServerJsonData()
    {
        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var filePath = mFiles[i];
            GenData(filePath, true, false);
        }
        AssetDatabase.Refresh();
    }

    public void GenClientBytesData()
    {
        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var filePath = mFiles[i];
            GenData(filePath, false, true);
        }
        AssetDatabase.Refresh();
    }

    public void GenServerBytesData()
    {
        for (int i = 0; i < mFiles.Count; i++)
        {
            if (!mExportOrNot[i])
                continue;

            var filePath = mFiles[i];
            GenData(filePath, false, false);
        }
        AssetDatabase.Refresh();
    }

    private void GenData(string path, bool json = false, bool client = true)
    {
        var filePath = path;
        var fileName = Path.GetFileNameWithoutExtension(path);
        var tableInfo = mTableInfo.tables.Find(t => t.name == fileName);
        if (tableInfo == null)
            return;

        bool isConfig = tableInfo.name.EndsWith(ConfigFilePostfix);
        string packageName = client ? mTableInfo.clientPackage : mTableInfo.serverPackage;
        string dataOutputPath = client ? mTableInfo.clientDataOutputPath : mTableInfo.serverDataOutputPath;

        var assembly = Assembly.Load("Assembly-CSharp");

        var tableTypeName = $"{packageName}.{tableInfo.name}Table";
        var tableType = assembly.GetType(tableTypeName);
        var table = Activator.CreateInstance(tableType) as Google.Protobuf.IMessage;

        if (isConfig)
        {
            for (int j = 0; j < tableInfo.columns.Count; j++)
            {
                var columnInfo = tableInfo.columns[j];
                bool export = client ? columnInfo.platform.Contains("c") : columnInfo.platform.Contains("s");
                if (!export)
                    continue;

                var propertyInfo = tableType.GetProperty(columnInfo.name);
                if (propertyInfo == null)
                {
                    UnityEngine.Debug.LogError($"can't find property name={columnInfo.name}");
                }
                else
                {
                    var value = columnInfo.defaultValue;
                    if (columnInfo.type == "int")
                    {
                        if (int.TryParse(value, out var intValue))
                        {
                            propertyInfo.SetValue(table, intValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "long")
                    {
                        if (long.TryParse(value, out var longValue))
                        {
                            propertyInfo.SetValue(table, longValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "uint")
                    {
                        if (uint.TryParse(value, out var uintValue))
                        {
                            propertyInfo.SetValue(table, uintValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "ulong")
                    {
                        if (ulong.TryParse(value, out var ulongValue))
                        {
                            propertyInfo.SetValue(table, ulongValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "float")
                    {
                        if (float.TryParse(value, out var floatValue))
                        {
                            propertyInfo.SetValue(table, floatValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "bool")
                    {
                        if (int.TryParse(value, out var intValue))
                        {
                            propertyInfo.SetValue(table, intValue >= 1);
                        }
                        else if (bool.TryParse(value, out var boolValue))
                        {
                            propertyInfo.SetValue(table, boolValue);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                        }
                    }
                    if (columnInfo.type == "string")
                        propertyInfo.SetValue(table, value);
                }
            }
        }
        else
        {
            var rowsProperty = tableType.GetProperty("Rows");
            var list = rowsProperty.GetValue(table);

            var rowTypeName = $"{packageName}.{tableInfo.name}";
            var rowType = assembly.GetType(rowTypeName);
            var addMethodInfo = list.GetType().GetMethod("Add", new Type[] { rowType });
            Dictionary<string, PropertyInfo> nameToInfo = new Dictionary<string, PropertyInfo>();
            for (int j = 0; j < tableInfo.columns.Count; j++)
            {
                var column = tableInfo.columns[j];
                bool export = client ? column.platform.Contains("c") : column.platform.Contains("s");
                if(export)
                {
                    var propertyInfo = rowType.GetProperty(column.name);
                    nameToInfo[column.name] = propertyInfo;
                    if (propertyInfo == null)
                    {
                        UnityEngine.Debug.LogError($"can't find property name={column.name}");
                    }
                }
            }

            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            DataSet excel = excelReader.AsDataSet();

            var dataSheet = excel.Tables[0];
            var columnName = new string[dataSheet.Columns.Count];
            for (int column = 0; column < dataSheet.Columns.Count; column++)
            {
                columnName[column] = dataSheet.Rows[1][column].ToString();
                if (columnName[column].StartsWith("#"))
                    columnName[column] = null;
            }

            bool rowEnd = false;
            for (int row = 2; row < dataSheet.Rows.Count; row++)
            {
                object rowObj = null;
                for (int column = 0; column < dataSheet.Columns.Count; column++)
                {
                    if (column == 0)
                    {
                        // 是否輸出
                        var value = dataSheet.Rows[row][column].ToString();
                        if (value == "1")
                        {
                            rowObj = Activator.CreateInstance(rowType);
                        }
                        else if (string.IsNullOrEmpty(value))
                        {
                            // 後面就不要輸出的
                            rowEnd = true;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(columnName[column]))
                            continue;

                        var columnInfo = tableInfo.columns.Find(c => c.name == columnName[column]);
                        if (columnInfo == null)
                        {
                            UnityEngine.Debug.LogError($"can't find columnInfo name={columnName[column]}");
                            continue;
                        }
                        bool export = client ? columnInfo.platform.Contains("c") : columnInfo.platform.Contains("s");
                        if(!export)
                        {
                            continue;
                        }

                        var value = dataSheet.Rows[row][column].ToString();
                        if (columnInfo.type == "int")
                        {
                            bool hasMin = int.TryParse(columnInfo.min, out var min);
                            bool hasMax = int.TryParse(columnInfo.max, out var max);
                            if (int.TryParse(value, out var intValue))
                            {
                                if (hasMin && intValue < min)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match min={min} !!");
                                if (hasMax && intValue > max)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match max={max} !!");
                                nameToInfo[columnName[column]].SetValue(rowObj, intValue);
                            }
                            else if (int.TryParse(columnInfo.defaultValue, out intValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, intValue);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                            }
                        }
                        if (columnInfo.type == "long")
                        {
                            bool hasMin = long.TryParse(columnInfo.min, out var min);
                            bool hasMax = long.TryParse(columnInfo.max, out var max);
                            if (long.TryParse(value, out var longValue))
                            {
                                if (hasMin && longValue < min)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match min={min} !!");
                                if (hasMax && longValue > max)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match max={max} !!");
                                nameToInfo[columnName[column]].SetValue(rowObj, longValue);
                            }
                            else if (long.TryParse(columnInfo.defaultValue, out longValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, longValue);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                            }
                        }
                        if (columnInfo.type == "uint")
                        {
                            bool hasMin = uint.TryParse(columnInfo.min, out var min);
                            bool hasMax = uint.TryParse(columnInfo.max, out var max);
                            if (uint.TryParse(value, out var uintValue))
                            {
                                if (hasMin && uintValue < min)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match min={min} !!");
                                if (hasMax && uintValue > max)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match max={max} !!");
                                nameToInfo[columnName[column]].SetValue(rowObj, uintValue);
                            }
                            else if (uint.TryParse(columnInfo.defaultValue, out uintValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, uintValue);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                            }
                        }
                        if (columnInfo.type == "ulong")
                        {
                            bool hasMin = ulong.TryParse(columnInfo.min, out var min);
                            bool hasMax = ulong.TryParse(columnInfo.max, out var max);
                            if (ulong.TryParse(value, out var ulongValue))
                            {
                                if (hasMin && ulongValue < min)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match min={min} !!");
                                if (hasMax && ulongValue > max)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match max={max} !!");
                                nameToInfo[columnName[column]].SetValue(rowObj, ulongValue);
                            }
                            else if (ulong.TryParse(columnInfo.defaultValue, out ulongValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, ulongValue);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                            }
                        }
                        if (columnInfo.type == "float")
                        {
                            bool hasMin = float.TryParse(columnInfo.min, out var min);
                            bool hasMax = float.TryParse(columnInfo.max, out var max);
                            if (float.TryParse(value, out var floatValue))
                            {
                                if (hasMin && floatValue < min)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match min={min} !!");
                                if (hasMax && floatValue > max)
                                    UnityEngine.Debug.LogError($"Table={tableInfo.name} row={row + 1} column={columnInfo.name} value={value} does't match max={max} !!");
                                nameToInfo[columnName[column]].SetValue(rowObj, floatValue);
                            }
                            else if (float.TryParse(columnInfo.defaultValue, out floatValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, floatValue);
                            }
                            else
                            {
                                //UnityEngine.Debug.LogError($"column[{columnInfo.name}] never set value !");
                            }
                        }
                        if (columnInfo.type == "bool")
                        {
                            bool defaultBoolValue;
                            if (!bool.TryParse(columnInfo.defaultValue, out defaultBoolValue))
                            {
                                if (int.TryParse(columnInfo.defaultValue, out var defaultIntValue))
                                {
                                    defaultBoolValue = defaultIntValue >= 1;
                                }
                            }
                            if (int.TryParse(value, out var intValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, intValue >= 1);
                            }
                            else if (bool.TryParse(value, out var boolValue))
                            {
                                nameToInfo[columnName[column]].SetValue(rowObj, boolValue);
                            }
                            else
                            {
                                boolValue = defaultBoolValue;
                                nameToInfo[columnName[column]].SetValue(rowObj, boolValue);
                            }
                        }
                        if (columnInfo.type == "string")
                            nameToInfo[columnName[column]].SetValue(rowObj, value);
                    }
                }
                if(rowEnd)
                {
                    break;
                }
                if (rowObj != null)
                {
                    addMethodInfo.Invoke(list, new[] { rowObj });
                }
            }
        }

        if (json)
        {
            TextWriter tw = File.CreateText(dataOutputPath + $"/{tableInfo.name}JsonData.bytes");
            JsonFormatter.Default.Format(table, tw);
            tw.Flush();
            tw.Close();
            tw.Dispose();
        }
        else
        {
            var stream = File.Open(dataOutputPath + $"/{tableInfo.name}ByteData.bytes", FileMode.Create, FileAccess.Write);
            table.WriteTo(stream);
            stream.Flush();
            stream.Close();
            stream.Dispose();
        }
    }

    public void GenClientTableGroup()
    {
        GenTableGroup(true);
    }

    public void GenServerTableGroup()
    {
        GenTableGroup(false);
    }

    public void GenTableGroup(bool client = true)
    {
        string packageName = client ? mTableInfo.clientPackage : mTableInfo.serverPackage;
        string scriptOutputPath = client ? mTableInfo.clientScriptOutputPath : mTableInfo.serverScriptOutputPath;
        var files = Directory.GetFiles(scriptOutputPath);
        List<string> tablePaths = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            var path = files[i];
            if (!path.EndsWith("Table.cs"))
                continue;

            tablePaths.Add(path.Replace("\\", "/"));
        }

        if (!Directory.Exists(scriptOutputPath))
            Directory.CreateDirectory(scriptOutputPath);
        var stream = File.Open(scriptOutputPath + $"/TableGroup.cs", FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(stream);

        WriteHeader(sw);
        sw.WriteLine($"namespace {packageName}");
        sw.WriteLine("{");
        sw.WriteLine($"    public partial class TableGroup");
        sw.WriteLine("    {");

        for (int i = 0; i < tablePaths.Count; i++)
        {
            var path = tablePaths[i];
            var name = Path.GetFileNameWithoutExtension(path);
            sw.WriteLine($"        public static {name} {name} {{ get; private set;}}");
        }

        sw.WriteLine("");
        sw.WriteLine("        public static System.Action<string, System.Action<byte[]>, System.Action<string>> Loader;");
        sw.WriteLine(@"        public static void ReloadAll(System.Action onLoaded = null)
        {");
        sw.WriteLine($"            int totalCount = {tablePaths.Count}; ");
        sw.WriteLine(@"            int loadedCount = 0;
            System.Action checkLoaded = () =>
            {
                loadedCount++;
                if (loadedCount == totalCount)
                    onLoaded?.Invoke();
            };");
        for (int i = 0; i < tablePaths.Count; i++)
        {
            var path = tablePaths[i];
            var name = Path.GetFileNameWithoutExtension(path);
            var nameWithoutTable = name.Substring(0, name.IndexOf("Table"));
            sw.WriteLine($"            Loader?.Invoke(\"{nameWithoutTable}\", (bytes) => {{ {name} = {name}.Parser.ParseFrom(bytes); checkLoaded(); }}, (json) => {{ {name} = {name}.Parser.ParseJson(json); checkLoaded(); }});");
        }
        sw.WriteLine("        }"); //ReloadAll


        sw.WriteLine("    }"); // class
        sw.WriteLine("}"); // namespace

        sw.Flush();
        sw.Close();
        sw.Dispose();

        AssetDatabase.Refresh();
    }

    private void GenXLuaWrapConfig()
    {
        string scriptOutputPath = mTableInfo.clientScriptOutputPath;
        var files = Directory.GetFiles(scriptOutputPath);
        List<string> tablePaths = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            var path = files[i];
            if (!path.EndsWith("Table.cs"))
                continue;

            tablePaths.Add(path.Replace("\\", "/"));
        }

        var configcs = "Assets/Scripts/Editor/xLua/GenConfig.cs";
        if (!File.Exists(configcs))
        {
            UnityEngine.Debug.LogError($"{configcs} file not exist !");
            return;
        }
        var stream = File.Open(configcs, FileMode.Open, FileAccess.Read);
        var sr = new StreamReader(stream);
        var totalString = sr.ReadToEnd();
        sr.Close();
        sr.Dispose();

        var startSymbol = "// [TableLuaCallCSharp Start]";
        var endSymbol = "// [TableLuaCallCSharp End]";
        var starIndex = totalString.IndexOf(startSymbol) + startSymbol.Length;
        var endIndex = totalString.IndexOf(endSymbol);
        if(starIndex < 0 || endIndex < 0 || endIndex < starIndex)
        {
            UnityEngine.Debug.LogError($"symbol error !!");
            return;
        }

        totalString = totalString.Remove(starIndex, endIndex - starIndex);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("");
        sb.AppendLine("    [LuaCallCSharp]");
        sb.AppendLine("    public static List<Type> TableLuaCallCSharp = new List<Type>()");
        sb.AppendLine("    {");
        sb.AppendLine("        typeof(Core.Game.Table.TableGroup),");
        for (int i = 0;i< tablePaths.Count;i++)
        {
            var path = tablePaths[i];
            var name = Path.GetFileNameWithoutExtension(path);
            var nameWithoutTable = name.Substring(0, name.IndexOf("Table"));
            sb.AppendLine($"        typeof(Core.Game.Table.{name}),");
            if(!name.EndsWith("ConfigTable"))
            {
                sb.AppendLine($"        typeof(Core.Game.Table.{nameWithoutTable}),");
            }
        }
        sb.AppendLine("    };");
        sb.Append("    ");
        totalString = totalString.Insert(starIndex, sb.ToString());
        stream = File.Open(configcs, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new StreamWriter(stream);
        sw.Write(totalString);
        sw.Flush();
        sw.Close();
        sw.Dispose();
    }

    private void WriteHeader(StreamWriter sw)
    {
        sw.WriteLine(@"// <auto-generated>
//     Generated by TableExportWindow.  DO NOT EDIT!
// </auto-generated>");
    }
}