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
    
    // [Name("nftProtocolInfo")]
    // public static async Task<List<NFTProtocolInfoDto>> NFTProtocolInfo([FromServices] IAElfIndexerClientEntityRepository<NFTProtocolInfoIndex,LogEventInfo> repository, [FromServices] IObjectMapper objectMapper, GetNFTProtocolInfoDto dto)
    // {
    //     var mustQuery = new List<Func<QueryContainerDescriptor<NFTProtocolInfoIndex>, QueryContainer>>();
    //
    //     mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
    //     mustQuery.Add(q => q.Term(i => i.Field(f => f.Symbol).Value(dto.Symbol)));
    //
    //     QueryContainer Filter(QueryContainerDescriptor<NFTProtocolInfoIndex> f) => f.Bool(b => b.Must(mustQuery));
    //
    //     var result = await repository.GetListAsync(Filter, sortExp: k => k.Symbol,
    //         sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
    //     return objectMapper.Map<List<NFTProtocolInfoIndex>,List<NFTProtocolInfoDto>>(result.Item2);
    // }

    [Name("caHolderTransaction")]
    public static async Task<CAHolderTransactionPageResultDto> CAHolderTransaction(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
    
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }
        if (dto.EndBlockHeight>0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));
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
                // shouldQuery.Add(s =>
                //     s.Match(i => i.Field(f => f.TransferInfo.FromAddress).Query(caAddress)));
                // shouldQuery.Add(s =>
                //     s.Match(i => i.Field(f => f.TransferInfo.FromCAAddress).Query(caAddress)));
                // shouldQuery.Add(s =>
                //     s.Match(i => i.Field(f => f.TransferInfo.ToAddress).Query(caAddress)));
            }
            
            var shouldMustQueryFrom = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            shouldMustQueryFrom.Add(s =>
                s.Term(i =>
                    i.Field("transferInfo.fromChainId").Value(dto.ChainId)));
            var shouldMushShouldQueryFrom = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldMushShouldQueryFrom.Add(s =>
                    s.Term(i => i.Field("transferInfo.fromAddress").Value(caAddress)));
                shouldMushShouldQueryFrom.Add(s =>
                    s.Term(i => i.Field("transferInfo.fromCAAddress").Value(caAddress)));
            }
            if (shouldMushShouldQueryFrom.Count > 0)
            {
                shouldMustQueryFrom.Add(q => q.Bool(b => b.Should(shouldMushShouldQueryFrom)));
            }
            shouldQuery.Add(q => q.Bool(b => b.Must(shouldMustQueryFrom)));
            
            
            var shouldMustQueryTo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            shouldMustQueryTo.Add(s =>
                s.Term(i =>
                    i.Field("transferInfo.toChainId").Value(dto.ChainId)));
            var shouldMushShouldQueryTo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldMushShouldQueryTo.Add(s =>
                    s.Term(i => i.Field("transferInfo.toAddress").Value(caAddress)));
            }
            if (shouldMushShouldQueryTo.Count > 0)
            {
                shouldMustQueryTo.Add(q => q.Bool(b => b.Should(shouldMushShouldQueryTo)));
            }
            shouldQuery.Add(q => q.Bool(b => b.Must(shouldMustQueryTo)));
            
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }
    
        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));
    
        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        var dataList=objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
    
        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }
    
    [Name("caHolderTransactionInfo")]
    public static async Task<CAHolderTransactionPageResultDto> CAHolderTransactionInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }
        if (dto.EndBlockHeight>0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));

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
        // return objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
        var dataList=objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
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
            mustQuery.Add(q => q.Term(i => i.Field("ManagerInfos.address").Value(dto.Manager)));

            QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Nested(q => q.Path("ManagerInfos")
                .Query(qq => qq.Bool(b => b.Must(mustQuery))));

            var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
                sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
            return objectMapper.Map<List<CAHolderIndex>, List<CAHolderManagerDto>>(result.Item2);
        }

    }

    public static async Task<List<LoginGuardianDto>> LoginGuardianInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(dto.CAHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.LoginGuardian.IdentifierHash).Value(dto.LoginGuardian)));

        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        return objectMapper.Map<List<LoginGuardianIndex>, List<LoginGuardianDto>>(result.Item2);
    }

    [Name("caHolderNFTCollectionBalanceInfo")]
    public static async Task<CAHolderNFTCollectionBalancePageResultDto> CAHolderNFTCollecitonBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderNFTCollectionInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>();
    
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftCollectionInfo.Symbol).Value(dto.Symbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Script(i => i.Script(sq=>sq.Source($"doc['tokenIds'].getLength()>0"))));

        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }
    
        QueryContainer Filter(QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex> f) => f.Bool(b => b.Must(mustQuery));
    
        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderNFTCollectionBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.NftCollectionInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList= objectMapper.Map<List<CAHolderNFTCollectionBalanceIndex>, List<CAHolderNFTCollectionBalanceInfoDto>>(result.Item2);
        
        var pageResult = new CAHolderNFTCollectionBalancePageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }

    [Name("caHolderNFTBalanceInfo")]
    public static async Task<CAHolderNFTBalancePageResultDto> CAHolderNFTBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderNFTInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>();
    
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.CollectionSymbol).Value(dto.CollectionSymbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q=> q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));
    
        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }
    
        QueryContainer Filter(QueryContainerDescriptor<CAHolderNFTBalanceIndex> f) => f.Bool(b => b.Must(mustQuery));
    
        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderNFTBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.NftInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList=objectMapper.Map<List<CAHolderNFTBalanceIndex>, List<CAHolderNFTBalanceInfoDto>>(result.Item2);
        var pageResult = new CAHolderNFTBalancePageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }
    
    
    [Name("caHolderTokenBalanceInfo")]
    public static async Task<CAHolderTokenBalancePageResultDto> CAHolderTokenBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTokenBalanceDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();
    
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Type).Value(dto.Type)));

        // mustQuery.Add(q=> q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));

        
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
    
        Func<SortDescriptor<CAHolderTokenBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.TokenInfo.Symbol).Ascending(d => d.ChainId);
        // var result = await repository.GetListAsync(Filter, sortExp: k => k.TokenInfo.Symbol,
            // sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
                limit: dto.MaxResultCount);
        var dataList= objectMapper.Map<List<CAHolderTokenBalanceIndex>, List<CAHolderTokenBalanceDto>>(result.Item2);
        
        var pageResult = new CAHolderTokenBalancePageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }

    [Name("caHolderTransactionAddressInfo")]
    public static async Task<CAHolderTransactionAddressPageResultDto> CAHolderTransactionAddressInfo(
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
        var dataList= objectMapper.Map<List<CAHolderTransactionAddressIndex>, List<CAHolderTransactionAddressDto>>(result.Item2);
        
        var pageResult = new CAHolderTransactionAddressPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }

    public static async Task<List<LoginGuardianChangeRecordDto>> LoginGuardianChangeRecordInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianChangeRecordDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianChangeRecordIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }
        if (dto.EndBlockHeight>0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }
        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianChangeRecordIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: 10000);
        return objectMapper.Map<List<LoginGuardianChangeRecordIndex>, List<LoginGuardianChangeRecordDto>>(result.Item2);
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
            sortType: SortOrder.Ascending, skip: 0, limit: 10000);
        return objectMapper.Map<List<CAHolderManagerChangeRecordIndex>, List<CAHolderManagerChangeRecordDto>>(result.Item2);
    }

    [Name("caHolderSearchTokenNFT")]
    public static async Task<CAHolderSearchTokenNFTPageResultDto> CAHolderSearchTokenNFT(
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
        mustQuery.Add(q=> q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));
        
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
            s.Wildcard(i => i.Field(f => f.NftInfo.Symbol).Value(wildCardSearchWord).CaseInsensitive(true)));
        // shouldQuery.Add(s =>
        //     s.Wildcard(i => i.Field(f => f.TokenInfo.TokenContractAddress).Value(wildCardSearchWord).CaseInsensitive(true)));
    
        long.TryParse(dto.SearchWord, out long tokenId);
        if (tokenId > 0)
        {
            shouldQuery.Add(s =>
                s.Term(i => i.Field(f => f.TokenId).Value(dto.SearchWord)));
        }
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.TokenInfo.TokenName).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NftInfo.TokenName).Value(wildCardSearchWord).CaseInsensitive(true)));
        
        mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        
        // mustQuery.Add(q => q.Wildcard(i => i.Field(f => f.NFTInfo.NftContractAddress).Value(dto.SearchWord).CaseInsensitive(true)));
        
        QueryContainer Filter(QueryContainerDescriptor<CAHolderSearchTokenNFTIndex> f) => f.Bool(b => b.Must(mustQuery));
    
        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderSearchTokenNFTIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.TokenInfo.Symbol).Ascending(a => a.NftInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList= objectMapper.Map<List<CAHolderSearchTokenNFTIndex>, List<CAHolderSearchTokenNFTDto>>(result.Item2);
        
        var pageResult = new CAHolderSearchTokenNFTPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
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