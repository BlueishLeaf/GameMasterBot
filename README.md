# GameMasterBot v1.0.0
## Overview
GameMasterBot is a discord bot for managing TRPG campaigns.
With it, you can easily automate the tasks associated with creating a campaign,
such as creating text and voice channels, creating and assigning roles,
and scheduling or cancelling sessions without having to worry about timezone issues.

## Roadmap/TODO
Check out the roadmap [here](https://github.com/BlueishLeaf/GameMasterBot/projects/1)!

## How to Build and Run
- Install .Net Core 3.1
- Set the following environmental variables: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS, and DISCORD_TOKEN.
- Create a DynamoDb table with the specifications as below.

### DynamoDb Table
- Name the table "GameMasterBotTbl".
- Set "Pk" as the partition key and "Sk" as the sort key.
- Create a global secondary index called "Entity-Sk-Index", which has "Entity" as its partition key, and "Sk" as its sort key.
- Enable TTL on the "Expiry" attribute.
