using Godot;
using System;

public partial class NyakoMgr : Node2D
{
    [Export] World world;
    [Export] Sprite2D base_spr_relax;
    [Export] Sprite2D base_spr_tama;
    [Export] Sprite2D face_spr;
    [Export] Texture2D baseSprRelax;
    [Export] Texture2D baseSprHoldTama;
    [Export] Texture2D faceSprHappy;
    [Export] Texture2D faceSprHappyBlink;
    [Export] Texture2D faceSprSmile;
    [Export] Texture2D faceSprSmileBlink;
    [Export] Texture2D faceSprWink;
    [Export] Texture2D faceSprUnhappy;
    [Export] Texture2D faceSprUnhappyBlink;
    [Export] Texture2D faceSprAngry;
    [Export] Texture2D faceSprCry;
    [Export] Texture2D faceSprCryBlink;
    [Export] Texture2D faceSprCry2;
    public enum BaseStatus { Relax, HoldTama }
    public enum FaceStatus { Happy, HappyBlink, Smile, SmileBlink, Wink, Unhappy, UnhappyBlink, Angry, Cry, CryBlink, Cry2 }
    [Export] public BaseStatus baseStatus;
    [Export] public FaceStatus faceStatus;
    [Export] float MinRotation = -0.03f * 2 * Mathf.Pi;
    [Export] float MaxRotation = -0.02f * 2 * Mathf.Pi;
    [Export] float RotateSineCoeff = 4f;
    [Export] float MousePosLerpCoeff = 4f;
    [Export] float MousePosScale = 0.00f;
    [Export] float BlinkHold = 0.2f;
    [Export] float BlinkResetTime = 2.0f;
    [Export] float BlinkNoise = 0.2f;
    public float BlinkTime = 0.0f;

    [Export] float spdBase = 8.0f;
    [Export] public float spdAlpha = 10.0f;
    public float curBase = 0.0f;
    public float targetBase = 0.0f;
    public float curAlpha = 0.0f;
    public float targetAlpha = 0.0f;

    public float curRotation = 0f;

    Vector2 originPos;
    Vector2 mousePos = Vector2.Zero;
    Vector2 mouseDeltaPos = Vector2.Zero;
    public override void _Ready()
    {
        originPos = Position;
    }
    
    public override void _Process(double delta)
    {
        curRotation =  Mathf.Lerp(MinRotation, MaxRotation, Mathf.Sin(RotateSineCoeff * world.time) * 0.5f + 0.5f);
        Rotation = curRotation;
        mousePos = GetGlobalMousePosition() - originPos;
        if(mousePos.Length() > 200f) mousePos *= 200f / mousePos.Length();
        mouseDeltaPos = mouseDeltaPos.Lerp(mousePos, MousePosLerpCoeff * (float)delta);
        Position = originPos - mouseDeltaPos * MousePosScale + (1.0f - curAlpha) * new Vector2(-300f, 0f);

        BlinkTime -= (float)delta;
        bool isBlink = BlinkTime <= BlinkHold;
        if(BlinkTime <= 0.0f)
            BlinkTime = BlinkResetTime + (world.rng.Randf() * 2.0f - 1.0f) * BlinkNoise;

        if(baseStatus == BaseStatus.Relax)
            targetBase = 0.0f;
        else if(baseStatus == BaseStatus.HoldTama)
            targetBase = 1.0f;
        curBase = Mathf.Lerp(curBase, targetBase, spdBase * (float)delta);
        base_spr_relax.Modulate = new Color(1, 1, 1, Mathf.Sqrt(1.0f - curBase));
        base_spr_tama.Modulate = new Color(1, 1, 1, Mathf.Sqrt(curBase));

        curAlpha = Mathf.Lerp(curAlpha, targetAlpha, spdAlpha * (float)delta);
        Modulate = new Color(1, 1, 1, curAlpha);

        if(faceStatus == FaceStatus.Happy)
            face_spr.Texture = isBlink ? faceSprHappyBlink : faceSprHappy;
        else if(faceStatus == FaceStatus.Unhappy)
            face_spr.Texture = isBlink ? faceSprUnhappyBlink : faceSprUnhappy;
        else if(faceStatus == FaceStatus.Smile)
            face_spr.Texture = isBlink ? faceSprSmileBlink : faceSprSmile;
        else if(faceStatus == FaceStatus.Cry)
            face_spr.Texture = isBlink ? faceSprCryBlink : faceSprCry;
        else if(faceStatus == FaceStatus.HappyBlink)
            face_spr.Texture = faceSprHappyBlink;
        else if(faceStatus == FaceStatus.UnhappyBlink)
            face_spr.Texture = faceSprUnhappyBlink;
        else if(faceStatus == FaceStatus.SmileBlink)
            face_spr.Texture = faceSprSmileBlink;
        else if(faceStatus == FaceStatus.CryBlink)
            face_spr.Texture = faceSprCryBlink;
        else if(faceStatus == FaceStatus.Wink)
            face_spr.Texture = faceSprWink;
        else if(faceStatus == FaceStatus.Angry)
            face_spr.Texture = faceSprAngry;
        else if(faceStatus == FaceStatus.Cry2)
            face_spr.Texture = faceSprCry2;
        Scale = Scale.Lerp(Vector2.One, Mathf.Clamp(12.0f * (float)delta, 0, 1));
    }

    public void setShow(bool show)
    {
        targetAlpha = show ? 1.0f : 0.0f;
    }
}
