//============================================================================
// Name        : waveInteraction.cpp
// Author      : Andrew Webb
// Date		   : 12/02/2010
// Version     : 1.0
// Copyright   : no copyright
// Description : Program to show interaction between two signals
//				(possibly out of phase, possibly of different signal strength)
//============================================================================

#include <iostream>
#include <GL/gl.h>
#include <GL/glut.h>
#include <string.h>
#include <cmath>
using namespace std;

int windowHeight=700;
int windowWidth=1200;


// number of steps to take when drawing sine wave
const int granularity=1000;
const int cycles=5;
// scaling factor for sin function
const double scalingFactor = (cycles*(2.0*M_PI))/(double)granularity;

// rate at which the phase changes when left/right keys are held
const float phaseChangeRate=0.001;
// starting phase of wave 2
float secondWavePhase=0.95;
// 0 indicates phase is not changing.  -1 or 1 indicates positive or negative change
int changingPhase=0;

const float freqChangeRate=0.001;
int changingFreq=0;
float secondWaveFreq=1.0;

// must be between 0 and 1
// denotes the relative power of signal 1 and signal 2
float amplitudeRatio = 0.0;
// the rate at which the relative power changes
const float amplitudeChangeRate=0.002;
float changingAmplitude=0;
double powerRatio=(1+amplitudeRatio)/(1-amplitudeRatio);
double power2=10;
double power1=powerRatio*10;
double decibels = 10*log10(power1/power2);

double firstWave[granularity];
double secondWave[granularity];
double combinedWave[granularity];

void wavePositions(double wave[], float phase, float amplitude, float freq)
{
	int i;
	for(i=0; i<granularity; i++)
	{
		// this is between 1 and zero
		wave[i]=sin( ((i * scalingFactor) + 2.0*M_PI*phase)*freq )*amplitude;
	}
}

void combinedPositions()
{
	int i;
	for(i=0; i<granularity; i++)
	{
		combinedWave[i]=(firstWave[i]+secondWave[i]);
	}

}

void specialKeys(int key, int x, int y)
{
  switch (key)
  {
    case GLUT_KEY_LEFT :  changingPhase=1;  break;
    case GLUT_KEY_RIGHT:  changingPhase=-1;  break;
    case GLUT_KEY_UP:	  changingAmplitude=1; break;
    case GLUT_KEY_DOWN:   changingAmplitude=-1; break;
    default: break;
  }

}

void specialKeysUp(int key, int x, int y)
{
  switch (key)
  {
    case GLUT_KEY_LEFT :  changingPhase=0;  break;
    case GLUT_KEY_RIGHT:  changingPhase=-0;  break;
    case GLUT_KEY_UP:	  changingAmplitude=0; break;
    case GLUT_KEY_DOWN:   changingAmplitude=-0; break;
    default: break;
  }

}

void drawWave(double wave[], float R, float G, float B, float size)
{
	glLineWidth(size);
	glColor3f(R,G,B);
	// granularity of 1000 but must scale to width of window
	// height scales from -1 to 1
	// first must change this to scaling from 0 to 1
	// then multiply by 9/10th of the window height
	glBegin(GL_LINE_STRIP);
	int i;
	for(i=0; i<granularity; i++)
	{
		glVertex2f( (int)(i/((double)granularity/(double)windowWidth)), ((wave[i]/2.0)*windowHeight*0.44)+windowHeight/2.0);
	}
	glEnd();
}

/* reverse:  reverse string s in place */
void reverse(char s[])
{
    int i, j;
    char c;

    for (i = 0, j = strlen(s)-1; i<j; i++, j--) {
        c = s[i];
        s[i] = s[j];
        s[j] = c;
    }
}

void itoa(int n, char s[])
{
    int i, sign;

    if ((sign = n) < 0)  /* record sign */
        n = -n;          /* make n positive */
    i = 0;
    do {       /* generate digits in reverse order */
        s[i++] = n % 10 + '0';   /* get next digit */
    } while ((n /= 10) > 0);     /* delete it */
    if (sign < 0)
        s[i++] = '-';
    s[i] = '\0';
    reverse(s);
}


void displaydB()
{
	glColor3f(1.0,1.0,1.0);
	glRasterPos2f(windowWidth/100.0, windowHeight/100.0);
	char string[10];
	itoa((int)decibels, string);
	int len = (int) strlen(string);
	int i;
	for (i = 0; i < len; i++)
    {
	  glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, string[i]);
	}
	glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, 'd');
	glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, 'B');

	glRasterPos2f(0.0,0.0);

}

