#include "FastLED.h"

FASTLED_USING_NAMESPACE

// FastLED Demo of lighting effects with companion windows app for control.
//
// -Mod of DemoReel100 by Mark Kriegsman, December 2014

#if FASTLED_VERSION < 3001000
#error "Requires FastLED 3.1 or later; check github for latest code."
#endif

#define DATA_PIN    9
//#define CLK_PIN   4
#define LED_TYPE    TM1803
#define COLOR_ORDER GBR
#define NUM_LEDS    10
CRGB leds[NUM_LEDS];

#define BRIGHTNESS          96
#define FRAMES_PER_SECOND  120

byte incomingByte;
uint16_t gCurrentBrightness = 96;

void setup() {
  delay(3000); // 3 second delay for recovery
  
  // tell FastLED about the LED strip configuration
  FastLED.addLeds<LED_TYPE,DATA_PIN,COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);
  //FastLED.addLeds<LED_TYPE,DATA_PIN,CLK_PIN,COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);

  // set master brightness control
  FastLED.setBrightness(BRIGHTNESS);
  
  Serial.begin(9600);
}


// List of patterns to cycle through.  Each is defined as a separate function below.
typedef void (*SimplePatternList[])();
SimplePatternList gPatterns = { rainbow, rainbowWithGlitter, confetti, sinelon, juggle, bpm };

uint8_t gCurrentPatternNumber = 0; // Index number of which pattern is current
uint8_t gHue = 0; // rotating "base color" used by many of the patterns

bool on = true;
bool cycle = false;

void loop()
{
    if (Serial.available() > 0) {
       incomingByte = Serial.read();
       if (incomingByte == '*') {
         nextPattern();
       } 
       if (incomingByte == '-') {
         if (on){
           on = false;
           FastLED.setBrightness(0);
           FastLED.show();
           returnSignal('b'); 
         }
         else{
           on = true;
           FastLED.setBrightness(gCurrentBrightness);
           returnSignal('a');
         } 
       }
       if (incomingByte == '1') {
         gCurrentPatternNumber = 0;
       }
       if (incomingByte == '2') {
         gCurrentPatternNumber = 1;
       }
       if (incomingByte == '3') {
         gCurrentPatternNumber = 2;
       }
       if (incomingByte == '4') {
         gCurrentPatternNumber = 3;
       }
       if (incomingByte == '5') {
         gCurrentPatternNumber = 4;
       }
       if (incomingByte == '6') {
         gCurrentPatternNumber = 5;
       }
       if (incomingByte == 'B') {
         gCurrentBrightness = (gCurrentBrightness + 10);
         if (gCurrentBrightness > 255)
         {
            gCurrentBrightness = 255; 
         }
         FastLED.setBrightness(gCurrentBrightness);
       }
       if (incomingByte == 'D') {
         gCurrentBrightness = (gCurrentBrightness - 10);
         if (gCurrentBrightness < 0)
         {
           gCurrentBrightness = 0; 
         }
         FastLED.setBrightness(gCurrentBrightness);
       }
       if (incomingByte == 'C'){
         
         if (cycle){
           feedbackFlash();
           feedbackFlash();
           cycle = false;
           returnSignal('d');
         }
         else{
           feedbackFlash();
           cycle = true;
           returnSignal('c');
         } 
         
       }
    }
    // Call the current pattern function once, updating the 'leds' array
    gPatterns[gCurrentPatternNumber]();
    if (on){
      // send the 'leds' array out to the actual LED strip
      FastLED.show();  
      // insert a delay to keep the framerate modest
      FastLED.delay(1000/FRAMES_PER_SECOND); 
    
      // do some periodic updates
      EVERY_N_MILLISECONDS( 20 ) { gHue++; } // slowly cycle the "base color" through the rainbow
      if (cycle){
        EVERY_N_SECONDS( 30 ) { nextPattern(); } // change patterns periodically
      }
    }
}

#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

void nextPattern()
{
  // add one to the current pattern number, and wrap around at the end
  gCurrentPatternNumber = (gCurrentPatternNumber + 1) % ARRAY_SIZE( gPatterns);
}

void rainbow() 
{
  // FastLED's built-in rainbow generator
  fill_rainbow( leds, NUM_LEDS, gHue, 7);
}

void rainbowWithGlitter() 
{
  // built-in FastLED rainbow, plus some random sparkly glitter
  rainbow();
  addGlitter(80);
}

void addGlitter( fract8 chanceOfGlitter) 
{
  if( random8() < chanceOfGlitter) {
    leds[ random16(NUM_LEDS) ] += CRGB::White;
  }
}

void confetti() 
{
  // random colored speckles that blink in and fade smoothly
  fadeToBlackBy( leds, NUM_LEDS, 10);
  int pos = random16(NUM_LEDS);
  leds[pos] += CHSV( gHue + random8(64), 200, 255);
}

void sinelon()
{
  // a colored dot sweeping back and forth, with fading trails
  fadeToBlackBy( leds, NUM_LEDS, 20);
  int pos = beatsin16(13,0,NUM_LEDS);
  leds[pos] += CHSV( gHue, 255, 192);
}

void bpm()
{
  // colored stripes pulsing at a defined Beats-Per-Minute (BPM)
  uint8_t BeatsPerMinute = 127;
  CRGBPalette16 palette = PartyColors_p;
  uint8_t beat = beatsin8( BeatsPerMinute, 64, 255);
  for( int i = 0; i < NUM_LEDS; i++) { //9948
    leds[i] = ColorFromPalette(palette, gHue+(i*2), beat-gHue+(i*10));
  }
}

void juggle() {
  // eight colored dots, weaving in and out of sync with each other
  fadeToBlackBy( leds, NUM_LEDS, 20);
  byte dothue = 0;
  for( int i = 0; i < 8; i++) {
    leds[beatsin16(i+7,0,NUM_LEDS)] |= CHSV(dothue, 200, 255);
    dothue += 32;
  }
}

void returnSignal(byte sig){
  Serial.write(sig);
}

void feedbackFlash() {
   FastLED.setBrightness(255);
   fill_solid(leds, NUM_LEDS, CRGB::White);
   FastLED.show();
   FastLED.delay(100);
   fill_solid(leds, NUM_LEDS, CRGB::Black);
   FastLED.show();
   FastLED.delay(100);
   FastLED.setBrightness(gCurrentBrightness);
}
