# CSTree3D
Plugin for procedural generation of 3D trees of varying complexity.
This is a C# rewrite of [gdTree3D](https://github.com/JekSun97/gdTree3D) for faster iteration (no need to restart the editor, errors are less fatal). I'll port them back to the C++ version and try to get them merged, but you can use this version if .NET is not an issue for you.

Changes:
* Fixes for managing child objects (trunk and twig meshes) during tree duplication, undo/redo.
* Fix for unsetting twig material on enable/disable
* Tree is made selectable in editor
* Trunk faces are facing outwards. Not like it was an issue because you could change culling in the material, but IMO it makes sense to have untextured trees displayed correctly too.

<img src="image/Tree3D.png">

## Supported Godot Engine Versions
- Godot 4.0
- Godot 4.1
- Godot 4.2
- Godot 4.3
- Godot 4.4
- Godot 4.5

<img src="image/preview.png">

## Notes
- When using this in a project you have to create a C# solution, build the project and then enable the plugin in project settings. You have to build the demo project too when you open it the first time!
- To change the season, you can make one unique leaf material for all the trees and then change its color or texture to make them yellow.
