using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    [Export] public Line2D HintLine;
    [Export] public Control GameUI;
    [Export] public Label TimeLabel;
    [Export] public Label LifeLabel;
    [Export] public PackedScene BaobaoScene;
    [Export] public float YOffset = 36f;
    [Export] public float TargetY = 2 * 2010f;
    [Export] public bool IsGaming = false;
    [Export] public float spdGameAlpha = 8f;
    [Export] public float spdYScroll = 5f;
    [Export] public float maxspdScroll = 6f;
    [Export] public float MouseYScrollUp = -90;
    [Export] public float MouseYScrollDown = 135;
    [Export] public float MouseScrollSpeed = 4f;
    [Export] public float EachHeight = 48f;
    [Export] public float SummonTimerMax = 6.0f;
    [Export] public float SummonTimerNoise = 0.5f;
    public float SummonTimer = -1.0f;
    public float targetGameAlpha = 0.0f;
    public float targetYScroll = 0.0f;
    public float curYScroll = 0.0f;
    public float curGameAlpha = 0.0f;
    public Baobao AnyDragged = null;
    Vector2 originStartPos = Vector2.Zero;
    public List<Baobao> baobaostack = new List<Baobao>();
    public bool doAutoScrollUp = false;
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
        HintLine.Modulate = new Color(HintLine.Modulate.R, HintLine.Modulate.G, HintLine.Modulate.B, 0.0f);
    }
    public override void _Process(double delta)
    {
        // TimeLabel.Visible = (int)(world.time * 2) % 2 == 0;
        curGameAlpha = Mathf.Lerp(curGameAlpha, targetGameAlpha, spdGameAlpha * (float)delta);
        Lines.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        GameUI.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        Vector2 MousePos = GetGlobalMousePosition();
        if(IsGaming)
        {
            int scrolld = 0;
            if(MousePos.Y < MouseYScrollUp)
            {
                scrolld += 1;
            }
            if(MousePos.Y > MouseYScrollDown) 
            {
                scrolld -= 1;
                doAutoScrollUp = false;
            }
            targetYScroll += scrolld * MouseScrollSpeed;

            if(baobaostack.Count > 0)
            {
                Baobao stacktop = baobaostack.Last();
                Vector2 NextTargetGameLocalPos = stacktop.Position - new Vector2(0.0f, EachHeight);
                Vector2 StackTopGamePos = -stacktop.Position - new Vector2(0.0f, YOffset);
                Vector2 NextTargetGamePos = StackTopGamePos + new Vector2(0.0f, EachHeight);
                float hintlinealpha = HintLine.Modulate.A;
                hintlinealpha = Mathf.Lerp(hintlinealpha, AnyDragged == null ? 0f : 1f, 16f * (float)delta);
                HintLine.Modulate = new Color(HintLine.Modulate.R, HintLine.Modulate.G, HintLine.Modulate.B, hintlinealpha);
                Vector2 GameTargetPosOffseted = -NextTargetGamePos - new Vector2(0, YOffset);
                Vector2 GameMousePos = GetLocalMousePosition() - GameOrigin.Position;
                HintLine.Points = new Vector2[]{ GameTargetPosOffseted, GameMousePos };
                Vector2 HintLineVec = HintLine.Points[0]  - HintLine.Points[1];
                if(scrolld == 0 && doAutoScrollUp) targetYScroll = Mathf.Lerp(targetYScroll, NextTargetGamePos.Y - 144f, 2f * (float)delta);
                targetYScroll = Mathf.Clamp(targetYScroll, 0f, NextTargetGamePos.Y);

                if(AnyDragged != null)
                {
                    if(HintLineVec.Length() < 90f)
                    {
                        AnyDragged.onMagnet = true;
                        AnyDragged.MagnetPos = new Vector2(GameTargetPosOffseted.X + Mathf.Clamp((GameMousePos.X - GameTargetPosOffseted.X) / 4, -40f, +40f), GameTargetPosOffseted.Y);
                    }
                    else
                    {
                        AnyDragged.onMagnet = false;
                    }
                }

                SummonTimer -= (float)delta;
                if(SummonTimer < 0.0f)
                {
                    var instance = BaobaoScene.Instantiate<Baobao>();
                    GameOrigin.AddChild(instance);
                    instance.Position = new Vector2(world.rng.RandfRange(-120f, 120f), stacktop.Position.Y - 288);
                    instance.Rotation = world.rng.RandfRange(-3f, 3f);
                    instance.Falling = true;
                    instance.targetNaturalScale = Vector2.One * 0.6f;
                    instance.Scale = instance.targetNaturalScale;
                    instance.Frame = world.rng.RandiRange(0, 16);
                    instance.PlayingFPS = world.rng.RandiRange(0, 2) == 0 ? -12 : 12;
                    instance.Playing = true;
                    instance.DragAble = true;
                    instance.world = world;
                    instance.game = this;
                    instance.isBase = false;
                    instance.curAlpha = 0.0f;
                    instance.FallingSpeed = 60f;
                    SummonTimer = SummonTimerMax + world.rng.RandfRange(-SummonTimerNoise, SummonTimerNoise);
                }
            }
        }
        targetYScroll = Mathf.Clamp(targetYScroll, 0f, TargetY);
        float nextCurYScroll = Mathf.Lerp(curYScroll, targetYScroll, spdYScroll * (float)delta);
        float scrollSpeed = Mathf.Clamp(nextCurYScroll - curYScroll, -maxspdScroll, maxspdScroll);
        curYScroll += scrollSpeed;
        GameOrigin.Position = originStartPos + new Vector2(0f, curYScroll);
    }
    public Vector2 ToWorldPos(Vector2 pos) => -pos;
    public Vector2 ToGamePos(Vector2 pos) => -pos;
    public void StartGame()
    {
        IsGaming = true;
    }
}
