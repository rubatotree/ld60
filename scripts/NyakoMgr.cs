using Godot;
using System;

public partial class NyakoMgr : Node2D
{
    [Export] World world;
    [Export] Sprite2D base_spr;
    [Export] Sprite2D face_spr;
    [Export] float MinRotation = -0.03f * 2 * Mathf.Pi;
    [Export] float MaxRotation = -0.02f * 2 * Mathf.Pi;
    [Export] float RotateSineCoeff = 4f;
    [Export] float MousePosLerpCoeff = 4f;
    [Export] float MousePosScale = 0.00f;

    float curRotation = 0f;

    Vector2 originPos;
    Vector2 mousePos = Vector2.Zero;
    Vector2 mouseDeltaPos = Vector2.Zero;
    public override void _Ready()
    {
        originPos = Position;
    }
    
    public override void _Process(double delta)
    {
        curRotation =  Mathf.Lerp(MinRotation, MaxRotation, Mathf.Sin(RotateSineCoeff * world.time) * 0.5f + 0.5f);
        Rotation = curRotation;
        mousePos = GetGlobalMousePosition() - originPos;
        if(mousePos.Length() > 200f) mousePos *= 200f / mousePos.Length();
        mouseDeltaPos = mouseDeltaPos.Lerp(mousePos, MousePosLerpCoeff * (float)delta);
        Position = originPos - mouseDeltaPos * MousePosScale;
    }
}
