shader_type canvas_item;

uniform vec2 shadow_offset = vec2(4.0, 12.0);
uniform float shadow_alpha = 0.1;
uniform vec4 shadow_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    vec2 shadow_uv = UV - shadow_offset * TEXTURE_PIXEL_SIZE;
    
    vec4 shadow = texture(TEXTURE, shadow_uv);
    shadow.rgb = shadow_color.rgb;
    shadow.a *= shadow_alpha;
    
    vec4 original = texture(TEXTURE, UV);
    
    COLOR = mix(shadow, original, original.a);
}