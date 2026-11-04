# SnakeGame 🐍

A polished, fully offline Snake arcade game built with **C# and Windows Forms**.

## Highlights

- Minimal dark arcade interface with a redesigned start screen
- Smooth animated movement with a 60 FPS rendering target
- Cached interface and playfield graphics to reduce repeated drawing
- Three game modes:
  - **Classic** — hit a wall and the run ends
  - **Wrap** — leave one edge and appear on the opposite side
  - **Rush** — score as much as possible in 60 seconds
- Three difficulty settings: Easy, Normal, and Hard
- Golden bonus fruit appears every five regular fruits; collect it for **30 points**
- Combo scoring for collecting fruit in quick succession
- Pause, resume, and restart controls
- Sound effects with a mute toggle
- Local personal-best score; no account, network, or third-party packages

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
| `Esc` | Pause or resume |

## Personal best

The best score is stored on your PC at:

`%LOCALAPPDATA%\SnakeGame\best-score.txt`

It is never uploaded.

---

Have fun!