// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: bingo_game_contract.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace Portkey.Contracts.BingoGameContract {

  #region Events
  public partial class Played : aelf::IEvent<Played>
  {
    public global::System.Collections.Generic.IEnumerable<Played> GetIndexed()
    {
      return new List<Played>
      {
      new Played
      {
        PlayBlockHeight = PlayBlockHeight
      },
      new Played
      {
        Amount = Amount
      },
      new Played
      {
        Type = Type
      },
      new Played
      {
        PlayId = PlayId
      },
      new Played
      {
        PlayerAddress = PlayerAddress
      },
      new Played
      {
        Symbol = Symbol
      },
      };
    }

    public Played GetNonIndexed()
    {
      return new Played
      {
      };
    }
  }

  public partial class Bingoed : aelf::IEvent<Bingoed>
  {
    public global::System.Collections.Generic.IEnumerable<Bingoed> GetIndexed()
    {
      return new List<Bingoed>
      {
      new Bingoed
      {
        PlayBlockHeight = PlayBlockHeight
      },
      new Bingoed
      {
        Amount = Amount
      },
      new Bingoed
      {
        Award = Award
      },
      new Bingoed
      {
        IsComplete = IsComplete
      },
      new Bingoed
      {
        PlayId = PlayId
      },
      new Bingoed
      {
        BingoBlockHeight = BingoBlockHeight
      },
      new Bingoed
      {
        Type = Type
      },
      new Bingoed
      {
        RandomNumber = RandomNumber
      },
      new Bingoed
      {
        Dices = Dices
      },
      new Bingoed
      {
        PlayerAddress = PlayerAddress
      },
      };
    }

    public Bingoed GetNonIndexed()
    {
      return new Bingoed
      {
      };
    }
  }

  public partial class Registered : aelf::IEvent<Registered>
  {
    public global::System.Collections.Generic.IEnumerable<Registered> GetIndexed()
    {
      return new List<Registered>
      {
      new Registered
      {
        Seed = Seed
      },
      new Registered
      {
        RegisterTime = RegisterTime
      },
      new Registered
      {
        PlayerAddress = PlayerAddress
      },
      };
    }

    public Registered GetNonIndexed()
    {
      return new Registered
      {
      };
    }
  }

  #endregion
  public static partial class BingoGameContractContainer
  {
    static readonly string __ServiceName = "BingoGameContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.PlayInput> __Marshaller_PlayInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.PlayInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Int64Value> __Marshaller_google_protobuf_Int64Value = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Int64Value.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Hash> __Marshaller_aelf_Hash = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Hash.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.BoolValue> __Marshaller_google_protobuf_BoolValue = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.BoolValue.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.LimitSettings> __Marshaller_LimitSettings = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.LimitSettings.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Address> __Marshaller_aelf_Address = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Address.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.PlayerInformation> __Marshaller_PlayerInformation = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.PlayerInformation.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Int32Value> __Marshaller_google_protobuf_Int32Value = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Int32Value.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.GetBoutInformationInput> __Marshaller_GetBoutInformationInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.GetBoutInformationInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.BoutInformation> __Marshaller_BoutInformation = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.BoutInformation.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.GetRandomHashInput> __Marshaller_GetRandomHashInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.GetRandomHashInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Portkey.Contracts.BingoGameContract.GetRandomHashOutput> __Marshaller_GetRandomHashOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Portkey.Contracts.BingoGameContract.GetRandomHashOutput.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Register = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Register",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Portkey.Contracts.BingoGameContract.PlayInput, global::Google.Protobuf.WellKnownTypes.Int64Value> __Method_Play = new aelf::Method<global::Portkey.Contracts.BingoGameContract.PlayInput, global::Google.Protobuf.WellKnownTypes.Int64Value>(
        aelf::MethodType.Action,
        __ServiceName,
        "Play",
        __Marshaller_PlayInput,
        __Marshaller_google_protobuf_Int64Value);

    static readonly aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.BoolValue> __Method_Bingo = new aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.BoolValue>(
        aelf::MethodType.Action,
        __ServiceName,
        "Bingo",
        __Marshaller_aelf_Hash,
        __Marshaller_google_protobuf_BoolValue);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Quit = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Quit",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Portkey.Contracts.BingoGameContract.LimitSettings, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetLimitSettings = new aelf::Method<global::Portkey.Contracts.BingoGameContract.LimitSettings, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetLimitSettings",
        __Marshaller_LimitSettings,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Int64Value> __Method_GetAward = new aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Int64Value>(
        aelf::MethodType.View,
        __ServiceName,
        "GetAward",
        __Marshaller_aelf_Hash,
        __Marshaller_google_protobuf_Int64Value);

    static readonly aelf::Method<global::AElf.Types.Address, global::Portkey.Contracts.BingoGameContract.PlayerInformation> __Method_GetPlayerInformation = new aelf::Method<global::AElf.Types.Address, global::Portkey.Contracts.BingoGameContract.PlayerInformation>(
        aelf::MethodType.View,
        __ServiceName,
        "GetPlayerInformation",
        __Marshaller_aelf_Address,
        __Marshaller_PlayerInformation);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Portkey.Contracts.BingoGameContract.LimitSettings> __Method_GetLimitSettings = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Portkey.Contracts.BingoGameContract.LimitSettings>(
        aelf::MethodType.View,
        __ServiceName,
        "GetLimitSettings",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_LimitSettings);

    static readonly aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Int32Value> __Method_GetRandomNumber = new aelf::Method<global::AElf.Types.Hash, global::Google.Protobuf.WellKnownTypes.Int32Value>(
        aelf::MethodType.View,
        __ServiceName,
        "GetRandomNumber",
        __Marshaller_aelf_Hash,
        __Marshaller_google_protobuf_Int32Value);

    static readonly aelf::Method<global::Portkey.Contracts.BingoGameContract.GetBoutInformationInput, global::Portkey.Contracts.BingoGameContract.BoutInformation> __Method_GetBoutInformation = new aelf::Method<global::Portkey.Contracts.BingoGameContract.GetBoutInformationInput, global::Portkey.Contracts.BingoGameContract.BoutInformation>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBoutInformation",
        __Marshaller_GetBoutInformationInput,
        __Marshaller_BoutInformation);

    static readonly aelf::Method<global::Portkey.Contracts.BingoGameContract.GetRandomHashInput, global::Portkey.Contracts.BingoGameContract.GetRandomHashOutput> __Method_GetRandomHash = new aelf::Method<global::Portkey.Contracts.BingoGameContract.GetRandomHashInput, global::Portkey.Contracts.BingoGameContract.GetRandomHashOutput>(
        aelf::MethodType.View,
        __ServiceName,
        "GetRandomHash",
        __Marshaller_GetRandomHashInput,
        __Marshaller_GetRandomHashOutput);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Portkey.Contracts.BingoGameContract.BingoGameContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS12.Acs12Reflection.Descriptor.Services[0],
          global::Portkey.Contracts.BingoGameContract.BingoGameContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion

    // /// <summary>Base class for the contract of BingoGameContract</summary>
    // public abstract partial class BingoGameContractBase : AElf.Sdk.CSharp.CSharpSmartContract<Portkey.Contracts.BingoGameContract.BingoGameContractState>
    // {
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Register(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Int64Value Play(global::Portkey.Contracts.BingoGameContract.PlayInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.BoolValue Bingo(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Quit(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty SetLimitSettings(global::Portkey.Contracts.BingoGameContract.LimitSettings input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Initialize(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Int64Value GetAward(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Portkey.Contracts.BingoGameContract.PlayerInformation GetPlayerInformation(global::AElf.Types.Address input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Portkey.Contracts.BingoGameContract.LimitSettings GetLimitSettings(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Google.Protobuf.WellKnownTypes.Int32Value GetRandomNumber(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Portkey.Contracts.BingoGameContract.BoutInformation GetBoutInformation(global::Portkey.Contracts.BingoGameContract.GetBoutInformationInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    //   public virtual global::Portkey.Contracts.BingoGameContract.GetRandomHashOutput GetRandomHash(global::Portkey.Contracts.BingoGameContract.GetRandomHashInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }

    // }

    // public static aelf::ServerServiceDefinition BindService(BingoGameContractBase serviceImpl)
    // {
    //   return aelf::ServerServiceDefinition.CreateBuilder()
    //       .AddDescriptors(Descriptors)
    //       .AddMethod(__Method_Register, serviceImpl.Register)
    //       .AddMethod(__Method_Play, serviceImpl.Play)
    //       .AddMethod(__Method_Bingo, serviceImpl.Bingo)
    //       .AddMethod(__Method_Quit, serviceImpl.Quit)
    //       .AddMethod(__Method_SetLimitSettings, serviceImpl.SetLimitSettings)
    //       .AddMethod(__Method_Initialize, serviceImpl.Initialize)
    //       .AddMethod(__Method_GetAward, serviceImpl.GetAward)
    //       .AddMethod(__Method_GetPlayerInformation, serviceImpl.GetPlayerInformation)
    //       .AddMethod(__Method_GetLimitSettings, serviceImpl.GetLimitSettings)
    //       .AddMethod(__Method_GetRandomNumber, serviceImpl.GetRandomNumber)
    //       .AddMethod(__Method_GetBoutInformation, serviceImpl.GetBoutInformation)
    //       .AddMethod(__Method_GetRandomHash, serviceImpl.GetRandomHash).Build();
    // }

  }
}
#endregion

