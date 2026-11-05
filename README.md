# SnakeGame 🐍

A polished, fully offline Snake arcade game built with **C# and Windows Forms**.

## Highlights

- In-app Home hub with Home, Play, Skins, and Settings sections
- Switch between sections without opening extra windows
- Leaving an active run pauses it; returning to Play lets you resume
- Dedicated skin collection with saved skin selection
- Settings for sound, keyboard controls, and personal-best management
- Welcoming Play screen with your personal best score
- Smooth animated snake and custom red/golden apple sprites
- 20 custom snake skins plus the **Legendary Phoenix** skin
- Skin patterns include stripes, waves, scales, flames, frost, circuits, galaxy, neon, and more
- Legendary Phoenix has a glowing aura and animated sparks around the snake
- Skin choice is saved locally and restored next time the game starts
- Golden apple blinks during its final 3 seconds
- Collecting a golden apple triggers gold particles, an expanding ring, and a +30 popup
- Three game modes:
  - **Classic** — hitting a wall ends the run
  - **Wrap** — leave one edge and appear on the opposite side
  - **Rush** — score as much as possible in 60 seconds
- Three difficulty settings: Easy, Normal, and Hard
- Current score and personal best; no level system
- Reset Best Score button with confirmation
- Pause, resume, restart, and sound toggle
- Fullscreen with **F11**, **Alt + Enter**, or the fullscreen button
- Cached UI, apple sprites, reusable drawing paths, and effect resources
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
| `Esc` | Close skin picker, pause, or exit fullscreen |
| `F11` | Toggle fullscreen |
| `Alt + Enter` | Toggle fullscreen |

## Local data

The personal best score is stored at:

`%LOCALAPPDATA%\SnakeGame\best-score.txt`

The selected skin is stored at:

`%LOCALAPPDATA%\SnakeGame\selected-skin.txt`

Both stay on your PC. No game data is uploaded.

---

Have fun!