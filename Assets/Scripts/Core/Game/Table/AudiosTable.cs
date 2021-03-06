// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: AudiosTable.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Core.Game.Table {

  /// <summary>Holder for reflection information generated from AudiosTable.proto</summary>
  public static partial class AudiosTableReflection {

    #region Descriptor
    /// <summary>File descriptor for AudiosTable.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AudiosTableReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFBdWRpb3NUYWJsZS5wcm90bxIPQ29yZS5HYW1lLlRhYmxlIjQKC0F1ZGlv",
            "c1RhYmxlEiUKBHJvd3MYASADKAsyFy5Db3JlLkdhbWUuVGFibGUuQXVkaW9z",
            "In8KBkF1ZGlvcxIPCgdVcmxQYXRoGAEgASgJEhEKCUF1ZGlvTmFtZRgCIAEo",
            "CRIRCglHcm91cE5hbWUYAyABKAkSFAoMU3BhdGlhbEJsZW5kGAQgASgCEhMK",
            "C01pbkRpc3RhbmNlGAUgASgNEhMKC01heERpc3RhbmNlGAYgASgNYgZwcm90",
            "bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Core.Game.Table.AudiosTable), global::Core.Game.Table.AudiosTable.Parser, new[]{ "Rows" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Core.Game.Table.Audios), global::Core.Game.Table.Audios.Parser, new[]{ "UrlPath", "AudioName", "GroupName", "SpatialBlend", "MinDistance", "MaxDistance" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class AudiosTable : pb::IMessage<AudiosTable>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<AudiosTable> _parser = new pb::MessageParser<AudiosTable>(() => new AudiosTable());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<AudiosTable> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Core.Game.Table.AudiosTableReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AudiosTable() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AudiosTable(AudiosTable other) : this() {
      rows_ = other.rows_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public AudiosTable Clone() {
      return new AudiosTable(this);
    }

    /// <summary>Field number for the "rows" field.</summary>
    public const int RowsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Core.Game.Table.Audios> _repeated_rows_codec
        = pb::FieldCodec.ForMessage(10, global::Core.Game.Table.Audios.Parser);
    private readonly pbc::RepeatedField<global::Core.Game.Table.Audios> rows_ = new pbc::RepeatedField<global::Core.Game.Table.Audios>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Core.Game.Table.Audios> Rows {
      get { return rows_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as AudiosTable);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(AudiosTable other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!rows_.Equals(other.rows_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= rows_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      rows_.WriteTo(output, _repeated_rows_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      rows_.WriteTo(ref output, _repeated_rows_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += rows_.CalculateSize(_repeated_rows_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(AudiosTable other) {
      if (other == null) {
        return;
      }
      rows_.Add(other.rows_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            rows_.AddEntriesFrom(input, _repeated_rows_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            rows_.AddEntriesFrom(ref input, _repeated_rows_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Audios : pb::IMessage<Audios>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Audios> _parser = new pb::MessageParser<Audios>(() => new Audios());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Audios> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Core.Game.Table.AudiosTableReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Audios() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Audios(Audios other) : this() {
      urlPath_ = other.urlPath_;
      audioName_ = other.audioName_;
      groupName_ = other.groupName_;
      spatialBlend_ = other.spatialBlend_;
      minDistance_ = other.minDistance_;
      maxDistance_ = other.maxDistance_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Audios Clone() {
      return new Audios(this);
    }

    /// <summary>Field number for the "UrlPath" field.</summary>
    public const int UrlPathFieldNumber = 1;
    private string urlPath_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string UrlPath {
      get { return urlPath_; }
      set {
        urlPath_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "AudioName" field.</summary>
    public const int AudioNameFieldNumber = 2;
    private string audioName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string AudioName {
      get { return audioName_; }
      set {
        audioName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "GroupName" field.</summary>
    public const int GroupNameFieldNumber = 3;
    private string groupName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string GroupName {
      get { return groupName_; }
      set {
        groupName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "SpatialBlend" field.</summary>
    public const int SpatialBlendFieldNumber = 4;
    private float spatialBlend_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float SpatialBlend {
      get { return spatialBlend_; }
      set {
        spatialBlend_ = value;
      }
    }

    /// <summary>Field number for the "MinDistance" field.</summary>
    public const int MinDistanceFieldNumber = 5;
    private uint minDistance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint MinDistance {
      get { return minDistance_; }
      set {
        minDistance_ = value;
      }
    }

    /// <summary>Field number for the "MaxDistance" field.</summary>
    public const int MaxDistanceFieldNumber = 6;
    private uint maxDistance_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint MaxDistance {
      get { return maxDistance_; }
      set {
        maxDistance_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Audios);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Audios other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (UrlPath != other.UrlPath) return false;
      if (AudioName != other.AudioName) return false;
      if (GroupName != other.GroupName) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(SpatialBlend, other.SpatialBlend)) return false;
      if (MinDistance != other.MinDistance) return false;
      if (MaxDistance != other.MaxDistance) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (UrlPath.Length != 0) hash ^= UrlPath.GetHashCode();
      if (AudioName.Length != 0) hash ^= AudioName.GetHashCode();
      if (GroupName.Length != 0) hash ^= GroupName.GetHashCode();
      if (SpatialBlend != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(SpatialBlend);
      if (MinDistance != 0) hash ^= MinDistance.GetHashCode();
      if (MaxDistance != 0) hash ^= MaxDistance.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (UrlPath.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(UrlPath);
      }
      if (AudioName.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(AudioName);
      }
      if (GroupName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(GroupName);
      }
      if (SpatialBlend != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(SpatialBlend);
      }
      if (MinDistance != 0) {
        output.WriteRawTag(40);
        output.WriteUInt32(MinDistance);
      }
      if (MaxDistance != 0) {
        output.WriteRawTag(48);
        output.WriteUInt32(MaxDistance);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (UrlPath.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(UrlPath);
      }
      if (AudioName.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(AudioName);
      }
      if (GroupName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(GroupName);
      }
      if (SpatialBlend != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(SpatialBlend);
      }
      if (MinDistance != 0) {
        output.WriteRawTag(40);
        output.WriteUInt32(MinDistance);
      }
      if (MaxDistance != 0) {
        output.WriteRawTag(48);
        output.WriteUInt32(MaxDistance);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (UrlPath.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(UrlPath);
      }
      if (AudioName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AudioName);
      }
      if (GroupName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(GroupName);
      }
      if (SpatialBlend != 0F) {
        size += 1 + 4;
      }
      if (MinDistance != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(MinDistance);
      }
      if (MaxDistance != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(MaxDistance);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Audios other) {
      if (other == null) {
        return;
      }
      if (other.UrlPath.Length != 0) {
        UrlPath = other.UrlPath;
      }
      if (other.AudioName.Length != 0) {
        AudioName = other.AudioName;
      }
      if (other.GroupName.Length != 0) {
        GroupName = other.GroupName;
      }
      if (other.SpatialBlend != 0F) {
        SpatialBlend = other.SpatialBlend;
      }
      if (other.MinDistance != 0) {
        MinDistance = other.MinDistance;
      }
      if (other.MaxDistance != 0) {
        MaxDistance = other.MaxDistance;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            UrlPath = input.ReadString();
            break;
          }
          case 18: {
            AudioName = input.ReadString();
            break;
          }
          case 26: {
            GroupName = input.ReadString();
            break;
          }
          case 37: {
            SpatialBlend = input.ReadFloat();
            break;
          }
          case 40: {
            MinDistance = input.ReadUInt32();
            break;
          }
          case 48: {
            MaxDistance = input.ReadUInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            UrlPath = input.ReadString();
            break;
          }
          case 18: {
            AudioName = input.ReadString();
            break;
          }
          case 26: {
            GroupName = input.ReadString();
            break;
          }
          case 37: {
            SpatialBlend = input.ReadFloat();
            break;
          }
          case 40: {
            MinDistance = input.ReadUInt32();
            break;
          }
          case 48: {
            MaxDistance = input.ReadUInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
