# Portkey Indexer

| BRANCH | AZURE PIPELINES                                              | TESTS                                                        | CODE COVERAGE                                                |
| ------ | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| MASTER | [![Build Status](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_apis/build/status%2FPortkey-Wallet.portkey-indexer?branchName=master)](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_build/latest?definitionId=8&branchName=master) | [![Test Status](https://img.shields.io/azure-devops/tests/Portkey-Finance/Portkey-Finance/8/master)](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_build/latest?definitionId=8&branchName=master) | [![codecov](https://codecov.io/github/Portkey-Wallet/portkey-indexer/branch/master/graph/badge.svg?token=NILLW9BJX9)](https://app.codecov.io/github/Portkey-Wallet/portkey-indexer) |
| DEV    | [![Build Status](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_apis/build/status%2FPortkey-Wallet.portkey-indexer?branchName=dev)](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_build/latest?definitionId=8&branchName=dev) | [![Test Status](https://img.shields.io/azure-devops/tests/Portkey-Finance/Portkey-Finance/8/dev)](https://dev.azure.com/Portkey-Finance/Portkey-Finance/_build/latest?definitionId=8&branchName=dev) | [![codecov](https://codecov.io/github/Portkey-Wallet/portkey-indexer/branch/dev/graph/badge.svg?token=NILLW9BJX9)](https://app.codecov.io/github/Portkey-Wallet/portkey-indexer) |

Portkey Indexer is an interface plugin project running on the aelf blockchain scanning system, acting as a bridge between blockchain data and Web3 applications. In simple terms, its main functions include:

- Processing block data delivered by the aelf scanning system:
    - The Portkey Indexer plugin runs on the aelf scanning system, obtaining and processing on-chain block data in real-time. This data includes transaction information, contract calls, etc., which can be filtered and organized according to the business requirements of the Portkey wallet.
- Recording the processed results to ElasticSearch:
    - To achieve efficient data retrieval and querying, Portkey Indexer needs to aggregate and store the processed blockchain data in ElasticSearch. ElasticSearch is a high-performance full-text search engine capable of quickly querying large amounts of data in real-time.
- Providing real-time data access API using GraphQL specification:
    - To facilitate third-party applications (such as Web3 Apps) to access the processed blockchain data, Portkey Indexer provides an API that follows the GraphQL specification. GraphQL is a data query and manipulation language that allows clients to obtain the required data more flexibly and improve data transmission efficiency.

Therefore, based on the design of the aelf scanning system, any Dapp that wants to obtain aelf on-chain business information needs a scanning interface plugin similar to Portkey Indexer. Such a plugin can not only provide real-time on-chain data for Dapps but also optimize data query and access performance, providing convenience for developers to build efficient blockchain applications.

## Test

You can easily run unit tests on Portkey Indexer. Navigate to the Portkey.Indexer.CA.Tests and run:

```Bash
cd test/Portkey.Indexer.CA.Tests
dotnet test
```

## Usage

The Portkey Indexer provides the following modules:

- `Entities`: Define the index structure used for ElasticSearch.
- `GraphQL`: Provide GraphQL APIs and related definitions of input and output.
- `Processors`: Process and store block data from aelf blockchain scanning system.

## Contributing

We welcome contributions to the Portkey Indexer project. If you would like to contribute, please fork the repository and submit a pull request with your changes. Before submitting a pull request, please ensure that your code is well-tested and adheres to the aelf coding standards.

## License

Portkey Indexer is licensed under [MIT](https://github.com/Portkey-Wallet/portkey-indexer/blob/feature/readme/README.md).

## Contact

If you have any questions or feedback, please feel free to contact us at the Portkey community channels. You can find us on Discord, Telegram, and other social media platforms.

Links:

- Website: https://portkey.finance/
- Twitter: https://twitter.com/Portkey_DID
- Discord: https://discord.com/invite/EUBq3rHQhr
- Telegram: https://t.me/Portkey_Official_Group