# SnakeGame 🐍

A polished, fully offline Snake arcade game built with **C# and Windows Forms**.

## Highlights

- A welcoming start screen with your personal best score
- Smooth animated snake with anti-aliased, rounded body rendering
- Custom red and golden apple sprites, drawn once and cached for fast rendering
- Golden apple warning blink during its final 3 seconds
- Golden apple collection triggers a particle burst, expanding ring, and +30 score popup
- Three game modes:
  - **Classic** — hitting a wall ends the run
  - **Wrap** — leave one edge and appear on the opposite side
  - **Rush** — score as much as possible in 60 seconds
- Three difficulty settings: Easy, Normal, and Hard
- Current score and saved personal best; no level system
- Pause, resume, and restart controls
- Sound effects with a mute toggle
- Fullscreen mode with **F11**, **Alt + Enter**, or the fullscreen button
- Cached interface and reusable drawing paths to reduce per-frame allocations
- No account, internet connection, external assets, or third-party NuGet packages

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

## Build a standalone Windows app

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The executable will be created at:

`bin\Release\net10.0-windows\win-x64\publish\SnakeGame.exe`

The self-contained build includes the .NET runtime, so a compatible 64-bit Windows PC does not need a separate .NET installation.

## Controls

| Key | Action |
| --- | --- |
| `W` / `↑` | Move up |
| `S` / `↓` | Move down |
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `Space` | Start, pause, or resume |
| `Esc` | Pause or exit fullscreen |
| `F11` | Toggle fullscreen |
| `Alt + Enter` | Toggle fullscreen |

## Personal best

Your best score is stored locally at:

`%LOCALAPPDATA%\SnakeGame\best-score.txt`

The save is written immediately when you beat your record, using a temporary file and a backup copy. Scores are never uploaded.

---

Have fun!