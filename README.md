# CherryMerryGram

Custom Telegram Desktop client based on TdLib

[Telegram channel](https://t.me/cherrymerrygram), [Telegram forum](https://t.me/cherrymerrygramchat)

## Compilation Guide
1. Clone CherryMerryGram source code (`git clone https://github.com/cherryymerryy/CherryMerryGram.git`)
2. Download TDLib NuGet package (`dotnet add package TDLib --version 1.8.29`)
3. Create Config directory and file Config.cs (`Config/Config.cs`)
4. Fill out config file like this
```c#
namespace CherryMerryGramDesktop.Config;

public class Config
{
    public const int ApiId = your_api_id;
    public const string ApiHash = "your_api_hash";
}
```
