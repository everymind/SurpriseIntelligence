/*
  Analog input, analog output, serial output

 Reads an analog input pin, maps the result to a range from 0 to 255
 and uses the result to set the pulsewidth modulation (PWM) of an output pin.
 Also prints the results to the serial monitor.

 The circuit:
 * potentiometer connected to analog pin 0.
   Center pin of the potentiometer goes to the analog pin.
   side pins of the potentiometer go to +5V and ground
 * LED connected from digital pin 9 to ground

 created 29 Dec. 2008
 modified 9 Apr 2012
 by Tom Igoe

 This example code is in the public domain.

 */

// These constants won't change.  They're used to give names
// to the pins used:
const int analogInPin = A3;  // Analog input pin that the potentiometer is attached to
const int analogLDR = A0;  // Analog input pin that the potentiometer is attached to
const int analogOutPin = 9; // Analog output pin that the LED is attached to
const int ledOutPin = 12;

int sensorValue = 0;        // value read from the pot
int LDR = 0;
int outputValue = 0;        // value output to the PWM (analog out)

const int ledPin =  13;      // the number of the LED pin

// Variables will change:
int ledState = LOW;             // ledState used to set the LED
long previousMillis = 0;        // will store last time LED was updated
long interval = 300;           // interval at which to blink (milliseconds)
void setup() {
  // initialize serial communications at 9600 bps:
  Serial.begin(9600);
 delay(100);
  while (!Serial);
    pinMode(ledPin, OUTPUT);      
    pinMode(ledOutPin, OUTPUT);      
}

void loop() {

  if (Serial.available() > 0)
  {
    char value = Serial.read();
    if (value == 'h') digitalWrite(ledOutPin, HIGH);
    else digitalWrite(ledOutPin, LOW);
  }
  
  // read the analog in value:
  sensorValue = analogRead(analogInPin);
  LDR = analogRead(analogLDR);
  // map it to the range of the analog out:
 //Serial.print("LDR: ");
 //Serial.println(LDR);
  sensorValue = map(sensorValue, 0, 1023, 0, 2023*2);
  //Serial.print("Touch sensor: ");
  //Serial.println(sensorValue);
  if ((LDR > 1000) || (sensorValue<250))
// if ((sensorValue>3900))
  {
    if (interval != 300) 
      Serial.println(1);

     interval = 300; 
  }
  else
  {
    if (interval != 100) 
      Serial.println(0);

    interval = 100; 

  }


    // blink the LED.
  unsigned long currentMillis = millis();
 
  if(currentMillis - previousMillis > interval) {
    // save the last time you blinked the LED 
    previousMillis = currentMillis;   

    // if the LED is off turn it on and vice-versa:
    if (ledState == LOW)
      ledState = HIGH;
    else
      ledState = LOW;

    // set the LED with the ledState of the variable:
    digitalWrite(ledPin, ledState);
  }
  // wait 2 milliseconds before the next loop
  // for the analog-to-digital converter to settle
  // after the last reading:
  delay(2);
}
