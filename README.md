# Kurisu

Kurisu is a simple bot containing all features needed to moderate my and friend's discord guilds, it has basic features like reminders, join messages and moderator commands.

## Setup

Make sure you have .NET Core 3.1 installed and build Kurisu
```powershell
dotnet build
```

## ConVars

Kurisu uses a basic concept of 'convars' based on Source SDK, you can set this in the initial config and at runtime. These are defined in code by the `ConVar` attribute.
```cs
[ConVar("token", HelpText = "The bot token")]
public static string Token { get; set; }
```

## Current features

- Reminders
- Welcome messages
- Info commands like avatar, whois, guild
- Scanning of attachments using VirusTotal and Hybrid Analysis
- Guild setting storage using RethinkDB
- Google assistant
