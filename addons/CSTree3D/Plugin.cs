#if TOOLS
using Godot;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var script = GD.Load<Script>("res://addons/CSTree3D/CSTree3D.cs");
        var icon = GD.Load<Texture2D>("res://addons/CSTree3D/ico/Tree3D.png");
        AddCustomType("CSTree3D", "Node3D", script, icon);
    }
}
#endif
