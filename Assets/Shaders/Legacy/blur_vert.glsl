//
// Separated Guassian Blur (vertical) Shader
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

uniform vec2 FBO_size;
uniform float Amount;

uniform float offset[3] = float[]( 0.0, 1.3846153846, 3.2307692308 );
uniform float weight[3] = float[]( 0.2270270270, 0.3162162162, 0.0702702703 );
 
void main(void)
{
	vec4 fragment = texture(Tex0, sTexcoord.st);
	vec4 blurred = fragment * weight[0];

	for( int i = 1; i < 3; i++ )
	{
		blurred += texture( Tex0, (vec2(gl_FragCoord) + vec2(0.0, offset[i])) / FBO_size ) * weight[i];
		blurred += texture( Tex0, (vec2(gl_FragCoord) - vec2(0.0, offset[i])) / FBO_size ) * weight[i];
	}

	fColour.rgb = mix( fragment.rgb, blurred.rgb, Amount );
	fColour.a = 1.0;
}
//####
