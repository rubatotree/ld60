using Godot;
using System;
using System.Text.Json.Serialization;

public partial class World : Node2D
{
    [Export] TextBox textBox1;
    [Export] TextBox textBox2;
    public float time = 0;

    string autoprefix = "[color=black][font_size=30][font=./fonts/unifont.ttf]";
    string autosuffix = "[/font][/font_size][/color]";
    string[] strs = {
        "",
        "[wave amp=24.0 freq=8.0 connected=1]你好！！你可以叫我[color=red]喵喵酱[/color]！[/wave]",
        "[wave amp=24.0 freq=8.0 connected=1]你有玩过[color=red]拓麻歌子[/color]吗？[/wave]",
        ""
    };
    int strc = 0;
    public override void _Ready()
    {
        textBox1.showText(autoprefix + strs[strc] + autosuffix);
    }

    public override void _Process(double delta)
    {
        time += (float)delta;
        if(Input.IsMouseButtonPressed(MouseButton.Left) && strc + 1 < strs.Length && textBox1.Complete)
        {
            strc++;
            textBox1.showText(autoprefix + strs[strc] + autosuffix);
        }
    }
}
