using UnityEngine;

[CreateAssetMenu(fileName = "Server Proto Asset", menuName = "Config/Create ServerProtoAsset")]
public class ServerProtoAsset : ScriptableObject
{
    public string csSourcFrom;
    public string csCopyTo;
    public string pbSourceFrom;
    public string pbCopyTo;
}
