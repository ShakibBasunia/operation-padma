# Operation Padma

Operation Padma is a turn-based tactical war game written in C#/.NET. Two armies of 6 units each fight on a 10x8 map with forests and a river. Blue is the player, Red is controlled by an enemy AI.

Units: Infantry (range 1), Tank (move 3, bonus damage against infantry) and Artillery (range 2 to 3, ignores forest cover). Forests cost 2 movement and give +2 defense. The river can only be crossed through open tiles.

Structure:
- Padma.Core: OOP game logic (abstract Unit class, Infantry, Tank, Artillery, GameEngine, EnemyAI)
- Padma.Web: Blazor WebAssembly interface, hosted on GitHub Pages
- Padma.Api: ASP.NET Core API that saves match results to SQL (EF Core + SQLite) or MongoDB through the IMatchStore interface

Run locally:
    dotnet run --project Padma.Web



# Operation Padma

C#/.NET turn-based tactical war game.

- Padma.Core: OOP game logic (Unit, Infantry, Tank, Artillery, GameEngine, EnemyAI)
- Padma.Web: Blazor WebAssembly UI (hosted free on GitHub Pages)
- Padma.Api: ASP.NET Core minimal API, SQL (EF Core + SQLite) or MongoDB via IMatchStore

Run locally:
    dotnet run --project Padma.Web
    dotnet run --project Padma.Api     (optional, then set ApiUrl in Padma.Web/wwwroot/appsettings.json)




