using Godot;
using System;

public partial class BaobaoGame : Node2D
{
    [Export] World world;
    [Export] public Node2D GameOrigin;
    [Export] public AnimatedSprite2D Line0m;
    [Export] public AnimatedSprite2D Line3m;
    [Export] public AnimatedSprite2D Line6m;
    [Export] public AnimatedSprite2D Line10m;
    [Export] public PackedScene LineOther;
    [Export] public Node2D Lines;
    [Export] public Control GameUI;
    [Export] public Label TimeLabel;
    [Export] public Label LifeLabel;
    [Export] public float YOffset = 36f;
    [Export] public float TargetY = 2010f;
    [Export] public bool IsGaming = false;
    [Export] public float spdGameAlpha = 8f;
    [Export] public float spdYScroll = 5f;
    [Export] public float maxspdScroll = 6f;
    public float targetGameAlpha = 0.0f;
    public float targetYScroll = 0.0f;
    public float curYScroll = 0.0f;
    public float curGameAlpha = 0.0f;
    Vector2 originStartPos = Vector2.Zero;
    public override void _Ready()
    {
        Line0m.Position = new Vector2(0f, -TargetY * 0f / 10f);
        Line3m.Position = new Vector2(0f, -TargetY * 3f / 10f);
        Line6m.Position = new Vector2(0f, -TargetY * 6f / 10f);
        Line10m.Position = new Vector2(0f, -TargetY * 10f / 10f);
        for(int i = 0; i <= 10; i++)
        {
            if(i == 0 || i == 3 || i == 6 || i == 10) continue;
            Node2D instance = LineOther.Instantiate<Node2D>();
            Lines.AddChild(instance);
            instance.Position = new Vector2(0f, -TargetY * i / 10f);
        }
        originStartPos = GameOrigin.Position;
    }
    public override void _Process(double delta)
    {
        // TimeLabel.Visible = (int)(world.time * 2) % 2 == 0;
        curGameAlpha = Mathf.Lerp(curGameAlpha, targetGameAlpha, spdGameAlpha * (float)delta);
        Lines.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        GameUI.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        targetYScroll = Mathf.Clamp(targetYScroll, 0f, TargetY * 1.2f);
        float nextCurYScroll = Mathf.Lerp(curYScroll, targetYScroll, spdYScroll * (float)delta);
        float scrollSpeed = Mathf.Clamp(nextCurYScroll - curYScroll, -maxspdScroll, maxspdScroll);
        curYScroll += scrollSpeed;
        GameOrigin.Position = originStartPos + new Vector2(0f, curYScroll);
    }
}
