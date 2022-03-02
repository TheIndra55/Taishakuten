# Taishakuten

Taishakuten (previously Kurisu) is a simple Discord bot containing all features needed to moderate my own and friends' guilds, it has basic features like reminders, join messages and moderator commands.

## Setup

Make sure you have .NET 6 installed on your system and exposed to your command line and run

```
dotnet build
```

Next create a file next to the executable named `config.json`.

```json
{
    "token": "bot token",

    // your database connection string
    "connection_string": "server=localhost;database=taishakuten;user=indra",
}
```

Lastly run the bot, you could also `dotnet run`

## Current features

* Reminders
* Welcome messages
* Info commands like avatar, user info and guild info
* Scanning of attachments using YARA
