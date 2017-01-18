// Arduino Domotica server with Klik-Aan-Klik-Uit-controller 
//
// By Sibbele Oosterhaven, Computer Science NHL, Leeuwarden
// V1.2, 16/12/2016, published on BB. Works with Xamarin (App: Domotica)
//
// Hardware: Arduino Uno, Ethernet shield W5100; RF transmitter on RFpin; debug LED for serverconnection on ledPin
// The Ethernet shield uses pin 10, 11, 12 and 13
// Use Ethernet2.h libary with the (new) Ethernet board, model 2
// IP address of server is based on DHCP. No fallback to static IP; use a wireless router
// Arduino server and smartphone should be in the same network segment (192.168.1.x)
// 
// Supported kaku-devices
// https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model left)
// kaku Action device, old model (with dipswitches); system code = 31, device = 'A' 
// system code = 31, device = 'A' true/false
// system code = 31, device = 'B' true/false
//
// // https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model right)
// Based on https://github.com/evothings/evothings-examples/blob/master/resources/arduino/arduinoethernet/arduinoethernet.ino.
// kaku, Action, new model, codes based on Arduino -> Voorbeelden -> RCsw-2-> ReceiveDemo_Simple
//   on      off       
// 1 2210415 2210414   replace with your own codes
// 2 2210413 2210412
// 3 2210411 2210410
// 4 2210407 2210406
//
// https://github.com/hjgode/homewatch/blob/master/arduino/libraries/NewRemoteSwitch/README.TXT
// kaku, Gamma, APA3, codes based on Arduino -> Voorbeelden -> NewRemoteSwitch -> ShowReceivedCode
// 1 Addr 21177114 unit 0 on/off, period: 270us   replace with your own code
// 2 Addr 21177114 unit 1 on/off, period: 270us
// 3 Addr 21177114 unit 2 on/off, period: 270us

// Supported KaKu devices -> find, download en install corresponding libraries
#define unitCodeApa3      21195726  // replace with your own code

// Include files.
#include <SPI.h>                  // Ethernet shield uses SPI-interface
#include <Ethernet.h>             // Ethernet library (use Ethernet2.h for new ethernet shield v2)
#include <NewRemoteTransmitter.h> // Remote Control, Gamma, APA3
#include <Servo.h>

// Set Ethernet Shield MAC address  (check yours)
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
int ethPort = 3300;                                  // Take a free port (check your router)

#define RFPin        3  // output, pin to control the RF-sender (and Click-On Click-Off-device)
#define lowPin       5  // output, always LOW
#define highPin      6  // output, always HIGH
#define switchPin    7  // input, connected to some kind of inputswitch
#define ledPin       8  // output, led used for "connect state": blinking = searching; continuously = connected
#define infoPin      9  // output, more information
#define analogPin    0  // sensor value
#define trigPin      6  //Zender van ultrasone sensor
#define echoPin      7  //Ontvanger van ultrasone sensor

Servo myservo;
EthernetServer server(ethPort);              // EthernetServer instance (listening on port <ethPort>).
NewRemoteTransmitter apa3Transmitter(unitCodeApa3, RFPin, 260, 3);  // APA3 (Gamma) remote, use pin <RFPin> 

bool pinState = false;                   // Variable to store actual pin state
bool stop0 = false;
bool stop1 = false;
bool stop2 = false;
bool pinChange = false;                  // Variable to store actual pin change
int  sensorValue = 0;                    // Variable to store actual sensor value
int  sensorValue2 = 0;
bool start = false;

