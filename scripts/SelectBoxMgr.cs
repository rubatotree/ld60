using Godot;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class SelectBoxMgr : Node2D
{
    [Export] World world;
    [Export] Control[] Seleboxs;
    [Export] RichTextLabel[] Labels;
    [Export] float HideY = 200;
    [Export] float HideSpd = 20f;
    [Export] bool ShowOptions = true;
    [Export] int OptionN = 3;
    [Export] Color HoverColor = new Color(0.7f, 0.8f, 1.0f);
    float curHideY = 0;
    public int Hovering = -1;
    public int prevHovering = -1;
    public bool OptionActive = false;
    public float DefaultY = 400f;
    string SeleName = "";
    public delegate void Behavior(string name, int selection);
    Behavior behavior;
    public override void _Ready()
    {

    }
    public void ShowSelection(string name, string[] options, Behavior behaviorHook = null)
    {
        if(OptionActive)
        {
            CancelSelection();
            curHideY = HideY;
        }
        SeleName = name;
        world.Selections[name] = -1;
        ShowOptions = true;
        OptionActive = true;
        OptionN = options.Count();
        for(int i = 0; i < OptionN; i++)
            Labels[i].Text = options[i];
        behavior = behaviorHook;
    }
    public void CancelSelection() 
    { 
        if(!OptionActive) return;
        Select(-1); 
    } 
    public void Select(int selection)
    {
        world.Selections[SeleName] = selection;
        ShowOptions = false;
        OptionActive = false;
        if(behavior != null) behavior(SeleName, selection);
        behavior = null;
        world.TriggerSelection(SeleName, Hovering);
        SeleName = "";
    }
    public override void _Process(double delta)
    {
        float targetHideY = ShowOptions ? 0f : HideY;
        curHideY = Mathf.Lerp(curHideY,targetHideY,HideSpd * (float)delta);
        Position = new Vector2(0, curHideY);
        if(OptionN == 0)
        {
            Seleboxs[0].Visible = false;
            Seleboxs[1].Visible = false;
            Seleboxs[2].Visible = false;
        }
        else if(OptionN == 1)
        {
            Seleboxs[0].Position = new Vector2(-190f, DefaultY);
            Seleboxs[0].Visible = true;
            Seleboxs[1].Visible = false;
            Seleboxs[2].Visible = false;
        }
        else if(OptionN == 2)
        {
            Seleboxs[0].Position = new Vector2(-190f - 240f, DefaultY);
            Seleboxs[1].Position = new Vector2(-190f + 240f, DefaultY);
            Seleboxs[0].Visible = true;
            Seleboxs[1].Visible = true;
            Seleboxs[2].Visible = false;
        }
        else if(OptionN == 3)
        {
            Seleboxs[0].Position = new Vector2(-190f - 450f, DefaultY);
            Seleboxs[1].Position = new Vector2(-190f, DefaultY);
            Seleboxs[2].Position = new Vector2(-190f + 450f, DefaultY);
            Seleboxs[0].Visible = true;
            Seleboxs[1].Visible = true;
            Seleboxs[2].Visible = true;
        }
        if(OptionActive)
        {
            Vector2 MousePos = GetGlobalMousePosition();
            Hovering = -1;
            for(int i = 0; i < OptionN; i++)
            {
                Vector2 MouseVec = MousePos - Seleboxs[i].GlobalPosition;
                if(Mathf.Abs(MouseVec.X - 190f) <= 200f && Mathf.Abs(MouseVec.Y - 108f / 2) <= 60f)
                {
                    Hovering = i;
                }
            }
        }
        if(prevHovering != Hovering && Hovering != -1)
        {
            Seleboxs[Hovering].Scale = Vector2.One * 1.1f;
        }
        for(int i = 0; i < 3; i++)
        {
            Seleboxs[i].Modulate = Hovering == i ? HoverColor : new Color(1,1,1,0.8f);
            Seleboxs[i].Scale = Seleboxs[i].Scale.Lerp(Vector2.One, 6f * (float)delta);
        }
        prevHovering = Hovering;
        if(world.isMouseJustPressed() && Hovering >= 0 && Hovering <= OptionN)
        {
            Select(Hovering);
        }
    }
}
