#include <SPI.h>
#include <WiFiNINA.h>
#include <ArduinoJson.h>
#include "src/WiFiInfo.h"
#include "src/RouteInfo.h"

// Boiler plate from:
// https://www.okdo.com/project/get-started-with-arduino-nano-33-iot/
char ssid[] = WIFI_SSID;
char pass[] = WIFI_PASS;
char piAddress[] = RASP_PI_LOCAL_IP;
char sonoffAddress[] = SONOFF_LOCAL_IP;
int status = WL_IDLE_STATUS;
char piEndpoint[] = "/api/v1/system/temperature";
char sonoffEndpoint[] = "";
const int requestTimeout = 10000; // MS?
const int thresholdTemperature = 75;
WiFiClient client;

/**
 * Setup script - Called once when the arduino
 * powers on.
 */
void setup() {
  // Setting up to make it blink.
  pinMode(LED_BUILTIN, OUTPUT);

  // Setup pattern (3 quick blinks)
  blinkPattern(100, 3);
  
  // Initialize serial and wait for port to open:
  Serial.begin(9600);
  
  while (!Serial) {

    // Blink once a second while waiting.
    blinkPattern(1000, 1);
  }
  
  connectToAP();    // Connect to Wifi Access Point
  printWifiStatus();
}

/**
 * Sets up a blink pattern as defined by inputs
 * @param timeout - int - How long to wait (ms) between blinks
 * @param repetition - int - How many blinks to do
 */
void blinkPattern(int timeout, int repetition){
  for (int i=0; i<repetition; i++) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(timeout);
    digitalWrite(LED_BUILTIN, LOW);
    delay(timeout);
  }
}

/**
 * Main loop. Runs code defined every <requestTimeout> ms.
 */
void loop() {
  // Blinking the LED
  blinkPattern(500, 2);
  
  Serial.println("Sending temperature read request.");
  // == Request Code ==

  // Make request to piEndpoint
  StaticJsonDocument<100> json;
  char* httpResponse = { makeHttpRequest(piAddress, piEndpoint) };

  // Parse Temperature.
  DeserializationError error = deserializeJson(json, httpResponse);

  if (error.f_str() != "EmptyInput") {
    // Stopping execution if JSON error is encountered.
    Serial.println("Error Deserializing JSON");
    Serial.println(error.f_str());
    return;
  }
  // Over thresholdTemperature ? Request to sonoffEndpoint : Continue;
  long temp = json["temperature"];
  const char* unit = json["unit"];

  Serial.println("Outputs: ");
  Serial.println(temp);
  Serial.println(unit);

//  switch (unit){
//    case "Celcius":
//      if (temp > 70) {
//        Serial.println("Temperature Exceeds 70C. Shutting down Pi");
//      }
//      break;
//    case "Kelvin":
//      if (temp > 343) {
//        Serial.println("Temperature Exceeds 343K. Shutting down Pi");
//      }
//      break;
//    case "Fahrenheit":
//      if (temp > 158) {
//        Serial.println("Temperature Exceeds 158F. Shutting down Pi");
//      }
//      break;
//  }

  // Wait until the next shot. Not reached if request sent to sonoff.
  delay(requestTimeout);
}

/**
 * Outputs WiFi status to screen
 */
void printWifiStatus() {
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  IPAddress ip = WiFi.localIP(); // Device IP address

  Serial.print("IP Address: ");
  Serial.println(ip);
}

/**
 * Connects to the SSID using the name and password defined in ./src/WifiInfo.h
 */
void connectToAP() {
  // Try to connect to Wifi network
  WiFi.begin(ssid, pass);
  Serial.print("Attmepting connection to wifi ");
  Serial.print(ssid);
  Serial.print(".\n");
 
  while(WiFi.status() != WL_CONNECTED) {
    Serial.println(WiFi.status());
    Serial.println("Wifi not connecting. Attempting connection...");
    // If not OK, don't continue and light LED
    digitalWrite(LED_BUILTIN, HIGH);
    delay(1000);
  }

  Serial.println("Connection established.");
}

char* makeHttpRequest(char server[], char endpoint[]) {
  IPAddress pi(192, 168, 0, 12);
  int index = 0;
  char response[45];
  
  if (client.connect(pi, 80)) {
    Serial.println("Connection success. Sending Request.");
    client.println("GET /api/v1/system/temperature HTTP/1.0");
    client.println("Connection: close");
    client.println();

  // We use delays, which will make us 'miss' the response if we don't wait for it here.
  // https://stackoverflow.com/questions/49141555/why-is-client-available-returning-a-0-arduino
  while (!client.available()) {}
  while (client.available()) {
    char c = client.read();
    response[index] = c;
    index++;
    Serial.write(c);
  }
  } else {
    Serial.println("Connection failed");
    // Send signal to Sonoff to kill the process.
  }

  return response;
}
