using Godot;
using System.Collections.Generic;

public partial class TargetingOverlay : Node2D
{
    [Export] public Texture2D LineTexture;
    [Export] public Color     ShaftModulate  = Colors.White;
    [Export] public float     ShaftThickness = 38f;
    [Export] public float     CurveBelly     = 0.35f;

    private bool    _arrow;
    private Vector2 _from, _to;

    private List<Rect2> _reticles = new();

    private static readonly Color AmberLine = new(1f, 0.78f, 0.2f, 0.92f);

    public override void _Ready()
    {
        LineTexture ??= GD.Load<Texture2D>("res://Test/TestImagesSprites/art/line.png");
    }

    // --- arrow ---
    public void ShowArrow(Vector2 from, Vector2 to)
    {
        _arrow = true; _from = from; _to = to; QueueRedraw();
    }

    public void HideArrow()
    {
        if (!_arrow) return;
        _arrow = false;
        QueueRedraw();
    }

    // --- reticles ---
    public void ShowReticles(List<Rect2> rects) { _reticles = rects ?? new(); QueueRedraw(); }
    public void HideReticles() { _reticles.Clear(); QueueRedraw(); }

    public override void _Draw()
    {
        if (_arrow)
        {
            float   dist = _from.DistanceTo(_to);
            Vector2 ctrl = (_from + _to) * 0.5f + new Vector2(0f, -Mathf.Min(dist * CurveBelly, 220f));

            DrawTexturedShaft(_from, ctrl, _to);

            // Head — tangent approximated from the curve near t=1
            Vector2 headDir = (_to - QuadBezier(_from, ctrl, _to, 0.9f)).Normalized();
            Vector2 n       = new(-headDir.Y, headDir.X);
            Vector2[] head  = { _to, _to - headDir * 26f + n * 14f, _to - headDir * 26f - n * 14f };
            DrawColoredPolygon(head, AmberLine);
        }

        foreach (var r in _reticles)
            DrawReticle(r);
    }

    // --- helpers ---

    private static Vector2 QuadBezier(Vector2 a, Vector2 c, Vector2 b, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * c + t * t * b;
    }

    private void DrawTexturedShaft(Vector2 from, Vector2 ctrl, Vector2 to)
    {
        if (LineTexture == null) return;

        float texW   = LineTexture.GetWidth();
        float texH   = LineTexture.GetHeight();
        float scaleY = ShaftThickness / texH;

        Vector2 prev = from;
        for (int i = 1; i <= 24; i++)
        {
            float   t   = (float)i / 24;
            Vector2 p   = QuadBezier(from, ctrl, to, t);
            Vector2 d   = p - prev;
            float   len = d.Length();
            if (len > 0.001f)
            {
                DrawSetTransform(prev, d.Angle(), new Vector2(len / texW, scaleY));
                DrawTexture(LineTexture, new Vector2(0f, -texH * 0.5f), ShaftModulate);
            }
            prev = p;
        }
        DrawSetTransform(Vector2.Zero, 0f, Vector2.One); // reset — obrigatório
    }

    private void DrawReticle(Rect2 r)
    {
        var c = new Color(1f, 0.78f, 0.2f, 0.95f);
        float len = Mathf.Min(r.Size.X, r.Size.Y) * 0.28f, w = 5f;
        Vector2 tl = r.Position,                             tr = r.Position + new Vector2(r.Size.X, 0);
        Vector2 bl = r.Position + new Vector2(0, r.Size.Y), br = r.Position + r.Size;
        DrawLine(tl, tl + new Vector2(len, 0),  c, w); DrawLine(tl, tl + new Vector2(0, len),  c, w);
        DrawLine(tr, tr - new Vector2(len, 0),  c, w); DrawLine(tr, tr + new Vector2(0, len),  c, w);
        DrawLine(bl, bl + new Vector2(len, 0),  c, w); DrawLine(bl, bl - new Vector2(0, len),  c, w);
        DrawLine(br, br - new Vector2(len, 0),  c, w); DrawLine(br, br - new Vector2(0, len),  c, w);
    }
}
