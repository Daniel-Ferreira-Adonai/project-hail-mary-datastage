using Godot;


public static class EnemyScaler
{
    private static readonly System.Collections.Generic.Dictionary<EnemySize, float> TargetHeights = new()
    {
        { EnemySize.Tiny,   80f },
        { EnemySize.Small,  120f },
        { EnemySize.Medium, 220f },
        { EnemySize.Large,  250f },
        { EnemySize.Boss,   500f }
    };
    
    private const float MaxWidth = 300f;
    private const float BossWidth = 500f;

   
   public static Vector2 CalculateScale(Texture2D sprite, EnemySize size, EnemyData data = null)
{
    if (sprite == null)
        return Vector2.One;

    Vector2 spriteSize = sprite.GetSize();
    
    float targetHeight = size == EnemySize.Custom && data != null && data.CustomTargetHeight > 0
        ? data.CustomTargetHeight
        : TargetHeights.ContainsKey(size) ? TargetHeights[size] : 220f;

    float maxWidth = size == EnemySize.Boss ? BossWidth
        : size == EnemySize.Custom && data != null && data.CustomMaxWidth > 0 ? data.CustomMaxWidth
        : MaxWidth;
    float scaleByHeight = targetHeight / spriteSize.Y;
    float resultingWidth = spriteSize.X * scaleByHeight;

    if (resultingWidth > maxWidth)
    {
        float scaleByWidth = maxWidth / spriteSize.X;
        return new Vector2(scaleByWidth, scaleByWidth);
    }

    return new Vector2(scaleByHeight, scaleByHeight);
}
    
  
public static float CalculateDisplayWidth(Texture2D sprite, EnemySize size, EnemyData data = null)
{
    if (sprite == null)
        return TargetHeights.ContainsKey(size) ? TargetHeights[size] : 220f;
    
    Vector2 scale = CalculateScale(sprite, size, data);
    return sprite.GetSize().X * scale.X;
}

public static float CalculateDisplayHeight(Texture2D sprite, EnemySize size, EnemyData data = null)
{
    if (sprite == null)
        return TargetHeights.ContainsKey(size) ? TargetHeights[size] : 220f;
    
    Vector2 scale = CalculateScale(sprite, size, data);
    return sprite.GetSize().Y * scale.Y;
}
    
   
    public static float GetIdealSpacing(EnemySize largestSize)
    {
        return largestSize switch
        {
            EnemySize.Tiny => 40f,
            EnemySize.Small => 40f,
            EnemySize.Medium => 40f,
            EnemySize.Large => 60f,
            EnemySize.Boss => 100f,
            _ => 80f
        };
    }
}