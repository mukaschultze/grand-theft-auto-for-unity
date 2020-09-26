# Grand Theft Auto for Unity #


Grand Theft Auto for Unity is a project to import files/map of Grand Theft Auto games (3D era) into Unity.

I'm not willing to recreate GTA inside Unity (that would be practically impossible), this is just a tool to import the map and files, it's also a good source of knowledge if you want to learn about compute shaders, surface shaders, custom editor windows, general C# scripting, how the games were back in the 2000s and so on.

Kudos to [GTA Wiki](http://gta.wikia.com), [GTA Modding](http://www.gtamodding.com/wiki/), [GTA Forums](http://gtaforums.com) and [this git repository](https://github.com/dennisyolkin/gta_gameworld_renderer), they were the main source of the knowledge put in this project.

This project is currently under development, some things might not work as intended, some textures will be considered as missing, and some objects will have problems loading.

Contact me if you want to help or learn more about the project: [samuelschultze@gmail.com](mailto:samuelschultze@gmail.com)

### How to use ###

It's very simple to use, just open the project and you will see the menu items on the top of unity editor, it works on windows build too, but you'll need to create your own script that calls the loading methods.

You need to provide the full game to be able to load the maps, this extension doesn't contain any native files from them.

### Currently supported games ###
* GTA III
* GTA Vice City
* GTA San Andreas

### Currently supported files ###
* DAT (The main ones, needed to load the maps)
* IDE
* IPL
* Binary IPL
* DFF
* TXD
* IMG
* Water (partial support, the script that loads it needs to be rewritten)

![GTASA_GoldenGate.png]("Assets/Screenshots/Screenshot  (3).png")
