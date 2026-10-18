using Godot;
using System;
using System.Net.Mail;

public partial class Baobao : Node2D
{
    [Export] float spdScaling = 16f;
    [Export] float spdRotating = 5f;
    [Export] public bool Hoverable = true;
    [Export] public bool Clickable = true;
    [Export] public bool DragAble = true;
    [Export] World world;
    [Export] AnimatedSprite2D sprite;
    [Export] SpriteFrames sprNormal;
    [Export] SpriteFrames sprNormalBlink;
    [Export] SpriteFrames sprHappy;
    [Export] SpriteFrames sprWaaa;
    [Export] BaobaoGame game;
    [Export] float BlinkHold = 0.2f;
    [Export] float BlinkResetTime = 2.0f;
    [Export] float BlinkNoise = 0.2f;
    public float shakeSize = 0.0f;
    public float shakeTime = 0.0f;
    public enum FaceStatus { Normal, NormalBlink, Happy, Waaa }
    public FaceStatus faceStatus;
    public float BlinkTime = 0.0f;
    public float PlayingFPS = 12.0f;
    public Vector2 targetNaturalScale = Vector2.One;
    public Vector2 targetSquashScale = Vector2.One;
    public bool Playing = false;
    public bool Attached = false;
    public bool Failed = false;
    [Export] public bool isBase = false;
    public float Frame = 0;
    public float targetSquashRotation = 0;
    public bool dragged = false;
    public bool onHover => Hoverable && GetLocalMousePosition().Length() < 96;
    int dragFrame = 0;
    [Export] public bool Falling = false;
    [Export] public float FallingSpeed = 150f / 5f;

    Vector2 dragMousePos = Vector2.Zero;
    public override void _Ready()
    {
        targetNaturalScale = Scale;
    }
    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition() - GlobalPosition;
        bool onSquash = onHover && Clickable && Input.IsMouseButtonPressed(MouseButton.Left);
        if(onHover && !onSquash)
        {
            targetSquashScale = new Vector2(1.05f, 1.05f);
            targetSquashRotation = -mousePos.X / 9 / 180 * Mathf.Pi;
        }
        else if(onSquash || dragged)
        {
            targetSquashScale = new Vector2(1.1f, 0.8f);
            targetSquashRotation = -mousePos.X / 4 / 180 * Mathf.Pi;
            if(world.isMouseJustPressed())
            {
                Scale *= 1.1f;
            }
            if(dragged)
            {
                float dy = mousePos.Y - dragMousePos.Y;
                if(dy < -2) targetSquashRotation /= 2;
                else if(dy > 4) targetSquashRotation *= 2;
            }
        }
        else
        {
            targetSquashScale = Vector2.One;
            targetSquashRotation = 0;
        }

        if(!Input.IsMouseButtonPressed(MouseButton.Left)) dragged = false;
        if(onSquash && !dragged)
        {
            if(DragAble)
            {
                dragMousePos = mousePos;
                dragged = true;
                dragFrame = sprite.Frame;
            }
        }
        if(dragged)
        {
            GlobalPosition = GetGlobalMousePosition() - dragMousePos;
            sprite.Frame = dragFrame;
        }
        if(Falling)
        {
            Position += new Vector2(0.0f, FallingSpeed * (float)delta);
        }
        if(Position.Y > -game.YOffset && !(Attached || Failed))
        {
            if(isBase)
            {
                Attached = true;
                onAttached();
            }
            else
            {
                Failed = true;
                onFail();
            }
        }
        Position = new Vector2(Mathf.Clamp(Position.X, -120f, 120f), Mathf.Clamp(Position.Y, -10000f, -game.YOffset));

        BlinkTime -= (float)delta;
        bool isBlink = BlinkTime <= BlinkHold;
        if(BlinkTime <= 0.0f)
            BlinkTime = BlinkResetTime + (world.rng.Randf() * 2.0f - 1.0f) * BlinkNoise;

        if(faceStatus == FaceStatus.Happy || onSquash)
            sprite.SpriteFrames = sprHappy;
        else if(faceStatus == FaceStatus.Normal)
            sprite.SpriteFrames = isBlink ? sprNormalBlink : sprNormal;
        else if(faceStatus == FaceStatus.NormalBlink)
            sprite.SpriteFrames = sprNormalBlink;
        else if(faceStatus == FaceStatus.Waaa)
            sprite.SpriteFrames = sprWaaa;

        if(Playing && !dragged) Frame += PlayingFPS * (float)delta;
        sprite.Frame = ((int)Frame) % 16;

        Scale = Scale.Lerp(targetNaturalScale * targetSquashScale, spdScaling * (float)delta);
        Rotation = Mathf.Lerp(Rotation, targetSquashRotation, spdRotating * (float)delta);

        shakeTime -= (float)delta;
        if(shakeTime > 0 && !onSquash)
            sprite.Position = new Vector2(world.rng.Randf() * 2.0f - 1.0f, world.rng.Randf() * 2.0f - 1.0f) * shakeSize;
        else sprite.Position = Vector2.Zero;
    }
    public void SetShake(float size, float time)
    {
        shakeSize = size;
        shakeTime = time;
    }
    public void onFail()
    {

    }
    public void onAttached()
    {
        Playing = false;
        DragAble = false;
        Scale *= new Vector2(1.2f, 0.8f);
    }
}
