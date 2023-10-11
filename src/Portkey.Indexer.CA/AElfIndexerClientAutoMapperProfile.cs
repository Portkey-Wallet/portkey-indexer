using AElf.Contracts.MultiToken;
using AElf.Contracts.NFT;
using AElfIndexer.Client.Handlers;
using AutoMapper;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using TransactionFee = Portkey.Indexer.CA.GraphQL.TransactionFee;

namespace Portkey.Indexer.CA;

public class TestGraphQLAutoMapperProfile : Profile
{
    public TestGraphQLAutoMapperProfile()
    {
        CreateMap<TokenCreated, TokenInfoIndex>();
        CreateMap<TokenCreated, NFTCollectionInfoIndex>();
        CreateMap<TokenCreated, NFTInfoIndex>();

        CreateMap<LogEventContext, TokenInfoIndex>();
        CreateMap<LogEventContext, NFTCollectionInfoIndex>();
        CreateMap<LogEventContext, NFTInfoIndex>();
        CreateMap<LogEventContext, LoginGuardianIndex>();
        CreateMap<LogEventContext, LoginGuardianChangeRecordIndex>();
        CreateMap<LogEventContext, CAHolderTransactionIndex>();
        CreateMap<LogEventContext, CAHolderIndex>();

        CreateMap<LogEventContext, CAHolderNFTCollectionBalanceIndex>();
        CreateMap<LogEventContext, CAHolderNFTBalanceIndex>();
        CreateMap<LogEventContext, CAHolderTokenBalanceIndex>();

        CreateMap<LogEventContext, CAHolderTransactionAddressIndex>();
        CreateMap<LogEventContext, CAHolderManagerChangeRecordIndex>();
        CreateMap<LogEventContext, CAHolderSearchTokenNFTIndex>();
        CreateMap<LogEventContext, CAHolderManagerIndex>();
        CreateMap<LogEventContext, TransferLimitIndex>();
        CreateMap<LogEventContext, TransferSecurityThresholdIndex>();

        CreateMap<TokenInfo, TokenInfoIndex>();
        CreateMap<TokenInfoIndex, TokenInfoDto>();
        CreateMap<TokenInfoIndex, TokenSearchInfo>();
        CreateMap<TokenInfoIndex, TokenSearchInfoDto>();
        CreateMap<TokenSearchInfo, TokenInfoDto>();

        CreateMap<NFTProtocolCreated, NFTCollectionInfoIndex>();
        CreateMap<NFTProtocolInfo, NFTCollectionInfoIndex>();
        CreateMap<NFTCollectionInfoIndex, NFTProtocolInfoDto>();
        CreateMap<NFTCollectionInfoIndex, NFTProtocol>();
        CreateMap<NFTCollectionInfoIndex, NFTCollectionDto>();
        CreateMap<NFTMinted, NFTInfoIndex>();
        CreateMap<NFTMinted, CAHolderNFTBalanceIndex>();
        CreateMap<NFTMinted, CAHolderNFTCollectionBalanceIndex>();
        CreateMap<NFTInfoIndex, NFTItemInfo>();
        CreateMap<NFTInfoIndex, NFTItemInfoDto>();
        CreateMap<NFTInfoIndex, NFTSearchInfo>();
        CreateMap<NFTInfoIndex, CAHolderNFTBalanceIndex>();
        CreateMap<NFTItemInfo, NFTItemInfoDto>();
        CreateMap<NFTProtocol, NFTCollectionDto>();
        CreateMap<NFTSearchInfo, NFTItemInfoDto>();

        CreateMap<CAHolderTransactionIndex, CAHolderTransactionDto>()
            .ForMember(c => c.TransactionFees, opt => opt.MapFrom<TransactionFeeResolver>());
        CreateMap<CAHolderIndex, CAHolderManagerDto>();

        CreateMap<CAHolderNFTBalanceIndex, CAHolderNFTBalanceInfoDto>();
        CreateMap<CAHolderNFTCollectionBalanceIndex, CAHolderNFTCollectionBalanceInfoDto>();
        CreateMap<CAHolderTokenBalanceIndex, CAHolderTokenBalanceDto>();
        CreateMap<CAHolderTransactionAddressIndex, CAHolderTransactionAddressDto>();
        CreateMap<CAHolderManagerChangeRecordIndex, CAHolderManagerChangeRecordDto>();
        CreateMap<CAHolderSearchTokenNFTIndex, CAHolderSearchTokenNFTDto>();

        CreateMap<LoginGuardianIndex, LoginGuardianDto>();
        CreateMap<LoginGuardianChangeRecordIndex, LoginGuardianChangeRecordDto>();
        CreateMap<Guardian, GuardianDto>();
        CreateMap<BingoGameIndex, BingoInfo>();
        CreateMap<BingoGameStaticsIndex, BingoStatics>();
        CreateMap<LogEventContext, BingoGameIndex>();
        CreateMap<LogEventContext, BeangoTownIndex>();
        CreateMap<LogEventContext, BingoGameStaticsIndex>();


        CreateMap<CAHolderIndex, CAHolderInfoDto>().ForMember(d => d.GuardianList,
            opt => opt.MapFrom(e => e.Guardians.IsNullOrEmpty() ? null : new GuardianList { Guardians = e.Guardians }));
        CreateMap<Contracts.CA.Guardian, Guardian>()
            .ForMember(d => d.IdentifierHash, opt => opt.MapFrom(e => e.IdentifierHash.ToHex()))
            .ForMember(d => d.VerifierId, opt => opt.MapFrom(e => e.VerifierId.ToHex()))
            .ForMember(d => d.Type, opt => opt.MapFrom(e => (int)e.Type));
        // CreateMap<Guardian, GuardianDto>();

        // CreateMap<TokenInfoIndex, TokenSearchInfo>();
        // CreateMap<NFTInfoIndex, NFTSearchInfo>();
        // CreateMap<TokenSearchInfo, TokenSearchInfoDto>();
        // CreateMap<NFTSearchInfo, NFTSearchInfoDto>();
        CreateMap<TransferLimitIndex, CAHolderTransferlimitDto>();
        CreateMap<TransferSecurityThresholdIndex, TransferSecurityThresholdDto>();
        CreateMap<LogEventContext, TransferLimitIndex>();
        CreateMap<ManagerApprovedIndex, CAHolderManagerApprovedDto>();
        CreateMap<LogEventContext, ManagerApprovedIndex>();
    }
}

public class
    TransactionFeeResolver : IValueResolver<CAHolderTransactionIndex, CAHolderTransactionDto, List<TransactionFee>>
{
    public List<TransactionFee> Resolve(CAHolderTransactionIndex source, CAHolderTransactionDto destination,
        List<TransactionFee> destMember,
        ResolutionContext context)
    {
        var list = new List<TransactionFee>();
        foreach (var (symbol, amount) in source.TransactionFee)
        {
            list.Add(new TransactionFee
            {
                Amount = amount,
                Symbol = symbol
            });
        }

        return list;
    }
}