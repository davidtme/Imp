#version 300 es
precision highp float;

in vec4 vertex;
uniform vec3 offset;
in vec4 position;
in vec4 texturePosition;
in float textureLayer;
in float depth;

uniform vec2 outputResolution;
uniform vec2 textureResolution;

out vec3 frag_texCoords;
out float frag_depth;

void main(void)
{
    // y might need to be flipped
    frag_texCoords =
        vec3(
            mix(vec2(0.0, 0.0), vec2(1.0, 1.0), (texturePosition.xy + (texturePosition.zw * vertex.zw)) / textureResolution),
            textureLayer);

    float x = mix(-1.0, 1.0, (offset.x + position.x + (position.z * vertex.x)) / outputResolution.x);
    float y = mix(1.0, -1.0, (offset.y + position.y + (position.w * (1.0 - vertex.y))) / outputResolution.y);

    gl_Position = vec4(x, y, 0.0, 1.0);
    frag_depth = offset.z + depth;
}