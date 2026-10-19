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
    [Export] public Label TimeLabelOut;
    [Export] public PackedScene BaobaoScene;
    [Export] public float YOffset = 36f;
    [Export] public float TargetY = 2.0f * 2010f;
    [Export] public bool IsGaming = false;
    [Export] public bool IsTiming = false;
    [Export] public float MaxGameTime = 400;
    public float GameTimer = 0;
    [Export] public float spdGameAlpha = 8f;
    [Export] public float spdYScroll = 5f;
    [Export] public float maxspdScroll = 6f;
    [Export] public float MouseYScrollUp = -90;
    [Export] public float MouseYScrollDown = 135;
    [Export] public float MouseScrollSpeed = 4f;
    [Export] public float EachHeight = 48f;
    [Export] public float SummonTimerMax = 4.0f;
    [Export] public float SummonTimerNoise = 0.5f;
    public float SummonTimer = -1.0f;
    public float targetGameAlpha = 0.0f;
    public float targetYScroll = 0.0f;
    public float curYScroll = 0.0f;
    public float curGameAlpha = 0.0f;
    public Baobao AnyDragged = null;
    Vector2 originStartPos = Vector2.Zero;
    public List<Baobao> baobaostack = new List<Baobao>();
    Baobao stacktop => baobaostack.Last();
    public bool doAutoScrollUp = false;

    public int maxStackN = 0;
    public int sumStackN = 0;
    public int FailN = 0;
    public int prevFailN = 0;
    bool firstfail = false;
    float normalized_timing => GameTimer / MaxGameTime * 5;
    int normalized_hour => 8 + (int)(normalized_timing * 2);
    float normalized_5min => (normalized_timing * 2 - Mathf.Floor(normalized_timing * 2)) * 12;
    int normalized_minute => ((int)normalized_5min) * 5;
    bool view_colon => world.time - Mathf.Floor(world.time) < 0.5f;
    string time_text => normalized_hour.ToString("D2") + (view_colon ? ":" : " ") + normalized_minute.ToString("D2");
    int prev_minute = -1;
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
            if(IsTiming)
            {
                GameTimer += (float)delta;
                if(normalized_minute != prev_minute)
                {
                    FirstGetTime(normalized_hour, normalized_minute);
                }
                prev_minute = normalized_minute;
            }
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
                    if(HintLineVec.Length() < 80f)
                    {
                        AnyDragged.onMagnet = true;
                        AnyDragged.MagnetPos = new Vector2(GameTargetPosOffseted.X + Mathf.Clamp((GameMousePos.X - GameTargetPosOffseted.X) / 4, -40f, +40f), GameTargetPosOffseted.Y);
                    }
                    else
                    {
                        AnyDragged.onMagnet = false;
                    }
                }

                if(IsTiming)
                {
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
        }
        targetYScroll = Mathf.Clamp(targetYScroll, 0f, TargetY);
        float nextCurYScroll = Mathf.Lerp(curYScroll, targetYScroll, spdYScroll * (float)delta);
        float scrollSpeed = Mathf.Clamp(nextCurYScroll - curYScroll, -maxspdScroll, maxspdScroll);
        curYScroll += scrollSpeed;
        GameOrigin.Position = originStartPos + new Vector2(0f, curYScroll);

        TimeLabel.Text = time_text;
        bool redText = false;
        if(normalized_hour == 8 && normalized_minute >= 45) redText = true;
        TimeLabel.Modulate = redText ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
        TimeLabelOut.Text = time_text;
    }
    public Vector2 ToWorldPos(Vector2 pos) => -pos;
    public Vector2 ToGamePos(Vector2 pos) => -pos;
    public void StartGame()
    {
        IsGaming = true;
        IsTiming = true;
        string[] str = {
            "[font_size=18][wave amp=24.0 freq=8.0 connected=1][color=red]抚摸[/color][/wave]每一只掉下来的[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]，把他们放在我的头上吧！[/font_size]", 
            "placeholder"
        };
        world.mainbaobao.cowsayBox.showText(str[world.Language]);
        world.regTimer(5f, ()=>{world.mainbaobao.cowsayBox.showText("");});
    }
    public void OnFail(Baobao failinst)
    {
        world.SetScreenShake(16f, 0.2f);
        SummonTimer = Mathf.Max(SummonTimer, 2f);
        if(FailN == 0)
        {
            string[] str = {
                "[shake rate=20.0 level=5 connected=1]呀啊，地震了...[/shake]！", 
                "placeholder"
            };
            failinst.cowsayBox.showText(str[world.Language]);
            world.regTimer(2f, ()=>{failinst.cowsayBox.showText("");});
        }
        FailN++;
        if(baobaostack.Count > 1)
        {
            float killratio = world.rng.RandfRange(0.05f, 0.4f);
            int killn = (int)(baobaostack.Count * killratio);
            killn = Mathf.Clamp(killn, 1, 6);
            int cnt = baobaostack.Count;
            for(int i = cnt - 1; i >= cnt - killn; i--)
            {
                if(i <= 0) break;
                baobaostack[i].MakeFailFalling();
                baobaostack.RemoveAt(i);
                FailN++;
            }
        }
    }
    public void OnAttached(Baobao attachinst)
    {
        // cowsayBox.showText("aaaaaa");
        // world.regTimer(2, ()=>{
        //     cowsayBox.showText("");
        // });
        if(attachinst.isBase) return;
        baobaostack.Add(attachinst);
        sumStackN++;
        int prevMaxStackN = maxStackN;
        maxStackN = Mathf.Max(baobaostack.Count, maxStackN);
        float maxHeight = maxStackN * EachHeight;
        if(maxStackN > prevMaxStackN)
        {
            if(maxStackN == 2)
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]没错，就是这样！[/wave][/font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxStackN == 4)
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]的身体是非常非常重的哦......[/font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxStackN == 5)
            {
                string[] str = {
                    "[font_size=24]一旦摔到地上，\n就会引发[shake rate=20.0 level=5 connected=1][color=red][font_size=36]地震[/font_size][/color][/shake]......[/font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Die;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxStackN == 6)
            {
                if(FailN == 0)
                {
                    string[] str = {
                        "[font_size=24][shake rate=20.0 level=5 connected=1]海豹塔会因为地震而[color=red]崩塌[/color]！[/shake][font_size]", 
                        "placeholder"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                    attachinst.cowsayBox.showText(str[world.Language]);
                    world.regTimer(5f, ()=>{attachinst.cowsayBox.showText("");});
                }
                else
                {
                    string[] str = {
                        "[font_size=24][shake rate=20.0 level=5 connected=1]发生地震后，海豹塔会[color=red]崩塌[/color]！就像刚才那样！[/shake][font_size]", 
                        "placeholder"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                    attachinst.cowsayBox.showText(str[world.Language]);
                    world.regTimer(5f, ()=>{attachinst.cowsayBox.showText("");});
                }
            }
            if(maxStackN == 7)
            {
                string[] str = {
                    "[font_size=18]所以请务必好好抚摸[wave amp=24.0 freq=8.0 connected=1][color=blue]每一只豹豹[/color][/wave]！[font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(FailN > 0 && !firstfail)
            {
                firstfail = true;
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]不要灰心，摔倒了还能继续~[/wave][font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxHeight >= 0.3f * TargetY - 2 * EachHeight && maxHeight < 0.3f * TargetY - EachHeight)
            {
                string[] str = {
                    "[font_size=24]准备好[wave amp=24.0 freq=8.0 connected=1][color=red]约会[/color][/wave]了吗？[/font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxHeight >= 0.3f * TargetY - EachHeight && maxHeight < 0.3f * TargetY)
            {
                string[] str = {
                    "[font_size=24]和[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]的[wave amp=24.0 freq=8.0 connected=1][color=red]初次约会[/color][/wave]要开始了哦！[/font_size]", 
                    "placeholder"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
        }
        prevFailN = FailN;
    }
    void FirstGetTime(int hour, int minute)
    {
        if(hour == 8 && minute == 45)
        {
            string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]不好，主人要回来了！！\n我要赶紧躲起来！[/wave][/font_size]", 
                    "placeholder"
                };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
        }
        if(hour == 8 && minute == 55)
        {
            string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]忘了说了，躲起来的时候，豹豹们也依然会掉下来哦！！[/wave][/font_size]", 
                    "placeholder"
                };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
        }
        world.FirstGetTime(hour, minute);
    }
}
