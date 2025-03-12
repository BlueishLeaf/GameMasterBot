# GameMasterBot - A TTRPG Campaign Management Bot for Discord
## Feature Overview
- Automated campaign creation including text/voice channels and separate roles for GMs and Players.
- Commands for managing your campaign such as adding/removing players and attaching a URL for online play.
- The ability to schedule sessions for a campaign that re-occur at a specified interval (Weekly, Monthly, etc.). Players will be reminded of a session 30 minutes from its start time and when the session is supposed to begin.
- Allows players to set their timezones so that they can be taken into account when scheduling a session.
- Various helper commands to view the upcoming sessions for a campaign, view the list of players in a campaign, etc.

## How to build and run locally
- Install the .Net 9 SDK and Docker.
- Set the DISCORD_TOKEN environmental variable. This is the secret that you get from the Discord developers portal.
- Set the DB_CONNECTION_STRING environmental variable. You can get the details for this from the docker compose file.
- Set the TEST_GUILD_ID environmental variable. This is the ID of the server you will be testing on.
- Run the GameMasterBot project in debug mode. As long as you are in debug mode, EF will migrate the DB automatically.