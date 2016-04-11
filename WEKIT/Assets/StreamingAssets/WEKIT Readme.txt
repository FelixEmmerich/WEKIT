  ___ ___   _   ___  __  __ ___ 
 | _ \ __| /_\ |   \|  \/  | __|
 |   / _| / _ \| |) | |\/| | _| 
 |_|_\___/_/ \_\___/|_|  |_|___|
                                
This Unity project serves the purpose of recording and replaying data from different input devices.
It is framerate independent, though a higher framerate generally means more precise data.
Currently supported are the Microsoft Kinect, Leap Motion and Thalmic Myo.
An implementation including all of the aforementioned devices can be found under Assets/WEKITScene. 

  ___ _   _ _  _  ___ _____ ___ ___  _  _   _   _    ___ _______   __
 | __| | | | \| |/ __|_   _|_ _/ _ \| \| | /_\ | |  |_ _|_   _\ \ / /
 | _|| |_| | .` | (__  | |  | | (_) | .` |/ _ \| |__ | |  | |  \ V / 
 |_|  \___/|_|\_|\___| |_| |___\___/|_|\_/_/ \_\____|___| |_|   |_|  
                                                                     
Over the WekitPlayerGUI or (if applicable) the buttons listed below, replays can be recorded, replayed, saved, loaded and deleted. 

If, like the Wekit scene, a scene contains multiple devices via the WekitPlayerContainer class, the bottom of the screen includes a list of devices.
Clicking on the name of a device will deactivate it if it is active and activate it if not.
Keyboard shortcuts can be set through a WekitKeyInput script, aside from the hide/show shortcut, which is defined through the GUI. Below are the default inputs.

Function			Shortcut	Description
Hide/Show			H			Hide or show the GUI

Record/Stop			F8			Start or stop the recording process. 
								Recording starts after a countdown which can be set in the text field below the button. 
								The slider below sets a step size from 1 to 3, i.e. a setting of 1 records every frame, 
								a setting of 2 every second frame, etc. Can be used to reduce file size.
								If you are using a WekitPlayerContainer, each device will have its own TimeStep slider situated above their name.

Replay/Stop			F9			Replay or stop replaying the data that was last recorded or loaded. 
								A slider underneath the Pause/Unpause switch indicates the currently played frame and 
								can be moved back and forth to go to specific sections in the replay.
								The slider and text box just below that are responsible for replay speed, 1 being the original tempo.

Pause/Unpause		F10			Only visible in replay mode. Pauses or unpauses the replay. 
								Replay speed and current frame can be changed while pause.

Load, Save, Delete				Save the current replay, or load or delete an existing one. 
								Replay names are specified in the text field below each button.
								The zip and compound archive toggles on the left can be used to save the data as a standalone zip 
								archive or as an entry in a zip archive with other replays (In this case, the name of the archive 
								is specified via a text field). Zip archives are smaller than regular files but take more time to 
								save and load. Files are located at Unity's StreamingAssets path which is visible in the editor and
								included in builds.

  ___ ___ _____ _   _ ___ 
 / __| __|_   _| | | | _ \
 \__ \ _|  | | | |_| |  _/
 |___/___| |_|  \___/|_|  
                          
The custom scripts for WEKIT are located in the Assets/Scripts/CustomScripts folder.

Data is handled by appropriate classes that are inherited from the WekitPlayer<T,TProvider> class, where T is the type the data is saved as and TProvider is the type of the class providing the data (a reference to an instance of this class must also be set in the editor under the Provider variable).
The actual method for getting the data will differ between devices and needs to be specified by overriding the AddFrame() method. 
Frame data from the WekitPlayer can in turn be accessed with the GetCurrentFrame() method. This should be once per frame (if the player is in replay moce), usually in the update.
Override the DefaultFrame method to specify what to do if the FrameList is empty. 

The state of the player can be checked with the Recording, Replaying and Playing booleans, Playing being true after the recording countdown or while a replay is not paused. The FrameListIsEmpty() method can similarly be used to verify whether a reply is currently loaded.
If you need an agnostic reference to a player, like for instance the GUI, create a WekitPlayer_Base variable. This is the base class WekitPlayer derives from.

The player can be further customized with the following strings: 

UncompressedFileExtension	The extension files receive when not put in a zip archive.
SavePath					The standard directory where files will be saved, loaded and deleted from. By default Unity's persistentDataPath.
CustomDirectory				Name of the folder within the standard directory where data from this player is saved. Folder will be created automatically if nonexistant.
PlayerName					What this player is referred to as if part of a WekitPlayerContainer.

Standard values for these variables can be set by implementing Unity's Reset() method. Note that this doesn't automatically change values on instances of the script that are already placed in a scene.

For examples on implementing a player, compare MyoPlayer, KinectPlayer and LeapPlayer.


* WekitPlayerGUI

Provides the graphic user interface. Simply put this into a scene and assign the player you want to control to the Player variable.


* WekitPlayerContainer

A class for controlling multiple players at once. In the Unity editor, simply drag the players you want into the _wekitPlayers list.


* WekitKeyInput

Allows you to control a WekitPlayer_Base class through keyboard input. 
Currently, only the Record, Replay and Pause key are supported, to prevent the other methods from being called erroneously. 