//
// Film Shader
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
uniform sampler3D FirstColourGradeLUT;
uniform sampler3D SecondColourGradeLUT;
uniform sampler3D DesatColourGradeLUT;

const float lut_size = 16.0;
const vec3 scale = vec3( ( lut_size - 1.0 ) / lut_size );
const vec3 offset = vec3( 1.0 / ( 2.0 * lut_size ) );
uniform float Time;
uniform float Intensity;
uniform float TransitionTime; // 0.0 - 1.0
uniform float DesaturateTransitionTime;

void main(void)
{
	vec4 screen = texture(Tex0, sTexcoord.st);

	vec3 source_colour = scale * screen.xyz + offset;
    vec3 start_colour = texture( FirstColourGradeLUT, source_colour ).rgb;
    vec3 end_colour = texture( SecondColourGradeLUT, source_colour ).rgb;
    vec3 full_colour= mix( start_colour, end_colour, TransitionTime );

    vec3 desat_colour = texture( DesatColourGradeLUT, full_colour ).rgb;

    screen.rgb = mix( full_colour, desat_colour, DesaturateTransitionTime );

	fColour = screen;
}
//####
