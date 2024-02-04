using Godot;

public partial class Main : Node2D
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed && keyEvent.Keycode == Key.F)
            {
                Dummy dummy = GD.Load<PackedScene>("res://Scenes/Dummy.tscn").Instantiate<Dummy>();
                dummy.GlobalPosition = GetGlobalMousePosition();
                AddChild(dummy);
                MoveChild(dummy, 0);
            }
        }
    }
}
