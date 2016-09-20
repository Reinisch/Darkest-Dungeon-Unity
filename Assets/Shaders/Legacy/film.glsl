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
uniform sampler3D TorchColourGradeLUTStart;
uniform sampler3D TorchColourGradeLUTEnd;
uniform sampler3D PresentationGradeLUTOverride;

const float lut_size = 16.0;
const vec3 scale = vec3( ( lut_size - 1.0 ) / lut_size );
const vec3 offset = vec3( 1.0 / ( 2.0 * lut_size ) );
uniform float Time;
uniform float Intensity;
uniform float TorchColourT;
uniform float PresentationGradeOverrideTime;

void main(void)
{
	vec4 screen = texture(Tex0, sTexcoord.st);

	vec3 source_colour = scale * screen.xyz + offset;
    vec3 start_colour = texture( TorchColourGradeLUTStart, source_colour ).rgb;
    vec3 end_colour = texture( TorchColourGradeLUTEnd, source_colour ).rgb;
    screen.rgb = mix( start_colour, end_colour, TorchColourT );

    vec3 presentation_override_colour = texture( PresentationGradeLUTOverride, source_colour ).rgb;

    screen.rgb = mix( screen.rgb, presentation_override_colour, PresentationGradeOverrideTime );

	float x = sTexcoord.x * sTexcoord.y * (Time * 100.0);
	x = mod( x, 13.0 ) * mod( x, 123.0 );
	float dx = mod( x, 0.01 );

	vec3 result = screen.rgb + screen.rgb * clamp( 0.1 + dx * 100.0, 0.0, 1.0 );

	fColour.rgb = mix(screen.rgb, result, Intensity);
	fColour.a = 1.0;
}
//####
