#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;

namespace Portkey.Contracts.BeangoTownContract
{
      /// <summary>Holder for reflection information generated from beango_town_contract.proto</summary>
  public static partial class BeangoTownContractReflection {

    #region Descriptor
    /// <summary>File descriptor for beango_town_contract.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BeangoTownContractReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChpiZWFuZ29fdG93bl9jb250cmFjdC5wcm90bxoPYWVsZi9jb3JlLnByb3Rv",
            "GhJhZWxmL29wdGlvbnMucHJvdG8aG2dvb2dsZS9wcm90b2J1Zi9lbXB0eS5w",
            "cm90bxoeZ29vZ2xlL3Byb3RvYnVmL3dyYXBwZXJzLnByb3RvGh9nb29nbGUv",
            "cHJvdG9idWYvdGltZXN0YW1wLnByb3RvGgthY3MxMi5wcm90byIgCglQbGF5",
            "SW5wdXQSEwoLcmVzZXRfc3RhcnQYASABKAgiKwoKUGxheU91dHB1dBIdChVl",
            "eHBlY3RlZF9ibG9ja19oZWlnaHQYASABKAMiNgoXR2V0Qm91dEluZm9ybWF0",
            "aW9uSW5wdXQSGwoHcGxheV9pZBgBIAEoCzIKLmFlbGYuSGFzaCKvAQoRUGxh",
            "eWVySW5mb3JtYXRpb24SJQoOcGxheWVyX2FkZHJlc3MYASABKAsyDS5hZWxm",
            "LkFkZHJlc3MSFgoOcGxheWFibGVfY291bnQYAiABKAUSMgoObGFzdF9wbGF5",
            "X3RpbWUYAyABKAsyGi5nb29nbGUucHJvdG9idWYuVGltZXN0YW1wEhEKCXN1",
            "bV9zY29yZRgEIAEoAxIUCgxjdXJfZ3JpZF9udW0YBSABKAUijwIKD0JvdXRJ",
            "bmZvcm1hdGlvbhIZChFwbGF5X2Jsb2NrX2hlaWdodBgBIAEoAxIcCglncmlk",
            "X3R5cGUYAiABKA4yCS5HcmlkVHlwZRIQCghncmlkX251bRgDIAEoBRINCgVz",
            "Y29yZRgEIAEoBRITCgtpc19jb21wbGV0ZRgFIAEoCBIbCgdwbGF5X2lkGAYg",
            "ASgLMgouYWVsZi5IYXNoEhoKEmJpbmdvX2Jsb2NrX2hlaWdodBgHIAEoAxIt",
            "CglwbGF5X3RpbWUYCCABKAsyGi5nb29nbGUucHJvdG9idWYuVGltZXN0YW1w",
            "EiUKDnBsYXllcl9hZGRyZXNzGAkgASgLMg0uYWVsZi5BZGRyZXNzIlcKEUdh",
            "bWVMaW1pdFNldHRpbmdzEhwKFGRhaWx5X21heF9wbGF5X2NvdW50GAEgASgF",
            "EiQKHGRhaWx5X3BsYXlfY291bnRfcmVzZXRfaG91cnMYAiABKAUiKAoMR3Jp",
            "ZFR5cGVMaXN0EhgKBXZhbHVlGAEgAygOMgkuR3JpZFR5cGUibQoGUGxheWVk",
            "EhkKEXBsYXlfYmxvY2tfaGVpZ2h0GAEgASgDEhsKB3BsYXlfaWQYAiABKAsy",
            "Ci5hZWxmLkhhc2gSJQoOcGxheWVyX2FkZHJlc3MYAyABKAsyDS5hZWxmLkFk",
            "ZHJlc3M6BKC7GAEi3gEKB0JpbmdvZWQSGQoRcGxheV9ibG9ja19oZWlnaHQY",
            "ASABKAMSHAoJZ3JpZF90eXBlGAIgASgOMgkuR3JpZFR5cGUSEAoIZ3JpZF9u",
            "dW0YAyABKAUSDQoFc2NvcmUYBCABKAMSEwoLaXNfY29tcGxldGUYBSABKAgS",
            "GwoHcGxheV9pZBgGIAEoCzIKLmFlbGYuSGFzaBIaChJiaW5nb19ibG9ja19o",
            "ZWlnaHQYByABKAMSJQoOcGxheWVyX2FkZHJlc3MYCCABKAsyDS5hZWxmLkFk",
            "ZHJlc3M6BKC7GAEqJwoIR3JpZFR5cGUSCAoEQmx1ZRAAEgcKA1JlZBABEggK",
            "BEdvbGQQAjLLBQoSQmVhbmdvVG93bkNvbnRyYWN0EiEKBFBsYXkSCi5QbGF5",
            "SW5wdXQaCy5QbGF5T3V0cHV0IgASLQoFQmluZ28SCi5hZWxmLkhhc2gaFi5n",
            "b29nbGUucHJvdG9idWYuRW1wdHkiABJEChRTZXRHYW1lTGltaXRTZXR0aW5n",
            "cxISLkdhbWVMaW1pdFNldHRpbmdzGhYuZ29vZ2xlLnByb3RvYnVmLkVtcHR5",
            "IgASPgoKSW5pdGlhbGl6ZRIWLmdvb2dsZS5wcm90b2J1Zi5FbXB0eRoWLmdv",
            "b2dsZS5wcm90b2J1Zi5FbXB0eSIAEjYKC0NoYW5nZUFkbWluEg0uYWVsZi5B",
            "ZGRyZXNzGhYuZ29vZ2xlLnByb3RvYnVmLkVtcHR5IgASSgoNQ2hlY2tCZWFu",
            "UGFzcxIWLmdvb2dsZS5wcm90b2J1Zi5FbXB0eRoaLmdvb2dsZS5wcm90b2J1",
            "Zi5Cb29sVmFsdWUiBYiJ9wEBEkAKFEdldFBsYXllckluZm9ybWF0aW9uEg0u",
            "YWVsZi5BZGRyZXNzGhIuUGxheWVySW5mb3JtYXRpb24iBYiJ9wEBEkcKEkdl",
            "dEJvdXRJbmZvcm1hdGlvbhIYLkdldEJvdXRJbmZvcm1hdGlvbklucHV0GhAu",
            "Qm91dEluZm9ybWF0aW9uIgWIifcBARI4CghHZXRBZG1pbhIWLmdvb2dsZS5w",
            "cm90b2J1Zi5FbXB0eRoNLmFlbGYuQWRkcmVzcyIFiIn3AQESSQoUR2V0R2Ft",
            "ZUxpbWl0U2V0dGluZ3MSFi5nb29nbGUucHJvdG9idWYuRW1wdHkaEi5HYW1l",
            "TGltaXRTZXR0aW5ncyIFiIn3AQEaSbLM9gE0Q29udHJhY3RzLkJlYW5nb1Rv",
            "d25Db250cmFjdC5CZWFuZ29Ub3duQ29udHJhY3RTdGF0ZcrK9gELYWNzMTIu",
            "cHJvdG9CH6oCHENvbnRyYWN0cy5CZWFuZ29Ub3duQ29udHJhY3RiBnByb3Rv",
            "Mw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::AElf.Types.CoreReflection.Descriptor, global::AElf.OptionsReflection.Descriptor, global::Google.Protobuf.WellKnownTypes.EmptyReflection.Descriptor, global::Google.Protobuf.WellKnownTypes.WrappersReflection.Descriptor, global::Google.Protobuf.WellKnownTypes.TimestampReflection.Descriptor, global::AElf.Standards.ACS12.Acs12Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Portkey.Contracts.BeangoTownContract.GridType), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.PlayInput), global::Portkey.Contracts.BeangoTownContract.PlayInput.Parser, new[]{ "ResetStart" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.PlayOutput), global::Portkey.Contracts.BeangoTownContract.PlayOutput.Parser, new[]{ "ExpectedBlockHeight" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput), global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput.Parser, new[]{ "PlayId" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.PlayerInformation), global::Portkey.Contracts.BeangoTownContract.PlayerInformation.Parser, new[]{ "PlayerAddress", "PlayableCount", "LastPlayTime", "SumScore", "CurGridNum" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.BoutInformation), global::Portkey.Contracts.BeangoTownContract.BoutInformation.Parser, new[]{ "PlayBlockHeight", "GridType", "GridNum", "Score", "IsComplete", "PlayId", "BingoBlockHeight", "PlayTime", "PlayerAddress" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.GameLimitSettings), global::Portkey.Contracts.BeangoTownContract.GameLimitSettings.Parser, new[]{ "DailyMaxPlayCount", "DailyPlayCountResetHours" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.GridTypeList), global::Portkey.Contracts.BeangoTownContract.GridTypeList.Parser, new[]{ "Value" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.Played), global::Portkey.Contracts.BeangoTownContract.Played.Parser, new[]{ "PlayBlockHeight", "PlayId", "PlayerAddress" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Portkey.Contracts.BeangoTownContract.Bingoed), global::Portkey.Contracts.BeangoTownContract.Bingoed.Parser, new[]{ "PlayBlockHeight", "GridType", "GridNum", "Score", "IsComplete", "PlayId", "BingoBlockHeight", "PlayerAddress" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum GridType {
    [pbr::OriginalName("Blue")] Blue = 0,
    [pbr::OriginalName("Red")] Red = 1,
    [pbr::OriginalName("Gold")] Gold = 2,
  }

