using Godot;
using System;

public partial class TamaScreen : Node2D
{
    [Export] public Node2D falseTama;
    [Export] public Node2D trueTama;
    [Export] public AnimatedSprite2D falseTamaSprite;
    [Export] public AnimatedSprite2D trueTamaSprite;
    [Export] float spdFalseT = 5f;
    [Export] float spdFalseScale = 5f;
    public float curFalseT;
    public float targetFalseT;
    public float curTrueT = 1.0f;
    public float targetTrueT = 1.0f;

    public override void _Process(double delta)
    {
        {
            curFalseT = Mathf.Clamp(Mathf.Lerp(curFalseT, targetFalseT, spdFalseT * (float) delta), 0, 1);
            float scalet = curFalseT * 2.4f + (1 - 2.4f) * curFalseT * curFalseT;
            Vector2 scale = new Vector2(4, 0).Lerp(Vector2.One, scalet);
            float alpha = Mathf.Sqrt(curFalseT);
            falseTama.Modulate = new Color(1, 1, 1, alpha);
            falseTamaSprite.Scale = scale;
        }
        {
            curTrueT = Mathf.Clamp(Mathf.Lerp(curTrueT, targetTrueT, spdFalseT * (float) delta), 0, 1);
            float scalet = curTrueT * 2.4f + (1 - 2.4f) * curTrueT * curTrueT;
            Vector2 scale = new Vector2(4, 0).Lerp(Vector2.One, scalet);
            float alpha = Mathf.Sqrt(curTrueT);
            trueTama.Modulate = new Color(1, 1, 1, alpha);
            trueTama.Scale = scale;
        }
    }
    public void initFalse()
    {
        curFalseT = 0.0f;
        targetFalseT = 1.0f;
        falseTama.Visible = true;
    }
    public void setTrue(bool view)
    {
        targetTrueT = view ? 1.0f : 0.0f;
    }

}
