Those are a list of thing that need to be done.

**Be aware that this document is in constant evolution, thing can be added, edited, or removed while working on it.**

*Why not in Issues?* - Simply because, for me (and I'm maybe wrong, but well, I am what I am) Issue are for problems, or idea from USERS, not from me (the developer). I already tried, but couldn't continue as I doesn't have the process in mind to update it.

*Note: The content here doesn't show what is inside the engine, some task are not checked because their are not 100% done. A task is checked once and only when it's done at 100% or in a state that is usable by the user.*

*Some word definition*

- User: The person using the engine to create a game
- We/Developer/I: The developer making the engine.
- Player: The person playing a game made with the engine.
- ModDev: The person creating module for the engine.
- Module: A plugin for the engine (I call them module by reflex).

### MILESTONE FOR PREVIEW 01

*Those are thing that need to be done for a first version, also called 'Preview01' of the engine, they will be very 'simple', and will be updated and upgrated with update.*

* [x] Simple collision system:
  A simple collision system that allow user to define collision with tileset and character.
  The character need to be blocked, or slide depending of what movement the player is currently doing.
  The user should be able to edit the collision box size for a character, placement, and size.
* [ ] Basic Main menu:
  A basic main menu, that would allow the player to start a new game, load a save, and quit the game. The user should be able to edit some parts of the main menu. It should not be too much edit for now.
  [We already have a basic GameUI Library (Can be found inside the RPGCreator.RTP project), we still need now to have a simple json file, so we don't have to rework it from the ground up when implementing the UI editor.]
* [ ] Simple movement system:
  Allow the user to define a movement with 3 types (free, based on 4 grid, and based on 8 grid).
  [Still need some work, mainly for the grid based movement, but the free movement is done.]
* [x] Character animation system:
  Allow the user to define an animation based on the state of the character (moving, idling, ...).
  [This is kinda done, but need some test to confirm it 100%.]
* [ ] Exporting a game:
  Allow the user to.. well export a game with the engine. It will not be too complex for now. Probably just a 'build' folder with the Player.exe inside, and the assets in a json format (format used by the engine while in editor).
  [This **should** be 'easy', as the player.exe is done, and load a gameData.json file, to link the project.json with each assets. But it still need some fix and UI to be OK]
* [x] Drawing a map.
* [x] Creating a character, editing it, and saving it.
  [Also kinda done, we can edit the name, the lvl (that is useless as there is no fight or way to gain xp for now, but well, its here), feature. But need to be tested from the ground up to confirm it.]
* [x] Loading/unloading a module:
  For now, when a module is loaded, the engine would need to be restarted. The loading/unloading need to be simple for the user.
  [This is kinda done too, what we need now is the UI, and the unloading part, but with the ModuleManager service, it should be quite easy? Probably, with chance.]
* [x] Importing a tileset, and using it.
* [x] Creating a layer, editing it, and saving it.
* [x] Creating a map, editing it, and saving it.
* [x] Camara system:
  The camera need to be able to lock the player, and follow him. For now this should be this simple, no massive parameters.
* [ ] Adding a "spawn player point" on the map editor, allowing the user to define where the player will appear on a map when the player access it.
* [ ] Convert map rendering by using ECS now. Currently we are using an "hybrid" style, the MapRenderingSystem is hardlinked to the POO Definition class, it should not be. The ideal solution would be to 'convert' the map definition by taking it's layer, converting them to an entity, and adding component (like ChunkStorageComponent and LayerHeaderComponent) then adding a componentTag (like 'TileLayerTagComponent' or 'EntityLayerTagComponent').

### Milestone for Preview 02

* [ ] Add a way for modDev to edit dynamically the UI. For example by editing the sourceGenerator to allow this. (ModDev CAN already edit the UI, but it's not 'dynamic', as they need to 'subscribe' to extensible parts).
* [ ] Add world feature. Feature that will allow the user and modDev to add thing to a world globally (like rain, day/night, ...)
* [ ] Add a better Game Feature system.
* [ ] Add a way for feature property to 'link' them together with an Id.
* [ ] Add a way for feature property to call an action method defined inside the feature when a property has changed.

