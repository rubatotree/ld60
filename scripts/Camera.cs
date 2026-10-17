using Godot;
using System;

public partial class Camera : Node2D
{
    [Export] Camera2D cam;
    [Export] float MousePosLerpCoeff = 4f;
    [Export] float MousePosScale = 0.04f;
    Vector2 mousePos = Vector2.Zero;
    Vector2 mouseDeltaPos = Vector2.Zero;
    public override void _Ready()
    {
    }
    
    public override void _Process(double delta)
    {
        mousePos = GetGlobalMousePosition() - Position;
        if(mousePos.Length() > 200f) mousePos *= 200f / mousePos.Length();
        mouseDeltaPos = mouseDeltaPos.Lerp(mousePos, MousePosLerpCoeff * (float)delta);
        cam.Position = -mouseDeltaPos * MousePosScale;
    }
}
