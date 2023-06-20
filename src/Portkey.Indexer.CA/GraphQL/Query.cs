using System.Linq.Expressions;
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
using System.Linq;

namespace Portkey.Indexer.CA.GraphQL;

public class Query
{
    public static async Task<List<TokenInfoDto>> TokenInfo(
        [FromServices] IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTokenInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<TokenInfoIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Symbol).Value(dto.Symbol)));

        QueryContainer Filter(QueryContainerDescriptor<TokenInfoIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Symbol,
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        Expression<Func<TokenInfoIndex, bool>> expression = p => p.ChainId == dto.ChainId && p.Symbol == dto.Symbol;
        List<Tuple<SortOrder, Expression<Func<TokenInfoIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<TokenInfoIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<TokenInfoIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<TokenInfoIndex, object>>>(SortOrder.Ascending, p => p.Symbol);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(expression, sortExp, dto.MaxResultCount, dto.SkipCount);
        return objectMapper.Map<List<TokenInfoIndex>, List<TokenInfoDto>>(result);
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
        //var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
        Expression<Func<CAHolderTransactionIndex, bool>> mustQuery = null;

        // mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        if (dto.StartBlockHeight > 0)
        {
            mustQuery = q => q.BlockHeight >= dto.StartBlockHeight;
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        }

        if (dto.EndBlockHeight > 0)
        {
            mustQuery = mustQuery is null ? q => q.BlockHeight <= dto.EndBlockHeight : PredicateBuilder.And(mustQuery, q=>q.BlockHeight <= dto.EndBlockHeight);
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        }

        mustQuery = mustQuery is null ? q => q.TokenInfo.Symbol == dto.Symbol : PredicateBuilder.And(mustQuery, q => q.TokenInfo.Symbol == dto.Symbol);
        mustQuery = PredicateBuilder.And(mustQuery, q=>q.BlockHash == dto.BlockHash && q.TransactionId == dto.TransactionId && q.TransferInfo.TransferTransactionId == dto.TransferTransactionId);
        /*mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q =>
            q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));*/
        if (dto.MethodNames != null)
        {
            /*var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();*/
            Expression<Func<CAHolderTransactionIndex, bool>> methodNameShouldQuery = null;

            foreach (var methodName in dto.MethodNames)
            {
                /*methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));*/
                methodNameShouldQuery = methodNameShouldQuery is null? s => s.MethodName == methodName : PredicateBuilder.Or(methodNameShouldQuery, s => s.MethodName == methodName);
            }

            PredicateBuilder.And(mustQuery, methodNameShouldQuery);
        }

        if (dto.CAAddressInfos != null)
        {
            //var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            Expression<Func<CAHolderTransactionIndex, bool>> shouldQuery = null;

            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryFromAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.FromAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryFromAddressInfo)));
                */
                Expression<Func<CAHolderTransactionIndex, bool>> mustQueryFromAddressInfo = p => p.FromAddress == info.CAAddress && p.ChainId == info.ChainId;
                shouldQuery = PredicateBuilder.And(shouldQuery, mustQueryFromAddressInfo);
               
                /*var mustQueryTransferFromAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.TransferInfo.FromAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.TransferInfo.FromChainId).Value(info.ChainId))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromAddressInfo)));
                */
                Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferFromAddressInfo = p => p.FromAddress == info.CAAddress && p.ChainId == info.ChainId;
                shouldQuery = PredicateBuilder.And(shouldQuery, mustQueryTransferFromAddressInfo);
               
                /*var mustQueryTransferFromCAAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.TransferInfo.FromCAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.TransferInfo.FromChainId).Value(info.ChainId))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferFromCAAddressInfo)));*/
                Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferFromCAAddressInfo = q => q.TransferInfo.FromCAAddress == info.CAAddress && q.TransferInfo.FromChainId == info.ChainId;
                shouldQuery = PredicateBuilder.And(shouldQuery, mustQueryTransferFromCAAddressInfo);
                
                /*var mustQueryTransferToAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.TransferInfo.ToAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.TransferInfo.ToChainId).Value(info.ChainId))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryTransferToAddressInfo)));*/
                Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferToAddressInfo = q=>q.TransferInfo.ToAddress == info.CAAddress && q.TransferInfo.ToChainId == info.ChainId;
                shouldQuery = PredicateBuilder.And(shouldQuery, mustQueryTransferToAddressInfo);
                
            }
            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = PredicateBuilder.And(mustQuery, shouldQuery);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>(SortOrder.Descending, p => p.Timestamp);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result);

        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    [Name("twoCaHolderTransaction")]
    public static async Task<CAHolderTransactionPageResultDto> TwoCAHolderTransaction(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTwoCAHolderTransactionDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));*/
        Expression<Func<CAHolderTransactionIndex, bool>> mustQuery = q => q.ChainId == dto.ChainId;

        if (dto.StartBlockHeight > 0)
        {
           // mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
           mustQuery = mustQuery.And(q=>q.BlockHeight >= dto.StartBlockHeight);
        }

        if (dto.EndBlockHeight > 0)
        {
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
            mustQuery = mustQuery.And(q=>q.BlockHeight <= dto.EndBlockHeight);
        }

        /*mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));*/
        mustQuery = mustQuery.And(q=>q.TokenInfo.Symbol == dto.Symbol && q.BlockHash == dto.BlockHash);

        if (dto.MethodNames is { Count: > 0 })
        {
            /*var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();*/
            Expression<Func<CAHolderTransactionIndex, bool>> methodNameShouldQuery = null;
            foreach (var methodName in dto.MethodNames)
            {
                /*methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));*/
                methodNameShouldQuery = methodNameShouldQuery is null ? s => s.MethodName == methodName
                    : methodNameShouldQuery.Or(s => s.MethodName == methodName);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(methodNameShouldQuery)));
            mustQuery = mustQuery.And(methodNameShouldQuery);
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
        //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        mustQuery = mustQuery.And(shouldQuery);
        
        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));
        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>(SortOrder.Descending, p => p.Timestamp);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result);

        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    /*private static List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>
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
    }*/
    
    private static Expression<Func<CAHolderTransactionIndex, bool>>
        GetTwoCaHolderQueryContainer(
            CAAddressInfo fromHolder, CAAddressInfo toHolder)
    {
        Expression<Func<CAHolderTransactionIndex, bool>> shouldQuery = null;
       
        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryFromAddressInfo = q=>q.FromAddress == fromHolder.CAAddress && q.ChainId == fromHolder.ChainId && q.TransferInfo.ToAddress == toHolder.CAAddress && q.TransferInfo.ToChainId == toHolder.ChainId;
        shouldQuery = mustQueryFromAddressInfo;

        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferFromAddressInfo = q=>q.TransferInfo.FromAddress == fromHolder.CAAddress && q.ChainId == fromHolder.ChainId && q.TransferInfo.ToAddress == toHolder.CAAddress && q.TransferInfo.ToChainId == toHolder.ChainId;
        shouldQuery = shouldQuery.Or( mustQueryTransferFromAddressInfo);
       
        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferFromCAAddressInfo = q=>q.TransferInfo.FromAddress == fromHolder.CAAddress && q.ChainId == fromHolder.ChainId && q.TransferInfo.ToAddress == toHolder.CAAddress && q.TransferInfo.ToChainId == toHolder.ChainId;
        shouldQuery = shouldQuery.Or(mustQueryTransferFromCAAddressInfo);
        
        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryToAddressInfo = q=>q.FromAddress == toHolder.CAAddress && q.ChainId == toHolder.ChainId && q.TransferInfo.ToAddress == fromHolder.CAAddress && q.TransferInfo.ToChainId == fromHolder.ChainId;
        shouldQuery = shouldQuery.Or(mustQueryToAddressInfo);
        
        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferToAddressInfo = q=>q.TransferInfo.FromAddress == toHolder.CAAddress && q.ChainId == toHolder.ChainId && q.TransferInfo.ToAddress == fromHolder.CAAddress && q.TransferInfo.ToChainId == fromHolder.ChainId;
        shouldQuery = shouldQuery.Or(mustQueryTransferToAddressInfo);
        
        Expression<Func<CAHolderTransactionIndex, bool>> mustQueryTransferToCAAddressInfo = q=>q.TransferInfo.FromAddress == toHolder.CAAddress && q.ChainId == toHolder.ChainId && q.TransferInfo.ToAddress == fromHolder.CAAddress && q.TransferInfo.ToChainId == fromHolder.ChainId;
        shouldQuery = shouldQuery.Or(mustQueryTransferToCAAddressInfo);
        return shouldQuery;
    }

    [Name("caHolderTransactionInfo")]
    public static async Task<CAHolderTransactionPageResultDto> CAHolderTransactionInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionIndex, TransactionInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));*/
        
        Expression<Func<CAHolderTransactionIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId;
        if (dto.StartBlockHeight > 0)
        {
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
            mustQuery = mustQuery.And(q=>q.BlockHeight>=dto.StartBlockHeight);
        }

        if (dto.EndBlockHeight > 0)
        {
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
            mustQuery = mustQuery.And(q=>q.BlockHeight<=dto.EndBlockHeight);
        }

        /*mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.BlockHash).Value(dto.BlockHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TransactionId).Value(dto.TransactionId)));
        mustQuery.Add(q =>
            q.Term(i => i.Field(f => f.TransferInfo.TransferTransactionId).Value(dto.TransferTransactionId)));*/
        mustQuery = mustQuery.And(q=>q.TokenInfo.Symbol == dto.Symbol && q.BlockHash == dto.BlockHash && q.TransactionId == dto.TransactionId && q.TransferInfo.TransferTransactionId == dto.TransferTransactionId);

        if (dto.MethodNames != null)
        {
            /*var methodNameShouldQuery =
                new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var methodName in dto.MethodNames)
            {
                methodNameShouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.MethodName).Query(methodName)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(methodNameShouldQuery)));*/
            Expression<Func<CAHolderTransactionIndex, bool>> methodNameShouldQuery = null;
            foreach (var methodName in dto.MethodNames)
            {
                methodNameShouldQuery = methodNameShouldQuery is null? q=>q.MethodName == methodName : methodNameShouldQuery.Or(q=>q.MethodName == methodName);
            }
            mustQuery = mustQuery.And(methodNameShouldQuery);
        }

        if (dto.CAAddresses != null)
        {
            /*var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.FromAddress).Query(caAddress)));
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.TransferInfo.FromAddress).Query(caAddress)));
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.TransferInfo.ToAddress).Query(caAddress)));
            }*/
            Expression<Func<CAHolderTransactionIndex, bool>> shouldQuery = null;
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery = shouldQuery.Or(q=>q.FromAddress == caAddress || q.TransferInfo.FromAddress == caAddress || q.TransferInfo.ToAddress == caAddress);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = mustQuery.And(shouldQuery);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.Timestamp,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        
        // return objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result.Item2);
        List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<CAHolderTransactionIndex, object>>>(SortOrder.Descending, p => p.Timestamp);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        
        var dataList = objectMapper.Map<List<CAHolderTransactionIndex>, List<CAHolderTransactionDto>>(result);
        var pageResult = new CAHolderTransactionPageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    [Name("caHolderManagerInfo")]
    public static async Task<List<CAHolderManagerDto>> CAHolderManagerInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderManagerInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(dto.CAHash)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));*/
        Expression<Func<CAHolderIndex, bool>> mustQuery = q =>
            q.ChainId == dto.ChainId && q.CAHash == dto.CAHash;

        if (dto.CAAddresses != null)
        {
            /*var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery.Add(s =>
                    s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));
            }*/
            Expression<Func<CAHolderIndex, bool>> shouldQuery = null;
            foreach (var caAddress in dto.CAAddresses)
            {
                shouldQuery = shouldQuery is null ? q=>q.CAAddress == caAddress : shouldQuery.Or(q=>q.CAAddress == caAddress);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = mustQuery.And(shouldQuery);
        }

        /*mustQuery.Add(n => n.Nested(n =>
            n.Path("ManagerInfos").Query(q => q.Term(i => i.Field("ManagerInfos.address").Value(dto.Manager)))));*/
        mustQuery = mustQuery.And(n => n.ManagerInfos.Any(m => m.Address == dto.Manager));

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<CAHolderIndex, object>>>(SortOrder.Ascending, p => p.BlockHeight);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        return objectMapper.Map<List<CAHolderIndex>, List<CAHolderManagerDto>>(result);

    }

    [Name("caHolderInfo")]
    public static async Task<List<CAHolderInfoDto>> CAHolderInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repositoryLoginGuardian,
        [FromServices] IObjectMapper objectMapper, GetCAHolderInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));*/
        Expression<Func<CAHolderIndex, bool>> mustQuery = q => q.ChainId == dto.ChainId;

        if (string.IsNullOrWhiteSpace(dto.CAHash) && string.IsNullOrWhiteSpace(dto.LoginGuardianIdentifierHash))
        {
            if (dto.CAAddresses != null)
            {
               // var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderIndex>, QueryContainer>>();
               Expression<Func<CAHolderIndex, bool>> shouldQuery = null;
                foreach (var caAddress in dto.CAAddresses)
                {
                    /*shouldQuery.Add(s =>
                        s.Match(i => i.Field(f => f.CAAddress).Query(caAddress)));*/
                    shouldQuery = shouldQuery is null ? s=>s.CAAddress == caAddress : shouldQuery.Or(s=>s.CAAddress == caAddress);
                }

                //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
                mustQuery = mustQuery.And(shouldQuery);
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
                /*var mustQueryLoginGuardian =
                    new List<Func<QueryContainerDescriptor<LoginGuardianIndex>, QueryContainer>>();
                mustQueryLoginGuardian.Add(q =>
                    q.Term(i => i.Field(f => f.LoginGuardian.IdentifierHash).Value(dto.LoginGuardianIdentifierHash)));
                    */
                Expression<Func<LoginGuardianIndex, bool>> mustQueryLoginGuardian = q => q.LoginGuardian.IdentifierHash == dto.LoginGuardianIdentifierHash;
                /*QueryContainer FilterLoginGuardian(QueryContainerDescriptor<LoginGuardianIndex> f) =>
                    f.Bool(b => b.Must(mustQueryLoginGuardian));

                var holderInfoResult = await repositoryLoginGuardian.GetListAsync(FilterLoginGuardian);*/
                var holderInfoResult = await repositoryLoginGuardian.GetListAsync(mustQueryLoginGuardian);
                
                if (holderInfoResult.Count == 0) return new List<CAHolderInfoDto>();

                hash = holderInfoResult.First().CAHash;
            }

            //mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(hash)));
            mustQuery = mustQuery.And(q => q.CAHash == hash);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        var result = await repository.GetListAsync(mustQuery, null, dto.MaxResultCount, dto.SkipCount);
        return objectMapper.Map<List<CAHolderIndex>, List<CAHolderInfoDto>>(result);
    }

    public static async Task<List<LoginGuardianDto>> LoginGuardianInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAHash).Value(dto.CAHash)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.LoginGuardian.IdentifierHash).Value(dto.LoginGuardian)));

        QueryContainer Filter(QueryContainerDescriptor<LoginGuardianIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        Expression<Func<LoginGuardianIndex, bool>> mustQuery = q=>q.ChainId == dto.ChainId && q.CAHash == dto.CAHash && q.CAAddress == dto.CAAddress && q.LoginGuardian.IdentifierHash == dto.LoginGuardian;
        List<Tuple<SortOrder, Expression<Func<LoginGuardianIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<LoginGuardianIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<LoginGuardianIndex, object>>> sort =
            new Tuple<SortOrder, Expression<Func<LoginGuardianIndex, object>>>(SortOrder.Ascending, p => p.BlockHeight);
        sortExp.Add(sort);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        return objectMapper.Map<List<LoginGuardianIndex>, List<LoginGuardianDto>>(result);
    }

    [Name("caHolderNFTCollectionBalanceInfo")]
    public static async Task<CAHolderNFTCollectionBalancePageResultDto> CAHolderNFTCollecitonBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderNFTCollectionBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderNFTCollectionInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftCollectionInfo.Symbol).Value(dto.Symbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        */
        Expression<Func<CAHolderNFTCollectionBalanceIndex, bool>> mustQuery = q=>q.ChainId == dto.ChainId && q.NftCollectionInfo.Symbol == dto.Symbol;
        //待定
        //mustQuery.Add(q => q.Script(i => i.Script(sq => sq.Source($"doc['tokenIds'].getLength()>0"))));
        mustQuery = mustQuery.And(q=>q.TokenIds.Count > 0);
        if (dto.CAAddressInfos != null)
        {
           // var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>();
            Expression<Func<CAHolderNFTCollectionBalanceIndex, bool>> shouldQuery = null;
            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));*/
                Expression<Func<CAHolderNFTCollectionBalanceIndex, bool>> mustQueryAddressInfo = q=>q.CAAddress == info.CAAddress && q.ChainId == info.ChainId;
                shouldQuery = shouldQuery is null ? mustQueryAddressInfo : shouldQuery.Or(mustQueryAddressInfo);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = mustQuery.And(shouldQuery);
        }

        /*
        QueryContainer Filter(QueryContainerDescriptor<CAHolderNFTCollectionBalanceIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
            */

        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        /*Func<SortDescriptor<CAHolderNFTCollectionBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.NftCollectionInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>>(SortOrder.Ascending, p => p.NftCollectionInfo.Symbol);
        Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>> sortChainId =
            new Tuple<SortOrder, Expression<Func<CAHolderNFTCollectionBalanceIndex, object>>>(SortOrder.Ascending, p => p.ChainId);
        sortExp.Add(sortSymbol);
        sortExp.Add(sortChainId);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        var dataList =
            objectMapper.Map<List<CAHolderNFTCollectionBalanceIndex>, List<CAHolderNFTCollectionBalanceInfoDto>>(
                result);

        var pageResult = new CAHolderNFTCollectionBalancePageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    [Name("caHolderNFTBalanceInfo")]
    public static async Task<CAHolderNFTBalancePageResultDto> CAHolderNFTBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderNFTBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderNFTInfoDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.Symbol).Value(dto.Symbol)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.NftInfo.CollectionSymbol).Value(dto.CollectionSymbol)));
        // mustQuery.Add(q => q.Term(i => i.Field(f => f.CAAddress).Value(dto.CAAddress)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));*/
        Expression<Func<CAHolderNFTBalanceIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId && p.NftInfo.Symbol == dto.Symbol && p.NftInfo.CollectionSymbol == dto.CollectionSymbol && p.Balance > 0;

        if (dto.CAAddressInfos != null)
        {
            //var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>();
            Expression<Func<CAHolderNFTBalanceIndex, bool>> shouldQuery = null;
            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderNFTBalanceIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };*/
                Expression<Func<CAHolderNFTBalanceIndex, bool>> mustQueryAddressInfo = q=>q.CAAddress == info.CAAddress && q.ChainId == info.ChainId;
                //shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
                shouldQuery = shouldQuery is null? mustQueryAddressInfo : shouldQuery.Or(mustQueryAddressInfo);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = mustQuery.And(shouldQuery);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderNFTBalanceIndex> f) => f.Bool(b => b.Must(mustQuery));

        // var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
        //     sortType: SortOrder.Ascending, skip:dto.SkipCount,limit: dto.MaxResultCount);
        Func<SortDescriptor<CAHolderNFTBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.NftInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>>(SortOrder.Ascending, p => p.NftInfo.Symbol);
        Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>> sortChainId =
            new Tuple<SortOrder, Expression<Func<CAHolderNFTBalanceIndex, object>>>(SortOrder.Ascending, p => p.ChainId);
        sortExp.Add(sortSymbol);
        sortExp.Add(sortChainId);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
            
        var dataList = objectMapper.Map<List<CAHolderNFTBalanceIndex>, List<CAHolderNFTBalanceInfoDto>>(result);
        var pageResult = new CAHolderNFTBalancePageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }


    [Name("caHolderTokenBalanceInfo")]
    public static async Task<CAHolderTokenBalancePageResultDto> CAHolderTokenBalanceInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTokenBalanceIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTokenBalanceDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.TokenInfo.Symbol).Value(dto.Symbol)));
        */
        Expression<Func<CAHolderTokenBalanceIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId && p.TokenInfo.Symbol == dto.Symbol;
        
        if (dto.CAAddressInfos != null)
        {
            //var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>();
            Expression<Func<CAHolderTokenBalanceIndex, bool>> shouldQuery = null;
            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTokenBalanceIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };*/
                Expression<Func<CAHolderTokenBalanceIndex, bool>> mustQueryAddressInfo = q=>q.CAAddress == info.CAAddress && q.ChainId == info.ChainId;
                //shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
                shouldQuery = shouldQuery is null? mustQueryAddressInfo : shouldQuery.Or(mustQueryAddressInfo);
            }

           // mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
              mustQuery = mustQuery.And(shouldQuery);
        }

        //QueryContainer Filter(QueryContainerDescriptor<CAHolderTokenBalanceIndex> f) => f.Bool(b => b.Must(mustQuery));

        /*Func<SortDescriptor<CAHolderTokenBalanceIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.TokenInfo.Symbol).Ascending(d => d.ChainId);

        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>>(SortOrder.Ascending, p => p.TokenInfo.Symbol);
        Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>> sortChainId =
            new Tuple<SortOrder, Expression<Func<CAHolderTokenBalanceIndex, object>>>(SortOrder.Ascending, p => p.ChainId);
        sortExp.Add(sortSymbol);
        sortExp.Add(sortChainId);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        
        var dataList = objectMapper.Map<List<CAHolderTokenBalanceIndex>, List<CAHolderTokenBalanceDto>>(result);

        var pageResult = new CAHolderTokenBalancePageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    [Name("caHolderTransactionAddressInfo")]
    public static async Task<CAHolderTransactionAddressPageResultDto> CAHolderTransactionAddressInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderTransactionAddressIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderTransactionAddressDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        */
        Expression<Func<CAHolderTransactionAddressIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId;  
        if (dto.CAAddressInfos != null)
        {
            //var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>();
            Expression<Func<CAHolderTransactionAddressIndex, bool>> shouldQuery = null;
            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderTransactionAddressIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };*/
                Expression<Func<CAHolderTransactionAddressIndex, bool>> mustQueryAddressInfo = q=>q.CAAddress == info.CAAddress && q.ChainId == info.ChainId;
                //shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
                shouldQuery = shouldQuery is null? mustQueryAddressInfo : shouldQuery.Or(mustQueryAddressInfo);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
            mustQuery = mustQuery.And(shouldQuery);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderTransactionAddressIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.TransactionTime,
            sortType: SortOrder.Descending, skip: dto.SkipCount, limit: dto.MaxResultCount);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderTransactionAddressIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderTransactionAddressIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderTransactionAddressIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderTransactionAddressIndex, object>>>(SortOrder.Descending,
                p => p.TransactionTime);
        sortExp.Add(sortSymbol);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        var dataList =
            objectMapper.Map<List<CAHolderTransactionAddressIndex>, List<CAHolderTransactionAddressDto>>(result);

        var pageResult = new CAHolderTransactionAddressPageResultDto
        {
            TotalRecordCount = result.Count,
            Data = dataList
        };
        return pageResult;
    }

    public static async Task<List<LoginGuardianChangeRecordDto>> LoginGuardianChangeRecordInfo(
        [FromServices] IAElfIndexerClientEntityRepository<LoginGuardianChangeRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetLoginGuardianChangeRecordDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<LoginGuardianChangeRecordIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        */
        Expression<Func<LoginGuardianChangeRecordIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId;
        if (dto.StartBlockHeight > 0)
        {
            //mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
            mustQuery = mustQuery.And(p=>p.BlockHeight >= dto.StartBlockHeight);
        }

        if (dto.EndBlockHeight > 0)
        {
           // mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
              mustQuery = mustQuery.And(p=>p.BlockHeight <= dto.EndBlockHeight);
        }

        /*QueryContainer Filter(QueryContainerDescriptor<LoginGuardianChangeRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: 10000);*/
        List<Tuple<SortOrder, Expression<Func<LoginGuardianChangeRecordIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<LoginGuardianChangeRecordIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<LoginGuardianChangeRecordIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<LoginGuardianChangeRecordIndex, object>>>(SortOrder.Ascending,
                p => p.BlockHeight);
        sortExp.Add(sortSymbol);
        var result = await repository.GetListAsync(mustQuery, sortExp, 10000, 0);
        return objectMapper.Map<List<LoginGuardianChangeRecordIndex>, List<LoginGuardianChangeRecordDto>>(result);
    }

    [Name("caHolderManagerChangeRecordInfo")]
    public static async Task<List<CAHolderManagerChangeRecordDto>> CAHolderManagerChangeRecordInfo(
        [FromServices] IAElfIndexerClientEntityRepository<CAHolderManagerChangeRecordIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetCAHolderManagerChangeRecordDto dto)
    {
        /*var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderManagerChangeRecordIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(dto.StartBlockHeight)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(dto.EndBlockHeight)));
        */
        Expression<Func<CAHolderManagerChangeRecordIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId && p.BlockHeight >= dto.StartBlockHeight && p.BlockHeight <= dto.EndBlockHeight;
        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderManagerChangeRecordIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await repository.GetListAsync(Filter, sortExp: k => k.BlockHeight,
            sortType: SortOrder.Ascending, skip: 0, limit: 10000);*/
        List<Tuple<SortOrder, Expression<Func<CAHolderManagerChangeRecordIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderManagerChangeRecordIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderManagerChangeRecordIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderManagerChangeRecordIndex, object>>>(SortOrder.Ascending,
                p => p.BlockHeight);
        sortExp.Add(sortSymbol);
        var result = await repository.GetListAsync(mustQuery, sortExp, 10000, 0);
        return objectMapper.Map<List<CAHolderManagerChangeRecordIndex>, List<CAHolderManagerChangeRecordDto>>(
            result);
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

        /*
        var mustQuery = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(f => f.ChainId).Value(dto.ChainId)));
        mustQuery.Add(q => q.Range(i => i.Field(f => f.Balance).GreaterThan(0)));
        */
        Expression<Func<CAHolderSearchTokenNFTIndex, bool>> mustQuery = p=>p.ChainId == dto.ChainId && p.Balance > 0;
        if (dto.CAAddressInfos != null)
        {
            //var shouldQueryCAAddress = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
            Expression<Func<CAHolderSearchTokenNFTIndex, bool>> shouldQueryCAAddress = p=>false;
            foreach (var info in dto.CAAddressInfos)
            {
                /*var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.CAAddress).Value(info.CAAddress)),
                    q => q.Term(i => i.Field(f => f.ChainId).Value(info.ChainId))
                };
                shouldQueryCAAddress.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));*/
                Expression<Func<CAHolderSearchTokenNFTIndex, bool>> mustQueryAddressInfo = p=>p.CAAddress == info.CAAddress && p.ChainId == info.ChainId;
                shouldQueryCAAddress = shouldQueryCAAddress is null ? mustQueryAddressInfo : shouldQueryCAAddress.Or(mustQueryAddressInfo);
            }

            //mustQuery.Add(q => q.Bool(b => b.Should(shouldQueryCAAddress)));
            mustQuery = mustQuery.And(shouldQueryCAAddress);
        }

        /*var shouldQuery = new List<Func<QueryContainerDescriptor<CAHolderSearchTokenNFTIndex>, QueryContainer>>();
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.TokenInfo.Symbol).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NftInfo.Symbol).Value(wildCardSearchWord).CaseInsensitive(true)));
            */
        //待定
        Expression<Func<CAHolderSearchTokenNFTIndex, bool>> shouldQuery = s => s.TokenInfo.Symbol.Contains(wildCardSearchWord) || s.NftInfo.Symbol.Contains(wildCardSearchWord);
        long.TryParse(dto.SearchWord, out long tokenId);
        if (tokenId > 0)
        {
            /*shouldQuery.Add(s =>
                s.Term(i => i.Field(f => f.TokenId).Value(dto.SearchWord)));*/
            shouldQuery = shouldQuery.Or(s => s.TokenId == tokenId);
        }

        /*shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.TokenInfo.TokenName).Value(wildCardSearchWord).CaseInsensitive(true)));
        shouldQuery.Add(s =>
            s.Wildcard(i => i.Field(f => f.NftInfo.TokenName).Value(wildCardSearchWord).CaseInsensitive(true)));*/
        shouldQuery = shouldQuery.Or( s => s.TokenInfo.TokenName.Contains(wildCardSearchWord) || s.NftInfo.TokenName.Contains(wildCardSearchWord));
        //mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        mustQuery = mustQuery.And(shouldQuery);
        // mustQuery.Add(q => q.Wildcard(i => i.Field(f => f.NFTInfo.NftContractAddress).Value(dto.SearchWord).CaseInsensitive(true)));

        /*QueryContainer Filter(QueryContainerDescriptor<CAHolderSearchTokenNFTIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        Func<SortDescriptor<CAHolderSearchTokenNFTIndex>, IPromise<IList<ISort>>> sort = s =>
            s.Ascending(a => a.TokenInfo.Symbol).Ascending(a => a.NftInfo.Symbol).Ascending(d => d.ChainId);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);*/
        
        List<Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>>> sortExp =
            new List<Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>>>();
        Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>> sortSymbol =
            new Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>>(SortOrder.Ascending,
                p => p.TokenInfo.Symbol);
        Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>> sortChainId =
            new Tuple<SortOrder, Expression<Func<CAHolderSearchTokenNFTIndex, object>>>(SortOrder.Ascending,
                p => p.ChainId);
        sortExp.Add(sortSymbol);
        sortExp.Add(sortChainId);
        var result = await repository.GetListAsync(mustQuery, sortExp, dto.MaxResultCount, dto.SkipCount);
        var dataList =
            objectMapper.Map<List<CAHolderSearchTokenNFTIndex>, List<CAHolderSearchTokenNFTDto>>(result);

        var pageResult = new CAHolderSearchTokenNFTPageResultDto
        {
            TotalRecordCount = result.Count,
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