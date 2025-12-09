#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace Portkey.Contracts.BeangoTownContract
{
    
  #region Events
  public partial class Played : aelf::IEvent<Played>
  {
    public global::System.Collections.Generic.IEnumerable<Played> GetIndexed()
    {
      return new List<Played>
      {
      };
    }

    public Played GetNonIndexed()
    {
      return new Played
      {
        PlayBlockHeight = PlayBlockHeight,
        PlayId = PlayId,
        PlayerAddress = PlayerAddress,
      };
    }
  }

  public partial class Bingoed : aelf::IEvent<Bingoed>
  {
    public global::System.Collections.Generic.IEnumerable<Bingoed> GetIndexed()
    {
      return new List<Bingoed>
      {
      };
    }

    public Bingoed GetNonIndexed()
    {
      return new Bingoed
      {
        PlayBlockHeight = PlayBlockHeight,
        GridType = GridType,
        GridNum = GridNum,
        Score = Score,
        IsComplete = IsComplete,
        PlayId = PlayId,
        BingoBlockHeight = BingoBlockHeight,
        PlayerAddress = PlayerAddress,
      };
    }
  }

  #endregion
  /// <summary>
  /// the contract definition: a gRPC service definition.
  /// </summary>
  public static partial class BeangoTownContractContainer
  {
    static readonly string __ServiceName = "BeangoTownContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.PlayInput> __Marshaller_PlayInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.PlayInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.PlayOutput> __Marshaller_PlayOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.PlayOutput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Hash> __Marshaller_aelf_Hash = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Hash.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.GameLimitSettings> __Marshaller_GameLimitSettings = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.GameLimitSettings.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Address> __Marshaller_aelf_Address = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Address.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.BoolValue> __Marshaller_google_protobuf_BoolValue = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.BoolValue.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.PlayerInformation> __Marshaller_PlayerInformation = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.PlayerInformation.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput> __Marshaller_GetBoutInformationInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BeangoTownContract.BoutInformation> __Marshaller_BoutInformation = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BeangoTownContract.BoutInformation.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::Portkey.Contracts.BeangoTownContract.PlayInput, global::Portkey.Contracts.BeangoTownContract.PlayOutput> __Method_Play = new aelf::Method<global::Portkey.Contracts.BeangoTownContract.PlayInput, global::Portkey.Contracts.BeangoTownContract.PlayOutput>(
        aelf::MethodType.Action,
        __ServiceName,
        "Play",
        __Marshaller_PlayInput,
        __Marshaller_PlayOutput);

    static readonly aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Bingo = new aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Bingo",
        __Marshaller_aelf_Hash,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Portkey.Contracts.BeangoTownContract.GameLimitSettings, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetGameLimitSettings = new aelf::Method<global::Portkey.Contracts.BeangoTownContract.GameLimitSettings, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetGameLimitSettings",
        __Marshaller_GameLimitSettings,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty> __Method_ChangeAdmin = new aelf::Method<global::AElf.Types.Address, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "ChangeAdmin",
        __Marshaller_aelf_Address,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.BoolValue> __Method_CheckBeanPass = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.BoolValue>(
        aelf::MethodType.View,
        __ServiceName,
        "CheckBeanPass",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_BoolValue);

    static readonly aelf::Method<global::AElf.Types.Address, global::Portkey.Contracts.BeangoTownContract.PlayerInformation> __Method_GetPlayerInformation = new aelf::Method<global::AElf.Types.Address, global::Portkey.Contracts.BeangoTownContract.PlayerInformation>(
        aelf::MethodType.View,
        __ServiceName,
        "GetPlayerInformation",
        __Marshaller_aelf_Address,
        __Marshaller_PlayerInformation);

    static readonly aelf::Method<global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput, global::Portkey.Contracts.BeangoTownContract.BoutInformation> __Method_GetBoutInformation = new aelf::Method<global::Portkey.Contracts.BeangoTownContract.GetBoutInformationInput, global::Portkey.Contracts.BeangoTownContract.BoutInformation>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBoutInformation",
        __Marshaller_GetBoutInformationInput,
        __Marshaller_BoutInformation);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address> __Method_GetAdmin = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::AElf.Types.Address>(
        aelf::MethodType.View,
        __ServiceName,
        "GetAdmin",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_aelf_Address);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Portkey.Contracts.BeangoTownContract.GameLimitSettings> __Method_GetGameLimitSettings = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Portkey.Contracts.BeangoTownContract.GameLimitSettings>(
        aelf::MethodType.View,
        __ServiceName,
        "GetGameLimitSettings",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_GameLimitSettings);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS12.Acs12Reflection.Descriptor.Services[0],
          global::Portkey.Contracts.BeangoTownContract.BeangoTownContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion

    /// <summary>Base class for the contract of BeangoTownContract</summary>
   

  }
}
#endregion