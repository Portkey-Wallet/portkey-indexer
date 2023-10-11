// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ca_contract.proto
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
  public partial class CAHolderCreated : aelf::IEvent<CAHolderCreated>
  {
    public global::System.Collections.Generic.IEnumerable<CAHolderCreated> GetIndexed()
    {
      return new List<CAHolderCreated>
      {
      new CAHolderCreated
      {
        Creator = Creator
      },
      new CAHolderCreated
      {
        CaHash = CaHash
      },
      new CAHolderCreated
      {
        CaAddress = CaAddress
      },
      new CAHolderCreated
      {
        Manager = Manager
      },
      };
    }

    public CAHolderCreated GetNonIndexed()
    {
      return new CAHolderCreated
      {
        ExtraData = ExtraData,
      };
    }
  }

  public partial class GuardianAdded : aelf::IEvent<GuardianAdded>
  {
    public global::System.Collections.Generic.IEnumerable<GuardianAdded> GetIndexed()
    {
      return new List<GuardianAdded>
      {
      new GuardianAdded
      {
        CaHash = CaHash
      },
      };
    }

    public GuardianAdded GetNonIndexed()
    {
      return new GuardianAdded
      {
        CaAddress = CaAddress,
        GuardianAdded_ = GuardianAdded_,
      };
    }
  }

  public partial class GuardianRemoved : aelf::IEvent<GuardianRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<GuardianRemoved> GetIndexed()
    {
      return new List<GuardianRemoved>
      {
      new GuardianRemoved
      {
        CaHash = CaHash
      },
      };
    }

    public GuardianRemoved GetNonIndexed()
    {
      return new GuardianRemoved
      {
        CaAddress = CaAddress,
        GuardianRemoved_ = GuardianRemoved_,
      };
    }
  }

  public partial class GuardianUpdated : aelf::IEvent<GuardianUpdated>
  {
    public global::System.Collections.Generic.IEnumerable<GuardianUpdated> GetIndexed()
    {
      return new List<GuardianUpdated>
      {
      new GuardianUpdated
      {
        CaHash = CaHash
      },
      };
    }

    public GuardianUpdated GetNonIndexed()
    {
      return new GuardianUpdated
      {
        CaAddress = CaAddress,
        GuardianUpdatedPre = GuardianUpdatedPre,
        GuardianUpdatedNew = GuardianUpdatedNew,
      };
    }
  }

  public partial class LoginGuardianAdded : aelf::IEvent<LoginGuardianAdded>
  {
    public global::System.Collections.Generic.IEnumerable<LoginGuardianAdded> GetIndexed()
    {
      return new List<LoginGuardianAdded>
      {
      new LoginGuardianAdded
      {
        CaHash = CaHash
      },
      new LoginGuardianAdded
      {
        Manager = Manager
      },
      new LoginGuardianAdded
      {
        LoginGuardian = LoginGuardian
      },
      };
    }

    public LoginGuardianAdded GetNonIndexed()
    {
      return new LoginGuardianAdded
      {
        CaAddress = CaAddress,
      };
    }
  }

  public partial class ManagerInfoSocialRecovered : aelf::IEvent<ManagerInfoSocialRecovered>
  {
    public global::System.Collections.Generic.IEnumerable<ManagerInfoSocialRecovered> GetIndexed()
    {
      return new List<ManagerInfoSocialRecovered>
      {
      new ManagerInfoSocialRecovered
      {
        CaHash = CaHash
      },
      new ManagerInfoSocialRecovered
      {
        CaAddress = CaAddress
      },
      new ManagerInfoSocialRecovered
      {
        Manager = Manager
      },
      };
    }

    public ManagerInfoSocialRecovered GetNonIndexed()
    {
      return new ManagerInfoSocialRecovered
      {
        ExtraData = ExtraData,
      };
    }
  }

  public partial class ManagerInfoAdded : aelf::IEvent<ManagerInfoAdded>
  {
    public global::System.Collections.Generic.IEnumerable<ManagerInfoAdded> GetIndexed()
    {
      return new List<ManagerInfoAdded>
      {
      new ManagerInfoAdded
      {
        CaHash = CaHash
      },
      new ManagerInfoAdded
      {
        CaAddress = CaAddress
      },
      new ManagerInfoAdded
      {
        Manager = Manager
      },
      };
    }

    public ManagerInfoAdded GetNonIndexed()
    {
      return new ManagerInfoAdded
      {
        ExtraData = ExtraData,
      };
    }
  }

  public partial class ManagerInfoRemoved : aelf::IEvent<ManagerInfoRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<ManagerInfoRemoved> GetIndexed()
    {
      return new List<ManagerInfoRemoved>
      {
      new ManagerInfoRemoved
      {
        CaHash = CaHash
      },
      new ManagerInfoRemoved
      {
        CaAddress = CaAddress
      },
      new ManagerInfoRemoved
      {
        Manager = Manager
      },
      };
    }

    public ManagerInfoRemoved GetNonIndexed()
    {
      return new ManagerInfoRemoved
      {
        ExtraData = ExtraData,
      };
    }
  }

  public partial class ManagerInfoUpdated : aelf::IEvent<ManagerInfoUpdated>
  {
    public global::System.Collections.Generic.IEnumerable<ManagerInfoUpdated> GetIndexed()
    {
      return new List<ManagerInfoUpdated>
      {
      new ManagerInfoUpdated
      {
        CaHash = CaHash
      },
      new ManagerInfoUpdated
      {
        CaAddress = CaAddress
      },
      new ManagerInfoUpdated
      {
        Manager = Manager
      },
      };
    }

    public ManagerInfoUpdated GetNonIndexed()
    {
      return new ManagerInfoUpdated
      {
        ExtraData = ExtraData,
      };
    }
  }

  public partial class LoginGuardianUnbound : aelf::IEvent<LoginGuardianUnbound>
  {
    public global::System.Collections.Generic.IEnumerable<LoginGuardianUnbound> GetIndexed()
    {
      return new List<LoginGuardianUnbound>
      {
      new LoginGuardianUnbound
      {
        CaHash = CaHash
      },
      new LoginGuardianUnbound
      {
        Manager = Manager
      },
      new LoginGuardianUnbound
      {
        LoginGuardianIdentifierHash = LoginGuardianIdentifierHash
      },
      };
    }

    public LoginGuardianUnbound GetNonIndexed()
    {
      return new LoginGuardianUnbound
      {
        CaAddress = CaAddress,
      };
    }
  }

  public partial class LoginGuardianRemoved : aelf::IEvent<LoginGuardianRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<LoginGuardianRemoved> GetIndexed()
    {
      return new List<LoginGuardianRemoved>
      {
      new LoginGuardianRemoved
      {
        CaHash = CaHash
      },
      new LoginGuardianRemoved
      {
        Manager = Manager
      },
      new LoginGuardianRemoved
      {
        LoginGuardian = LoginGuardian
      },
      };
    }

    public LoginGuardianRemoved GetNonIndexed()
    {
      return new LoginGuardianRemoved
      {
        CaAddress = CaAddress,
      };
    }
  }

  public partial class VerifierServerEndPointsAdded : aelf::IEvent<VerifierServerEndPointsAdded>
  {
    public global::System.Collections.Generic.IEnumerable<VerifierServerEndPointsAdded> GetIndexed()
    {
      return new List<VerifierServerEndPointsAdded>
      {
      new VerifierServerEndPointsAdded
      {
        VerifierServer = VerifierServer
      },
      };
    }

    public VerifierServerEndPointsAdded GetNonIndexed()
    {
      return new VerifierServerEndPointsAdded
      {
      };
    }
  }

  public partial class VerifierServerEndPointsRemoved : aelf::IEvent<VerifierServerEndPointsRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<VerifierServerEndPointsRemoved> GetIndexed()
    {
      return new List<VerifierServerEndPointsRemoved>
      {
      new VerifierServerEndPointsRemoved
      {
        VerifierServer = VerifierServer
      },
      };
    }

    public VerifierServerEndPointsRemoved GetNonIndexed()
    {
      return new VerifierServerEndPointsRemoved
      {
      };
    }
  }

  public partial class VerifierServerRemoved : aelf::IEvent<VerifierServerRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<VerifierServerRemoved> GetIndexed()
    {
      return new List<VerifierServerRemoved>
      {
      new VerifierServerRemoved
      {
        VerifierServer = VerifierServer
      },
      };
    }

    public VerifierServerRemoved GetNonIndexed()
    {
      return new VerifierServerRemoved
      {
      };
    }
  }

  public partial class CAServerAdded : aelf::IEvent<CAServerAdded>
  {
    public global::System.Collections.Generic.IEnumerable<CAServerAdded> GetIndexed()
    {
      return new List<CAServerAdded>
      {
      };
    }

    public CAServerAdded GetNonIndexed()
    {
      return new CAServerAdded
      {
        CaSeverAdded = CaSeverAdded,
      };
    }
  }

  public partial class CAServerRemoved : aelf::IEvent<CAServerRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<CAServerRemoved> GetIndexed()
    {
      return new List<CAServerRemoved>
      {
      };
    }

    public CAServerRemoved GetNonIndexed()
    {
      return new CAServerRemoved
      {
        CaServerRemoved = CaServerRemoved,
      };
    }
  }

  public partial class CAHolderSynced : aelf::IEvent<CAHolderSynced>
  {
    public global::System.Collections.Generic.IEnumerable<CAHolderSynced> GetIndexed()
    {
      return new List<CAHolderSynced>
      {
      new CAHolderSynced
      {
        Creator = Creator
      },
      new CAHolderSynced
      {
        CaHash = CaHash
      },
      new CAHolderSynced
      {
        CaAddress = CaAddress
      },
      new CAHolderSynced
      {
        ManagerInfosAdded = ManagerInfosAdded
      },
      new CAHolderSynced
      {
        ManagerInfosRemoved = ManagerInfosRemoved
      },
      new CAHolderSynced
      {
        LoginGuardiansAdded = LoginGuardiansAdded
      },
      new CAHolderSynced
      {
        LoginGuardiansUnbound = LoginGuardiansUnbound
      },
      new CAHolderSynced
      {
        GuardiansAdded = GuardiansAdded
      },
      new CAHolderSynced
      {
        GuardiansRemoved = GuardiansRemoved
      },
      };
    }

    public CAHolderSynced GetNonIndexed()
    {
      return new CAHolderSynced
      {
      };
    }
  }

  public partial class CreatorControllerAdded : aelf::IEvent<CreatorControllerAdded>
  {
    public global::System.Collections.Generic.IEnumerable<CreatorControllerAdded> GetIndexed()
    {
      return new List<CreatorControllerAdded>
      {
      new CreatorControllerAdded
      {
        Address = Address
      },
      };
    }

    public CreatorControllerAdded GetNonIndexed()
    {
      return new CreatorControllerAdded
      {
      };
    }
  }

  public partial class CreatorControllerRemoved : aelf::IEvent<CreatorControllerRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<CreatorControllerRemoved> GetIndexed()
    {
      return new List<CreatorControllerRemoved>
      {
      new CreatorControllerRemoved
      {
        Address = Address
      },
      };
    }

    public CreatorControllerRemoved GetNonIndexed()
    {
      return new CreatorControllerRemoved
      {
      };
    }
  }

  public partial class ServerControllerAdded : aelf::IEvent<ServerControllerAdded>
  {
    public global::System.Collections.Generic.IEnumerable<ServerControllerAdded> GetIndexed()
    {
      return new List<ServerControllerAdded>
      {
      new ServerControllerAdded
      {
        Address = Address
      },
      };
    }

    public ServerControllerAdded GetNonIndexed()
    {
      return new ServerControllerAdded
      {
      };
    }
  }

  public partial class ServerControllerRemoved : aelf::IEvent<ServerControllerRemoved>
  {
    public global::System.Collections.Generic.IEnumerable<ServerControllerRemoved> GetIndexed()
    {
      return new List<ServerControllerRemoved>
      {
      new ServerControllerRemoved
      {
        Address = Address
      },
      };
    }

    public ServerControllerRemoved GetNonIndexed()
    {
      return new ServerControllerRemoved
      {
      };
    }
  }

  public partial class AdminChanged : aelf::IEvent<AdminChanged>
  {
    public global::System.Collections.Generic.IEnumerable<AdminChanged> GetIndexed()
    {
      return new List<AdminChanged>
      {
      new AdminChanged
      {
        Address = Address
      },
      };
    }

    public AdminChanged GetNonIndexed()
    {
      return new AdminChanged
      {
      };
    }
  }

  #endregion
}
#endregion

