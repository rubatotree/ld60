using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

public partial class TextBox : Control
{
    [Export] public World world;
    [Export] RichTextLabel richTextLabel;
    [Export] NinePatchRect ninePatchRect;
    [Export] public Node2D NodeParent;
    [Export] public Node2D Following;
    [Export] public Vector2 FollowingOffset = Vector2.Zero;
    [Export] public float FollowingSpd = 20f;
    [Export] float textSpd = 20f;
    [Export] float texn = 0;

    [Export] float sizeSpd = 10f;
    [Export] float alphaSpd = 10f;
    [Export] float fadeTime = -1f;
    float fadeTimer = 0f;

    public Vector2 curSize = new Vector2(40f, 40f);
    public Vector2 targetSize = new Vector2(40f, 40f);
    public bool Complete{get;set;}
    public bool prevComplete = false;
    public float alpha = 0;
    public float targetAlpha = 0;
    World.Behavior behavior = null;
    [Export] public bool iscowsay = false;

    public override void _Ready()
    {
        Size = curSize;
        richTextLabel.VisibleCharacters = (int)texn;
        targetSize = new Vector2(richTextLabel.GetContentWidth(), richTextLabel.GetContentHeight());
        Complete = true;
        prevComplete = true;
        if(richTextLabel.Text == "")
        {
            alpha = 0;
            targetAlpha = 0;
        }
        else
        {
            alpha = 1;
            targetAlpha = 1;
        }
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, alpha);
    }
    public override void _Process(double delta)
    {
        if(texn >= -0.5f) texn += textSpd * (float)delta;
        richTextLabel.VisibleCharacters = (int)texn;
        string parsedText = richTextLabel.GetParsedText();
        Complete = parsedText == "" || richTextLabel.VisibleCharacters >= parsedText.Length;
        if(parsedText == "")
        {
            targetAlpha = 0;
            targetSize = Vector2.Zero;
        }
        else
        {
            targetAlpha = 1;
            targetSize = new Vector2(richTextLabel.GetContentWidth(), richTextLabel.GetContentHeight());
        }

        curSize = curSize.Lerp(targetSize, sizeSpd * (float)delta);
        curSize.X = Mathf.Max(40f, curSize.X);
        curSize.Y = Mathf.Max(40f, curSize.Y);
        Size = curSize;

        alpha = Mathf.Lerp(alpha, targetAlpha, alphaSpd * (float)delta);
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, alpha);
        // Scale = new Vector2(1.0f / world.Zoom, 1.0f / world.Zoom);
        // ninePatchRect.Scale = Scale;
        if(Following != null && !iscowsay)
        {
            NodeParent.GlobalPosition = NodeParent.GlobalPosition.Lerp((Following.GetScreenTransform().Origin - new Vector2(640, 480) + FollowingOffset) / world.Zoom, Mathf.Clamp(FollowingSpd * (float)delta, 0.0f, 1.0f));
        }
        fadeTimer -= (float)delta;
        if(fadeTime > 0 && fadeTimer < 0)
        {
            Complete = true;
            showText("");
        }
        if(!prevComplete && Complete && behavior != null)
        {
            behavior();
            behavior = null;
        }
        prevComplete = Complete;
    }
    public void showText(string text, float spd = 20f, World.Behavior behaviorHook = null)
    {
        if(world != null && world.Language == 1) spd *= 2;
        if(spd < -0.5)
        {
            textSpd = 0;
            texn = -1;
        }
        else
        {
            textSpd = spd;
            texn = 0;
        }
        fadeTimer = fadeTime;
        richTextLabel.Text = text;
        richTextLabel.VisibleCharacters = (int)texn;
        behavior = behaviorHook;
        // Debug.WriteLine("Show: ", text);
    }
}
