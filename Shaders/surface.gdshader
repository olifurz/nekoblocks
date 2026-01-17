shader_type spatial;

uniform sampler2D texture_atlas : source_color;
uniform vec2 atlas_grid = vec2(2.0, 20.0); 

// Surface Indices
instance uniform int surf_top;
instance uniform int surf_bottom;
instance uniform int surf_front;
instance uniform int surf_back;
instance uniform int surf_left;
instance uniform int surf_right;

instance uniform vec4 colour : source_color = vec4(1.0);
instance uniform float roughness = 0.3;

varying vec3 local_pos;
varying vec3 normal;

void vertex() {
    local_pos = VERTEX; 
    normal = NORMAL;
}

void fragment() {
    vec2 custom_uv;
    int surface_id = 0;
    
    vec3 a = abs(normal);

    if (a.y > a.x && a.y > a.z) {
        custom_uv = local_pos.xz;
        surface_id = (normal.y > 0.0) ? surf_top : surf_bottom;
    } else if (a.z > a.x && a.z > a.y) {
        custom_uv = local_pos.xy;
        surface_id = (normal.z < 0.0) ? surf_front : surf_back;
    } else {
        custom_uv = local_pos.zy;
        surface_id = (normal.x < 0.0) ? surf_left : surf_right;
    }

    float row_start = float(surface_id) * 4.0; 
    vec2 tiled_uv = mod(custom_uv, 2.0); 
    
    float col = floor(tiled_uv.x);
    float row = row_start + floor(tiled_uv.y);
    
    vec2 atlas_offset = vec2(col, row);
    vec2 final_uv = (fract(tiled_uv) + atlas_offset) / atlas_grid;
    
    vec4 tex_data = texture(texture_atlas, final_uv);
    
    vec3 background = colour.rgb;
    
    vec3 foreground = tex_data.rgb * colour.rgb;
    
    ALBEDO = mix(background, foreground, tex_data.a);
	ROUGHNESS = roughness;
}