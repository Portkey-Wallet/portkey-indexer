using Portkey.Indexer.CA.Entities;
using Portkey.Indexer.CA.Options;

namespace Portkey.Indexer.CA;

public static class CheckHelper
{
    public static bool CheckNeedModifyBalance(string address, SubscribersOptions options)
    {
        return !options.CaAddresses.IsNullOrEmpty() && options.CaAddresses.Contains(address);
    }

    public static bool IsTokenType(string symbol)
    {
        return TokenHelper.GetTokenType(symbol) == TokenType.Token;
    }

    public static bool CheckNeedRecordBalance(string address, SubscribersOptions options, string symbol)
    {
        return CheckNeedModifyBalance(address, options) && IsTokenType(symbol);
    }
}