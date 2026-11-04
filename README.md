# SnakeGame 🐍

A small, polished **offline Snake game** made with C# and Windows Forms.

The game uses custom-drawn graphics, so it needs no downloaded images, game engine, account, or internet connection.

## Features

- Clean dark arcade interface with neon-green accents
- Classic Snake gameplay with smooth, grid-based movement
- Keyboard controls: **WASD** or **Arrow Keys**
- Three difficulty levels: Easy, Normal, and Hard
- Speed increases as your score grows
- Pause and resume at any time
- Personal best saved locally on your PC
- Restart button and game-over screen
- No third-party NuGet packages

## Requirements

- Windows 10 or Windows 11
- .NET 10 SDK to run from source

## Run the game

Open PowerShell or Windows Terminal:

```powershell
git clone https://github.com/sadra1388ppp/SnakeGame.git
cd SnakeGame
dotnet run
```

## Build a release

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The standalone Windows executable will be in:

`bin\Release\net10.0-windows\win-x64\publish\SnakeGame.exe`

The self-contained build includes the .NET runtime, so it can run on a compatible 64-bit Windows PC without installing .NET separately.

## Controls

| Key | Action |
| --- | --- |
| `W` / `↑` | Move up |
| `S` / `↓` | Move down |
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `Space` | Start, pause, or resume |
| `Esc` | Pause or resume |

## High score

Your best score is stored locally at:

`%LOCALAPPDATA%\SnakeGame\best-score.txt`

It is not uploaded or shared.

---

Have fun!