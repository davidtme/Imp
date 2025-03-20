#version 300 es
precision highp float;

in vec3 frag_texCoords;
in float frag_depth;
uniform mediump sampler2DArray diffuseTexture;

out vec4 outputColor;

void main() {
    outputColor = texture(diffuseTexture, frag_texCoords);
    gl_FragDepth = outputColor.a > 0.0 ? frag_depth : 1.0;
}