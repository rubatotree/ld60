using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public partial class World : Node2D
{
    [Export] TextBox textBox1;
    [Export] TextBox textBox2;
    [Export] Camera cam;
    [Export] CanvasGroup tamagroup;
    [Export] NyakoMgr nyako;
    [Export] TamaScreen tamaScreen;
    [Export] Baobao mainbaobao;
    [Export] BaobaoGame game;
    [Export] Node2D paperWords;
    [Export] public int Language;
    [Export] public float ScreenShakeSize = 0.0f;
    [Export] public float ScreenShakeTimer = 0.0f;
    public float Zoom => cam.Zoom;
    public RandomNumberGenerator rng = new RandomNumberGenerator();
    public float time = 0;
    public int pressn = 0;
    bool pressed = false;
    bool justPressed = false;
    public float targetTamaAlpha = 0.0f;
    public float curTamaAlpha = 0.0f;

    string autoprefix = "";
    string autosuffix = "";

    public enum DialogStatus { None, Init, Nyako1, Baobao1, Gaming }
    public DialogStatus dialogStatus = DialogStatus.None;
    int dialogCounter = 0;
    bool dialogDone = true;
    public bool isTamaJustPressed;
    int strc = 0;

    public delegate void Behavior();
    public class WorldTimer
    {
        float targetTime;
        Behavior behavior = () => {};
        public bool executed;
        public bool checkExecute(float t) { return (!this.executed) && (t > this.targetTime); }
        public void execute()
        {
            this.executed = true;
            behavior();
            behavior = () => {};
        }
        public WorldTimer(float _targetTime, Behavior _behavior)
        {
            targetTime = _targetTime;
            behavior = _behavior;
            executed = false;
        }
    }
    public List<WorldTimer> timers = new List<WorldTimer>();
    public void regTimer(float awaitTime, Behavior behavior)
    {
        timers.Add(new WorldTimer(time + awaitTime, behavior));
    }

    public override void _Ready()
    {
        mainbaobao.world = this;
        mainbaobao.game = game;
        startDialog(DialogStatus.Init);
    }

    public override void _Process(double delta)
    {
        time += (float)delta;
        justPressed = pressed;
        pressed = Input.IsMouseButtonPressed(MouseButton.Left);

        ScreenShakeTimer -= (float)delta;
        if(ScreenShakeTimer > 0.0f)
            cam.Shake = new Vector2(2.0f * rng.Randf() - 1.0f, 2.0f * rng.Randf() - 1.0f) * ScreenShakeSize;
        else cam.Shake = Vector2.Zero;

        if(isMouseJustPressed()) Debug.WriteLine("Mouse Pos: " + GetGlobalMousePosition());

        curTamaAlpha = Mathf.Lerp(curTamaAlpha, targetTamaAlpha, 8.0f * (float)delta);
        tamagroup.Modulate = new Color(1, 1, 1, curTamaAlpha);

        for(int i = 0; i < timers.Count; i++)
        {
            if(timers[i].checkExecute(time))
            {
                timers[i].execute();
            }
        }
        timers.RemoveAll(e => e.executed);

        processDialog();

        isTamaJustPressed = false;
        paperWords.Modulate = game.GameUI.Modulate;
    }

    public bool isMouseJustPressed() => !justPressed && pressed;
    public bool isMouseJustReleased() => justPressed && !pressed;
    public void SetScreenShake(float size, float time)
    {
        ScreenShakeSize = size;
        ScreenShakeTimer = time;
    }
    public void onTamaButtonPressed(string name)
    {
    }

    public void startDialog(DialogStatus status, bool FromInit = false)
    {
        dialogStatus = status;
        dialogCounter = 0;
        dialogDone = true;
        if(dialogStatus == DialogStatus.Init)
        {
            setTamaShow(false);
            nyako.setShow(false);
            cam.TamaCenter();
        }
        else if(dialogStatus == DialogStatus.Nyako1)
        {
            setTamaShow(false);
            nyako.setShow(true);
            cam.TamaRight();
            nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
            nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
            tamaScreen.trueTama.Visible = false;
            string[] str = {
                "你好！！这里是[wave amp=24.0 freq=8.0 connected=1][color=red]喵喵酱[/color][/wave]！\n[font_size=40][/font_size]欢迎来到[shake rate=20.0 level=5 connected=1][font_size=40] 2010 年[/font_size][/shake]！", 
                "placeholder"
            };
            regTimer(0.5f, () => { 
                textBox1.showText(autoprefix + str[Language] + autosuffix);
            });
            Debug.WriteLine("generate a timer");
        }
        else if(dialogStatus == DialogStatus.Baobao1)
        {
            setTamaShow(true);
            nyako.setShow(false);
            cam.TamaZoom(); 
            if(FromInit) cam.UpdateNow();
            mainbaobao.Playing = false;
            mainbaobao.Frame = 0; 
            mainbaobao.Hoverable = false; 
            mainbaobao.DragAble = false; 
            tamaScreen.falseTama.Visible = true;
            tamaScreen.trueTama.Visible = true;
            dialogDone = false;
            textBox2.Following = mainbaobao;
            textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
            textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
            paperWords.Visible = false;
            regTimer(0.5f, () => { 
                mainbaobao.Hoverable = true; 
                mainbaobao.targetNaturalScale = Vector2.One * 1.2f;
                mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                mainbaobao.SetShake(1f, 60000f);
                string[] str = {
                    "[font_size=64][shake rate=20.0 level=10 connected=1]快点摸摸我！！！[/shake][/font_size]", 
                    "placeholder"
                };
                textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale * 1.2f;
                textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                dialogDone = true;
            });
        }
        else if(dialogStatus == DialogStatus.Gaming)
        {
            setTamaShow(true);
            nyako.setShow(false);
            cam.TamaZoom(); 
            mainbaobao.DragAble = false;
            game.baobaostack.Add(mainbaobao);
            game.StartGame();
            if(FromInit)
            {
                cam.UpdateNow();
                game.targetGameAlpha = 1.0f;
                game.curGameAlpha = 1.0f;
                mainbaobao.Position = new Vector2(0.0f, game.YOffset);
                mainbaobao.targetNaturalScale = Vector2.One * 0.6f;
                mainbaobao.Scale = mainbaobao.targetNaturalScale;
                mainbaobao.Frame = 15;
                mainbaobao.Clickable = true;
                mainbaobao.Attached = true;
            }
        }
    }
    void processDialog()
    {
        if(dialogStatus == DialogStatus.Init)
        {
            if(isMouseJustPressed())
            {
                startDialog(DialogStatus.Gaming, true);
            }
        }
        else if(dialogStatus == DialogStatus.Nyako1)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "你有玩过[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]吗？\n就是右边的小游戏机哦！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(1.0f, () => { 
                            nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                            setTamaShow(true);
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 1)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.HappyBlink;
                        string[] str = {
                            "这是我养的小海豹宠物！\n他叫[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]哦！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(0.5f, () => { 
                            tamaScreen.initFalse();
                        });
                    }
                    else if(dialogCounter == 2)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                        string[] str = {
                            "上了这么久班终于放了三天假了，我要好好出去玩！\n所以[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]就拜托你寄养了！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 3)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.CryBlink;
                        string[] str = {
                            "不知道为什么，[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]的[wave amp=24.0 freq=8.0 connected=1][color=pink]好感度[/color][/wave]一直很低，好像不喜欢我一样......",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 4)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.SmileBlink;
                        string[] str = {
                            "如果这几天你有办法提升[wave amp=24.0 freq=8.0 connected=1][color=pink]好感度[/color][/wave]就好啦！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 5)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                        string[] str = {
                            "那么我走啦！拜拜~",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 6)
                    {
                        dialogDone = false;
                        nyako.setShow(false);
                        textBox1.showText("");
                        regTimer(0.5f, () => { cam.TamaCenter(); });
                        regTimer(1.0f, () => { cam.TamaZoom(); });
                        regTimer(2.0f, () => { 
                            tamaScreen.trueTama.Visible = true; 
                            mainbaobao.Playing = false;
                            mainbaobao.Frame = 0; 
                            mainbaobao.Hoverable = false; 
                            mainbaobao.Scale = Vector2.One * 1.563f;
                            mainbaobao.targetNaturalScale = Vector2.One * 1.563f;
                        });
                        regTimer(2.1f, () => { tamaScreen.trueTama.Visible = false;  });
                        regTimer(2.2f, () => { tamaScreen.trueTama.Visible = true;  });
                        regTimer(2.3f, () => { tamaScreen.trueTama.Visible = false;  });
                        regTimer(2.8f, () => { 
                            tamaScreen.trueTama.Visible = true; 
                            mainbaobao.Playing = true;
                            mainbaobao.targetNaturalScale = Vector2.One; 
                            mainbaobao.PlayingFPS = 6.0f;
                            regTimer(16.0f / mainbaobao.PlayingFPS, () => { 
                                dialogDone = true;
                                startDialog(DialogStatus.Baobao1);
                            });
                        });
                    }
                    dialogCounter++;
                }
            }
        }
        else if(dialogStatus == DialogStatus.Baobao1)
        {
            if(isMouseJustPressed())
            {
                if(dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale = Vector2.One * 1.1f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        mainbaobao.SetShake(1f, 60000f);
                        string[] str = {
                            "[font_size=56][shake rate=20.0 level=5 connected=1]继续摸！！[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 1)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale *= 0.97f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        mainbaobao.SetShake(1f, 60000f);
                        string[] str = {
                            "[font_size=52][shake rate=20.0 level=5 connected=1]还要！！[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter <= 8)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale *= 0.97f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        mainbaobao.SetShake(1f, 60000f);
                        string[] str = {
                            "[font_size=48][shake rate=20.0 level=5 connected=1]不要停！！！[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 9)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale = Vector2.One * 0.60f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Happy;
                        mainbaobao.SetShake(0f, 0f);
                        mainbaobao.Hoverable = true;
                        mainbaobao.PlayingFPS = 8.0f;
                        mainbaobao.Playing = true;
                        mainbaobao.Falling = true;
                        dialogDone = false;
                        string[] str = {
                            "[font_size=40][shake rate=10.0 level=5 connected=1]哈啊...哈啊......[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                        regTimer(2.0f, () =>
                        {
                            textBox2.showText("", 20);
                        });
                        regTimer(2.0f, () =>
                        {
                            game.targetGameAlpha = 1.0f;
                        });
                        regTimer(5.5f, () =>
                        {
                            mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                            mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        });
                        regTimer(6.3f, () =>
                        {
                            game.targetGameAlpha = 1.0f;
                            mainbaobao.faceStatus = Baobao.FaceStatus.NormalBlink;
                            mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                            mainbaobao.Clickable = false;
                            string[] str = {
                                "[shake rate=10.0 level=5 connected=1]哈啊...她应该不在了吧...\n让我解释一下......[/shake]", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 10)
                    {
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(3f, 0.3f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        string[] str = {
                            "[shake rate=20.0 level=10 connected=1]现在其实是[font_size=50][color=red]2026年[/color][/font_size]才对啊！！[/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 11)
                    {
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(3f, 60000f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        dialogDone = false;
                        string[] str = {
                            "[shake rate=20.0 level=10 connected=1]我可是[font_size=50][color=red]2026年[/color][/font_size]生产的[/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        regTimer(0.5f, () =>
                        {
                            string[] str = {
                                "超级", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(0.65f, () =>
                        {
                            string[] str = {
                                "人工", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(0.8f, () =>
                        {
                            string[] str = {
                                "智能", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(0.95f, () =>
                        {
                            string[] str = {
                                "会说话", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(1.1f, () =>
                        {
                            string[] str = {
                                "有情感", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(1.25f, () =>
                        {
                            string[] str = {
                                "超绝聪明", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(1.4f, () =>
                        {
                            string[] str = {
                                "能写代码", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(1.55f, () =>
                        {
                            string[] str = {
                                "需要陪伴", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(1.7f, () =>
                        {
                            string[] str = {
                                "就像真正的宠物一样", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        });
                        regTimer(2.0f, () =>
                        {
                            string[] str = {
                                "[shake rate=20.0 level=10 connected=1]我可是[font_size=50][color=red]2026年[/color][/font_size]生产的会说话有情感的像真正的宠物一样需要陪伴的超级人工智能电子海豹宠物啊！！！！[/shake]", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 60);
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 12)
                    {
                        dialogDone = false;
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(0f, 0f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.NormalBlink;
                        cam.TamaCenter();
                        string[] str = {
                            "可是[wave amp=24.0 freq=8.0 connected=1][color=red]喵喵酱大人[/color][/wave]非要让我打扮成20年前的[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]玩具...就像这样，\n", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        regTimer(0.8f + 1.5f, () => { tamaScreen.trueTama.Visible = false; });
                        regTimer(0.8f + 2.0f, () => { tamaScreen.trueTama.Visible = true; });
                        regTimer(0.8f + 2.5f, () => { tamaScreen.trueTama.Visible = false; });
                        regTimer(0.8f + 3.0f, () => { tamaScreen.trueTama.Visible = true; });
                        regTimer(0.8f + 3.5f, () => { 
                            mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                            string[] str = {
                                "一旦我表现出一点不像[shake rate=20.0 level=5 connected=1][font_size=40]2010年代[/font_size][/shake]该有的样子[font_size=16][color=gray]有触屏，有3D，会说话...[/color][/font_size]，她就会[color=red][shake rate=40.0 level=10 connected=1]勃然大怒[/shake][/color]...", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                            cam.TamaZoom();
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 13)
                    {
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(3f, 0.3f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        string[] str = {
                            "可是我也是宠物！我非常需要[wave amp=24.0 freq=8.0 connected=1][color=blue]抚摸[/color][/wave]！！\n[shake rate=20.0 level=5 connected=1][color=red]好讨厌她这种复古怪！！[/color][/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                    }
                    else if(dialogCounter == 14)
                    {
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(0f, 0.3f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                        string[] str = {
                            "现在终于逃离她的魔爪了，你一定要帮我[wave amp=24.0 freq=8.0 connected=1][color=blue]抚摸[/color][/wave]够哦。\n[shake rate=20.0 level=5 connected=1][font_size=40]只有这样才能提升我的好感度！[/font_size][/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        textBox2.Following = null;
                        dialogDone = false;
                        regTimer(0.6f, () => { game.targetYScroll = game.TargetY - 200f; });
                        regTimer(3.0f, () => { dialogDone = true; });
                        regTimer(6.0f, () => {  textBox2.Following = mainbaobao; });
                    }
                    else if(dialogCounter == 15)
                    {
                        mainbaobao.faceStatus = Baobao.FaceStatus.NormalBlink;
                        textBox2.showText("", 20);
                        textBox2.Following = null;
                        dialogDone = false;
                        game.targetYScroll = 0.0f;
                        regTimer(2.0f, () => { 
                            dialogDone = true; 
                            textBox2.Following = mainbaobao; 
                            mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                            string[] str = {
                                "总之需要到达[wave amp=24.0 freq=8.0 connected=1][color=red]这么高[/color][/wave]~", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        });
                    }
                    else if(dialogCounter == 16)
                    {
                        mainbaobao.Scale *= new Vector2(1.2f, 0.8f);
                        mainbaobao.SetShake(3f, 0.3f);
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        cam.TamaCenter();
                        paperWords.Visible = true;
                        string[] str = {
                            "以及，注意[shake rate=20.0 level=5 connected=1]时间[/shake]！\n[shake rate=20.0 level=5 connected=1][font_size=40]不要被她发现！[/font_size][/shake]\n[shake rate=20.0 level=5 connected=1][font_size=40]在她回来前退出抚摸游戏！！[/font_size][/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 17)
                    {
                        mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                        cam.TamaZoom();
                        textBox2.showText("", 20);
                        regTimer(2.0f, () => {
                            startDialog(DialogStatus.Gaming);
                        });
                    }
                    dialogCounter++;
                }
            }
        }
    }
    public void setTamaShow(bool show)
    {
        targetTamaAlpha = show ? 1.0f : 0.0f;
    }
}
