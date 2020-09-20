#include <TheThingsNetwork.h>
//https://www.thethingsnetwork.org/docs/devices/arduino/usage.html
// OTAA or ABP keys
const char *appEui = "70B3D57ED0034F18";
const char *appKey = "849E49C3D6146017302135F9B31337BC";

#define loraSerial Serial1
#define debugSerial Serial

// Replace REPLACE_ME with TTN_FP_EU868 or TTN_FP_US915
#define freqPlan TTN_FP_IN865_867 //for india

TheThingsNetwork ttn(loraSerial, debugSerial, freqPlan);

void setup() {
  loraSerial.begin(57600);
  debugSerial.begin(9600);

  // OTAA or ABP activation
 ttn.join(appEui, appKey);
 ttn.onMessage(message);
}

void loop() {

  // Send and receive messages
  byte data[4] = {10,01,10,01}; //read this from sensor
/* data[0] = (digitalRead() == HIGH) ? 1 : 0;
 data[1] = (digitalRead(LED_BUILTIN) == HIGH) ? 1 : 0;
 data[2] = (digitalRead() == HIGH) ? 1 : 0;
 data[3] = (digitalRead(LED_BUILTIN) == HIGH) ? 1 : 0;
 */
  ttn.sendBytes(data, sizeof(data));

  delay(10000);
}
void message(const byte* payload, size_t length, port_t port) {
  debugSerial.println("-- MESSAGE");
  debugSerial.print("Received " + String(length) + " bytes on port " + String(port) + ":");
    
  for (int i = 0; i < length; i++) {
    debugSerial.print(" " + String(payload[i]));
  }
    
  debugSerial.println();
}