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
    [Export] public int Language;
    public RandomNumberGenerator rng = new RandomNumberGenerator();
    public float time = 0;
    public int pressn = 0;
    bool pressed = false;
    bool justPressed = false;
    public float targetTamaAlpha = 0.0f;
    public float curTamaAlpha = 0.0f;

    string autoprefix = "[color=black][font_size=30][font=./fonts/unifont.ttf]";
    string autosuffix = "[/font][/font_size][/color]";

    public enum DialogStatus { None, Test }
    public DialogStatus dialogStatus = DialogStatus.None;
    int dialogCounter = 0;
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
        startDialog(DialogStatus.Test);
    }

    public override void _Process(double delta)
    {
        time += (float)delta;
        justPressed = pressed;
        pressed = Input.IsMouseButtonPressed(MouseButton.Left);

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
    public void onTamaButtonPressed(string name)
    {
    }

    public void startDialog(DialogStatus status)
    {
        dialogStatus = status;
        dialogCounter = 0;
        if(dialogStatus == DialogStatus.Test)
        {
            setTamaShow(false);
            nyako.setShow(false);
        }
    }

    void processDialog()
    {
        if(dialogStatus == DialogStatus.Test)
        {
            if(isMouseJustPressed() && !isTamaJustPressed)
            {
                if(textBox1.Complete)
                {
                    if(dialogCounter == 0)
                    {
                        nyako.setShow(true);
                        cam.TamaRight();
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;

                        string[] str = {
                            "你好！！这里是[wave amp=24.0 freq=8.0 connected=1][color=red]喵喵酱[/color][/wave]！", 
                            "placeholder"
                        };
                        regTimer(0.5f, () => { 
                            textBox1.showText(autoprefix + str[Language] + autosuffix);
                        });
                        Debug.WriteLine("generate a timer");
                    }
                    else if(dialogCounter == 1)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.HoldTama;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Happy;
                        string[] str = {
                            "你有玩过[wave amp=24.0 freq=8.0 connected=1][color=red]拓麻歌子[/color][/wave]吗？",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 2)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Smile;
                        setTamaShow(true);
                        string[] str = {
                            "就是右边的小游戏机哦！",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 3)
                    {
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Angry;
                        string[] str = {
                            "[font_size=64][color=red]（哈！）[/color][/font_size]",
                            "placeholder"
                        };
                        textBox1.showText(autoprefix + str[Language] + autosuffix);
                    }
                    else if(dialogCounter == 4)
                    {
                        nyako.setShow(false);
                        nyako.baseStatus = NyakoMgr.BaseStatus.Relax;
                        nyako.faceStatus = NyakoMgr.FaceStatus.Wink;
                        textBox1.showText("");
                        regTimer(0.5f, () => { cam.TamaCenter(); });
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
