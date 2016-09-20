//
// Popup text Shader
//

//
//##Vertex//---------------------------------------------------------------------------------------
//
#version 140
#line ....

// Transform
uniform mat4 modelviewProjection;

in vec3 vPosition;		// vertex in
in vec4 vColour;
in vec2 vTexcoord;
in vec4 vMisc;

out vec4 sFillColour;		// setup (for interpolator) out
out vec4 sOutlineColour;
out vec2 sTexcoord;

// Main
void main(void)
{
	sTexcoord		= vTexcoord;
	sFillColour		= vColour;
	sOutlineColour	= vMisc;

	gl_Position = modelviewProjection * vec4(vPosition, 1.0);
}
//####

//
//##Fragment//-------------------------------------------------------------------------------------
//
#version 140
#line ....

in vec4 sFillColour;		// setup (interpolated) in
in vec4 sOutlineColour;
in vec2 sTexcoord;

out vec4 fColour;		// fragment out

uniform sampler2D Tex0;
uniform float fade;

// Main
void main(void)
{
	vec4 tex = texture(Tex0, sTexcoord.st);
	float grey = (tex.r + tex.g + tex.b) / 3.0f;

	fColour = mix( sOutlineColour, sFillColour, tex );

	fColour.a = tex.a * fade * mix( sOutlineColour.a, sFillColour.a, grey );
    fColour.rgb *= fColour.a;
}
//####
