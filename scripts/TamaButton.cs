using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class TamaButton : Sprite2D
{
    [Export] public Vector2 Center;
    [Export] public float radius = 40;
    [Export] public Sprite2D HoverSprite; 
    [Export] public World world; 
    [Export] public string name = "A";
    [Export] public Color clickModulate = new Color(0.8f, 0.8f, 0.8f, 1.0f);
    [Export] public float hoverAlphaSpd = 20f;
    public bool hover = false;
    public bool pressed = false;
    float hoverTargetAlpha = 0;
    float hoverCurAlpha = 0;
    public override void _Process(double delta)
    {
        Vector2 mvec = GetGlobalMousePosition() - Center;
        hover = mvec.Length() <= radius;
        if(hover && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if(!pressed)
            {
                world.onTamaButtonPressed(name);
                world.isTamaJustPressed = true;
            }
            pressed = true;
        }
        else
        {
            pressed = false;
        }
        hoverTargetAlpha = (hover && !pressed) ? 1 : 0;
        hoverCurAlpha = Mathf.Lerp(hoverCurAlpha, hoverTargetAlpha, hoverAlphaSpd * (float)delta);
        HoverSprite.Modulate = new Color(1, 1, 1, hoverCurAlpha);
        Modulate = pressed ? clickModulate : new Color(1, 1, 1, 1);
    }
}
