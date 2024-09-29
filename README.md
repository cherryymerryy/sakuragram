# sakuragram

Custom Telegram Desktop client based on TDLib ([tdsharp](https://github.com/egramtel/tdsharp)) and WinUI 3

[Telegram channel](https://t.me/sakuragram), [Telegram forum](https://t.me/sakuragramchat)

## Compilation Guide
1. Clone sakuragram source code (`git clone https://github.com/cherryymerryy/sakuragram.git`)
2. Download TDLib NuGet package (`dotnet add package TDLib --version 1.8.29`)
3. Create Config directory and file Config.cs (`Config/Config.cs`)
4. Fill out config file like this
```c#
namespace sakuragram.Config;

public class Config
{
    public const int ApiId = your_api_id;
    public const string ApiHash = "your_api_hash";
}
```
