using Godot;
using System;

public partial class World : Node2D
{
    public float time = 0;

    public override void _Process(double delta)
    {
        time += (float)delta;
    }
}
