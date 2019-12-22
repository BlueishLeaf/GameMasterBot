# GameMasterBot v1.0.0
## Overview
GameMasterBot is a discord bot for managing TRPG campaigns.
With it, you can easily automate the tasks associated with creating a campaign,
such as creating text and voice channels, creating and assigning roles,
and scheduling or cancelling sessions without having to worry about timezone issues.

## Roadmap/TODO
- Refactor some of the code to reduce duplication.
- Improve the usability of certain long-winded commands after getting feedback.
- Test the system under heavy stress.
- Provide a CloudFormations template to automate the AWS infrastructure required to build the bot.
- Create a wiki to easily document the commands in a more user friendly way.
- Create a web dashboard for finer control over the bot.

## How to Build and Run
- Install .Net Core 3.1
- Set the following environmental variables: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS, and DISCORD_TOKEN.
- Create a DynamoDb table with the specifications as below.

### DynamoDb Table
- Name the table "GameMasterBotTbl".
- Set "Pk" as the partition key and "Sk" as the sort key.
- Create a global secondary index called "Entity-Sk-Index", which has "Entity" as its partition key, and "Sk" as its sort key.
- Enable TTL on the "Expiry" attribute.
