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

// lighting overlay gradient
uniform vec4 GradA;
uniform vec4 GradB;
uniform float YGradientStartLine; 

// wash colour
uniform vec4 Wash;

//lighting RS matrix
uniform mat2 lightmat;

in vec3 vPosition;		// vertex in
in vec4 vColour;
in vec2 vTexcoord;
in vec4 vMisc;

out vec4 sGradient;		// setup (for interpolator) out
out vec4 sWash;
out vec4 sColour;
out vec2 sTexcoord;
out vec2 sPos;

// Main
void main(void)
{
	// map gradient to -1..1 range
	vec2 gradPos;
	gradPos.x = (vPosition.x - vMisc.x) / (vMisc.y - vMisc.x) * 2.0 - 1.0;
//	gradPos.y = (vPosition.y - vMisc.z) / (vMisc.w - vMisc.z) * 2.0 - 1.0;
    gradPos.y = ( YGradientStartLine - vPosition.y ) / (YGradientStartLine - 30) * 2.0 - 1.0;
	
	// rotate|scale
	gradPos = lightmat * gradPos;

	// map to 0..1 range
	gradPos = gradPos * 0.5 + 0.5;

	sTexcoord	= vTexcoord;
	sWash		= vColour * Wash;
	sColour		= vColour;
	sGradient	= mix( GradA, GradB, gradPos.y );
	sPos		= (modelview * vec4(vPosition, 1.0)).xy;

	gl_Position = modelviewProjection * vec4(vPosition, 1.0);
}
//####

//
//##Fragment//-------------------------------------------------------------------------------------
//
#version 140
#line ....

in vec4 sGradient;		// setup (interpolated) in
in vec4 sWash;
in vec4 sColour;
in vec2 sTexcoord;
in vec2 sPos;

out vec4 fColour;		// fragment out

uniform sampler2D Tex0;
uniform sampler3D TintColourCube;

uniform vec3 BaseLight;
uniform vec3 HalfLight;
uniform vec3 EdgeLight;

uniform vec2 LightRange;

uniform vec3 LightPosition;
uniform vec2 LightFalloffStart;
uniform vec2 LightFalloffDistance;

uniform float Saturation;
uniform float Intensity;
uniform float LightScalar;

const float lut_size = 16.0;
const vec3 scale = vec3( ( lut_size - 1.0 ) / lut_size );
const vec3 offset = vec3( 1.0 / ( 2.0 * lut_size ) );


const vec2 light_position = vec2( 960.0, 270.0 );

//const float light_x_1falloff_start = 200.0;
//const float light_x1_falloff_distance = 800.0;
//const float light_y1_falloff_start = 100.0;
//const float light_y_1falloff_distance = 300.0;


// Photoshop 'Overlay' blend math for a single channel
// FIXME: remove branch, use conditional move instead
float overlayf( float base, float blend )
{
	if( base < 0.5 ) {
		return 2.0 * base * blend;
	}
	else {
		return 1.0 - 2.0 * (1.0 - base) * (1.0 - blend);
	}
}

// ...and for rgb
vec3 overlay( vec3 base, vec3 blend )
{   
	return vec3( overlayf(base.r, blend.r), overlayf(base.g, blend.g), overlayf(base.b, blend.b) );
}


// Main
void main(void)
{
	// Lit texture
	vec4 tex0 = texture( Tex0, sTexcoord.st);

    // Remap the colours for the character before
    // any lightning is calculated or any other effects
    // are applied.
    tex0.xyz = texture( TintColourCube, scale * tex0.xyz + offset ).rgb;

	// vertex colour
	tex0.rgb *= sColour.rgb;

	// final lighting
    //tex0.rgb *= (d < 0.5) ? spanA : spanB;

    vec2 light_vector = sPos - vec2( LightPosition.x, LightPosition.y );
    vec2 eclipse = vec2( abs( light_vector.x ) - LightFalloffStart.x, abs( light_vector.y ) - LightFalloffStart.y );
    eclipse = clamp( eclipse, vec2( 0.0, 0.0 ), LightFalloffDistance );
    eclipse = eclipse * eclipse;
    eclipse /= LightFalloffDistance * LightFalloffDistance;
    float light_intensity = clamp( 1.0 - sqrt( eclipse.x + eclipse.y ), 0.0, 1.0 );
    tex0.rgb *= BaseLight * light_intensity * Intensity * LightScalar;

	// mix lit texture and wash
	fColour.rgb = mix( tex0.rgb, tex0.rgb * sWash.rgb, sWash.a );
	

	// mix previous result and overlay
	fColour.a = tex0.a * sColour.a;
    fColour.rgb *= sColour.a;
}
//####
