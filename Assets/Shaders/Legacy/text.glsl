//
// Sprite Shader
//

//
//##Vertex//---------------------------------------------------------------------------------------
//
#version 140
#line ....

// Transform
uniform mat4 modelviewProjection;
uniform float fade;	// global fade

in vec3 vPosition;		// vertex in
in vec4 vColour;
in vec2 vTexcoord;

out vec4 sColour;		// setup (for interpolator) out
out vec2 sTexcoord;

// Main
void main(void)
{
	sTexcoord	= vTexcoord;
	sColour		= vColour;
	sColour.a	*= fade;

	gl_Position = modelviewProjection * vec4(vPosition, 1.0);
}
//####

//
//##Fragment//-------------------------------------------------------------------------------------
//
#version 140
#line ....

in vec4 sColour;		// setup (interpolated) in
in vec2 sTexcoord;

out vec4 fColour;		// fragment out

uniform sampler2D Tex0;
uniform float Saturation;

// Main
void main(void)
{
	vec4 tex0 = texture(Tex0, sTexcoord.st);
    tex0.rgb *= sColour.rgb;

	float luminance = tex0.r * 0.2126 + tex0.g * 0.7152 + tex0.b * 0.0772;
	vec4 grey = vec4( luminance, luminance, luminance, tex0.a );
	
	fColour = mix( grey, tex0, Saturation);
    fColour.rgb *= sColour.a * tex0.a;
    fColour.a = tex0.a * sColour.a;
}
//####
