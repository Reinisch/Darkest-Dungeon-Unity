//
// Greyscale Shader
//

//
//##Vertex//---------------------------------------------------------------------------------------
//
#version 140
#line ....

// Transform
uniform mat4 modelviewProjection;

in vec3 vPosition;		// vertex in
in vec2 vTexcoord;

out vec2 sTexcoord;		// setup (for interpolator) out	

void main(void)
{
	sTexcoord	= vTexcoord;

	gl_Position = vec4(vPosition, 1.0);
}
//####

//
//##Fragment//-------------------------------------------------------------------------------------
//
#version 140
#line ....

in vec2 sTexcoord;		// setup (interpolated) in

out vec4 fColour;		// fragment out

uniform sampler2D Tex0;
uniform float Saturation;
uniform float Intensity;

void main(void)
{
	vec4 screen = texture(Tex0, sTexcoord.st);

	float luminance = screen.r * 0.2126 + screen.g * 0.7152 + screen.b * 0.0772;
	vec3 grey = vec3( luminance, luminance, luminance );

	fColour.rgb = mix( grey, screen.rgb, Saturation ) * Intensity;
	fColour.a = 1.0;
}
//####
