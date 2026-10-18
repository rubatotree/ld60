using Godot;
using System;

public partial class Baobao : Node2D
{
    [Export] float spdScaling = 16f;
    [Export] float spdRotating = 5f;
    public Vector2 targetNaturalScale = Vector2.One;
    public Vector2 targetSquashScale = Vector2.One;
    public bool Hoverable = true;
    public override void _Ready()
    {
        targetNaturalScale = Scale;
    }
    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition() - Position;
        bool onHover = Hoverable && mousePos.Length() < 64;
        bool onSquash = onHover && Input.IsMouseButtonPressed(MouseButton.Left);
        if(onHover && !onSquash)
            targetSquashScale = new Vector2(1.1f, 1.1f);
        else if(onSquash)
            targetSquashScale = new Vector2(1.2f, 0.8f);
        else
            targetSquashScale = Vector2.One;
        Scale = Scale.Lerp(targetNaturalScale * targetSquashScale, spdScaling * (float)delta);
    }
}
