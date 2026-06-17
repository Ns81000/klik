<div align="center">

<img src="klik.png" alt="Klik" width="80" height="80" />

# Klik

### Never `'Hello'` a developer again.

*Lightweight text automation for Windows — compose, set, repeat.*

<br/>

[![Platform](https://img.shields.io/badge/Windows_10_%2F_11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://github.com)
[![Framework](https://img.shields.io/badge/.NET_9_WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://github.com)
[![Version](https://img.shields.io/badge/v1.0.0-107C10?style=for-the-badge)](https://github.com)
[![Design](https://img.shields.io/badge/Fluent_UI-0078D4?style=for-the-badge)](https://github.com)

</div>

---

## What is Klik?

Klik is a lightweight Windows desktop utility that automates repeated text input and key presses. Built for developers, support engineers, and anyone who types the same thing more than twice — Klik sends a message on a set schedule so you don't have to.

> Set your message. Set your interval. Press **F6**. Walk away.

---

## Features

| | Feature | Description |
|---|---|---|
| ⌨️ | **Global F6 Hotkey** | Start and stop from anywhere — no window switching needed |
| 📋 | **Preset Manager** | Save named message + interval combos, switch in seconds |
| 🔑 | **Custom Send Key** | Remap the trigger key — `Enter`, `Tab`, `Space`, anything |
| ⏱️ | **Precision Intervals** | Set timing in min / sec / ms — 10ms floor prevents lockups |
| 📊 | **Live Status Panel** | Real-time send counter, elapsed timer, and status warnings |
| 🌗 | **System Theme Sync** | Follows Windows light/dark mode automatically |
| 🔒 | **Single Instance** | One Klik running at a time, enforced via global mutex |
| 💾 | **Persistent Storage** | Settings and presets saved to `%APPDATA%/Klik` |

---

## How it works

```
  You                        Klik
  ─────────────────────────────────────────────────────
  Compose message  ──────►  Store in field or preset
  Set interval     ──────►  Validate (min 10ms)
  Press F6         ──────►  3-second countdown begins
                            │
                            ▼
                   ┌─────────────────┐
                   │  Type message   │
                   │  Press send key │◄── loops until stopped
                   │  Wait interval  │
                   └─────────────────┘
                            │
  Press F6 again   ──────►  Loop stops immediately
```

---

## Quick Start

**Requirements:** Windows 10 / 11 · .NET 9 SDK

```bash
# Clone and build
git clone https://github.com/your-username/klik.git
cd klik
dotnet build

# Run
dotnet run

# Publish as a single-file executable
dotnet publish -c Release -r win-x64 --self-contained true
```

The published output is a single `.exe` — no installer, fully portable.

---

## Project Structure

```
klik/
├── Views/               # WPF windows — main UI + preset manager
├── ViewModels/          # MVVM logic — automation, presets, UI state
├── Services/
│   ├── KeyboardService      # Injects text + send key via Windows API
│   ├── HotkeyService        # Registers global F6 shortcut
│   └── PresetStorageService # Reads/writes JSON in %APPDATA%/Klik
├── Helpers/
│   ├── ThemeHelper          # Listens for Windows theme changes
│   └── KeyInteropHelper     # Key name ↔ VirtualKey mapping
├── Models/
│   ├── MessagePreset        # Preset data model
│   └── AppSettings          # Persisted app configuration
└── Themes/
    ├── DarkTheme.xaml
    └── LightTheme.xaml
```

---

## Storage

Klik persists everything in the current user's app data:

```
%APPDATA%/Klik/
├── presets.json    # Saved message presets
└── settings.json   # Send key, interval, always-on-top preference
```

---

## Developer Notes

- **Minimum interval** is enforced at 10ms to prevent keyboard input lockups.
- **Single instance** is managed via a named mutex: `Klik_SingleInstance`.
- **Theme sync** reads `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme` and updates WPF resource dictionaries + title bar appearance at runtime.
- **NuGet warning** — `H.NotifyIcon.Wpf` targets older .NET Framework profiles. The warning during restore is expected and does not affect the build.

---

<div align="center">

Built for developers who value their time.

</div>
