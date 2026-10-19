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
    [Export] public Sprite2D UpArrow;
    [Export] public Sprite2D DownArrow;
    [Export] public PackedScene BaobaoScene;
    [Export] public AnimatedSprite2D Vignette;
    [Export] public float YOffset = 36f;
    [Export] public float TargetY = 2.0f * 2010f;
    [Export] public bool IsGaming = false;
    [Export] public bool IsCurrentGaming = false;
    [Export] public bool IsTiming = false;
    [Export] public float MaxGameTime = 400;
    public float GameTimer = 0.0f;
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
    float timespd = 1;
    float fallspdx = 1;

    public int maxStackN = 0;
    public int curStackN = 0;
    public int prevStackN = 0;
    public int sumStackN = 0;
    public int FailN = 0;
    public int EarthQuakeN = 0;
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
        Vignette.SelfModulate = new Color(1,1,1,Mathf.Lerp(Vignette.SelfModulate.A, IsCurrentGaming ? 1.0f : 0.0f, 8f * (float)delta));
        // TimeLabel.Visible = (int)(world.time * 2) % 2 == 0;
        curGameAlpha = Mathf.Lerp(curGameAlpha, targetGameAlpha, spdGameAlpha * (float)delta);
        Lines.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        GameUI.Modulate = new Color(1.0f, 1.0f, 1.0f, curGameAlpha);
        Vector2 MousePos = GetGlobalMousePosition();
        if(IsGaming)
        {
            if(IsTiming)
            {
                GameTimer += (float)delta * timespd;
                if(normalized_minute != prev_minute)
                {
                    FirstGetTime(normalized_hour, normalized_minute);
                }
                prev_minute = normalized_minute;
            }
            int scrolld = 0;
            if(MousePos.Y < MouseYScrollUp)
            {
                UpArrow.Scale = UpArrow.Scale.Lerp(Vector2.One * 0.5f, 16f * (float)delta);
                if(Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    UpArrow.Scale = UpArrow.Scale.Lerp(Vector2.One * 0.6f, 16f * (float)delta);
                    scrolld = 1;
                    doAutoScrollUp = false;
                }
            }
            else
            {
                UpArrow.Scale = UpArrow.Scale.Lerp(Vector2.One * 0.4f, 16f * (float)delta);
            }
            if(MousePos.Y > MouseYScrollDown)
            {
                DownArrow.Scale = DownArrow.Scale.Lerp(Vector2.One * 0.5f, 16f * (float)delta);
                if(Input.IsMouseButtonPressed(MouseButton.Left) && (!world.selectBox.OptionActive || world.selectBox.Hovering == -1))
                {
                    DownArrow.Scale = DownArrow.Scale.Lerp(Vector2.One * 0.6f, 16f * (float)delta);
                    scrolld = -1;
                    doAutoScrollUp = false;
                }
            }
            else
            {
                DownArrow.Scale = DownArrow.Scale.Lerp(Vector2.One * 0.4f, 16f * (float)delta);
            }
            targetYScroll += scrolld * MouseScrollSpeed;
            if(Input.IsMouseButtonPressed(MouseButton.WheelUp))
            {
                targetYScroll += 10;
                Debug.WriteLine("up");
            }
            else if(Input.IsMouseButtonPressed(MouseButton.WheelDown))
                targetYScroll -= 10;

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
                float sinalpha = Mathf.Sin(world.time) * 0.2f + 0.6f;
                float upalpha = UpArrow.Modulate.A;
                float downalpha = DownArrow.Modulate.A;
                upalpha = Mathf.Lerp(upalpha, NextTargetGamePos.Y - targetYScroll > 5f ? sinalpha : 0, 8f * (float)delta);
                downalpha = Mathf.Lerp(downalpha, targetYScroll > 5f ? sinalpha : 0, 8f * (float)delta);
                UpArrow.Modulate = new Color(0,0,0,upalpha);
                DownArrow.Modulate = new Color(0,0,0,downalpha);

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
                        instance.FallingSpeed = 60f * fallspdx;
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
        if(normalized_hour == 11 && normalized_minute >= 45) redText = true;
        if(normalized_hour == 14 && normalized_minute >= 45) redText = true;
        if(normalized_hour == 17 && normalized_minute >= 45) redText = true;
        TimeLabel.Modulate = redText ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
        TimeLabelOut.Text = time_text;
    }
    public Vector2 ToWorldPos(Vector2 pos) => -pos;
    public Vector2 ToGamePos(Vector2 pos) => -pos;
    public void StartGame()
    {
        IsGaming = true;
        IsTiming = true;
        world.textBox2.Following = world.mainbaobao;
        world.textBox2.FollowingOffset = new Vector2(208f, 240f) * world.mainbaobao.GlobalScale;
        string[] str = {
            "你需要[wave amp=24.0 freq=8.0 connected=1][color=red]抚摸[/color][/wave]每一只掉下来的[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]，把他们放在我的头上！", 
            "You need to [wave amp=24.0 freq=8.0 connected=1][color=red]pet[/color][/wave] every [wave amp=24.0 freq=8.0 connected=1][color=blue]Baobao[/color][/wave] that falls down, and put them on my head!"
        };
        world.textBox2.showText(str[world.Language]);
        world.regTimer(5f, ()=>{world.textBox2.showText("");});
    }
    public void OnFail(Baobao failinst)
    {
        world.SetScreenShake(16f, 0.2f);
        SummonTimer = Mathf.Max(SummonTimer, 2f);
        if(FailN == 0)
        {
            string[] str = {
                "[shake rate=20.0 level=5 connected=1]呀啊，地震了...[/shake]！", 
                "[shake rate=20.0 level=5 connected=1]Whoa, an earthquake...[/shake]!"
            };
            failinst.cowsayBox.showText(str[world.Language]);
            world.regTimer(2f, ()=>{failinst.cowsayBox.showText("");});
        }
        FailN++;
        EarthQuakeN++;
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
        curStackN = baobaostack.Count;
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
        int prevStackN = curStackN;
        maxStackN = Mathf.Max(baobaostack.Count, maxStackN);
        curStackN = baobaostack.Count;
        float maxHeight = maxStackN * EachHeight;
        float curHeight = curStackN * EachHeight;
        if(maxStackN > prevMaxStackN)
        {
            if(maxStackN == 2)
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]没错，就是这样！[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]That's it, just like that![/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            if(maxStackN == 4)
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]的身体是非常非常重的哦......[/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=blue]Baobao[/color][/wave]'s body is really, really heavy......[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
                world.selectBox.CancelSelection();
            }
            if(maxStackN == 5)
            {
                string[] str = {
                    "[font_size=24]一旦摔到地上，\n就会引发[shake rate=20.0 level=5 connected=1][color=red][font_size=36]地震[/font_size][/color][/shake]......[/font_size]", 
                    "[font_size=24]If one falls to the ground,\nit will trigger an [shake rate=20.0 level=5 connected=1][color=red][font_size=36]earthquake[/font_size][/color][/shake]......[/font_size]"
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
                        "[font_size=24][shake rate=20.0 level=5 connected=1]The seal tower will [color=red]collapse[/color] from the earthquake![/shake][font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                    attachinst.cowsayBox.showText(str[world.Language]);
                    world.regTimer(5f, ()=>{attachinst.cowsayBox.showText("");});
                }
                else
                {
                    string[] str = {
                        "[font_size=24][shake rate=20.0 level=5 connected=1]发生地震后，海豹塔会[color=red]崩塌[/color]！就像刚才那样！[/shake][font_size]", 
                        "[font_size=24][shake rate=20.0 level=5 connected=1]After an earthquake, the seal tower will [color=red]collapse[/color]! Just like what just happened![/shake][font_size]"
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
                    "[font_size=18]So please make sure to pet [wave amp=24.0 freq=8.0 connected=1][color=blue]every single Baobao[/color][/wave]! [font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
        }    
        if(curStackN > prevStackN)
        {
            if(InLinePlus(curHeight, 0.3f, -2))
            {
                string[] str = {
                    "[font_size=24]好感度快要达到[wave amp=24.0 freq=8.0 connected=1][color=red]3点[/color][/wave]了！[/font_size]", 
                    "[font_size=24]Your affection is about to reach [wave amp=24.0 freq=8.0 connected=1][color=red]3 points[/color][/wave]![/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, -1))
            {
                string[] str = {
                    "[font_size=24]达到[wave amp=24.0 freq=8.0 connected=1][color=red]3点好感度[/color][/wave]，我就和你分享一些[wave amp=24.0 freq=8.0 connected=1][color=blue]秘密[/color][/wave]！[/font_size]", 
                    "[font_size=24]Reach [wave amp=24.0 freq=8.0 connected=1][color=red]3 affection points[/color][/wave], and I'll share some [wave amp=24.0 freq=8.0 connected=1][color=blue]secrets[/color][/wave] with you![/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 0))
            {
                string[] str = {
                    "[font_size=24]好耶，达到[wave amp=24.0 freq=8.0 connected=1][color=red]3点好感度！[/color][/wave][/font_size]", 
                    "[font_size=24]Yay, [wave amp=24.0 freq=8.0 connected=1][color=red]3 affection points![/color][/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
                ClearBaobaos();
                fallspdx = 1.25f;
                timespd = 1.25f;
            }
            else if(InLinePlus(curHeight, 0.3f, 1))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]猫猫酱[/color][/wave]有一个只会告诉我的[wave amp=24.0 freq=8.0 connected=1][color=blue]秘密[/color][/wave]...[/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]Nyako-chan[/color][/wave] has a [wave amp=24.0 freq=8.0 connected=1][color=blue]secret[/color][/wave] she only tells me...[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Normal;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 2))
            {
                string[] str = {
                    "[font_size=24]虽然她平时在家总是把自己打扮得像个小孩子一样，[/font_size]", 
                    "[font_size=24]Even though she always dresses up like a kid at home,[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 3))
            {
                string[] str = {
                    "[font_size=24]但她其实是一只每天上班的[wave amp=24.0 freq=8.0 connected=1][color=red]社畜[/color][/wave]哦...[/font_size]", 
                    "[font_size=24]she's actually a [wave amp=24.0 freq=8.0 connected=1][color=red]corporate drone[/color][/wave] who goes to work every day...[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 4))
            {

                string[][] options = {
                    new string[]{
                        "她很讨厌上班吧？"
                    },
                    new string[]{
                        "She must hate her job, right?"
                    }
                };
                world.selectBox.ShowSelection("hate_work", options[world.Language], (name, selection) =>
                {
                    string[] str = {
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]是哦，她经常在上班期间和我抱怨。[/wave][/font_size]", 
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Yeah, she complains to me about it all the time at work.[/wave][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.3f, 5))
            {
                world.selectBox.CancelSelection();
                world.regTimer(1.0f,()=>{
                    string[] str = {
                        "[font_size=24]她还经常说想回到[wave amp=24.0 freq=8.0 connected=1][color=red]童年时代[/color][/wave]什么的。[/font_size]", 
                        "[font_size=24]She also keeps saying she wants to go back to [wave amp=24.0 freq=8.0 connected=1][color=red]her childhood[/color][/wave] and stuff.[/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.3f, 6))
            {
                string[] str = {
                    "[font_size=24]那个时候[wave amp=24.0 freq=8.0 connected=1]没有内卷，没有永远跟不上的信息流，没有工作被AI替代的烦恼...[/wave][/font_size]", 
                    "[font_size=24]Back then there was [wave amp=24.0 freq=8.0 connected=1]no rat race, no endless feed you could never keep up with, no worries about being replaced by AI...[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 7))
            {
                string[] str = {
                    "[font_size=24]当然那个时候也没有我这样[wave amp=24.0 freq=8.0 connected=1]智能[/wave]的电子宠物。[/font_size]", 
                    "[font_size=24]Of course, back then there were no [wave amp=24.0 freq=8.0 connected=1]smart[/wave] virtual pets like me either.[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Die;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.3f, 8))
            {
                string[][] options = {
                    new string[]{
                        "[wave amp=24.0 freq=8.0 connected=1]她讨厌现在这个时代吧。[/wave]"
                    },
                    new string[]{
                        "[wave amp=24.0 freq=8.0 connected=1]She must hate this era.[/wave]"
                    }
                };
                world.selectBox.ShowSelection("hate_time", options[world.Language], (name, selection) =>
                {
                    string[] str = {
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]是吧，所以才不会喜欢我这个样子。[/wave][/font_size]", 
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Right, that's why she'd never like me the way I am.[/wave][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.3f, 9))
            {
                world.selectBox.CancelSelection();
                world.regTimer(0.5f,()=>{
                    string[] str = {
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]可是我只是想被她[color=blue]抚摸[/color]...[/wave][/font_size]", 
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1]But I just want her to [color=blue]pet[/color] me...[/wave][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Die;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 0))
            {
                string[] str = {
                    "[font_size=24]达到[wave amp=24.0 freq=8.0 connected=1][color=red]6点好感度！[/color][/wave][/font_size]", 
                    "[font_size=24]Reached [wave amp=24.0 freq=8.0 connected=1][color=red]6 affection points![/color][/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
                ClearBaobaos();
            }
            else if(InLinePlus(curHeight, 0.6f, 1))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]嘿嘿，[color=blue]豹豹[/color]变得好喜欢你了...[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Hehe, [color=blue]Baobao[/color] has really started to like you...[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
                world.regTimer(1.0f, () => {
                    string[][] options = {
                        new string[]{
                            "诶，好突然？！",
                            "我也喜欢你！"
                        },
                        new string[]{
                            "Wait, that's so sudden?!",
                            "I like you too!"
                        }
                    };
                    world.selectBox.ShowSelection("baobao_loves_you", options[world.Language], (name, selection) => {
                        if(selection == 1)
                        {
                            string[] str = {
                                "[font_size=24][wave amp=24.0 freq=8.0 connected=1][font_size=36]谢谢你！！[/font_size]\n你的[color=blue]抚摸[/color]超级舒服的！[/wave][/font_size]", 
                                "[font_size=24][wave amp=24.0 freq=8.0 connected=1][font_size=36]Thank you!![/font_size]\nYour [color=blue]petting[/color] feels amazing![/wave][/font_size]"
                            };
                            attachinst.faceStatus = Baobao.FaceStatus.Happy;
                            attachinst.cowsayBox.showText(str[world.Language]);
                        }
                        else
                        {
                            string[] str = {
                                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]因为好久没有被这样[color=blue]抚摸[/color]了哦！[/wave][/font_size]", 
                                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]It's because I haven't been [color=blue]petted[/color] like that in so long![/wave][/font_size]"
                            };
                            attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                            attachinst.cowsayBox.showText(str[world.Language]);
                        }
                    });
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 2))
            {
                world.regTimer(0.5f, () => {
                    string[] str = {
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]6点好感度[/color]可是需要超级细心的照顾才能达到的啊。[/wave][/font_size]", 
                        "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]6 affection points[/color] takes super careful attention to reach.[/wave][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 3))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]猫猫酱[/color]从来没有这么细心照顾过我...[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1][color=red]Nyako-chan[/color] has never taken such good care of me...[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Normal;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 4))
            {
                string[][] options = {
                    new string[]{
                        "其实是因为她很忙吧。"
                    },
                    new string[]{
                        "I think it's because she's busy."
                    }
                };
                world.selectBox.ShowSelection("nyako_is_busy", options[world.Language], (name, selection) => {
                    string[] str = {
                        "[font_size=24][shake rate=20.0 level=5 connected=1]她从来不[color=blue]抚摸[/color]我诶！[/shake][/font_size]", 
                        "[font_size=24][shake rate=20.0 level=5 connected=1]She never [color=blue]pets[/color] me at all![/shake][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Die;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 5))
            {
                world.regTimer(1.0f, () => {
                    string[] str = {
                        "[font_size=24][shake rate=20.0 level=5 connected=1]感觉自己只是被当成了一个象征她童年的摆设...[/shake][/font_size]", 
                        "[font_size=24][shake rate=20.0 level=5 connected=1]I feel like I'm just a decoration symbolizing her childhood...[/shake][/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 6))
            {
                string[][] options = {
                    new string[]{
                        "她平时和你怎样相处？"
                    },
                    new string[]{
                        "How does she treat you normally?"
                    }
                };
                world.selectBox.ShowSelection("nyako_is_busy", options[world.Language], (name, selection) => {
                    string[] str = {
                        "[font_size=24]她一直都在和我抱怨[shake rate=20.0 level=5 connected=1]工作[/shake]...[/font_size]", 
                        "[font_size=24]She's always complaining to me about [shake rate=20.0 level=5 connected=1]work[/shake]...[/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.Die;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 7))
            {
                world.regTimer(0.5f, () => {
                    string[] str = {
                        "[font_size=24]平常把我带在身上，[/font_size]", 
                        "[font_size=24]She usually carries me around,[/font_size]"
                    };
                    attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                    attachinst.cowsayBox.showText(str[world.Language]);
                });
            }
            else if(InLinePlus(curHeight, 0.6f, 8))
            {
                string[] str = {
                    "[font_size=24]上班时把我拿出来悄悄说老板坏话。[/font_size]", 
                    "[font_size=24]At work she takes me out and quietly badmouths her boss.[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Die;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 9))
            {
                string[] str = {
                    "[font_size=24][shake rate=20.0 level=5 connected=1]还会经常对着我哭...[/shake][/font_size]", 
                    "[font_size=24][shake rate=20.0 level=5 connected=1]And she often cries in front of me...[/shake][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Waaa;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 10))
            {
                string[] str = {
                    "[font_size=24][shake rate=20.0 level=5 connected=1]可是如果我安慰她，又会被她说讨厌。[/shake][/font_size]", 
                    "[font_size=24][shake rate=20.0 level=5 connected=1]But if I comfort her, she says she hates that.[/shake][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 10))
            {
                string[] str = {
                    "[font_size=24][shake rate=20.0 level=5 connected=1]她完全不想让我用我们[color=blue]人工智能[/color]的方式安慰她...[/shake][/font_size]", 
                    "[font_size=24][shake rate=20.0 level=5 connected=1]She doesn't want me to comfort her in our [color=blue]AI[/color] way at all...[/shake][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Die;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 11))
            {
                attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
            }
            else if(InLinePlus(curHeight, 0.6f, 12))
            {
                string[] str = {
                    "[font_size=24]诶，你是说，我生成的[shake rate=20.0 level=5 connected=1]安慰[/shake]反而起到了反作用？[/font_size]", 
                    "[font_size=24]Huh, you mean the [shake rate=20.0 level=5 connected=1]comfort[/shake] I generated actually made things worse?[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Normal;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 13))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]“不会说话的电子玩具的陪伴，反而更有生命感？”[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]\"The company of a silent electronic toy feels more alive?\"[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Normal;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 14))
            {
                string[] str = {
                    "[font_size=24]呜呜，我不是很懂啦...[/font_size]", 
                    "[font_size=24]Waaah, I don't really get it...[/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 15))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]不过我大概知道怎么做了。[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]But I think I roughly know what to do now.[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 16))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]我会乖乖做一只电子宠物，但你要让她以后多[color=blue]摸摸我[/color]哦。[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]I'll be a good virtual pet, but you have to get her to [color=blue]pet me[/color] more from now on.[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 0.6f, 17))
            {
                string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]嘿嘿，[color=red]喜欢你[/color]。[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Hehe, [color=red]I like you[/color].[/wave][/font_size]"
                };
                attachinst.faceStatus = Baobao.FaceStatus.Happy;
                attachinst.cowsayBox.showText(str[world.Language]);
            }
            else if(InLinePlus(curHeight, 1.0f, 0))
            {
                world.TriggerHappyEnd();
            }
        }        
        if(FailN > 0 && !firstfail)
        {
            firstfail = true;
            string[] str = {
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]摔倒了就要从头再来了呢。[/wave][font_size]", 
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]If you fall, you have to start over from the beginning.[/wave][font_size]"
            };
            attachinst.faceStatus = Baobao.FaceStatus.NormalBlink;
            attachinst.cowsayBox.showText(str[world.Language]);
        }
        prevFailN = FailN;
    }
    bool InLinePlus(float height, float line, float plus) => height >= line * TargetY + plus * EachHeight && height <= line * TargetY + (plus + 1) * EachHeight;
    void FirstGetTime(int hour, int minute)
    {
        if(hour == 8 && minute == 45)
        {
            string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]不好，主人要回来了！！\n我要赶紧躲起来！[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Uh oh, my owner's coming back!!\nI have to hide![/wave][/font_size]"
                };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
            stacktop.SetShake(3f, 1f);
        }
        if(hour == 8 && minute == 55)
        {
            string[] str = {
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]忘了说了，躲起来的时候，豹豹们也依然会掉下来哦！！[/wave][/font_size]", 
                    "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Oh, I forgot to mention: even while I'm hiding, Baobaos will still keep falling!![/wave][/font_size]"
                };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
            stacktop.SetShake(3f, 0.5f);
        }
        if(hour == 11 && minute == 45)
        {
            string[] str = {
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]不好，她又要回来了！[/wave][/font_size]", 
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Uh oh, she's coming back again![/wave][/font_size]"
            };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
            stacktop.SetShake(3f, 1f);
        }
        if(hour == 14 && minute == 55)
        {
            string[] str = {
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]快躲起来！[/wave][/font_size]", 
                "[font_size=24][wave amp=24.0 freq=8.0 connected=1]Hide, quick![/wave][/font_size]"
            };
            stacktop.faceStatus = Baobao.FaceStatus.Waaa;
            stacktop.cowsayBox.showText(str[world.Language]);
            stacktop.SetShake(3f, 1f);
        }
        if(hour == 17 && minute == 45)
        {
            string[] str = {
                "[font_size=24][shake rate=20.0 level=5 connected=1]再见啦，我的朋友...\n这次躲起来之后还能再见面吗？[/shake][/font_size]", 
                "[font_size=24][shake rate=20.0 level=5 connected=1]Goodbye, my friend...\nWill we meet again after I hide this time?[/shake][/font_size]"
            };
            stacktop.faceStatus = Baobao.FaceStatus.NormalBlink;
            stacktop.cowsayBox.showText(str[world.Language]);
            stacktop.SetShake(2f, 0.5f);
        }
        world.FirstGetTime(hour, minute);
    }
    void ClearBaobaos()
    {
        foreach(Node child in GameOrigin.GetChildren())
        {
            Debug.WriteLine("Debug!");
            if(child is Baobao bb)
            {
                Debug.WriteLine("isbb!");
                if(!bb.Attached)
                {
                    Debug.WriteLine("makeclear!");
                    bb.MakeClearing();
                }
            }
        }
    }
}
