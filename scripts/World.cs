using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
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
            mainbaobao.DragAble = true; 
            dialogDone = false;
            textBox2.Following = mainbaobao;
            textBox2.FollowingOffset = new Vector2(160f, 160f) * mainbaobao.Scale;
            textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
            regTimer(0.5f, () => { 
                mainbaobao.Hoverable = true; 
                mainbaobao.targetNaturalScale = Vector2.One * 1.2f;
                mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                mainbaobao.SetShake(1f, 60000f);
                string[] str = {
                    "[font_size=64][shake rate=20.0 level=5 connected=1]快点摸摸我！！！[/shake][/font_size]", 
                    "placeholder"
                };
                textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                dialogDone = true;
            });
        }
    }
    void processDialog()
    {
        if(dialogStatus == DialogStatus.Init)
        {
            if(isMouseJustPressed())
            {
                startDialog(DialogStatus.Baobao1, true);
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
                        textBox2.FollowingOffset = new Vector2(160f, 160f) * mainbaobao.Scale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 1)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale = Vector2.One * 1.05f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        mainbaobao.SetShake(1f, 60000f);
                        string[] str = {
                            "[font_size=52][shake rate=20.0 level=5 connected=1]还要！！[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.FollowingOffset = new Vector2(160f, 160f) * mainbaobao.Scale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter <= 8)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale = Vector2.One * 1.00f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Waaa;
                        mainbaobao.SetShake(1f, 60000f);
                        string[] str = {
                            "[font_size=48][shake rate=20.0 level=5 connected=1]不要停！！！[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.FollowingOffset = new Vector2(160f, 160f) * mainbaobao.Scale;
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 40);
                        dialogDone = true;
                    }
                    else if(dialogCounter == 9)
                    {
                        if(!mainbaobao.onHover) return;
                        mainbaobao.targetNaturalScale = Vector2.One * 0.80f;
                        mainbaobao.faceStatus = Baobao.FaceStatus.Happy;
                        mainbaobao.SetShake(0f, 0f);
                        mainbaobao.Hoverable = false;
                        mainbaobao.PlayingFPS = 8.0f;
                        mainbaobao.Playing = true;
                        string[] str = {
                            "[font_size=40][shake rate=10.0 level=5 connected=1]哈啊...哈啊......[/shake][/font_size]", 
                            "placeholder"
                        };
                        textBox2.curSize = new Vector2(40f, textBox2.curSize.Y);
                        textBox2.showText(autoprefix + str[Language] + autosuffix, 20);
                        textBox2.FollowingOffset = new Vector2(160f, 160f) * mainbaobao.Scale;
                        dialogDone = true;
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
