using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public partial class World : Node2D
{
    [Export] public TextBox textBox1;
    [Export] public TextBox textBox2;
    [Export] public SelectBoxMgr selectBox;
    [Export] Camera cam;
    [Export] CanvasGroup tamagroup;
    [Export] NyakoMgr nyako;
    [Export] public TamaScreen tamaScreen;
    [Export] public Baobao mainbaobao;
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

    public enum DialogStatus { None, Init, Nyako1, Nyako2, Nyako3, Nyako4, Nyako5, Baobao1, HappyEnd, Gaming }
    public DialogStatus dialogStatus = DialogStatus.None;
    int dialogCounter = 0;
    bool dialogDone = true;
    public bool isTamaJustPressed;
    int strc = 0;
    public Dictionary<string, int> Selections = new Dictionary<string, int>();
    int tmpEarthQuakeN = 0;
    int tmpFailN = 0;
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
        }
        else if(dialogStatus == DialogStatus.Nyako2)
        {
            setTamaShow(true);
            nyako.setShow(true);
            cam.TamaRight();
            nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
            nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
            string[] str = {
                "哈啊，我回来了！", 
                "placeholder"
            };
            regTimer(0.5f, () => { 
                textBox1.showText(autoprefix + str[Language] + autosuffix);
            });
        }
        else if(dialogStatus == DialogStatus.Nyako3)
        {
            setTamaShow(true);
            nyako.setShow(true);
            cam.TamaRight();
            nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
            nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
            string[] str = {
                "哈啊，我回来了！", 
                "placeholder"
            };
            regTimer(0.5f, () => { 
                textBox1.showText(autoprefix + str[Language] + autosuffix);
            });
        }
        else if(dialogStatus == DialogStatus.Nyako4)
        {
            setTamaShow(true);
            nyako.setShow(true);
            cam.TamaRight();
            nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
            nyako.faceStatus = NyakoMgr.FaceStatus.SmileBlink;
            string[] str = {
                "一天的旅行终于结束了！", 
                "placeholder"
            };
            regTimer(0.5f, () => { 
                textBox1.showText(autoprefix + str[Language] + autosuffix);
            });
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
            if(!game.IsGaming)
            {
                game.baobaostack.Add(mainbaobao);
                game.StartGame();
            }
            cam.ZoomSpd = 4f;
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
                if(textBox1.Complete && dialogDone && !selectBox.OptionActive)
                {
                    if(dialogCounter == 0)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "你有玩过[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]吗？",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(1.0f, () =>
                        {
                            string[][] options = {
                                new string[]{
                                    "玩过",
                                    "没有"
                                },
                                new string[]{
                                    "placeholder",
                                    "placeholder"
                                }
                            };
                            selectBox.ShowSelection("played_or_not", options[Language]);
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 1)
                    {
                        dialogDone = true;
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        setTamaShow(true);
                        Debug.WriteLine(Selections["played_or_not"]);
                        switch(Selections["played_or_not"])
                        {
                        case 0:
                            {
                                string[] str = {
                                    "太好了！找到同好啦~给你看看我的[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]吧！",
                                    "placeholder"
                                };
                                nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                                textBox1.showText(autoprefix + str[Language] + autosuffix);
                                break;
                            }
                        case 1:
                            {
                                string[] str = {
                                    "右边的小游戏机就是[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]哦~\n这是[shake rate=20.0 level=5 connected=1]2010年[/shake]的女孩子之间最流行的[wave amp=24.0 freq=8.0 connected=1]电子宠物[/wave]游戏机！",
                                    "placeholder"
                                };
                                nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
                                textBox1.showText(autoprefix + str[Language] + autosuffix);
                                break;
                            }
                        }
                    }
                    else if(dialogCounter == 2)
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
                    else if(dialogCounter == 3)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                        string[] str = {
                            "上了这么久班终于放了一天假了，今天我要好好出去玩！\n所以[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]就拜托你寄养了！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 4)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.CryBlink;
                        string[] str = {
                            "不知道为什么，[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]的[wave amp=24.0 freq=8.0 connected=1][color=red]好感度[/color][/wave]一直很低，好像不喜欢我一样......",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 5)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.SmileBlink;
                        string[] str = {
                            "如果这几天你有办法提升[wave amp=24.0 freq=8.0 connected=1][color=red]好感度[/color][/wave]就好啦！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 6)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                        string[] str = {
                            "那么我走啦！拜拜~",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 7)
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
                                "一旦我表现出一点不像[shake rate=20.0 level=5 connected=1][font_size=40]2010年代[/font_size][/shake]该有的样子[font_size=28][color=gray]有触屏，有3D，会说话...[/color][/font_size]，她就会[color=red][shake rate=40.0 level=10 connected=1]勃然大怒[/shake][/color]...", 
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
                            "现在终于逃离她的魔爪了，你一定要一直[wave amp=24.0 freq=8.0 connected=1][color=blue]抚摸[/color][/wave]我哦。\n[shake rate=20.0 level=5 connected=1][font_size=40]只有这样才能提升我的好感度！[/font_size][/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        textBox2.Following = null;
                        dialogDone = false;
                        regTimer(0.6f, () => { game.targetYScroll = game.TargetY - 200f; });
                        regTimer(4.0f, () => {  textBox2.Following = mainbaobao; dialogDone = true; });
                    }
                    else if(dialogCounter == 15)
                    {
                        mainbaobao.faceStatus = Baobao.FaceStatus.NormalBlink;
                        textBox2.showText("", 20);
                        textBox2.Following = null;
                        dialogDone = false;
                        game.targetYScroll = 0.0f;
                        regTimer(2.0f, () => { 
                            textBox2.Following = mainbaobao; 
                            mainbaobao.faceStatus = Baobao.FaceStatus.Normal;
                            string[] str = {
                                "总之[wave amp=24.0 freq=8.0 connected=1][color=red]好感度[/color][/wave]需要到达[wave amp=24.0 freq=8.0 connected=1][color=red]这么高[/color][/wave]~", 
                                "placeholder"
                            };
                            textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                            dialogDone = true; 
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
                            "以及，注意[shake rate=20.0 level=5 connected=1]时间[/shake]！\n[shake rate=20.0 level=5 connected=1][font_size=40]不要被她发现！[/font_size][/shake]\n[shake rate=20.0 level=5 connected=1][font_size=40]在她回来前我会躲起来！！[/font_size][/shake]", 
                            "placeholder"
                        };
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);

                        dialogDone = false;
                        regTimer(2.0f, () => { 
                            dialogDone = true;
                        });
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
        else if(dialogStatus == DialogStatus.Nyako2)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        tmpEarthQuakeN = game.EarthQuakeN;
                        tmpFailN = game.FailN;
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "我的[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]好玩吗？",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(1.0f, () =>
                        {
                            string[][] options = {
                                new string[]{
                                    "好玩",
                                    "不好玩"
                                },
                                new string[]{
                                    "placeholder",
                                    "placeholder"
                                }
                            };
                            selectBox.ShowSelection("good_or_not", options[Language], (name, selection) =>
                            {
                                if(selection == 0)
                                {
                                    dialogCounter = 1;
                                }
                                else
                                {
                                    dialogCounter = 2;
                                }
                                dialogDone = true;
                            });
                        });
                    }
                    else if(dialogCounter == 1)
                    {
                        dialogDone = true;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "[wave amp=24.0 freq=8.0 connected=1]太好了！[color=blue]豹豹[/color]很可爱吧！[/wave]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 3;
                    }
                    else if(dialogCounter == 2)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Unhappy;
                        
                        string[] str = {
                            "[shake rate=20.0 level=5 connected=1][font_size=40]怎么这样！！[/font_size]\n[color=blue]豹豹[/color]可是我最喜欢最喜欢的宠物！！罚你回去重选！！[/shake]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix, 20);
                        regTimer(2.0f, () => { dialogDone = true; dialogCounter = 0; });
                    }
                    else if(dialogCounter == 3)
                    {
                        dialogDone = true;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
                        string[] str = {
                            "我喜欢他[wave amp=24.0 freq=8.0 connected=1][color=blue]1bit[/color][/wave]的模样！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 4;
                    }
                    else if(dialogCounter == 4)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "你和[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]玩了一些什么？",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(1.0f, () =>
                        {
                            string[][] options = {
                                new string[]{
                                    "我和他[wave amp=24.0 freq=8.0 connected=1][color=red]聊天[/color][/wave]了！",
                                    "我一直在[wave amp=24.0 freq=8.0 connected=1][color=blue]抚摸[/color][/wave]他！"
                                },
                                new string[]{
                                    "placeholder",
                                    "placeholder"
                                }
                            };
                            selectBox.ShowSelection("playing_what", options[Language], (name, selection) =>
                            {
                                if(selection == 0)
                                {
                                    dialogCounter = 5;
                                }
                                else if(selection == 1)
                                {
                                    dialogCounter = 5;
                                }
                                dialogDone = true;
                            });
                        });
                    }
                    else if(dialogCounter == 5)
                    {
                        dialogDone = false;
                        {
                            nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                            nyako.faceStatus = NyakoMgr.FaceStatus.Angry;
                            nyako.Scale = Vector2.One * 1.05f;
                            string[] str = {
                                "[shake rate=20.0 level=5 connected=1][font_size=40]什么？！[/font_size][/shake]",
                                "placeholder"
                            };
                            SetScreenShake(8f, 0.2f);
                            textBox1.showText(autoprefix + str[Language] + autosuffix, 20);
                        }
                        regTimer(1.0f, ()=>{
                            nyako.faceStatus = NyakoMgr.FaceStatus.UnhappyBlink;
                            nyako.Scale = Vector2.One * 1.05f;
                            string[] str = {
                                "噢，也许是我想多了...",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix);
                        });
                        regTimer(2.0f, ()=>{
                            nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
                                string[] str = {
                                "抱歉抱歉被吓了一跳...你也很喜欢豹豹[wave amp=24.0 freq=8.0 connected=1][color=blue]1bit[/color][/wave]的画风吧？你也很喜欢[wave amp=24.0 freq=8.0 connected=1][color=blue]按键[/color][/wave]的手感吧！",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix);
                            regTimer(1.0f, () =>
                            {
                                string[][] options = {
                                    new string[]{
                                        "[wave amp=24.0 freq=8.0 connected=1]我很喜欢！[/wave]",
                                        "[shake rate=20.0 level=5 connected=1]可是他明明是[color=red]3D[/color]的...[/shake]"
                                    },
                                    new string[]{
                                        "placeholder",
                                        "placeholder",
                                    }
                                };
                                selectBox.ShowSelection("is_new_tech", options[Language], (name, selection) =>
                                {
                                    if(selection == 0)
                                    {
                                        dialogCounter = 6;
                                    }
                                    else if(selection == 1)
                                    {
                                        dialogCounter = 8;
                                    }
                                    dialogDone = true;
                                });
                            });
                        });
                    }
                    else if(dialogCounter == 6)
                    {
                        dialogDone = false;
                        {
                            nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                            nyako.faceStatus = NyakoMgr.FaceStatus.SmileBlink;
                            string[] str = {
                                "（果然是想多了...）",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix);
                        }
                        regTimer(1.0f, ()=>{
                            nyako.faceStatus = NyakoMgr.FaceStatus.HappyBlink;
                            nyako.Scale = Vector2.One * 1.05f;
                            string[] str = {
                                "看来你有把[wave amp=24.0 freq=8.0 connected=1][color=blue]豹豹[/color][/wave]当作[wave amp=24.0 freq=8.0 connected=1][color=red]真正的朋友哇！[/color][/wave]\n[wave amp=24.0 freq=8.0 connected=1]（嘿嘿你好可爱...）[/wave]",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix);
                            dialogCounter = 7;
                            dialogDone = true;
                        });
                    }
                    else if(dialogCounter == 7)
                    {
                        dialogDone = true;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        string[] str = {
                            "[wave amp=24.0 freq=8.0 connected=1]那就不打扰你们继续玩啦~我要出门啦，拜拜咯！[/wave]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 100;
                    }
                    else if(dialogCounter == 8)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Angry;
                        nyako.Scale = Vector2.One * 1.05f;
                        string[] str = {
                            "[shake rate=20.0 level=5 connected=1][font_size=40][color=blue]豹豹[/color]是2010年的拓麻歌子！他不可能是3D的！[/font_size][/shake]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix, 40);
                        regTimer(1.0f, () => { dialogCounter = 9; dialogDone = true; });
                    }
                    else if(dialogCounter == 9)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Unhappy;
                        nyako.Scale = Vector2.One * 1.05f;
                        string[] str = {
                            "[shake rate=20.0 level=5 connected=1][font_size=40]我最讨厌你们这些[color=red]新技术[/color]了！[/font_size][/shake]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix, 40);
                        regTimer(1.0f, () => { dialogCounter = 10; dialogDone = true; });
                    }
                    else if(dialogCounter == 10)
                    {
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Angry;
                        nyako.Scale = Vector2.One * 1.05f;
                        string[] str = {
                            "[shake rate=20.0 level=5 connected=1][font_size=40]我小时候[color=blue]豹豹[/color]是什么样，他现在就该是什么样！[/font_size][/shake]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix, 40);
                        regTimer(1.0f, () => { dialogCounter = 11; dialogDone = true; });
                    }
                    else if(dialogCounter == 11)
                    {
                        dialogDone = false;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Unhappy;
                            string[] str = {
                            "所以你很喜欢豹豹[wave amp=24.0 freq=8.0 connected=1][color=blue]1bit[/color][/wave]的画风吧？你很喜欢[wave amp=24.0 freq=8.0 connected=1][color=blue]按键[/color][/wave]的手感吧?！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        regTimer(1.0f, () =>
                        {
                            string[][] options = {
                                new string[]{
                                    "[wave amp=24.0 freq=8.0 connected=1]我很喜欢！[/wave]",
                                    "[shake rate=20.0 level=5 connected=1][color=red]可是他明明是...[/color][/shake]"
                                },
                                new string[]{
                                    "placeholder",
                                    "placeholder",
                                }
                            };
                            selectBox.ShowSelection("is_new_tech2", options[Language], (name, selection) =>
                            {
                                if(selection == 0)
                                {
                                    dialogCounter = 7;
                                }
                                else if(selection == 1)
                                {
                                    dialogCounter = 12;
                                }
                                dialogDone = true;
                            });
                        });
                    }
                    else if(dialogCounter == 12)
                    {
                        dialogDone = false;
                        {
                            nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                            nyako.faceStatus = NyakoMgr.FaceStatus.Angry;
                            nyako.Scale = Vector2.One * 1.05f;
                            SetScreenShake(8f, 0.5f);
                            string[] str = {
                                "[shake rate=20.0 level=5 connected=1][font_size=40][color=red]你不许说了！！[/color][/font_size][/shake]",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix, 40);
                        }
                        regTimer(1.5f, () => { 
                            nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                            nyako.faceStatus = NyakoMgr.FaceStatus.Unhappy;
                            nyako.Scale = Vector2.One * 1.05f;
                            string[] str = {
                                "[shake rate=20.0 level=5 connected=1][font_size=40]............[/font_size][/shake]",
                                "placeholder"
                            };
                            textBox1.showText(autoprefix + str[Language] + autosuffix, 2);
                        });
                        regTimer(10.0f, () => { 
                            dialogCounter = 13; dialogDone = true; 
                        });
                    }
                    else if(dialogCounter == 13)
                    {
                        dialogDone = true;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.UnhappyBlink;
                        string[] str = {
                            "[shake rate=20.0 level=5 connected=1]总之你接下来也要帮我照看[color=blue]豹豹[/color]！[/shake]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 100;
                    }
                    else if(dialogCounter == 100)
                    {
                        tamaScreen.setTrue(true);
                        startDialog(DialogStatus.Gaming);
                        game.targetYScroll = 0;
                        textBox1.showText("");
                        regTimer(0.3f, () => { 
                            int earthquake_n = game.EarthQuakeN - tmpEarthQuakeN;
                            int lost_n = game.FailN - tmpFailN;
                            textBox2.Following = mainbaobao;
                            textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                            if(earthquake_n > 0)
                            {
                                {
                                    string[] str = {
                                        $"[font_size=36]刚刚一共发生了{earthquake_n}起地震，伤亡[color=blue]豹[/color]数为{lost_n}...[/font_size]", 
                                        "placeholder"
                                    };
                                    textBox2.showText(str[Language]);
                                    mainbaobao.faceStatus = Baobao.FaceStatus.Die;
                                }
                                regTimer(2.0f,()=>{
                                    string[] str = {
                                        $"[font_size=36][wave amp=24.0 freq=8.0 connected=1]总之还是要选择[color=red]正确的选项[/color]！\n别让她继续唠叨了！[/wave][/font_size]", 
                                        "placeholder"
                                    };
                                    textBox2.showText(str[Language]);
                                    mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                                });
                                regTimer(6.0f,()=>{textBox2.showText("");});
                            }
                            else
                            {
                                string[] str = {
                                    $"[font_size=36][wave amp=24.0 freq=8.0 connected=1]哇哦，全员幸存！\n你也太懂[color=red]她[/color]了！[/wave][/font_size]", 
                                    "placeholder"
                                };
                                textBox2.showText(str[Language]);
                                mainbaobao.faceStatus = Baobao.FaceStatus.Happy;
                                regTimer(2.0f,()=>{
                                    string[] str = {
                                        $"[font_size=36][wave amp=24.0 freq=8.0 connected=1]以后也继续选择[color=red]正确的选项[/color]吧！\n别让她继续唠叨了！[/wave][/font_size]", 
                                        "placeholder"
                                    };
                                    textBox2.showText(str[Language]);
                                    mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                                });
                                regTimer(6.0f,()=>{textBox2.showText("");});
                            }
                        });
                    }
                }
            }
        }
        else if(dialogStatus == DialogStatus.Nyako3)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        tmpEarthQuakeN = game.EarthQuakeN;
                        tmpFailN = game.FailN;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "aaa",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 100;
                    }
                    else if(dialogCounter == 100)
                    {
                        tamaScreen.setTrue(true);
                        startDialog(DialogStatus.Gaming);
                        game.targetYScroll = 0;
                        textBox1.showText("");
                        regTimer(0.3f, () => { 
                            int earthquake_n = game.EarthQuakeN - tmpEarthQuakeN;
                            int lost_n = game.FailN - tmpFailN;
                            textBox2.Following = mainbaobao;
                            textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                            if(earthquake_n > 0)
                            {
                                {
                                    string[] str = {
                                        $"[font_size=36]刚刚一共发生了{earthquake_n}起地震，伤亡[color=blue]豹[/color]数为{lost_n}...[/font_size]", 
                                        "placeholder"
                                    };
                                    textBox2.showText(str[Language]);
                                    mainbaobao.faceStatus = Baobao.FaceStatus.Die;
                                }
                                regTimer(2.0f,()=>{textBox2.showText("");});
                            }
                            else
                            {
                                string[] str = {
                                    $"[font_size=36][wave amp=24.0 freq=8.0 connected=1]哇哦，全员幸存！[/wave][/font_size]", 
                                    "placeholder"
                                };
                                textBox2.showText(str[Language]);
                                mainbaobao.faceStatus = Baobao.FaceStatus.Happy;
                                regTimer(2.0f,()=>{textBox2.showText("");});
                            }
                        });
                    }
                }
            }
        }
        else if(dialogStatus == DialogStatus.Nyako4)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        tmpEarthQuakeN = game.EarthQuakeN;
                        tmpFailN = game.FailN;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "bbb",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                        dialogCounter = 100;
                    }
                    else if(dialogCounter == 100)
                    {
                        tamaScreen.setTrue(true);
                        startDialog(DialogStatus.Gaming);
                        game.targetYScroll = 0;
                        textBox1.showText("");
                        regTimer(0.3f, () => { 
                            int earthquake_n = game.EarthQuakeN - tmpEarthQuakeN;
                            int lost_n = game.FailN - tmpFailN;
                            textBox2.Following = mainbaobao;
                            textBox2.FollowingOffset = new Vector2(208f, 240f) * mainbaobao.GlobalScale;
                            if(earthquake_n > 0)
                            {
                                {
                                    string[] str = {
                                        $"[font_size=36]刚刚一共发生了{earthquake_n}起地震，伤亡[color=blue]豹[/color]数为{lost_n}...[/font_size]", 
                                        "placeholder"
                                    };
                                    textBox2.showText(str[Language]);
                                    mainbaobao.faceStatus = Baobao.FaceStatus.Die;
                                }
                                regTimer(2.0f,()=>{textBox2.showText("");});
                            }
                            else
                            {
                                string[] str = {
                                    $"[font_size=36][wave amp=24.0 freq=8.0 connected=1]哇哦，全员幸存！[/wave][/font_size]", 
                                    "placeholder"
                                };
                                textBox2.showText(str[Language]);
                                mainbaobao.faceStatus = Baobao.FaceStatus.Happy;
                                regTimer(2.0f,()=>{textBox2.showText("");});
                            }
                        });
                    }
                }
            }
        }
        else if(dialogStatus == DialogStatus.Nyako5)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        tmpEarthQuakeN = game.EarthQuakeN;
                        tmpFailN = game.FailN;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "aaa",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                }
            }
        }
        else if(dialogStatus == DialogStatus.HappyEnd)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete && dialogDone)
                {
                    if(dialogCounter == 0)
                    {
                        tmpEarthQuakeN = game.EarthQuakeN;
                        tmpFailN = game.FailN;
                        dialogDone = false;
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        
                        string[] str = {
                            "happyhappy",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                }
            }
        }
    }
    public void setTamaShow(bool show)
    {
        targetTamaAlpha = show ? 1.0f : 0.0f;
    }
    public void FirstGetTime(int hour, int minute)
    {
        if(hour == 9 && minute == 0)
        {
            tamaScreen.setTrue(false);
            cam.TamaCenter();
            regTimer(2.0f, () => {
                startDialog(DialogStatus.Nyako2);
            });
        }
        if(hour == 12 && minute == 0)
        {
            tamaScreen.setTrue(false);
            cam.TamaCenter();
            regTimer(2.0f, () => {
                startDialog(DialogStatus.Nyako3);
            });
        }
        if(hour == 15 && minute == 0)
        {
            tamaScreen.setTrue(false);
            cam.TamaCenter();
            regTimer(2.0f, () => {
                startDialog(DialogStatus.Nyako4);
            });
        }
        if(hour == 18 && minute == 0)
        {
            tamaScreen.setTrue(false);
            cam.TamaCenter();
            regTimer(2.0f, () => {
                startDialog(DialogStatus.Nyako5);
            });
        }
    }
    public void TriggerSelection(string name, int option)
    {
        processDialog();
    }
    public void TriggerHappyEnd()
    {
        cam.TamaCenter();
        regTimer(2.0f, () => {
            startDialog(DialogStatus.HappyEnd);
        });
    }
}
