//
// Particle Shader
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
in vec4 vMisc;			// offset and rotation

out vec4 sColour;		// setup (for interpolator) out
out vec2 sTexcoord;

// Main
void main(void)
{
	sTexcoord	= vTexcoord;
	sColour		= vColour;
	sColour.a	*= fade;

	float s = sin( vMisc.w );
	float c = cos( vMisc.w );

	mat2 rotmat = mat2( c, -s, s, c );
	vec3 offset = vec3( vMisc.xy * rotmat, vMisc.z );
	
	gl_Position = modelviewProjection * vec4( vPosition + offset, 1.0 );
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

// Main
void main(void)
{
	vec4 tex0 = texture(Tex0, sTexcoord.st);
	tex0.rgb *= tex0.a;	// pre-multiply
	fColour = sColour * tex0;
}
//####
