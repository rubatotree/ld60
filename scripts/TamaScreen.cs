using Godot;
using System;

public partial class TamaScreen : Node2D
{
    [Export] Node2D falseTama;
    [Export] AnimatedSprite2D falseTamaSprite;
    [Export] float spdFalseT = 5f;
    [Export] float spdFalseScale = 5f;
    float curFalseT;
    float targetFalseT;
    float curTrueAlpha;
    float targetTrueAlpha;

    public override void _Process(double delta)
    {
        curFalseT = Mathf.Clamp(Mathf.Lerp(curFalseT, targetFalseT, spdFalseT * (float) delta), 0, 1);
        float scalet = curFalseT * 2.4f + (1 - 2.4f) * curFalseT * curFalseT;
        Vector2 scale = new Vector2(4, 0).Lerp(Vector2.One, scalet);
        float alpha = Mathf.Sqrt(curFalseT);
        falseTama.Modulate = new Color(1, 1, 1, alpha);
        falseTamaSprite.Scale = scale;
    }
    public void initFalse()
    {
        curFalseT = 0.0f;
        targetFalseT = 1.0f;
        falseTama.Visible = true;
    }

}