  #endregion

  #region Messages
  public sealed partial class PlayInput : pb::IMessage<PlayInput>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayInput> _parser = new pb::MessageParser<PlayInput>(() => new PlayInput());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PlayInput> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayInput() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayInput(PlayInput other) : this() {
      resetStart_ = other.resetStart_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayInput Clone() {
      return new PlayInput(this);
    }

    /// <summary>Field number for the "reset_start" field.</summary>
    public const int ResetStartFieldNumber = 1;
    private bool resetStart_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool ResetStart {
      get { return resetStart_; }
      set {
        resetStart_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PlayInput);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PlayInput other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ResetStart != other.ResetStart) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ResetStart != false) hash ^= ResetStart.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ResetStart != false) {
        output.WriteRawTag(8);
        output.WriteBool(ResetStart);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ResetStart != false) {
        output.WriteRawTag(8);
        output.WriteBool(ResetStart);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ResetStart != false) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PlayInput other) {
      if (other == null) {
        return;
      }
      if (other.ResetStart != false) {
        ResetStart = other.ResetStart;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            ResetStart = input.ReadBool();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ResetStart = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class PlayOutput : pb::IMessage<PlayOutput>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayOutput> _parser = new pb::MessageParser<PlayOutput>(() => new PlayOutput());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PlayOutput> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayOutput() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayOutput(PlayOutput other) : this() {
      expectedBlockHeight_ = other.expectedBlockHeight_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayOutput Clone() {
      return new PlayOutput(this);
    }

    /// <summary>Field number for the "expected_block_height" field.</summary>
    public const int ExpectedBlockHeightFieldNumber = 1;
    private long expectedBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long ExpectedBlockHeight {
      get { return expectedBlockHeight_; }
      set {
        expectedBlockHeight_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PlayOutput);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PlayOutput other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ExpectedBlockHeight != other.ExpectedBlockHeight) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ExpectedBlockHeight != 0L) hash ^= ExpectedBlockHeight.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ExpectedBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(ExpectedBlockHeight);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ExpectedBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(ExpectedBlockHeight);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ExpectedBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ExpectedBlockHeight);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PlayOutput other) {
      if (other == null) {
        return;
      }
      if (other.ExpectedBlockHeight != 0L) {
        ExpectedBlockHeight = other.ExpectedBlockHeight;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            ExpectedBlockHeight = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            ExpectedBlockHeight = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class GetBoutInformationInput : pb::IMessage<GetBoutInformationInput>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<GetBoutInformationInput> _parser = new pb::MessageParser<GetBoutInformationInput>(() => new GetBoutInformationInput());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<GetBoutInformationInput> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GetBoutInformationInput() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GetBoutInformationInput(GetBoutInformationInput other) : this() {
      playId_ = other.playId_ != null ? other.playId_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GetBoutInformationInput Clone() {
      return new GetBoutInformationInput(this);
    }

    /// <summary>Field number for the "play_id" field.</summary>
    public const int PlayIdFieldNumber = 1;
    private global::AElf.Types.Hash playId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Hash PlayId {
      get { return playId_; }
      set {
        playId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as GetBoutInformationInput);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(GetBoutInformationInput other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(PlayId, other.PlayId)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (playId_ != null) hash ^= PlayId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (playId_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PlayId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (playId_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PlayId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (playId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayId);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(GetBoutInformationInput other) {
      if (other == null) {
        return;
      }
      if (other.playId_ != null) {
        if (playId_ == null) {
          PlayId = new global::AElf.Types.Hash();
        }
        PlayId.MergeFrom(other.PlayId);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class PlayerInformation : pb::IMessage<PlayerInformation>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayerInformation> _parser = new pb::MessageParser<PlayerInformation>(() => new PlayerInformation());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PlayerInformation> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInformation() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInformation(PlayerInformation other) : this() {
      playerAddress_ = other.playerAddress_ != null ? other.playerAddress_.Clone() : null;
      playableCount_ = other.playableCount_;
      lastPlayTime_ = other.lastPlayTime_ != null ? other.lastPlayTime_.Clone() : null;
      sumScore_ = other.sumScore_;
      curGridNum_ = other.curGridNum_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInformation Clone() {
      return new PlayerInformation(this);
    }

    /// <summary>Field number for the "player_address" field.</summary>
    public const int PlayerAddressFieldNumber = 1;
    private global::AElf.Types.Address playerAddress_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Address PlayerAddress {
      get { return playerAddress_; }
      set {
        playerAddress_ = value;
      }
    }

    /// <summary>Field number for the "playable_count" field.</summary>
    public const int PlayableCountFieldNumber = 2;
    private int playableCount_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int PlayableCount {
      get { return playableCount_; }
      set {
        playableCount_ = value;
      }
    }

    /// <summary>Field number for the "last_play_time" field.</summary>
    public const int LastPlayTimeFieldNumber = 3;
    private global::Google.Protobuf.WellKnownTypes.Timestamp lastPlayTime_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Google.Protobuf.WellKnownTypes.Timestamp LastPlayTime {
      get { return lastPlayTime_; }
      set {
        lastPlayTime_ = value;
      }
    }

    /// <summary>Field number for the "sum_score" field.</summary>
    public const int SumScoreFieldNumber = 4;
    private long sumScore_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long SumScore {
      get { return sumScore_; }
      set {
        sumScore_ = value;
      }
    }

    /// <summary>Field number for the "cur_grid_num" field.</summary>
    public const int CurGridNumFieldNumber = 5;
    private int curGridNum_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CurGridNum {
      get { return curGridNum_; }
      set {
        curGridNum_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PlayerInformation);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PlayerInformation other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(PlayerAddress, other.PlayerAddress)) return false;
      if (PlayableCount != other.PlayableCount) return false;
      if (!object.Equals(LastPlayTime, other.LastPlayTime)) return false;
      if (SumScore != other.SumScore) return false;
      if (CurGridNum != other.CurGridNum) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (playerAddress_ != null) hash ^= PlayerAddress.GetHashCode();
      if (PlayableCount != 0) hash ^= PlayableCount.GetHashCode();
      if (lastPlayTime_ != null) hash ^= LastPlayTime.GetHashCode();
      if (SumScore != 0L) hash ^= SumScore.GetHashCode();
      if (CurGridNum != 0) hash ^= CurGridNum.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (playerAddress_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PlayerAddress);
      }
      if (PlayableCount != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(PlayableCount);
      }
      if (lastPlayTime_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(LastPlayTime);
      }
      if (SumScore != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(SumScore);
      }
      if (CurGridNum != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(CurGridNum);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (playerAddress_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(PlayerAddress);
      }
      if (PlayableCount != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(PlayableCount);
      }
      if (lastPlayTime_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(LastPlayTime);
      }
      if (SumScore != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(SumScore);
      }
      if (CurGridNum != 0) {
        output.WriteRawTag(40);
        output.WriteInt32(CurGridNum);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (playerAddress_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayerAddress);
      }
      if (PlayableCount != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(PlayableCount);
      }
      if (lastPlayTime_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(LastPlayTime);
      }
      if (SumScore != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(SumScore);
      }
      if (CurGridNum != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(CurGridNum);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PlayerInformation other) {
      if (other == null) {
        return;
      }
      if (other.playerAddress_ != null) {
        if (playerAddress_ == null) {
          PlayerAddress = new global::AElf.Types.Address();
        }
        PlayerAddress.MergeFrom(other.PlayerAddress);
      }
      if (other.PlayableCount != 0) {
        PlayableCount = other.PlayableCount;
      }
      if (other.lastPlayTime_ != null) {
        if (lastPlayTime_ == null) {
          LastPlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        LastPlayTime.MergeFrom(other.LastPlayTime);
      }
      if (other.SumScore != 0L) {
        SumScore = other.SumScore;
      }
      if (other.CurGridNum != 0) {
        CurGridNum = other.CurGridNum;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
          case 16: {
            PlayableCount = input.ReadInt32();
            break;
          }
          case 26: {
            if (lastPlayTime_ == null) {
              LastPlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(LastPlayTime);
            break;
          }
          case 32: {
            SumScore = input.ReadInt64();
            break;
          }
          case 40: {
            CurGridNum = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
          case 16: {
            PlayableCount = input.ReadInt32();
            break;
          }
          case 26: {
            if (lastPlayTime_ == null) {
              LastPlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(LastPlayTime);
            break;
          }
          case 32: {
            SumScore = input.ReadInt64();
            break;
          }
          case 40: {
            CurGridNum = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class BoutInformation : pb::IMessage<BoutInformation>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<BoutInformation> _parser = new pb::MessageParser<BoutInformation>(() => new BoutInformation());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<BoutInformation> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoutInformation() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoutInformation(BoutInformation other) : this() {
      playBlockHeight_ = other.playBlockHeight_;
      gridType_ = other.gridType_;
      gridNum_ = other.gridNum_;
      score_ = other.score_;
      isComplete_ = other.isComplete_;
      playId_ = other.playId_ != null ? other.playId_.Clone() : null;
      bingoBlockHeight_ = other.bingoBlockHeight_;
      playTime_ = other.playTime_ != null ? other.playTime_.Clone() : null;
      playerAddress_ = other.playerAddress_ != null ? other.playerAddress_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoutInformation Clone() {
      return new BoutInformation(this);
    }

    /// <summary>Field number for the "play_block_height" field.</summary>
    public const int PlayBlockHeightFieldNumber = 1;
    private long playBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long PlayBlockHeight {
      get { return playBlockHeight_; }
      set {
        playBlockHeight_ = value;
      }
    }

    /// <summary>Field number for the "grid_type" field.</summary>
    public const int GridTypeFieldNumber = 2;
    private global::Portkey.Contracts.BeangoTownContract.GridType gridType_ = global::Portkey.Contracts.BeangoTownContract.GridType.Blue;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Portkey.Contracts.BeangoTownContract.GridType GridType {
      get { return gridType_; }
      set {
        gridType_ = value;
      }
    }

    /// <summary>Field number for the "grid_num" field.</summary>
    public const int GridNumFieldNumber = 3;
    private int gridNum_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int GridNum {
      get { return gridNum_; }
      set {
        gridNum_ = value;
      }
    }

    /// <summary>Field number for the "score" field.</summary>
    public const int ScoreFieldNumber = 4;
    private int score_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Score {
      get { return score_; }
      set {
        score_ = value;
      }
    }

    /// <summary>Field number for the "is_complete" field.</summary>
    public const int IsCompleteFieldNumber = 5;
    private bool isComplete_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool IsComplete {
      get { return isComplete_; }
      set {
        isComplete_ = value;
      }
    }

    /// <summary>Field number for the "play_id" field.</summary>
    public const int PlayIdFieldNumber = 6;
    private global::AElf.Types.Hash playId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Hash PlayId {
      get { return playId_; }
      set {
        playId_ = value;
      }
    }

    /// <summary>Field number for the "bingo_block_height" field.</summary>
    public const int BingoBlockHeightFieldNumber = 7;
    private long bingoBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long BingoBlockHeight {
      get { return bingoBlockHeight_; }
      set {
        bingoBlockHeight_ = value;
      }
    }

    /// <summary>Field number for the "play_time" field.</summary>
    public const int PlayTimeFieldNumber = 8;
    private global::Google.Protobuf.WellKnownTypes.Timestamp playTime_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Google.Protobuf.WellKnownTypes.Timestamp PlayTime {
      get { return playTime_; }
      set {
        playTime_ = value;
      }
    }

    /// <summary>Field number for the "player_address" field.</summary>
    public const int PlayerAddressFieldNumber = 9;
    private global::AElf.Types.Address playerAddress_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Address PlayerAddress {
      get { return playerAddress_; }
      set {
        playerAddress_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as BoutInformation);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(BoutInformation other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PlayBlockHeight != other.PlayBlockHeight) return false;
      if (GridType != other.GridType) return false;
      if (GridNum != other.GridNum) return false;
      if (Score != other.Score) return false;
      if (IsComplete != other.IsComplete) return false;
      if (!object.Equals(PlayId, other.PlayId)) return false;
      if (BingoBlockHeight != other.BingoBlockHeight) return false;
      if (!object.Equals(PlayTime, other.PlayTime)) return false;
      if (!object.Equals(PlayerAddress, other.PlayerAddress)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (PlayBlockHeight != 0L) hash ^= PlayBlockHeight.GetHashCode();
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) hash ^= GridType.GetHashCode();
      if (GridNum != 0) hash ^= GridNum.GetHashCode();
      if (Score != 0) hash ^= Score.GetHashCode();
      if (IsComplete != false) hash ^= IsComplete.GetHashCode();
      if (playId_ != null) hash ^= PlayId.GetHashCode();
      if (BingoBlockHeight != 0L) hash ^= BingoBlockHeight.GetHashCode();
      if (playTime_ != null) hash ^= PlayTime.GetHashCode();
      if (playerAddress_ != null) hash ^= PlayerAddress.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        output.WriteRawTag(16);
        output.WriteEnum((int) GridType);
      }
      if (GridNum != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(GridNum);
      }
      if (Score != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Score);
      }
      if (IsComplete != false) {
        output.WriteRawTag(40);
        output.WriteBool(IsComplete);
      }
      if (playId_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        output.WriteRawTag(56);
        output.WriteInt64(BingoBlockHeight);
      }
      if (playTime_ != null) {
        output.WriteRawTag(66);
        output.WriteMessage(PlayTime);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(74);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        output.WriteRawTag(16);
        output.WriteEnum((int) GridType);
      }
      if (GridNum != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(GridNum);
      }
      if (Score != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Score);
      }
      if (IsComplete != false) {
        output.WriteRawTag(40);
        output.WriteBool(IsComplete);
      }
      if (playId_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        output.WriteRawTag(56);
        output.WriteInt64(BingoBlockHeight);
      }
      if (playTime_ != null) {
        output.WriteRawTag(66);
        output.WriteMessage(PlayTime);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(74);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (PlayBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) GridType);
      }
      if (GridNum != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(GridNum);
      }
      if (Score != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Score);
      }
      if (IsComplete != false) {
        size += 1 + 1;
      }
      if (playId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(BingoBlockHeight);
      }
      if (playTime_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayTime);
      }
      if (playerAddress_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayerAddress);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(BoutInformation other) {
      if (other == null) {
        return;
      }
      if (other.PlayBlockHeight != 0L) {
        PlayBlockHeight = other.PlayBlockHeight;
      }
      if (other.GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        GridType = other.GridType;
      }
      if (other.GridNum != 0) {
        GridNum = other.GridNum;
      }
      if (other.Score != 0) {
        Score = other.Score;
      }
      if (other.IsComplete != false) {
        IsComplete = other.IsComplete;
      }
      if (other.playId_ != null) {
        if (playId_ == null) {
          PlayId = new global::AElf.Types.Hash();
        }
        PlayId.MergeFrom(other.PlayId);
      }
      if (other.BingoBlockHeight != 0L) {
        BingoBlockHeight = other.BingoBlockHeight;
      }
      if (other.playTime_ != null) {
        if (playTime_ == null) {
          PlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        PlayTime.MergeFrom(other.PlayTime);
      }
      if (other.playerAddress_ != null) {
        if (playerAddress_ == null) {
          PlayerAddress = new global::AElf.Types.Address();
        }
        PlayerAddress.MergeFrom(other.PlayerAddress);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 16: {
            GridType = (global::Portkey.Contracts.BeangoTownContract.GridType) input.ReadEnum();
            break;
          }
          case 24: {
            GridNum = input.ReadInt32();
            break;
          }
          case 32: {
            Score = input.ReadInt32();
            break;
          }
          case 40: {
            IsComplete = input.ReadBool();
            break;
          }
          case 50: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 56: {
            BingoBlockHeight = input.ReadInt64();
            break;
          }
          case 66: {
            if (playTime_ == null) {
              PlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(PlayTime);
            break;
          }
          case 74: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 16: {
            GridType = (global::Portkey.Contracts.BeangoTownContract.GridType) input.ReadEnum();
            break;
          }
          case 24: {
            GridNum = input.ReadInt32();
            break;
          }
          case 32: {
            Score = input.ReadInt32();
            break;
          }
          case 40: {
            IsComplete = input.ReadBool();
            break;
          }
          case 50: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 56: {
            BingoBlockHeight = input.ReadInt64();
            break;
          }
          case 66: {
            if (playTime_ == null) {
              PlayTime = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(PlayTime);
            break;
          }
          case 74: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class GameLimitSettings : pb::IMessage<GameLimitSettings>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<GameLimitSettings> _parser = new pb::MessageParser<GameLimitSettings>(() => new GameLimitSettings());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<GameLimitSettings> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[5]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameLimitSettings() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameLimitSettings(GameLimitSettings other) : this() {
      dailyMaxPlayCount_ = other.dailyMaxPlayCount_;
      dailyPlayCountResetHours_ = other.dailyPlayCountResetHours_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GameLimitSettings Clone() {
      return new GameLimitSettings(this);
    }

    /// <summary>Field number for the "daily_max_play_count" field.</summary>
    public const int DailyMaxPlayCountFieldNumber = 1;
    private int dailyMaxPlayCount_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int DailyMaxPlayCount {
      get { return dailyMaxPlayCount_; }
      set {
        dailyMaxPlayCount_ = value;
      }
    }

    /// <summary>Field number for the "daily_play_count_reset_hours" field.</summary>
    public const int DailyPlayCountResetHoursFieldNumber = 2;
    private int dailyPlayCountResetHours_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int DailyPlayCountResetHours {
      get { return dailyPlayCountResetHours_; }
      set {
        dailyPlayCountResetHours_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as GameLimitSettings);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(GameLimitSettings other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (DailyMaxPlayCount != other.DailyMaxPlayCount) return false;
      if (DailyPlayCountResetHours != other.DailyPlayCountResetHours) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (DailyMaxPlayCount != 0) hash ^= DailyMaxPlayCount.GetHashCode();
      if (DailyPlayCountResetHours != 0) hash ^= DailyPlayCountResetHours.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (DailyMaxPlayCount != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(DailyMaxPlayCount);
      }
      if (DailyPlayCountResetHours != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(DailyPlayCountResetHours);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (DailyMaxPlayCount != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(DailyMaxPlayCount);
      }
      if (DailyPlayCountResetHours != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(DailyPlayCountResetHours);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (DailyMaxPlayCount != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(DailyMaxPlayCount);
      }
      if (DailyPlayCountResetHours != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(DailyPlayCountResetHours);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(GameLimitSettings other) {
      if (other == null) {
        return;
      }
      if (other.DailyMaxPlayCount != 0) {
        DailyMaxPlayCount = other.DailyMaxPlayCount;
      }
      if (other.DailyPlayCountResetHours != 0) {
        DailyPlayCountResetHours = other.DailyPlayCountResetHours;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            DailyMaxPlayCount = input.ReadInt32();
            break;
          }
          case 16: {
            DailyPlayCountResetHours = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            DailyMaxPlayCount = input.ReadInt32();
            break;
          }
          case 16: {
            DailyPlayCountResetHours = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class GridTypeList : pb::IMessage<GridTypeList>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<GridTypeList> _parser = new pb::MessageParser<GridTypeList>(() => new GridTypeList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<GridTypeList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[6]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GridTypeList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GridTypeList(GridTypeList other) : this() {
      value_ = other.value_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public GridTypeList Clone() {
      return new GridTypeList(this);
    }

    /// <summary>Field number for the "value" field.</summary>
    public const int ValueFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Portkey.Contracts.BeangoTownContract.GridType> _repeated_value_codec
        = pb::FieldCodec.ForEnum(10, x => (int) x, x => (global::Portkey.Contracts.BeangoTownContract.GridType) x);
    private readonly pbc::RepeatedField<global::Portkey.Contracts.BeangoTownContract.GridType> value_ = new pbc::RepeatedField<global::Portkey.Contracts.BeangoTownContract.GridType>();
    /// <summary>
    /// The gridType list.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Portkey.Contracts.BeangoTownContract.GridType> Value {
      get { return value_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as GridTypeList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(GridTypeList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!value_.Equals(other.value_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= value_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      value_.WriteTo(output, _repeated_value_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      value_.WriteTo(ref output, _repeated_value_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += value_.CalculateSize(_repeated_value_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(GridTypeList other) {
      if (other == null) {
        return;
      }
      value_.Add(other.value_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 10:
          case 8: {
            value_.AddEntriesFrom(input, _repeated_value_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10:
          case 8: {
            value_.AddEntriesFrom(ref input, _repeated_value_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Played : pb::IMessage<Played>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Played> _parser = new pb::MessageParser<Played>(() => new Played());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Played> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[7]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Played() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Played(Played other) : this() {
      playBlockHeight_ = other.playBlockHeight_;
      playId_ = other.playId_ != null ? other.playId_.Clone() : null;
      playerAddress_ = other.playerAddress_ != null ? other.playerAddress_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Played Clone() {
      return new Played(this);
    }

    /// <summary>Field number for the "play_block_height" field.</summary>
    public const int PlayBlockHeightFieldNumber = 1;
    private long playBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long PlayBlockHeight {
      get { return playBlockHeight_; }
      set {
        playBlockHeight_ = value;
      }
    }

    /// <summary>Field number for the "play_id" field.</summary>
    public const int PlayIdFieldNumber = 2;
    private global::AElf.Types.Hash playId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Hash PlayId {
      get { return playId_; }
      set {
        playId_ = value;
      }
    }

    /// <summary>Field number for the "player_address" field.</summary>
    public const int PlayerAddressFieldNumber = 3;
    private global::AElf.Types.Address playerAddress_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Address PlayerAddress {
      get { return playerAddress_; }
      set {
        playerAddress_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Played);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Played other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PlayBlockHeight != other.PlayBlockHeight) return false;
      if (!object.Equals(PlayId, other.PlayId)) return false;
      if (!object.Equals(PlayerAddress, other.PlayerAddress)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (PlayBlockHeight != 0L) hash ^= PlayBlockHeight.GetHashCode();
      if (playId_ != null) hash ^= PlayId.GetHashCode();
      if (playerAddress_ != null) hash ^= PlayerAddress.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (playId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(PlayId);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (playId_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(PlayId);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (PlayBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(PlayBlockHeight);
      }
      if (playId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayId);
      }
      if (playerAddress_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayerAddress);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Played other) {
      if (other == null) {
        return;
      }
      if (other.PlayBlockHeight != 0L) {
        PlayBlockHeight = other.PlayBlockHeight;
      }
      if (other.playId_ != null) {
        if (playId_ == null) {
          PlayId = new global::AElf.Types.Hash();
        }
        PlayId.MergeFrom(other.PlayId);
      }
      if (other.playerAddress_ != null) {
        if (playerAddress_ == null) {
          PlayerAddress = new global::AElf.Types.Address();
        }
        PlayerAddress.MergeFrom(other.PlayerAddress);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 18: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 26: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 18: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 26: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Bingoed : pb::IMessage<Bingoed>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Bingoed> _parser = new pb::MessageParser<Bingoed>(() => new Bingoed());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Bingoed> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.MessageTypes[8]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Bingoed() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Bingoed(Bingoed other) : this() {
      playBlockHeight_ = other.playBlockHeight_;
      gridType_ = other.gridType_;
      gridNum_ = other.gridNum_;
      score_ = other.score_;
      isComplete_ = other.isComplete_;
      playId_ = other.playId_ != null ? other.playId_.Clone() : null;
      bingoBlockHeight_ = other.bingoBlockHeight_;
      playerAddress_ = other.playerAddress_ != null ? other.playerAddress_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Bingoed Clone() {
      return new Bingoed(this);
    }

    /// <summary>Field number for the "play_block_height" field.</summary>
    public const int PlayBlockHeightFieldNumber = 1;
    private long playBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long PlayBlockHeight {
      get { return playBlockHeight_; }
      set {
        playBlockHeight_ = value;
      }
    }

    /// <summary>Field number for the "grid_type" field.</summary>
    public const int GridTypeFieldNumber = 2;
    private global::Portkey.Contracts.BeangoTownContract.GridType gridType_ = global::Portkey.Contracts.BeangoTownContract.GridType.Blue;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Portkey.Contracts.BeangoTownContract.GridType GridType {
      get { return gridType_; }
      set {
        gridType_ = value;
      }
    }

    /// <summary>Field number for the "grid_num" field.</summary>
    public const int GridNumFieldNumber = 3;
    private int gridNum_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int GridNum {
      get { return gridNum_; }
      set {
        gridNum_ = value;
      }
    }

    /// <summary>Field number for the "score" field.</summary>
    public const int ScoreFieldNumber = 4;
    private long score_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long Score {
      get { return score_; }
      set {
        score_ = value;
      }
    }

    /// <summary>Field number for the "is_complete" field.</summary>
    public const int IsCompleteFieldNumber = 5;
    private bool isComplete_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool IsComplete {
      get { return isComplete_; }
      set {
        isComplete_ = value;
      }
    }

    /// <summary>Field number for the "play_id" field.</summary>
    public const int PlayIdFieldNumber = 6;
    private global::AElf.Types.Hash playId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Hash PlayId {
      get { return playId_; }
      set {
        playId_ = value;
      }
    }

    /// <summary>Field number for the "bingo_block_height" field.</summary>
    public const int BingoBlockHeightFieldNumber = 7;
    private long bingoBlockHeight_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long BingoBlockHeight {
      get { return bingoBlockHeight_; }
      set {
        bingoBlockHeight_ = value;
      }
    }

    /// <summary>Field number for the "player_address" field.</summary>
    public const int PlayerAddressFieldNumber = 8;
    private global::AElf.Types.Address playerAddress_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::AElf.Types.Address PlayerAddress {
      get { return playerAddress_; }
      set {
        playerAddress_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Bingoed);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Bingoed other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PlayBlockHeight != other.PlayBlockHeight) return false;
      if (GridType != other.GridType) return false;
      if (GridNum != other.GridNum) return false;
      if (Score != other.Score) return false;
      if (IsComplete != other.IsComplete) return false;
      if (!object.Equals(PlayId, other.PlayId)) return false;
      if (BingoBlockHeight != other.BingoBlockHeight) return false;
      if (!object.Equals(PlayerAddress, other.PlayerAddress)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (PlayBlockHeight != 0L) hash ^= PlayBlockHeight.GetHashCode();
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) hash ^= GridType.GetHashCode();
      if (GridNum != 0) hash ^= GridNum.GetHashCode();
      if (Score != 0L) hash ^= Score.GetHashCode();
      if (IsComplete != false) hash ^= IsComplete.GetHashCode();
      if (playId_ != null) hash ^= PlayId.GetHashCode();
      if (BingoBlockHeight != 0L) hash ^= BingoBlockHeight.GetHashCode();
      if (playerAddress_ != null) hash ^= PlayerAddress.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        output.WriteRawTag(16);
        output.WriteEnum((int) GridType);
      }
      if (GridNum != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(GridNum);
      }
      if (Score != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(Score);
      }
      if (IsComplete != false) {
        output.WriteRawTag(40);
        output.WriteBool(IsComplete);
      }
      if (playId_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        output.WriteRawTag(56);
        output.WriteInt64(BingoBlockHeight);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(66);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (PlayBlockHeight != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        output.WriteRawTag(16);
        output.WriteEnum((int) GridType);
      }
      if (GridNum != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(GridNum);
      }
      if (Score != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(Score);
      }
      if (IsComplete != false) {
        output.WriteRawTag(40);
        output.WriteBool(IsComplete);
      }
      if (playId_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        output.WriteRawTag(56);
        output.WriteInt64(BingoBlockHeight);
      }
      if (playerAddress_ != null) {
        output.WriteRawTag(66);
        output.WriteMessage(PlayerAddress);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (PlayBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(PlayBlockHeight);
      }
      if (GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) GridType);
      }
      if (GridNum != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(GridNum);
      }
      if (Score != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Score);
      }
      if (IsComplete != false) {
        size += 1 + 1;
      }
      if (playId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayId);
      }
      if (BingoBlockHeight != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(BingoBlockHeight);
      }
      if (playerAddress_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(PlayerAddress);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Bingoed other) {
      if (other == null) {
        return;
      }
      if (other.PlayBlockHeight != 0L) {
        PlayBlockHeight = other.PlayBlockHeight;
      }
      if (other.GridType != global::Portkey.Contracts.BeangoTownContract.GridType.Blue) {
        GridType = other.GridType;
      }
      if (other.GridNum != 0) {
        GridNum = other.GridNum;
      }
      if (other.Score != 0L) {
        Score = other.Score;
      }
      if (other.IsComplete != false) {
        IsComplete = other.IsComplete;
      }
      if (other.playId_ != null) {
        if (playId_ == null) {
          PlayId = new global::AElf.Types.Hash();
        }
        PlayId.MergeFrom(other.PlayId);
      }
      if (other.BingoBlockHeight != 0L) {
        BingoBlockHeight = other.BingoBlockHeight;
      }
      if (other.playerAddress_ != null) {
        if (playerAddress_ == null) {
          PlayerAddress = new global::AElf.Types.Address();
        }
        PlayerAddress.MergeFrom(other.PlayerAddress);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 16: {
            GridType = (global::Portkey.Contracts.BeangoTownContract.GridType) input.ReadEnum();
            break;
          }
          case 24: {
            GridNum = input.ReadInt32();
            break;
          }
          case 32: {
            Score = input.ReadInt64();
            break;
          }
          case 40: {
            IsComplete = input.ReadBool();
            break;
          }
          case 50: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 56: {
            BingoBlockHeight = input.ReadInt64();
            break;
          }
          case 66: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            PlayBlockHeight = input.ReadInt64();
            break;
          }
          case 16: {
            GridType = (global::Portkey.Contracts.BeangoTownContract.GridType) input.ReadEnum();
            break;
          }
          case 24: {
            GridNum = input.ReadInt32();
            break;
          }
          case 32: {
            Score = input.ReadInt64();
            break;
          }
          case 40: {
            IsComplete = input.ReadBool();
            break;
          }
          case 50: {
            if (playId_ == null) {
              PlayId = new global::AElf.Types.Hash();
            }
            input.ReadMessage(PlayId);
            break;
          }
          case 56: {
            BingoBlockHeight = input.ReadInt64();
            break;
          }
          case 66: {
            if (playerAddress_ == null) {
              PlayerAddress = new global::AElf.Types.Address();
            }
            input.ReadMessage(PlayerAddress);
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