void display()
{
	glClearColor (0.0,0.0,0.0,1.0);	/* clear colour is black */
	glClear(GL_COLOR_BUFFER_BIT);	/* clear */

	drawWave(combinedWave,0.0,0.0,1.0,4.0);
	drawWave(firstWave,1.0,0.0,0.0,3.0);
	drawWave(secondWave,0.0,1.0,0.0,2.0);

	displaydB();

	glutSwapBuffers();
}

// when keys are pressed
// left here as a template for now
void keyboard(unsigned char key, int x, int y)
{
  switch (key)
  {
    case 'm': // m
    	changingFreq=1;
      break;
    case 'n': // n
    	changingFreq=-1;
      break;
    case 'r': //r
    	secondWaveFreq=1.0;
    	amplitudeRatio=0.0;
    	secondWavePhase=0.95;
    	wavePositions(firstWave, 0.0, 1+amplitudeRatio,1.0);
    	wavePositions(secondWave, secondWavePhase, 1-amplitudeRatio,secondWaveFreq);
    	combinedPositions();
    	powerRatio=(1+amplitudeRatio)/(1-amplitudeRatio);
    	power1=powerRatio*10;
    	decibels=10*log10(powerRatio);
    	display();
    	break;
   } // end of switch
} // keyboard()

// when keys are pressed
// left here as a template for now
void keyboardUp(unsigned char key, int x, int y)
{
  switch (key)
  {
    case 'm': // m
    	changingFreq=0;
      break;
    case 'n': // n
    	changingFreq=0;
      break;
   } // end of switch
} // keyboard()

void reshape(int width, int height)
{
	windowHeight=height;
	windowWidth=width;
	glViewport(0, 0, (GLsizei) width, (GLsizei) height);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	// CHANGE THE 0.0s to height(or width)/1.1 to get a good view
	// of the upper right quadrant
	glOrtho(0.0, (GLfloat) width, 0.0, (GLfloat) height, -1.0, 1.0);
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();
}

// main loop
void idleFunction()
{
	if(changingPhase==1 || changingPhase==-1)
	{
		secondWavePhase += phaseChangeRate*(float)changingPhase;
		if(secondWavePhase>1/secondWaveFreq)
			secondWavePhase-=1/secondWaveFreq;
		if(secondWavePhase<0)
			secondWavePhase+=1/secondWaveFreq;

		wavePositions(secondWave, secondWavePhase, 1-amplitudeRatio,secondWaveFreq);
		combinedPositions();
	}
	if(changingAmplitude==1 || changingAmplitude==-1)
	{
		amplitudeRatio += (amplitudeChangeRate*(float)changingAmplitude)*(1-abs(amplitudeRatio));

		if(amplitudeRatio>0.9999)
			amplitudeRatio=0.9999;
		if(amplitudeRatio<-0.9999)
			amplitudeRatio=-0.9999;

		wavePositions(firstWave, 0.0, 1+amplitudeRatio,1.0);
		wavePositions(secondWave, secondWavePhase, 1-amplitudeRatio,secondWaveFreq);
		combinedPositions();

		powerRatio=(1+amplitudeRatio)/(1-amplitudeRatio);
		power1=powerRatio*10;
		decibels=10*log10(powerRatio);

	}

	if(changingFreq==1 || changingFreq==-1)
	{
		secondWaveFreq += (freqChangeRate*(float)changingFreq);

		if(secondWaveFreq>20.0)
			secondWaveFreq=19.999999;
		if(secondWaveFreq<1)
			secondWaveFreq=1.0000001;

		wavePositions(secondWave, secondWavePhase, 1-amplitudeRatio,secondWaveFreq);
		combinedPositions();
	}
	display();
}

int main(int argc, char* argv[])
{
	// initialize the first sine wave
	wavePositions(firstWave, 0.0, 1+amplitudeRatio,1.0);
	wavePositions(secondWave, secondWavePhase, 1-amplitudeRatio,secondWaveFreq);
	combinedPositions();

	glutInit(&argc, argv);                /* Initialize OpenGL */
	glutInitDisplayMode (GLUT_DOUBLE);    /* Set the display mode */
	glutInitWindowSize (windowWidth,windowHeight);         /* Set the window size */
	glutInitWindowPosition (0, 0);    /* Set the window position */
	glutCreateWindow ("Wave Interaction"); 		  /* Create the window */
	glutDisplayFunc(display);             /* Register the "display" function */
	glutReshapeFunc(reshape);             /* Register the "reshape" function */
	glutIdleFunc(idleFunction);			  /* idle function */
	glutKeyboardFunc (keyboard);
	glutKeyboardUpFunc (keyboardUp);
	glutSpecialFunc  (specialKeys);
	glutSpecialUpFunc (specialKeysUp);
	glutMainLoop();                       /* Enter the main OpenGL loop */

	return 0;
}
