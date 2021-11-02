using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Table Info Asset", menuName = "Config/Table Info Asset")]
public class TableInfoAsset : ScriptableObject
{
    [System.Serializable]
    public class ColumnInfo
    {
        public string name;
        public bool isKey;
        public string type;
        public string defaultValue;
        public string max;
        public string min;
        public string relativeTable;
        public string platform;
    }

    [System.Serializable]
    public class TableInfo
    {
        public string name;
        public List<ColumnInfo> columns = new List<ColumnInfo>();
    }

    public List<TableInfo> tables = new List<TableInfo>();
    public string sourcePath;
    public string clientDataOutputPath;
    public string clientScriptOutputPath;
    public string clientPackage;
    public string serverDataOutputPath;
    public string serverScriptOutputPath;
    public string serverPackage;
}
