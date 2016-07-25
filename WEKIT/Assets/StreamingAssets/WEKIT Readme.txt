  ___ ___   _   ___  __  __ ___ 
 | _ \ __| /_\ |   \|  \/  | __|
 |   / _| / _ \| |) | |\/| | _| 
 |_|_\___/_/ \_\___/|_|  |_|___|
								
This Unity project serves the purpose of recording and replaying data from different input devices.
It is framerate independent, though a higher framerate generally means more precise data.
Currently supported are the Microsoft Kinect, Leap Motion, Thalmic Myo, audio recordings, and subtitles. Please note that drivers or SDKs may be required for these
An implementation including all of the aforementioned devices can be found under Assets/WEKITScene. 
Wekit Ghost uses semitransparent models for replays while still showing realtime input. Wekit uses regular models but blocks input during replays. Wekit Networked was an attempt at transmitting Leap data over a network (currently on hold). Use Cases is a scene that has the user select a use case (set up) at the beginning.
There are currently two different GUIs, WekitGui_Full and WekitGui_Replay. The replay GUI lacks recording, saving and deleting functionality and is intended for end users.

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
								If you are using a WekitPlayerContainer, each device will have its own TimeStep slider situated above its name.

Replay/Stop			F9			Replay or stop replaying the data that was last recorded or loaded. 
								A slider underneath the Pause/Unpause switch indicates the currently played frame and 
								can be moved back and forth to go to specific sections in the replay.
								The slider and text box just below that are responsible for replay speed, 1 being the original tempo.

Pause/Unpause		F10			Only visible in replay mode. Pauses or unpauses the replay. 
								Replay speed and current frame can still be changed while paused.

Load, Save, Delete				Save the current replay, or load or delete an existing one. 
								Replay names are specified in the text field below each button.
								The zip and compound archive toggles on the left can be used to save the data as a standalone zip 
								archive or as an entry in a zip archive with other replays (In this case, the name of the archive 
								is specified via a text field). Zip archives are smaller than regular files but take more time to 
								save and load. 
								If your GUI's WekitPlayer is a PlayerContainer, you also have the choice of saving it all in a single 
								or in multiple individual files
								It is also possible to save an XML file along with the replay (see below).
								Files are located at Unity's StreamingAssets path which is visible in the editor and
								included in builds.

  ___ ___ _____ _   _ ___ 
 / __| __|_   _| | | | _ \
 \__ \ _|  | | | |_| |  _/
 |___/___| |_|  \___/|_|  
						  
The custom scripts for WEKIT are located in the Assets/Scripts/ folder.

Data is handled by appropriate classes that are usually inherited from the WekitPlayer<T,TProvider> class, where T is the type the data is saved as and TProvider is the type of the class providing the data (a reference to an instance of this class must also be set in the editor under the Provider variable).
Alternatively, for special cases, your class may directly inherit from WekitPlayer_Base (see AudioPlayer or TextPlayer for example). Be aware that you will need to manually implement functionality such as saving and loading data.
The actual method for getting the data will differ between devices and needs to be specified by overriding the AddFrame() method. 
Frame data from the WekitPlayer can in turn be accessed with the GetCurrentFrame() method. This should be once per frame (if the player is in replay mode), usually in the update, in order to avoid desynchronization.
Override the DefaultFrame method if you want to specify what to do if the FrameList is empty. 

The state of the player can be checked with the Recording, Replaying and Playing booleans, Playing being true after the recording countdown or while a replay is not paused. The FrameListIsEmpty() method can similarly be used to verify whether a replay is currently loaded.
If you need an agnostic reference to a player, for instance in the GUI, create a WekitPlayer_Base variable. This is the base class WekitPlayer is derived from and includes most functionality.

The player can be further customized with the following strings: 

UncompressedFileExtension	The extension files receive when not put in a zip archive.
SavePath					The standard directory where files will be saved, loaded and deleted from. By default Unity's persistentDataPath.
CustomDirectory				Name of the folder within the standard directory where data from this player is saved. Folder will be created automatically if nonexistant.
PlayerName					What this player is referred to as, if part of a WekitPlayerContainer.

Standard values for these variables can be set by implementing Unity's Reset() method. Note that this doesn't automatically change values on instances of the script that are already placed in a scene.