void setup()
{
   Serial.begin(9600);
   //while (!Serial) { ; }               // Wait for serial port to connect. Needed for Leonardo only.

   Serial.println("Domotica project, Arduino Domotica Server\n");
   
   //Init I/O-pins
   pinMode(switchPin, INPUT);            // hardware switch, for changing pin state
   pinMode(lowPin, OUTPUT);
   pinMode(highPin, OUTPUT);
   pinMode(RFPin, OUTPUT);
   pinMode(ledPin, OUTPUT);
   pinMode(infoPin, OUTPUT);

   //Ultrasone pins
   pinMode(trigPin, OUTPUT);
   pinMode(echoPin, INPUT);
   myservo.attach(2);
   
   //Default states
   digitalWrite(switchPin, HIGH);        // Activate pullup resistors (needed for input pin)
   digitalWrite(lowPin, LOW);
   digitalWrite(highPin, HIGH);
   digitalWrite(RFPin, LOW);
   digitalWrite(ledPin, LOW);
   digitalWrite(infoPin, LOW);

   //Try to get an IP address from the DHCP server.
   if (Ethernet.begin(mac) == 0)
   {
      Serial.println("Could not obtain IP-address from DHCP -> do nothing");
      while (true){         

      //While not connected C
      if (!start){
       //Opdracht C
      int duration, distance;
      digitalWrite(trigPin, HIGH);
      delayMicroseconds(1000);
      digitalWrite(trigPin, LOW);
      duration = pulseIn(echoPin, HIGH);
      distance = (duration/2) / 29.1;
      if (distance > 10) {
      Serial.println("All clear");
      myservo.write(180);}
      else {
      Serial.println("Unknown entity detected");
      myservo.write(90);}}   // no point in carrying on, so do nothing forevermore; check your router
      }
   }
   
   Serial.print("LED (for connect-state and pin-state) on pin "); Serial.println(ledPin);
   Serial.print("Input switch on pin "); Serial.println(switchPin);
   Serial.println("Ethernetboard connected (pins 10, 11, 12, 13 and SPI)");
   Serial.println("Connect to DHCP source in local network (blinking led -> waiting for connection)");
   
   //Start the ethernet server.
   server.begin();

   // Print IP-address and led indication of server state
   Serial.print("Listening address: ");
   Serial.print(Ethernet.localIP());
   
   // for hardware debug: LED indication of server state: blinking = waiting for connection
   int IPnr = getIPComputerNumber(Ethernet.localIP());   // Get computernumber in local network 192.168.1.3 -> 3)
   Serial.print(" ["); Serial.print(IPnr); Serial.print("] "); 
   Serial.print("  [Testcase: telnet "); Serial.print(Ethernet.localIP()); Serial.print(" "); Serial.print(ethPort); Serial.println("]");
   signalNumber(ledPin, IPnr);
}

void loop()
{      
  // Listen for incomming connection (app)
   EthernetClient ethernetClient = server.available();
   if (!ethernetClient) {
      blink(ledPin);
      return; // wait for connection and blink LED
   }

   Serial.println("Application connected");
   digitalWrite(ledPin, LOW);

   // Do what needs to be done while the socket is connected.
   while (ethernetClient.connected()) 
   {
      checkEvent(switchPin, pinState);          // update pin state
      sensorValue = readSensor(0, 100);         // update sensor value
      sensorValue2 = analogRead(1);

      //C connected
      if (!start){
        //Opdracht C
        int duration, distance;
        digitalWrite(trigPin, HIGH);
        delayMicroseconds(1000);
        digitalWrite(trigPin, LOW);
        duration = pulseIn(echoPin, HIGH);
        distance = (duration/2) / 29.1;
          if (distance > 10) {
          //Serial.println("All clear !start");
          myservo.write(180);}
          else {
          //Serial.println("Unknown entity detected !start");
          myservo.write(90);}}

      else {
        int duration, distance;
        digitalWrite(trigPin, HIGH);
        delayMicroseconds(1000);
        digitalWrite(trigPin, LOW);
        duration = pulseIn(echoPin, HIGH);
        distance = (duration/2) / 29.1;
          if (distance > 10) {
          //Serial.println("All clear start");
          myservo.write(90);}
          else {
          //Serial.println("Unknown entity detected start");
          myservo.write(180);}}
      
      // Activate pin based op pinState
      if (pinChange) {
         if (pinState) { digitalWrite(ledPin, HIGH); }
         else { digitalWrite(ledPin, LOW); }
         pinChange = false;
      }
   
      // Execute when byte is received.
      while (ethernetClient.available())
      {
         char inByte = ethernetClient.read();   // Get byte from the client.
         executeCommand(inByte);                // Wait for command to execute
         inByte = NULL;                         // Reset the read byte.
      } 
   }
   Serial.println("Application disonnected");
}

// Choose and switch your Kaku device, state is true/false (HIGH/LOW)
void switchDefault(int stopcontact, bool state)
{   
   apa3Transmitter.sendUnit(stopcontact, state);          // APA3 Kaku (Gamma)                
   delay(100);
}

