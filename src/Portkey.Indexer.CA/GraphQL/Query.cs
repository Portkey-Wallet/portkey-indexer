using AElfIndexer.Client;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.Grain.Client;
using AElfIndexer.Grains.State.Client;
using GraphQL;
using Nest;
using Orleans;
using Portkey.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace Portkey.Indexer.CA.GraphQL;

public class Query
{
    public static async Task<List<TokenInfoDto>> TokenInfo(
        [FromServices] IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTokenInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TokenInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Symbol).Value(dto.Symbol)));
        mustQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.Symbol).Value(dto.SymbolKeyword).CaseInsensitive(true)));

        QueryContainer Filter(QueryContainerDescriptor<TokenInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Symbol,
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<TokenInfoIndex>, List<TokenInfoDto>>(result.Item2);
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

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q =>
            q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));
        if (dto.MethodNames != null)
        {
            var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var methodName in dto.MethodNames)
            {
                methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(methodNameShouldQuery)));
        }

        if (dto.CAAddressInfos != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryFromAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.FromAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryFromAddressInfo)));
                var mustQueryTransferFromAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.TransferInfo.FromAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.TransferInfo.FromChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromAddressInfo)));
                var mustQueryTransferFromCAAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.TransferInfo.FromCAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.TransferInfo.FromChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromCAAddressInfo)));
                var mustQueryTransferToAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferToAddressInfo)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);

        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }

    [Name("twoCaHolderTransaction")]
    public static async Task<CAHolderTransactionPageResultDto> TwoCAHolderTransaction(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTwoCAHolderTransactionDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));

        if (dto.MethodNames is { Count: > 0 })
        {
            var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var methodName in dto.MethodNames)
            {
                methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(methodNameShouldQuery)));
        }

        if (dto.CAAddressInfos is not { Count: 2 })
        {
            return new CAHolderTransactionPageResultDto
            {
                TotalRecordCount = 0,
                Data = new List<CAHolderTransactionDto>()
            };
        }

        var shouldQuery = GetTwoCaHolderQueryContainer(dto.CAAddressInfos[0], dto.CAAddressInfos[1]);
        mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);

        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList
        };
        return pageResult;
    }

    private static List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
        GetTwoCaHolderQueryContainer(
            CAAddressInfo fromHolder, CAAddressInfo toHolder)
    {
        var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        var mustQueryFromAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.FromAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(fromHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(toHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryFromAddressInfo)));
        var mustQueryTransferFromAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.TransferInfo.FromAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(fromHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(toHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromAddressInfo)));
        var mustQueryTransferFromCAAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.TransferInfo.FromCAAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(fromHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(toHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromCAAddressInfo)));

        var mustQueryToAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.FromAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(toHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(fromHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryToAddressInfo)));
        var mustQueryTransferToAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.TransferInfo.FromAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(toHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(fromHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferToAddressInfo)));
        var mustQueryTransferToCAAddressInfo =
            new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
            {
                q => q.Term(i => i.Field(f => f.TransferInfo.FromCAAddress).Value(toHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.ChainId).Value(toHolder.ChainId)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(fromHolder.CAAddress)),
                q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(fromHolder.ChainId))
            };
        shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferToCAAddressInfo)));

        return shouldQuery;
    }

    [Name("caHolderTransactionInfo")]
    public static async Task<CAHolderTransactionPageResultDto> CAHolderTransactionInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q =>
            q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));

        if (dto.MethodNames != null)
        {
            var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
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
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
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

        mustQuery.Add(n => n.Nested(n =>
            n.Path("ManagerInfos").Query(q => q.Term(i => i.Field("ManagerInfos.address").Value(dto.Manager)))));

        QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderIndex>, List<CAHolderManagerDto>>(result.Item2);
    }

    [Name("caHolderInfo")]
    public static async Task<List<CAHolderInfoDto>> CAHolderInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repositoryLoginGuardian,
        [FromServices] IObjectMapper objectMapper, GetCAHolderInfoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));

        if (string.IsNullOrWhiteSpace(dto.CAHash) && string.IsNullOrWhiteSpace(dto.LoginGuardianIdentifierHash))
        {
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
        }
        else
        {
            string hash;
            if (!string.IsNullOrWhiteSpace(dto.CAHash))
            {
                hash = dto.CAHash;
            }
            else
            {
                var mustQueryLoginGuardian =
                    new List<Func<QueryContainerDescriptor<LoginGuardianIndex>, QueryContainer>>();
                mustQueryLoginGuardian.Add(q =>
                    q.Term(i => i.Field(f => f.LoginGuardian.IdentifierHash).Value(dto.LoginGuardianIdentifierHash)));

                QueryContainer FilterLoginGuardian(QueryContainerDescriptor<LoginGuardianIndex> f) =>
                    f.Bool(b => b.Must(mustQueryLoginGuardian));

                var holderInfoResult = await repositoryLoginGuardian.GetListAsync(FilterLoginGuardian);

                if (holderInfoResult.Item1 == 0) return new List<CAHolderInfoDto>();

                hash = holderInfoResult.Item2.First().CAHash;
            }

            mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(hash)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, skip: dto.SkipCount, limit: dto.MaxResultCount);
        return objectMapper.Map<List<CAHolderIndex>, List<CAHolderInfoDto>>(result.Item2);
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
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);
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
        mustQuery.Add(q => q.Script(i => i.Script(sq => sq.Source($"doc['tokenIds'].getLength()>0"))));

        if (dto.CAAddressInfos != null)
        {
            var shouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderNFTCollectionBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.NftCollectionInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList =
            objectMapper.Map<List<CAHolderNFTCollectionBalanceIndex>, List<CAHolderNFTCollectionBalanceInfoDto>>(
                result.Item2);

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
        mustQuery.Add(q => q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));

        if (dto.CAAddressInfos != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
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
        var dataList = objectMapper.Map<List<CAHolderNFTBalanceIndex>, List<CAHolderNFTBalanceInfoDto>>(result.Item2);
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


        if (dto.CAAddressInfos != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
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
        var dataList = objectMapper.Map<List<CAHolderTokenBalanceIndex>, List<CAHolderTokenBalanceDto>>(result.Item2);

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
        if (dto.CAAddressInfos != null)
        {
            var shouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionAddressIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.TransactionTime,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        var dataList =
            objectMapper.Map<List<CAHolderTransactionAddressIndex>, List<CAHolderTransactionAddressDto>>(result.Item2);

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

        if (dto.EndBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianChangeRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

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

        QueryContainer Filter(QueryContainerDescriptor<CAHolderManagerChangeRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: 10000);
        return objectMapper.Map<List<CAHolderManagerChangeRecordIndex>, List<CAHolderManagerChangeRecordDto>>(
            result.Item2);
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
        mustQuery.Add(q => q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));

        if (dto.CAAddressInfos != null)
        {
            var shouldQueryCAAddress =
                new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
            foreach (var info in dto.CAAddressInfos)
            {
                var mustQueryAddressInfo =
                    new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>
                    {
                        q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                        q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                    };
                shouldQueryCAAddress.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
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

        QueryContainer Filter(QueryContainerDescriptor<CAHolderSearchTokenNFTIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderSearchTokenNFTIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.TokenInfo.Symbol).Ascending(a => a.NftInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList =
            objectMapper.Map<List<CAHolderSearchTokenNFTIndex>, List<CAHolderSearchTokenNFTDto>>(result.Item2);

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