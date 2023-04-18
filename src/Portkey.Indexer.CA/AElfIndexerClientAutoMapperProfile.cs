using AElf.Contracts.MultiToken;
using AElf.Contracts.NFT;
using AElfIndexer.Client.Handlers;
using AutoMapper;
using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.GraphQL;
using TransactionFee = Portkey.Indexer.CA.GraphQL.TransactionFee;

namespace Portkey.Indexer.CA;

public class TestGraphQLAutoMapperProfile:Profile
{
    public TestGraphQLAutoMapperProfile()
    {
        CreateMap<TokenCreated, TokenInfoIndex>();
        CreateMap<LogEventContext, TokenInfoIndex>();
        CreateMap<TokenInfo, TokenInfoIndex>();
        CreateMap<TokenInfoIndex, TokenInfoDto>();
        CreateMap<NFTProtocolCreated, NFTProtocolInfoIndex>();
        CreateMap<LogEventContext, NFTProtocolInfoIndex>();
        CreateMap<NFTProtocolInfo, NFTProtocolInfoIndex>();
        CreateMap<NFTProtocolInfoIndex, NFTProtocolInfoDto>();
        CreateMap<NFTMinted, NFTInfoIndex>();
        CreateMap<LogEventContext, NFTInfoIndex>();
        CreateMap<LogEventContext, CAHolderTransactionIndex>();
        CreateMap<CAHolderTransactionIndex, CAHolderTransactionDto>().ForMember(c=>c.TransactionFees,opt=>opt.MapFrom<TransactionFeeResolver>());
        CreateMap<CAHolderIndex, CAHolderManagerDto>();
        CreateMap<LogEventContext, CAHolderIndex>();
        CreateMap<LoginGuardianAccountIndex, LoginGuardianAccountDto>();
        CreateMap<LogEventContext, LoginGuardianAccountIndex>();
        CreateMap<LogEventContext,UserNFTInfoIndex>();
        CreateMap<LogEventContext,UserNFTProtocolInfoIndex>();
        CreateMap<NFTMinted,UserNFTInfoIndex>();
        CreateMap<NFTMinted,UserNFTProtocolInfoIndex>();
        CreateMap<NFTInfoIndex, NFTItemInfo>();
        CreateMap<NFTInfoIndex,UserNFTInfoIndex>();
        CreateMap<NFTProtocolInfoIndex, NFTProtocol>();
        CreateMap<NFTProtocolInfoIndex,UserNFTProtocolInfoIndex>();
        CreateMap<UserNFTInfoIndex,UserNFTInfoDto>();
        CreateMap<UserNFTProtocolInfoIndex,UserNFTProtocolInfoDto>();
        CreateMap<NFTItemInfo, NFTItemInfoDto>();
        CreateMap<NFTProtocol, NFTProtocolDto>();
        CreateMap<LogEventContext, CAHolderTokenBalanceIndex>();
        CreateMap<LogEventContext, CAHolderTransactionAddressIndex>();
        CreateMap<LogEventContext, LoginGuardianAccountChangeRecordIndex>();
        CreateMap<LoginGuardianAccountChangeRecordIndex, LoginGuardianAccountChangeRecordDto>();
        CreateMap<GuardianAccount, GuardianAccountDto>();
        CreateMap<Guardian, GuardianDto>();
        CreateMap<CAHolderTokenBalanceIndex, CAHolderTokenBalanceDto>();
        CreateMap<CAHolderTransactionAddressIndex, CAHolderTransactionAddressDto>();
        CreateMap<LogEventContext, CAHolderManagerChangeRecordIndex>();
        CreateMap<CAHolderManagerChangeRecordIndex, CAHolderManagerChangeRecordDto>();
        CreateMap<LogEventContext, CAHolderSearchTokenNFTIndex>();
        CreateMap<TokenInfoIndex, TokenSearchInfo>();
        CreateMap<NFTInfoIndex, NFTSearchInfo>();
        CreateMap<CAHolderSearchTokenNFTIndex, CAHolderSearchTokenNFTDto>();
        CreateMap<TokenSearchInfo, TokenSearchInfoDto>();
        CreateMap<NFTSearchInfo, NFTSearchInfoDto>();
    }
    
}

public class TransactionFeeResolver : IValueResolver<CAHolderTransactionIndex, CAHolderTransactionDto, List<TransactionFee>>
{
    public List<TransactionFee> Resolve(CAHolderTransactionIndex source, CAHolderTransactionDto destination,List<TransactionFee> destMember,
        ResolutionContext context)
    {
        var list = new List<TransactionFee>();
        foreach (var (symbol,amount) in source.TransactionFee)
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