# GameMasterBot v0.1.1
## Overview
GameMasterBot is a discord bot for managing TRPG campaigns. With it, you can easily automate the tasks associated with creating a campaign, such as creating text and voice channels, creating and assigning roles, and scheduling or cancelling sessions.

## Roadmap
- Support for more fine-grain control over scheduling and cancelling sessions.
- Support for music playback and the ability to create playlists for campaigns.
- More detailed views regarding schedules and players.
- A more streamlined approach to creating campaigns.
- Provide a cloudformations template to automate the infrastructure required to build the bot.

## How to Build and Run
- Install .Net Core 2.2
- Set the following environmental variables: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS, and DISCORD_TOKEN
- Create a DynamoDb table with the specifications as below.

### DynamoDb Table
- Name the table "GameMasterBotTbl".
- Set "Pk" as the partition key and "Sk" as the sort key.
- Create a global secondary index called "Entity-Sk-Index", which has "Entity" as its partition key, and "Sk" as its sort key.
