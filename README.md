## Game Save Source Control 

  

The project is based on an idea one of my friends had to allow any player to host the same save via a .bat that syncs the save to a OneDrive shared folder. I built this simple utility to do the same thing with git to allow the save to be hosted with all the handy features of git, as well as manage many shared saves in one place 

The app manages multiple files pulling them all to a local repo and notifies if new saves are available when started. Sync of saves can be set manually at any time or set for a running applications closure (provided the app is mapped in the system) 

  

### Upcoming Changes 

- Use IOC and move code out of Program.cs 

- Introduce logging to remove any unneeded extra information from UI 

- Introduce UI system to manage user interaction and feedback separate from business logic 

- Allow More branch name choice other than master 

- Allow Save Folders to be tracked (without conflict between players) 

- Update the README to be more useful for anyone looking at the repo 