// Implementation of (simple) protocol between app and Arduino
// Request (from app) is single char ('a', 's', 't', 'i' etc.)
// Response (to app) is 4 chars  (not all commands demand a response)
void executeCommand(char cmd)
{     
         char buf[4] = {'\0', '\0', '\0', '\0'};
         double Temp;
         Temp = log(10000.0*((1024.0/sensorValue2-1))); 
         Temp = 1 / (0.001129148 + (0.000234125 + (0.0000000876741 * Temp * Temp ))* Temp );
         Temp = (Temp - 273.15) / 10;          
         // Command protocol
         Serial.print("["); Serial.print(cmd); Serial.print("] -> ");
         switch (cmd) {
         case 'a': // Report sensor value to the app  
            intToCharBuf(sensorValue, buf, 4);                // convert to charbuffer
            server.write(buf, 4);                             // response is always 4 chars (\n included)
            Serial.print("Sensor: "); Serial.println(buf);
            break;
         case 'b': // Report sensor 2 value to the app  
            intToCharBuf(Temp, buf, 4);                // convert to charbuffer
            server.write(buf, 4);                             // response is always 4 chars (\n included)
            Serial.print("Sensor 2: "); Serial.println(buf);
            break;            
         case 's': // Report switch state to the app
            if (pinState) { server.write(" ON\n"); Serial.println("Pin state is ON"); }  // always send 4 chars
            else { server.write("OFF\n"); Serial.println("Pin state is OFF"); }
            break;
         case 't': // Toggle state; If state is already ON then turn it OFF
            if (pinState) { pinState = false; Serial.println("Set pin state to \"OFF\""); }
            else { pinState = true; Serial.println("Set pin state to \"ON\""); }  
            pinChange = true; 
            break;
         case 'i':    
            digitalWrite(infoPin, HIGH);
            break;
         case 'x': // Toggle state; If state is already ON then turn it OFF
            setSensor(0, stop0);
            break;
         case 'y': // Toggle state; If state is already ON then turn it OFF
            setSensor(1, stop1); 
            break;
         case 'z': // Toggle state; If state is already ON then turn it OFF
            setSensor(2, stop2); 
            break;
         case 'd':
            if (stop0){server.write(" ON\n");}
            else {server.write("OFF\n");}
            break;
         case 'e':
            if (stop1){server.write(" ON\n");}
            else {server.write("OFF\n");}
            break;
         case 'f':
            if (stop2){server.write(" ON\n");}
            else {server.write("OFF\n");}            
            break;
         case 'g':
            if (!start){start = true; myservo.write(90);}
            else {start = false;}
         default:
            digitalWrite(infoPin, LOW);
         }
}
// do something with kaku

void setSensor(int stopcontact, bool &state) {
  if (state) { state = false; Serial.println("Set schakelaar state to \"OFF\""); switchDefault(stopcontact, false); }
  else { state = true; Serial.println("Set schakelaar state to \"ON\""); switchDefault(stopcontact, true);}
}
// read value from pin pn, return value is mapped between 0 and mx-1
int readSensor(int pn, int mx)
{
  return map(analogRead(pn), 0, 1023, 0, mx-1);    
}

// Convert int <val> char buffer with length <len>
void intToCharBuf(int val, char buf[], int len)
{
   String s;
   s = String(val);                        // convert tot string
   if (s.length() == 1) s = "0" + s;       // prefix redundant "0" 
   if (s.length() == 2) s = "0" + s;  
   s = s + "\n";                           // add newline
   s.toCharArray(buf, len);                // convert string to char-buffer
}

// Check switch level and determine if an event has happend
// event: low -> high or high -> low
void checkEvent(int p, bool &state)
{
   static bool swLevel = false;       // Variable to store the switch level (Low or High)
   static bool prevswLevel = false;   // Variable to store the previous switch level

   swLevel = digitalRead(p);
   if (swLevel)
      if (prevswLevel) delay(1);
      else {               
         prevswLevel = true;   // Low -> High transition
         state = true;
         pinChange = true;
      } 
   else // swLevel == Low
      if (!prevswLevel) delay(1);
      else {
         prevswLevel = false;  // High -> Low transition
         state = false;
         pinChange = true;
      }
}

// blink led on pin <pn>
void blink(int pn)
{
  digitalWrite(pn, HIGH); 
  delay(100); 
  digitalWrite(pn, LOW); 
  delay(100);
}

// Visual feedback on pin, based on IP number, used for debug only
// Blink ledpin for a short burst, then blink N times, where N is (related to) IP-number
void signalNumber(int pin, int n)
{
   int i;
   for (i = 0; i < 30; i++)
       { digitalWrite(pin, HIGH); delay(20); digitalWrite(pin, LOW); delay(20); }
   delay(1000);
   for (i = 0; i < n; i++)
       { digitalWrite(pin, HIGH); delay(300); digitalWrite(pin, LOW); delay(300); }
    delay(1000);
}

// Convert IPAddress tot String (e.g. "192.168.1.105")
String IPAddressToString(IPAddress address)
{
    return String(address[0]) + "." + 
           String(address[1]) + "." + 
           String(address[2]) + "." + 
           String(address[3]);
}

// Returns B-class network-id: 192.168.1.3 -> 1)
int getIPClassB(IPAddress address)
{
    return address[2];
}

// Returns computernumber in local network: 192.168.1.3 -> 3)
int getIPComputerNumber(IPAddress address)
{
    return address[3];
}

// Returns computernumber in local network: 192.168.1.105 -> 5)
int getIPComputerNumberOffset(IPAddress address, int offset)
{
    return getIPComputerNumber(address) - offset;
}

