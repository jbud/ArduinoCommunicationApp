#include "FastLED.h"

FASTLED_USING_NAMESPACE


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
#define RAINBOW_SPEED       20

#define P_RAINBOW             0
#define P_RAINBOWWITHGLITTER  1
#define P_CONFETTI            2
#define P_SINELON             3
#define P_SINELON2            4
#define P_SINELON3            5
#define P_JUGGLE              6
#define P_BPM                 7
#define P_FIRE                8
#define P_CLR                 9
#define P_KLAXON              10


// COOLING: How much does the air cool as it rises?
// Less cooling = taller flames.  More cooling = shorter flames.
// Default 50, suggested range 20-100 
#define COOLING  75

// SPARKING: What chance (out of 255) is there that a new spark will be lit?
// Higher chance = more roaring fire.  Lower chance = more flickery fire.
// Default 120, suggested range 50-200.
#define SPARKING 75

char incomingString[50];
char hh[6];
String incomingStr;
char ccc[50];
uint16_t gCurrentBrightness = 255;
unsigned long hexo = 0x0;
unsigned long hexor = 0x0;
void setup() {
  delay(3000); // 3 second delay for recovery
  
  // tell FastLED about the LED strip configuration
  FastLED.addLeds<LED_TYPE,DATA_PIN,COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);

  // set master brightness control
  FastLED.setBrightness(BRIGHTNESS);
  
  Serial.begin(9600);
}


// List of patterns to cycle through.  Each is defined as a separate function below.
typedef void (*SimplePatternList[])();
SimplePatternList gPatterns = { 
  rainbow, 
  rainbowWithGlitter, 
  confetti, 
  sinelon, 
  sinelon2, 
  sinelon3, 
  juggle, 
  bpm, 
  fire, 
  clr, 
  klaxon 
};

uint8_t gCurrentPatternNumber = 0; // Index number of which pattern is current
uint8_t gHue = 0; // rotating "base color" used by many of the patterns

bool on = true;
bool cycle = false;

int pos = 0;


