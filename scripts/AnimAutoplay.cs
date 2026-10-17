using Godot;
using System;

public partial class AnimAutoplay : AnimatedSprite2D
{
    public override void _Ready()
    {
        Play();
    }

    public override void _Process(double delta)
    {
    }
    
}
