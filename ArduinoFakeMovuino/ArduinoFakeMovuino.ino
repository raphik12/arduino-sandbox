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

const int analogInPin0 = A0;
const int analogInPin1 = A1;

int sensorValue0 = 0;
int sensorValue1 = 0;

bool sendData = false;

void setup() {
  // initialize serial communications at 9600 bps:
  Serial.begin(9600);
}

void loop() {
  
  char dasso = Serial.read();
  
  if(dasso == 'l')
  {
    sendData = true;
  }
  else if(dasso == 'L')
  {
    sendData = false;
  }
  
  if(sendData)
  {  
    // read the analog in value:
    sensorValue0 = analogRead(analogInPin0);
    sensorValue1 = analogRead(analogInPin1);
  
    String output = "S: 26 F: l g 840691 ";
  //    String output = "";
    output += sensorValue0;
    output += " ";
    output += sensorValue1;
    output += " -212";
  
    Serial.println(output);
  
    // wait 100 milliseconds before the next loop
    // for the analog-to-digital converter to settle
    // after the last reading:
    delay(100);
  }
}
