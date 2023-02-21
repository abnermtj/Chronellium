//The MIT License(MIT)

//Copyright(c) 2014 Zach Saw, Shiandow

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files(the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
//coPRISM_PIes of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions :

//The above copyright notice and this permission notice shall be included in all
//coPRISM_PIes or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

/* --- Settings --- */
#define Curves_mode       3 //[0|1|2]3=CIELAB Choose what to apply contrast to. 0 = Luma, 1 = Chroma, 2 = both Luma and Chroma. Default is 0 (Luma)
//#define Curves_contrast 0.15 //[-1.00 to 1.00] The amount of contrast you want

// -- Advanced curve settings --
//6 = positive contrast, 2 = negative contrast
#define Curves_formula     6 //[1|2|3|4|5|6|7|8|9|10] The contrast s-curve you want to use.
							 //1 = Sine, 2 = Abs split, 3 = Smoothstep, 4 = Exp formula, 5 = Simplified Catmull-Rom (0,0,1,1), 6 = Perlins Smootherstep
							 //7 = Abs add, 8 = Techicolor Cinestyle, 9 = Parabola, 10 = Half-circles.
							 //Note that Technicolor Cinestyle is practically identical to Sine, but runs slower. In fact I think the difference might only be due to rounding errors.
							 //I prefer 2 myself, but 3 is a nice alternative with a little more effect (but harsher on the highlight and shadows) and it's the fastest formula.


#define PRISM_PI 3.1415926589
/* ---  Main code --- */

   /*-----------------------------------------------------------.
  /                          Curves                             /
  '-----------------------------------------------------------*/
  /*
	by Christian Cann Schuldt Jensen ~ CeeJay.dk
	Curves, uses S-curves to increase contrast, without clipPRISM_PIng highlights and shadows.
  */


float CurvesPassCIELAB(float colorInput, float Curves_contrast)
{

	 /*-----------------------------------------------------------.
	/               Separation of Luma and Chroma                 /
	'-----------------------------------------------------------*/

	// -- Which value to put through the contrast formula? --
	// I name it x because makes it easier to copy-paste to Graphtoy or Wolfram Alpha or another graphing program
	float x = colorInput; //if the curve should be applied to both Luma and Chroma

 /*-----------------------------------------------------------.
/                     Contrast formulas                       /
'-----------------------------------------------------------*/
	// --Curve 1 --
#if Curves_formula == 1
		x = sin(PRISM_PI * 0.5 * x); // Sin - 721 amd fps, +vign 536 nv
	x *= x;

	//x = 0.5 - 0.5*cos(PRISM_PI*x);
	//x = 0.5 * -sin(PRISM_PI * -x + (PRISM_PI*0.5)) + 0.5;
#endif

// -- Curve 2 --
#if Curves_formula == 2
	x = x - 0.5;
	x = (x / (0.5 + abs(x))) + 0.5;

	//x = ( (x - 0.5) / (0.5 + abs(x-0.5)) ) + 0.5;
#endif

// -- Curve 3 --
#if Curves_formula == 3
  //x = smoothstep(0.0,1.0,x); //smoothstep
	x = x * x*(3.0 - 2.0*x); //faster smoothstep alternative - 776 amd fps, +vign 536 nv
	//x = x - 2.0 * (x - 1.0) * x* (x- 0.5);  //2.0 is contrast. Range is 0.0 to 2.0
#endif

// -- Curve 4 --
#if Curves_formula == 4
	x = (1.0524 * exp(6.0 * x) - 1.05248) / (20.0855 + exp(6.0 * x)); //exp formula
#endif

// -- Curve 5 --
#if Curves_formula == 5
  //x = 0.5 * (x + 3.0 * x * x - 2.0 * x * x * x); //a simplified catmull-rom (0,0,1,1) - btw smoothstep can also be expressed as a simplified catmull-rom using (1,0,1,0)
	x = x * (x * (1.5 - x) + 0.5); //horner form - fastest version

	Curves_contrast = Curves_contrast * 2.0; //I multiply by two to give it a strength closer to the other curves.
#endif

  // -- Curve 6 --
#if Curves_formula == 6
	x = x * x*x*(x*(x*6.0 - 15.0) + 10.0); //Perlins smootherstep
#endif

// -- Curve 7 --
#if Curves_formula == 7
  //x = ((x-0.5) / ((0.5/(4.0/3.0)) + abs((x-0.5)*1.25))) + 0.5;
	x = x - 0.5;
	x = x / ((abs(x)*1.25) + 0.375) + 0.5;
	//x = ( (x-0.5) / ((abs(x-0.5)*1.25) + (0.5/(4.0/3.0))) ) + 0.5;
#endif

// -- Curve 8 --
#if Curves_formula == 8
	x = (x * (x * (x * (x * (x * (x * (1.6 * x - 7.2) + 10.8) - 4.2) - 3.6) + 2.7) - 1.8) + 2.7) * x * x; //Techicolor Cinestyle - almost identical to curve 1
#endif

// -- Curve 9 --
#if Curves_formula == 9
	x = -0.5 * (x*2.0 - 1.0) * (abs(x*2.0 - 1.0) - 2.0) + 0.5; //parabola
#endif

// -- Curve 10 --
#if Curves_formula == 10 //Half-circles

#if Curves_mode == 0
	float xstep = step(x, 0.5);
	float xstep_shift = (xstep - 0.5);
	float shifted_x = x + xstep_shift;
#else
	float3 xstep = step(x, 0.5);
	float3 xstep_shift = (xstep - 0.5);
	float3 shifted_x = x + xstep_shift;
#endif

	x = abs(xstep - sqrt(-shifted_x * shifted_x + shifted_x)) - xstep_shift;

	Curves_contrast = Curves_contrast * 0.5; //I divide by two to give it a strength closer to the other curves.
#endif

// -- Curve 11 --
#if Curves_formula == 11 //(broken and incorrect) Cubic catmull
	float a = 1.00; //control point 1
	float b = 0.00; //start point
	float c = 1.00; //endpoint
	float d = 0.20; //control point 2
	x = 0.5 * ((-a + 3 * b - 3 * c + d)*x*x*x + (2 * a - 5 * b + 4 * c - d)*x*x + (-a + c)*x + 2 * b); //A customizable cubic catmull-rom spline
#endif

	// -- Curve 14 --
#if Curves_formula == 12
	x = 1.0 / (1.0 + exp(-(x * 10.0 - 5.0))); //alternative exp formula
#endif

 /*-----------------------------------------------------------.
/                 Joining of Luma and Chroma                  /
'-----------------------------------------------------------*/
	float color = x;  //if the curve should be applied to both Luma and Chroma
	colorInput = lerp(colorInput, color, Curves_contrast); //Blend by Curves_contrast

//Return the result
	return colorInput;
}

float4 CurvesPass(float4 colorInput, float Curves_contrast)
{
	float3 lumCoeff = float3(0.2126, 0.7152, 0.0722);  //Values to calculate luma with
	float Curves_contrast_blend = Curves_contrast;
	//float PRISM_PI = 3.1415926589; //3.1415926589

	 /*-----------------------------------------------------------.
	/               Separation of Luma and Chroma                 /
	'-----------------------------------------------------------*/

	// -- Calculate Luma and Chroma if needed --
#if Curves_mode != 2

  //calculate luma (grey)
	float luma = dot(lumCoeff, colorInput.rgb);

	//calculate chroma
	float3 chroma = colorInput.rgb - luma;
#endif

	// -- Which value to put through the contrast formula? --
	// I name it x because makes it easier to copy-paste to Graphtoy or Wolfram Alpha or another graphing program
#if Curves_mode == 2
	float3 x = colorInput.rgb; //if the curve should be applied to both Luma and Chroma
#elif Curves_mode == 1
	float3 x = chroma; //if the curve should be applied to Chroma
	x = x * 0.5 + 0.5; //adjust range of Chroma from -1 -> 1 to 0 -> 1
#else // Curves_mode == 0
	float x = luma; //if the curve should be applied to Luma
#endif

 /*-----------------------------------------------------------.
/                     Contrast formulas                       /
'-----------------------------------------------------------*/

// -- Curve 1 --
#if Curves_formula == 1
	x = sin(PRISM_PI * 0.5 * x); // Sin - 721 amd fps, +vign 536 nv
	x *= x;

	//x = 0.5 - 0.5*cos(PRISM_PI*x);
	//x = 0.5 * -sin(PRISM_PI * -x + (PRISM_PI*0.5)) + 0.5;
#endif

// -- Curve 2 --
#if Curves_formula == 2
	x = x - 0.5;
	x = (x / (0.5 + abs(x))) + 0.5;

	//x = ( (x - 0.5) / (0.5 + abs(x-0.5)) ) + 0.5;
#endif

// -- Curve 3 --
#if Curves_formula == 3
  //x = smoothstep(0.0,1.0,x); //smoothstep
	x = x * x*(3.0 - 2.0*x); //faster smoothstep alternative - 776 amd fps, +vign 536 nv
	//x = x - 2.0 * (x - 1.0) * x* (x- 0.5);  //2.0 is contrast. Range is 0.0 to 2.0
#endif

// -- Curve 4 --
#if Curves_formula == 4
	x = (1.0524 * exp(6.0 * x) - 1.05248) / (20.0855 + exp(6.0 * x)); //exp formula
#endif

// -- Curve 5 --
#if Curves_formula == 5
  //x = 0.5 * (x + 3.0 * x * x - 2.0 * x * x * x); //a simplified catmull-rom (0,0,1,1) - btw smoothstep can also be expressed as a simplified catmull-rom using (1,0,1,0)
	x = x * (x * (1.5 - x) + 0.5); //horner form - fastest version

	Curves_contrast_blend = Curves_contrast * 2.0; //I multiply by two to give it a strength closer to the other curves.
#endif

  // -- Curve 6 --
#if Curves_formula == 6
	x = x * x*x*(x*(x*6.0 - 15.0) + 10.0); //Perlins smootherstep
#endif

// -- Curve 7 --
#if Curves_formula == 7
  //x = ((x-0.5) / ((0.5/(4.0/3.0)) + abs((x-0.5)*1.25))) + 0.5;
	x = x - 0.5;
	x = x / ((abs(x)*1.25) + 0.375) + 0.5;
	//x = ( (x-0.5) / ((abs(x-0.5)*1.25) + (0.5/(4.0/3.0))) ) + 0.5;
#endif

// -- Curve 8 --
#if Curves_formula == 8
	x = (x * (x * (x * (x * (x * (x * (1.6 * x - 7.2) + 10.8) - 4.2) - 3.6) + 2.7) - 1.8) + 2.7) * x * x; //Techicolor Cinestyle - almost identical to curve 1
#endif

// -- Curve 9 --
#if Curves_formula == 9
	x = -0.5 * (x*2.0 - 1.0) * (abs(x*2.0 - 1.0) - 2.0) + 0.5; //parabola
#endif

// -- Curve 10 --
#if Curves_formula == 10 //Half-circles

#if Curves_mode == 0
	float xstep = step(x, 0.5);
	float xstep_shift = (xstep - 0.5);
	float shifted_x = x + xstep_shift;
#else
	float3 xstep = step(x, 0.5);
	float3 xstep_shift = (xstep - 0.5);
	float3 shifted_x = x + xstep_shift;
#endif

	x = abs(xstep - sqrt(-shifted_x * shifted_x + shifted_x)) - xstep_shift;

	Curves_contrast_blend = Curves_contrast * 0.5; //I divide by two to give it a strength closer to the other curves.
#endif

// -- Curve 11 --
#if Curves_formula == 11 //(broken and incorrect) Cubic catmull
	float a = 1.00; //control point 1
	float b = 0.00; //start point
	float c = 1.00; //endpoint
	float d = 0.20; //control point 2
	x = 0.5 * ((-a + 3 * b - 3 * c + d)*x*x*x + (2 * a - 5 * b + 4 * c - d)*x*x + (-a + c)*x + 2 * b); //A customizable cubic catmull-rom spline
#endif

	// -- Curve 14 --
#if Curves_formula == 12
	x = 1.0 / (1.0 + exp(-(x * 10.0 - 5.0))); //alternative exp formula
#endif

 /*-----------------------------------------------------------.
/                 Joining of Luma and Chroma                  /
'-----------------------------------------------------------*/

#if Curves_mode == 2 //Both Luma and Chroma
	float3 color = x;  //if the curve should be applied to both Luma and Chroma
	colorInput.rgb = lerp(colorInput.rgb, color, Curves_contrast_blend); //Blend by Curves_contrast

#elif Curves_mode == 1 //Only Chroma
	x = x * 2.0 - 1.0; //adjust the Chroma range back to -1 -> 1
	float3 color = luma + x; //Luma + Chroma
	colorInput.rgb = lerp(colorInput.rgb, color, Curves_contrast_blend); //Blend by Curves_contrast
#else // Curves_mode == 0 //Only Luma
	x = lerp(luma, x, Curves_contrast_blend); //Blend by Curves_contrast
	colorInput.rgb = x + chroma; //Luma + Chroma

#endif

//Return the result
	return colorInput;
}


// Interpolates from 0 to 1 with slope of k at x=0 and 1/k at x=1.
// k must be greater than zero.
// k<1: ease in, k=1: lerp, k>1: ease out
float rxEase(float x, float k)
{

	//k = 5.;
	k = abs(k);
	k = clamp(k, 0.0001, 10000.0) - 1.0; // clamp optional, if you know your k
	x = clamp(x, 0.0, 1.0);
	float kx = k * x;
	return (x + kx) / (kx + 1.0);
}

float3 CurvesPassThresh(float3 colorInput, float Curves_contrast)
{
	float3 lumCoeff = float3(0.2126, 0.7152, 0.0722);  //Values to calculate luma with
	float Curves_contrast_blend = Curves_contrast;

	float3 x = colorInput.rgb;

	//x = x * x*(3.0 - 2.0*x);
	x *= rxEase(Curves_contrast, colorInput.r);
	return x;

	float3 color = x;  //if the curve should be applied to both Luma and Chroma
	colorInput.rgb = lerp(colorInput.rgb, color, Curves_contrast_blend); //Blend by Curves_contrast

	return color;
}