## Game Save Source Control 

  

The project is based on an idea one of my friends had to allow any player to host the same save via a .bat that syncs the save to a OneDrive shared folder. I built this simple Console utility to do the same thing with git to allow the save to be hosted with all the handy features of git, as well as manage many shared saves in one place 

The app manages multiple files pulling them all to a local repo and notifies if new saves are available when started. Sync of saves can be set manually at any time or set for a running applications closure (provided the app is mapped in the system) 
Currently the app is still not complete, work is needed to reduce complexity and make the app stable enough for an average user, see the following TODO list to get a general overview of the apps current state.

### Important TODO

- Allow More branch name choice other than master(this is no longer standard)

- Allow Save Folders to be tracked (without potential conflict between file and folders) 

- Make app more identifiable with an icon and name in open windows

- Add assistance to players when new saves are available and make it easier to match with freinds

- Fix dialog windows dissapearing

- Make application config easier in general when linking to github(currently needs a token or password for auth)

- Provide some form of gude for users working with the app