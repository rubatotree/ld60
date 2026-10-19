using Godot;
using System;

public partial class Camera : Node2D
{
    [Export] Camera2D cam;
    [Export] Node2D frontCards;
    [Export] float MousePosLerpCoeff = 4f;
    [Export] float MousePosScale = 0.04f;
    [Export] float MousePosScale2 = -0.02f;
    [Export] Vector2 TamaRightPos = new Vector2(-340, 0);
    [Export] Vector2 TamaCenterPos = new Vector2(0, 0);
    [Export] Vector2 TamaZoomPos = new Vector2(0, -130);
    [Export] float NormalScale = 1.25f;
    [Export] float ZoomScale = 0.3f;
    [Export] float PosSpd = 2f;
    [Export] public float ZoomSpd = 1f;
    public float Zoom => 1.0f / curScale;
    public Vector2 Shake = Vector2.Zero;
    Vector2 mousePos = Vector2.Zero;
    Vector2 mouseDeltaPos = Vector2.Zero;
    Vector2 targetPos;
    float targetScale;
    Vector2 curPos;
    float curScale;
    public enum Status { Center, Zoom, Right, Custom }
    public Status status;
    public override void _Ready()
    {
        targetPos = TamaCenterPos;
        targetScale = NormalScale;
        UpdateNow();
    }
    public override void _Process(double delta)
    {
        mousePos = GetGlobalMousePosition() - Position;
        float scale1 = MousePosScale;
        float scale2 = MousePosScale2;
        if(status == Status.Right)
        {
            if(mousePos.Length() > 200f) 
                mousePos *= 200f / mousePos.Length();
            mousePos *= 1.0f;
        }
        else if(status == Status.Center || status == Status.Zoom)
        {
            if(mousePos.Length() > 50f) 
                mousePos *= 200f / mousePos.Length();
            mousePos *= 0.3f;
            scale1 *= 0.3f;
            scale2 *= 0.1f;
        }
        else
        {
            mousePos *= 0.0f;
        }
        mouseDeltaPos = mouseDeltaPos.Lerp(mousePos, MousePosLerpCoeff * (float)delta);
        curPos = curPos.Lerp(targetPos, PosSpd * (float)delta);
        curScale = Mathf.Lerp(curScale, targetScale, ZoomSpd * (float)delta);
        cam.Position = curPos - mouseDeltaPos * MousePosScale + Shake;
        frontCards.Position = -mouseDeltaPos * MousePosScale2 + Shake * 0.5f;
        cam.Scale = new Vector2(curScale, curScale);
        cam.Zoom = new Vector2(1.0f / curScale, 1.0f / curScale);
    }
    public void FocusAt(Vector2 pos, float scale)
    {
        targetPos = pos;
        targetScale = scale;
        status = Status.Custom;
    }
    public void TamaCenter() { FocusAt(TamaCenterPos, NormalScale); status = Status.Center; }
    public void TamaRight() { FocusAt(TamaRightPos, NormalScale); status = Status.Right; }
    public void TamaZoom() { FocusAt(TamaZoomPos, ZoomScale); status = Status.Zoom; }
    public void UpdateNow() 
    { 
        curPos = targetPos;
        curScale = targetScale;
    }
}
