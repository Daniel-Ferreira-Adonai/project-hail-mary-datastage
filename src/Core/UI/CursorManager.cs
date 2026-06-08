using Godot;

public partial class CursorManager : Node
{
    public static CursorManager Instance;

    [Export] public Texture2D CorvoCursor;
    [Export] public Texture2D SeuZeCursor;
    [Export] public int  CursorSize   = 108;
    [Export] public float RotationDeg = 45f;   // CCW em tela; 45 = diagonal superior-esquerda
    [Export] public Vector2 Hotspot   = new Vector2(48, 11); // ponta da seta após rotação

    public override void _Ready()
    {
        Instance = this;
        CorvoCursor ??= GD.Load<Texture2D>("res://Data/Cursors/CursorCorvo.png");
        SeuZeCursor ??= GD.Load<Texture2D>("res://Data/Cursors/CursorZé.png");
        SetCorvo();
    }

    public void SetCorvo() => Apply(CorvoCursor);
    public void SetSeuZe() => Apply(SeuZeCursor);

    private void Apply(Texture2D tex)
    {
        if (tex == null)
        {
            GD.PushWarning("CursorManager: textura nula, cursor do SO mantido.");
            return;
        }
        Image img = Resize(tex.GetImage(), CursorSize);
        if (RotationDeg != 0f)
            img = Rotate(img, RotationDeg);
        Input.SetCustomMouseCursor(ImageTexture.CreateFromImage(img),
                                   Input.CursorShape.Arrow, Hotspot);
    }

    private static Image Resize(Image src, int size)
    {
        if (src.GetWidth() == size && src.GetHeight() == size) return src;
        var img = (Image)src.Duplicate();
        img.Resize(size, size, Image.Interpolation.Lanczos);
        return img;
    }

    // Rotação por amostragem inversa (nearest-neighbour).
    // angleDeg > 0  →  conteúdo gira CCW na tela (45° = diagonal superior-esquerda).
    private static Image Rotate(Image src, float angleDeg)
    {
        int w = src.GetWidth(), h = src.GetHeight();
        var dst = Image.Create(w, h, false, src.GetFormat());
        float rad  = Mathf.DegToRad(angleDeg);
        float cosA = Mathf.Cos(rad), sinA = Mathf.Sin(rad);
        float cx = w * 0.5f, cy = h * 0.5f;

        for (int oy = 0; oy < h; oy++)
        {
            float dy = oy - cy;
            for (int ox = 0; ox < w; ox++)
            {
                float dx = ox - cx;
                int sx = Mathf.RoundToInt(cx + dx * cosA + dy * sinA);
                int sy = Mathf.RoundToInt(cy - dx * sinA + dy * cosA);
                if ((uint)sx < (uint)w && (uint)sy < (uint)h)
                    dst.SetPixel(ox, oy, src.GetPixel(sx, sy));
            }
        }
        return dst;
    }
}