void loop()
{
    random16_add_entropy( random());
    
    if (Serial.available() > 0) {
       incomingStr = Serial.readString();
       incomingStr.toCharArray(incomingString,50);
       returnSignal(incomingString);
       if (incomingString[0]== '*') {
         nextPattern();
       } 
       if (incomingString[0] == '-') {
         if (on){
           on = false;
           FastLED.setBrightness(0);
           FastLED.show();
           returnSignal("b"); 
         }
         else{
           on = true;
           FastLED.setBrightness(gCurrentBrightness);
           returnSignal("a");
         } 
       }
       if (incomingString[0] == '1') {
         gCurrentPatternNumber = P_RAINBOW;
       }
       if (incomingString[0] == '2') {
         gCurrentPatternNumber = P_RAINBOWWITHGLITTER;
       }
       if (incomingString[0] == '3') {
         gCurrentPatternNumber = P_CONFETTI;
       }
       if (incomingString[0] == '4') {
         gCurrentPatternNumber = P_SINELON;
       }
       if (incomingString[0] == '5') {
         gCurrentPatternNumber = P_SINELON2;
       }
       if (incomingString[0] == '6') {
         gCurrentPatternNumber = P_SINELON3;
       }
       if (incomingString[0] == '7') {
         gCurrentPatternNumber = P_JUGGLE;
       }
       if (incomingString[0] == '8') {
         gCurrentPatternNumber = P_BPM;
       }
       if (incomingString[0] == '9') {
         gCurrentPatternNumber = P_FIRE;
       }
       if (incomingString[0] == 'K') {
         gCurrentPatternNumber = P_KLAXON;
       }
       
       if (incomingString[0] == 'C'){
         
         if (cycle){
           feedbackFlash();
           feedbackFlash();
           cycle = false;
           returnSignal("d");
         }
         else{
           feedbackFlash();
           cycle = true;
           returnSignal("c");
         } 
         
       }
       if (incomingString[0] == 'B'){
         char nn[3];
         uint16_t ii;
         // incomingstring = char* {B, ,2,5,5}
         nn[0]=incomingString[2];
         nn[1]=incomingString[3];
         nn[2]=incomingString[4];
         ii = atol(nn);
         gCurrentBrightness = ii;
         FastLED.setBrightness(gCurrentBrightness);
       }
       if (incomingString[0] == 'R'){
           char n[8], m[8];
           char *p, *q;
           // incomingString = char* {R,G,B, ,0,0,0,0,0,0}
           n[0] = incomingString[4];
           n[1] = incomingString[5];
           n[2] = incomingString[6];
           n[3] = incomingString[7];
           n[4] = incomingString[8];
           n[5] = incomingString[9];
           // Rearrange bits to GGRRBB (only needed for my setup)
           m[0] = incomingString[6];
           m[1] = incomingString[7];
           m[2] = incomingString[4];
           m[3] = incomingString[5];
           m[4] = incomingString[8];
           m[5] = incomingString[9];
           
           hexo = strtoul(n, &p, 16);
           hexor = strtoul(m, &q, 16);
           returnSignal(n);
           gCurrentPatternNumber = P_CLR;
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
      EVERY_N_MILLISECONDS( 60 ) { pos++; }
      EVERY_N_MILLISECONDS( RAINBOW_SPEED ) { gHue++; } // slowly cycle the "base color" through the rainbow
      if (cycle){
        EVERY_N_SECONDS( 30 ) { nextPattern(); } // change patterns periodically
      }
    }
}

#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

void clr()
{
  for( int i = 0; i < 20; i++) {
    leds[i]= hexo;
  }
  // Render rearranged bits to GGRRBB (only needed for my setup)
  for( int i = 10; i < 20; i++) {
    leds[i]= hexor;
  }
  FastLED.show();
}
void fire()
{
// Array of temperature readings at each simulation cell
  static byte heat[NUM_LEDS];

  // Step 1.  Cool down every cell a little
    for( int i = 0; i < NUM_LEDS; i++) {
      heat[i] = qsub8( heat[i],  random8(0, ((COOLING * 10) / NUM_LEDS) + 2));
    }
  
    // Step 2.  Heat from each cell drifts 'up' and diffuses a little
    for( int k= NUM_LEDS - 1; k >= 2; k--) {
      heat[k] = (heat[k - 1] + heat[k - 2] + heat[k - 2] ) / 3;
    }
    
    // Step 3.  Randomly ignite new 'sparks' of heat near the bottom
    if( random8() < SPARKING ) {
      int y = random8(7);
      heat[y] = qadd8( heat[y], random8(160,255) );
    }

    // Step 4.  Map from heat cells to LED colors
    for( int j = 0; j < NUM_LEDS; j++) {
        leds[j] = HeatColor( heat[j]);
    }
}

void nextPattern()
{
  // add one to the current pattern number, and wrap around at the end
  gCurrentPatternNumber = (gCurrentPatternNumber + 1) % ARRAY_SIZE(gPatterns);
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
uint16_t snln3_i = 0;

int prevpossnln3 = 0;

void sinelon3(){
  fadeToBlackBy( leds, NUM_LEDS, 10);
  int l = 8;
  if (pos != prevpossnln3){ 
     snln3_i++;
     prevpossnln3 = pos;
  }
  if (snln3_i >= l){
     snln3_i = 0; 
  }
  leds[snln3_i] += CHSV( gHue, 255, gCurrentBrightness);
}
void klaxon(){
  fadeToBlackBy( leds, NUM_LEDS, 10);
  int l = 8;
  if (pos != prevpossnln3){ 
     snln3_i++;
     prevpossnln3 = pos;
  }
  if (snln3_i >= l){
     snln3_i = 0; 
  }
  leds[snln3_i] += CHSV( 0, 255, gCurrentBrightness); //red
}
void sinelon2()
{
  // a colored dot sweeping back and forth, with fading trails
  fadeToBlackBy( leds, NUM_LEDS, 10);
  int pos = beatsin16(26,0,4);
  leds[pos] += CHSV( gHue, 255, 192);
  int pos2 = beatsin16(26,4,8);
  leds[pos2] += CHSV( gHue, 255, 192);
}
void sinelon()
{
  // a colored dot sweeping back and forth, with fading trails
  fadeToBlackBy( leds, NUM_LEDS, 20);
  int pos = beatsin16(26,0,NUM_LEDS);
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
void sendsig(uint16_t i){
  char out[50];
  sprintf(out, "pos: %x", i);
  Serial.write(out); 
}
void returnSignal(char* sig){
  char out[50];
  sprintf(out, "rcv: %s", sig);
  Serial.println(out);
}
void returnSignal2(unsigned long sig){
  char out[50];
  sprintf(out, "rcv: %x", sig);
  Serial.println(out);
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
