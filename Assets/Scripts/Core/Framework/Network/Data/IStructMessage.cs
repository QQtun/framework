using System.IO;

namespace Core.Framework.Network.Data
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: 相對GoogleProtobuf，自定義的資料結構介面
    /// </summary>
    public interface IStructMessage
    {
        byte[] Serialize();
        int CalculateSize();
        int Serialize(Stream ms);
    }
}