# UnityNetcodeGame
Unity game based on "Netcode for gameobjects" + ASP Server

This is an open project. You can use my code as you like.

What's inside:
Unity game client and game server using "Netcode for GameObjects"       
ASP Core server + SQLite database for authorization and data storage.    

- https://github.com/BobikStudio/NetGame-UnityProject
- https://github.com/BobikStudio/NetGame-ASP-Server
  
What it has:
- User authorization by Login/Password
- "Remember me" token to keep account between sessions
- User authorization check when connecting to the game server
- Animated player character with movement logic

How to use:
- Open the ASP Server folder and find BobikServer.sln, run it and build the solution.
- Open the Unity project. Window -> Multiplayer -> Multiplayer Play Mode. Add a virtual player with the tag "Server". This tag allows running the game server without building a Headless server, which is very convenient.
- Enter Play Mode
- If you did everything correctly, you will have:
   - console (ASP Server) - authorization server + database.
   - Unity player with tag "Server" (Game server)
   - Unity player (Game client)

- Create an account through the Unity UI.
- Log in to the account through the Unity UI.
- You should be connected to the game server, and see the nickname above the character.
