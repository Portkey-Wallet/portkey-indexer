// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ca_contract_impl.proto
// </auto-generated>
// Original file comments:
// *
// CA contract.
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace Portkey.Contracts.CA {

  #region Events
  public partial class DefaultTokenTransferLimitChanged : aelf::IEvent<DefaultTokenTransferLimitChanged>
  {
    public global::System.Collections.Generic.IEnumerable<DefaultTokenTransferLimitChanged> GetIndexed()
    {
      return new List<DefaultTokenTransferLimitChanged>
      {
      };
    }

    public DefaultTokenTransferLimitChanged GetNonIndexed()
    {
      return new DefaultTokenTransferLimitChanged
      {
        Symbol = Symbol,
        DefaultLimit = DefaultLimit,
      };
    }
  }

  public partial class ForbiddenForwardCallContractMethodChanged : aelf::IEvent<ForbiddenForwardCallContractMethodChanged>
  {
    public global::System.Collections.Generic.IEnumerable<ForbiddenForwardCallContractMethodChanged> GetIndexed()
    {
      return new List<ForbiddenForwardCallContractMethodChanged>
      {
      };
    }

    public ForbiddenForwardCallContractMethodChanged GetNonIndexed()
    {
      return new ForbiddenForwardCallContractMethodChanged
      {
        MethodName = MethodName,
        Address = Address,
        Forbidden = Forbidden,
      };
    }
  }

  #endregion
}
#endregion