For examples on implementing a player, compare MyoPlayer, KinectPlayer and LeapPlayer. For a nonstandard implementation, check AudioPlayer.

  ___ ___ ___  _   _ ___ _  _  ___ ___ ___ 
 / __| __/ _ \| | | | __| \| |/ __| __/ __|
 \__ \ _| (_) | |_| | _|| .` | (__| _|\__ \
 |___/___\__\_\\___/|___|_|\_|\___|___|___/
													   
It is possible to create a sequence of replays by using XML files. To do this, simply pick the option "Use XML" when saving. In addition to your replay, an appropriate XML file will be created at the save path (in the zip archive if using that option). 
For example, if your replay is called Demo and was not saved as a zip file, the XML file might look like this:

<?xml version="1.0" encoding="Windows-1252"?>
<XMLData xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Files>
	<XMLFileInfo>
	  <FileName>Demo</FileName>
	  <EntryName>Demo</EntryName>
	  <Zip>false</Zip>
	</XMLFileInfo>
  </Files>
</XMLData>

To append other replays, simply copy the <XMLFileInfo> section, paste it right after the original, and replace the information as necessary. 
For example:

<?xml version="1.0" encoding="Windows-1252"?>
<XMLData xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Files>
	<XMLFileInfo>
	  <FileName>Demo</FileName>
	  <EntryName>Demo</EntryName>
	  <Zip>false</Zip>
	</XMLFileInfo>
	<XMLFileInfo>
	  <FileName>Demo2</FileName>
	  <EntryName>Demo2Entry</EntryName>
	  <Zip>true</Zip>
	</XMLFileInfo>
  </Files>
</XMLData>

In this case, the second replay is saved in a zipfile called Demo2.zip, with the entry being named Demo2Entry. An easy way to get the correct XMLFileInfo is to save an XML file for each replay in the collection and then copy the data from there.
Please note that the XML system only searches in the custom folder for your WekitPlayer. For example if your scene only contains a LeapPlayer and you try to add a reference to a replay in the kinect folder, it won't be found. The best way to avoid this is by using a WekitPlayerContainer in your scene.

  _   _ ___ ___    ___   _   ___ ___ ___ 
 | | | / __| __|  / __| /_\ / __| __/ __|
 | |_| \__ \ _|  | (__ / _ \\__ \ _|\__ \
  \___/|___/___|  \___/_/ \_\___/___|___/
										 
The term "use case" describes a specific player setup. For example, a "precision task" use case may require a Leap Motion and audio but not a Kinect. 
The UseCaseSelector class allows you to specify these use cases. Simply place it in a scene, assign the public variables and set the players (including the container) inactive. An example of this is already set up in the Use Cases scene. 
Use cases are loaded from the file UseCases.txt in the StreamingAssets folder. To create custom use cases, simply modify, add or remove the below sections:

<UseCase>		A use case, contains <UseCaseName> and <Elements>
<UseCaseName>	The name which is displayed for this use case
<Elements>		Contains the various <UseCaseElement>s (players and descriptions) for this use case
<PlayerName>	The name of the player activated after the user clicks the <Message>. If this is left blank or does not correspond to a player in the scene, nothing will be activated. This is useful if you want to tell the user to connect or do something that is not supported by the system, like a point-of-view camera. 
<Message		Message displayed to the user

If the StreamingAssets folder does not contain a file called UseCases.txt, one will be created on starting a scene with a UseCaseSelector. This example file will include one use case for each player in the scene.

   ___ _____ _  _ ___ ___    ___ _      _   ___ ___ ___ ___ 
  / _ \_   _| || | __| _ \  / __| |    /_\ / __/ __| __/ __|
 | (_) || | | __ | _||   / | (__| |__ / _ \\__ \__ \ _|\__ \
  \___/ |_| |_||_|___|_|_\  \___|____/_/ \_\___/___/___|___/
															
* WekitGui / WekitGUI_Replay / WekitGui_Full

Provides the graphic user interface. Simply put this into a scene and assign the player you want to control to the Player variable.


* WekitPlayerContainer

A class for controlling multiple players at once. In the Unity editor, simply drag the players you want into the _wekitPlayers list.


* WekitKeyInput

Allows you to control a WekitPlayer_Base class through keyboard input. 
Currently, only the Record, Replay and Pause key are supported, to prevent the other methods from being called erroneously. 