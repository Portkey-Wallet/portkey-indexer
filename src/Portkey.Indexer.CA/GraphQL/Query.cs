using AElfIndexer.Client;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.Grain.Client;
using AElfIndexer.Grains.State.Client;
using GraphQL;
using Nest;
using Orleans;
using Portkey.Indexer.CA.Entities;
using IObjectMapper = Volo.Abp.ObjectMapping.IObjectMapper;

namespace Portkey.Indexer.CA.GraphQL;

public class Query
{
    public static async Task<List<TokenInfoDto>> TokenInfo([FromServices] IAElfIndexerClientEntityRepository<TokenInfoIndex,LogEventInfo> repository, [FromServices] IObjectMapper objectMapper, GetTokenInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TokenInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Symbol).Value(dto.Symbol)));

        QueryContainer Filter(QueryContainerDescriptor<TokenInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Symbol,
            sortType: SortOrder.Ascending, skip:dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<TokenInfoIndex>,List<TokenInfoDto>>(result.Item2);
    }
    
    [Name("nftProtocolInfo")]
    public static async Task<List<NFTProtocolInfoDto>> NFTProtocolInfo([FromServices] IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex,LogEventInfo> repository, [FromServices] IObjectMapper objectMapper, GetNFTProtocolInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<NFTProtocolInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Symbol).Value(dto.Symbol)));

        QueryContainer Filter(QueryContainerDescriptor<NFTProtocolInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Symbol,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<NFTProtocolInfoIndex>,List<NFTProtocolInfoDto>>(result.Item2);
    }

    [Name("caHolderTransaction")]
    public static async Task<List<CAHolderTransactionDto>> CAHolderTransaction(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));

        if (dto.MethodNames != null)
        {
            var methodNameShouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var methodName in dto.MethodNames)
            {
                methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(methodNameShouldQuery)));
        }

        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.FromAddress).Query(caAddress)));
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.TransferInfo.FromAddress).Query(caAddress)));
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.TransferInfo.ToAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
    }

    [Name("caHolderManagerInfo")]
    public static async Task<List<CAHolderManagerDto>> CAHolderManagerInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderManagerInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();

        if (string.IsNullOrEmpty(dto.Manager))
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
            mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(dto.CAHash)));
            // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));

            if (dto.CAAddresses != null)
            {
                var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
                foreach (var caAddress in dto.CAAddresses)
                {
                    shouldQuery.Add(s =>
                        s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
                }
                mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            }

            QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

            var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
                sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
            return objectMapper.Map<List<CAHolderIndex>, List<CAHolderManagerDto>>(result.Item2);
        }
        else
        {
            mustQuery.Add(q => q.Term(i => i.Field("Managers.manager").Value(dto.Manager)));

            QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Nested(q => q.Path("Managers")
                .Query(qq => qq.Bool(b => b.Must(mustQuery))));

            var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
                sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
            return objectMapper.Map<List<CAHolderIndex>, List<CAHolderManagerDto>>(result.Item2);
        }

    }

    public static async Task<List<LoginGuardianAccountDto>> LoginGuardianAccountInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianAccountIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianAccountInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianAccountIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(dto.CAHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.LoginGuardianAccount.Value).Value(dto.LoginGuardianAccount)));

        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianAccountIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<LoginGuardianAccountIndex>, List<LoginGuardianAccountDto>>(result.Item2);
    }

    public static async Task<List<UserNFTProtocolInfoDto>> UserNFTProtocolInfo(
        [FromServices] IAElfIndexerClientEntityRepository<UserNFTProtocolInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetUserNFTProtocolInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserNFTProtocolInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftProtocolInfo.Symbol).Value(dto.Symbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));

        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<UserNFTProtocolInfoIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserNFTProtocolInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<UserNFTProtocolInfoIndex>, List<UserNFTProtocolInfoDto>>(result.Item2);
    }

    public static async Task<List<UserNFTInfoDto>> UserNFTInfo(
        [FromServices] IAElfIndexerClientEntityRepository<UserNFTInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetUserNFTInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserNFTInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.Symbol).Value(dto.Symbol)));
        if(dto.TokenId>0)
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.TokenId).Value(dto.TokenId)));
        }
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));

        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<UserNFTInfoIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserNFTInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<UserNFTInfoIndex>, List<UserNFTInfoDto>>(result.Item2);
    }
    
    
    [Name("caHolderTokenBalanceInfo")]
    public static async Task<List<CAHolderTokenBalanceDto>> CAHolderTokenBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTokenBalanceDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTokenBalanceIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.TokenInfo.Symbol,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderTokenBalanceIndex>, List<CAHolderTokenBalanceDto>>(result.Item2);
    }
    
    [Name("caHolderTransactionAddressInfo")]
    public static async Task<List<CAHolderTransactionAddressDto>> CAHolderTransactionAddressInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionAddressDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionAddressIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.TransactionTime,
            sortType: SortOrder.Descending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderTransactionAddressIndex>, List<CAHolderTransactionAddressDto>>(result.Item2);
    }

    public static async Task<List<LoginGuardianAccountChangeRecordDto>> LoginGuardianAccountChangeRecordInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianAccountChangeRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianAccountChangeRecordDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianAccountChangeRecordIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));

        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianAccountChangeRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: int.MaxValue);
        return objectMapper.Map<List<LoginGuardianAccountChangeRecordIndex>, List<LoginGuardianAccountChangeRecordDto>>(result.Item2);
    }
    
    [Name("caHolderManagerChangeRecordInfo")]
    public static async Task<List<CAHolderManagerChangeRecordDto>> CAHolderManagerChangeRecordInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderManagerChangeRecordDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderManagerChangeRecordIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAHash)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));

        QueryContainer Filter(QueryContainerDescriptor<CAHolderManagerChangeRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: int.MaxValue);
        return objectMapper.Map<List<CAHolderManagerChangeRecordIndex>, List<CAHolderManagerChangeRecordDto>>(result.Item2);
    }

    [Name("caHolderSearchTokenNFT")]
    public static async Task<List<CAHolderSearchTokenNFTDto>> CAHolderSearchTokenNFT(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderSearchTokenNFTIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderSearchTokenNFTDto dto)
    {
        string wildCardSearchWord = "";
        if (!string.IsNullOrEmpty(dto.SearchWord))
        {
            wildCardSearchWord = "*" + dto.SearchWord + "*";
        }
        
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        if (dto.CAAddresses != null)
        {
            var shouldQueryCAAddress = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQueryCAAddress.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQueryCAAddress)));
        }

        var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.TokenInfo.Symbol).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.TokenInfo.TokenContractAddress).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NFTInfo.Symbol).Value(wildCardSearchWord).CaseInsensitive(true)));

        Int64.TryParse(dto.SearchWord, out long tokenId);
        if (tokenId > 0)
        {
            shouldQuery.Add(s =>
                s.Term(i => i.Field(f => f.NFTInfo.TokenId).Value(dto.SearchWord)));
        }
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NFTInfo.Alias).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NFTInfo.NftContractAddress).Value(wildCardSearchWord).CaseInsensitive(true)));
        
        mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        
        // mustQuery.Add(q => q.Wildcard(i => i.Field(f => f.NFTInfo.NftContractAddress).Value(dto.SearchWord).CaseInsensitive(true)));
        
        QueryContainer Filter(QueryContainerDescriptor<CAHolderSearchTokenNFTIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderSearchTokenNFTIndex>, List<CAHolderSearchTokenNFTDto>>(result.Item2);
    }

    public static async Task<SyncStateDto> SyncState(
        [FromServices] IClusterClient clusterClient, [FromServices] IAElfIndexerClientInfoProvider clientInfoProvider,
        [FromServices] IObjectMapper objectMapper, GetSyncStateDto dto)
    {
        var version = clientInfoProvider.GetVersion();
        var clientId = clientInfoProvider.GetClientId();
        var blockStateSetInfoGrain =
            clusterClient.GetGrain<IBlockStateSetInfoGrain>(
                GrainIdHelper.GenerateGrainId("BlockStateSetInfo", clientId, dto.ChainId, version));
        var confirmedHeight = await blockStateSetInfoGrain.GetConfirmedBlockHeight(dto.FilterType);
        return new SyncStateDto
        {
            ConfirmedBlockHeight = confirmedHeight
        };
    }
}