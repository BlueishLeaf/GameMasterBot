# GameMasterBot - A TRPG management bot for Discord
## Overview
GameMasterBot is a discord bot for managing TRPG campaigns.
With it, you can easily automate the tasks associated with creating a campaign,
such as creating text and voice channels, creating and assigning roles,
and scheduling or cancelling sessions without having to worry about timezone issues.

## Roadmap/TODO
Check out the roadmap [here](https://github.com/BlueishLeaf/GameMasterBot/projects/1)!

## How to build and run locally
- Install .Net Core 3.1
- Set the DISCORD_TOKEN environmental variable. This is the secret that you get from the Discord developers portal.
- Set the environmental variables needed to build the mariaDB connection string: DB_USER, DB_PASSWORD, DB_HOST and DB_NAME.
- Update the database with 'dotnet ef database update'.