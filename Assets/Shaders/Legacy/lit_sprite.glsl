//
// Lit Sprite Shader
//

//
//##Vertex//---------------------------------------------------------------------------------------
//
#version 140
#line ....

// Transform
uniform mat4 modelviewProjection;
uniform mat4 modelview;

in vec3 vPosition;		// vertex in
in vec4 vColour;
in vec2 vTexcoord;

out vec4 sColour;		// setup (for interpolator) out
out vec2 sTexcoord;
out vec2 sPos;

// Main
void main(void)
{
	sTexcoord	= vTexcoord;
	sColour		= vColour;
	sPos		= (modelview * vec4(vPosition, 1.0)).xy;

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
in vec2 sPos;

out vec4 fColour;		// fragment out

uniform sampler2D Tex0;

// Lighting
uniform vec3 BaseLight;
uniform vec3 HalfLight;
uniform vec3 EdgeLight;

uniform float Saturation;
uniform float Intensity;

// Main
void main(void)
{
	// 0 .. 1 depending on distance from screen centre. 0 at edge, 1 at centre
//	float d = (1.0 - (abs(960.0 - sPos.x) / 960.0));
    float d = (1.0 - (abs(sPos.x) / 960.0));

	// light contribution from first span (0.0 to 0.5)
	float dA = clamp( d * 2.0, 0.0, 1.0);
	vec3 spanA = mix( EdgeLight, HalfLight, dA );

	// light contribution from second span (0.5 to 1.0)
	float dB = clamp( (d-0.5) * 2.0, 0.0, 1.0 );
	vec3 spanB = mix( HalfLight, BaseLight, dB );

	vec4 tex0 = texture(Tex0, sTexcoord.st);
    
	fColour = sColour * tex0;
	fColour.rgb *= (d < 0.5) ? spanA : spanB;

	// saturation / intensity control
	float luminance = fColour.r * 0.2126 + fColour.g * 0.7152 + fColour.b * 0.0772;
	vec3 grey = vec3( luminance, luminance, luminance );

	fColour.rgb = mix( grey, fColour.rgb, Saturation ) * Intensity;
}
//####
