// ---------- Included libraries ------------
#include <SPI.h>
#include <Ethernet.h>
#include <Servo.h>
#include <NewPing.h>

// ---------- Hardware pin defines -----------
const int triggerPin = 6; // Select the pin for ultrasonic trigger.
const int echoPin = 7;   // Select the pin for echo.
const int servoPin = 2; // Select the pin for servo.

// ---------- Variable initialization -----------
const int triggerWithin = 10;   // In centimeter.
const int maxDistance = 50;    // Maximum distance we want to ping for (in centimeters). Maximum sensordistance is rated at 400 - 500cm.
const int readingDelay = 500; // In milliseconds.
const int startStopDelay = 500; // In milliseconds.
int angleHigh = 0; // Angle to rotate in degrees.
int angleRight = 90; // Angle to rotate in degrees.

// ---------- Boolean initialization -----------
bool isConnected = false; // Boolean to check if we're connected to ethernet
bool isStarted = false; // Boolean to check if the "host" is started.

// Enter a MAC address for your controller below, usually found on a sticker
// on the back of your Ethernet shield.
const byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };

// ---------- Library initialization -----------
Servo servo;  // Create servo object to control a servo.
NewPing sonar(triggerPin, echoPin, maxDistance);

EthernetServer server(3300); // Create a server listening on the given port.

void setup() {
  Serial.begin(9600);
  // Declare hardware connections
  servo.attach(servoPin);  // Attaches the servo on pin 9 to the servo object

  // Rotate if necessary
  if (servo.read() != angleHigh) {
    servo.write(angleHigh);
  }

  Serial.println("Trying to connect to ethernet. Can take a while if not connected..");

  // Try to get an IP address from the DHCP server.
  isConnected = Ethernet.begin(mac) != 0;

  Serial.println(isConnected ? "Connected to ethernet" : "Not connected to ethernet");

  // If we are connected
  if (isConnected) {
    // Start the server.
    server.begin();

    Serial.print("Server address: ");
    Serial.println(Ethernet.localIP());
  }
}

void loop() {
  if (isConnected) {
    // Listen for incoming client requests.
    EthernetClient client = server.available();
    if (!client)
    {
      if (isStarted) {
        checkDistance();
      }
      return;
    }

    Serial.println("Client connected");

    String request = readRequest(&client);
    executeRequest(&client, &request);

    Serial.println("Client disconnected");
  } else {
    checkDistance();
  }
}

void checkDistance() {
  int dist = sonar.ping_median(5); // Median off 5 values
  dist = sonar.convert_cm(dist); //Convert that to cm

  // Prints the distance on the Serial Monitor
  Serial.print("Distance in cm: ");
  Serial.println(dist);

  if (dist < triggerWithin && dist != 0) // If an object is within 20cm.
  {
    // Rotate if necessary
    if (servo.read() != angleRight) {
      servo.write(angleRight);
    }
  }
  else {
    // Rotate if necessary
    if (servo.read() != angleHigh) {
      servo.write(angleHigh);
    }
  }

  // Delay 500ms before next reading.
  delay(readingDelay);
}


// Read the request line,
String readRequest(EthernetClient* client)
{
  String request = "";

  // Loop while the client is connected.
  while (client->connected())
  {
    // Read available bytes.
    while (client->available())
    {
      // Read a byte.
      char c = client->read();

      // Print the value (for debugging).
      Serial.write(c);

      // Exit loop if end of line.
      if ('\n' == c)
      {
        return request;
      }

      // Add byte to request line.
      request += c;
    }
  }
  return request;
}

void executeRequest(EthernetClient* client, String* request)
{
  String command = request->substring(0, request->length());
  String response;

  // Start command received
  if (command.equals("START"))
  {
    // Rotate servo if necessary
    if (servo.read() != angleRight) {
      servo.write(angleRight);
    }

    // Swap angles if we started
    int tempAngle = angleRight;
    angleRight = angleHigh;
    angleHigh = tempAngle;

    // Set started flag to true
    isStarted = true;

    // Delay a bit
    delay(startStopDelay);

    // Set response
    response = "STARTED";
  }
  // Stop command received
  else if (command.equals("STOP"))
  {
    // Swap angles if we stopped
    int tempAngle = angleRight;
    angleRight = angleHigh;
    angleHigh = tempAngle;

    // Rotate servo if necessary
    if (servo.read() != angleHigh) {
      servo.write(angleHigh);
    }

    // Set started flag to false
    isStarted = false;

    // Delay a bit
    delay(startStopDelay);

    response = "STOPPED";
  }

  // Remember to send response back to client
  sendResponse(client, response);
}

void sendResponse(EthernetClient* client, String response)
{
  // Send response to client.
  client->println(response);

  // Debug print.
  Serial.println("sendResponse:");
  Serial.println(response);
}
