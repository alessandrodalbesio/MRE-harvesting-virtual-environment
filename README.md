# Virtual Environment
This repository has been developed as part of the project *"Mixed Reality Environment For Harvesting Study"* done by Alessandro Dalbesio.

## Introduction
This repository contains all the code relative to the Virtual Environment. <br>
The development has been done for the Oculus Quest 2 but it should be compatible with other Oculus devices. <br>

## Installation
To install this Unity project follow these steps:
1. Clone the repository
2. Open Unity Hub
3. Click on "Add" and select the cloned repository
4. Open the project
5. Import from Asset store the package "Oculus Integration"
6. Go to File > Build Settings
7. Go to "Android" and click on "Switch Platform"
8. If in the bottom right of the unity windows the Oculus icon has some dot on it click on it and then click to "Apply all"

*Note*: The Oculus Integration is very helpfull to manage the Oculus Quest (It's possible to use the Oculus Quest without it but it's much simpler to use it). <br>
If you see lots of warnings in the console, don't worry, they are all related to the Oculus Integration and they don't affect the project.

## Run the project onto the Oculus Quest
Folow these steps to be able to run the project onto the Oculus Quest:
1. Connect the Oculus Quest to the computer with the link cable
2. If you have any VPN active, disable it (ZeroTier doesn't give any problem but other VPNs do)
3. Open the project in Unity
4. Go to File > Build Settings > Platform: Android
5. If the Oculus Quest has been correctly connected to the computer and the access permissions has been done from the Headset you should be able to see the device in the section "Run Device"
6. <i>[Optional]</i> If you wish to see the logs from the Headset you should click on the button "Development Build"
7. Click on "Build and Run"
8. <i>[Optional]</i> If you have clicked on the button "Development Build" you can now go in the "Console" window and click on "Editors" and then on "Oculus". All the logs from the Headset will be shown in the console.


## Code structure
Here a overview over the main folders is given. Below you will find a more detailed description on the files of the <code>Scripts</code> folder. <br>
The main folders of the project are:
- <code>Materials</code>: this folder contains all the materials used in the project
- <code>Prefabs</code>: this folder contains all the prefabs used in the project.
- <code>OBJImport</code>: this is a third party library used to import .obj files into Unity. It's not well documented but it's very easy to use and to modify (you just need to spend a little bit of time).
- <code>Scenes</code>: this folder contains all the scenes of the project.
- <code>Scripts</code>: this folder contains all the scripts of the project.
The other folders are generated automatically from Unity and so a deep description is not given. <br><br>

Below you can find a structure of the code in the folder <code>Scripts</code>
- <code>OptitrackObjects/Rigidbody.cs</code>: This file contains the class Rigidbody that is used to store all the information relative to a rigidbody from the Optitrack server.
- <code>OptitrackObjects/Tracker.cs</code>: This file contains the class Tracker that is used to store all the information relative to a tracker from the Optitrack server.
- <code>DataSynchronizer.cs</code>: This file manage the synchronization of the data between the server and the client.
- <code>Model.cs</code>: This file contains the class Model that is used to store all the information relative to a model.
- <code>Parameters.cs</code>: This file contains some settings related to the parameters of the project.
- <code>SceneLoader.cs</code>: This file contains all the methods needed to load the scenes.
- <code>StorageManager.cs</code>: This file contains all the function to store and retrieve the data from the storage. This file might also be used to implement a database (but it's not implemented yet because I had problems compatibility issues between SQLite and the Oculus).
- <code>Texture</code>: This file contains the class Texture that is used to store all the information relative to a texture.
- <code>TrackerSynchronizer.cs</code>: This file manage the synchronization of tracker data from the Optitrack (through the websocket server) to the client.

## Used versions
The following versions have been used:
- Unity 2021.3.19f1
- Oculus Integration 54.0

## Authors
This repository is part of the project *"Mixed Reality Environment For Harvesting Study"* done by Alessandro Dalbesio.<br>
The project has been done in the CREATE LAB (EPFL).<br>
Professor: Josie Hughes<br>
Supervisor: Ilic Stefan<br>

## License
This project is under **MIT** license. <br>

