int headphonePin = 4;
int ambientPin = 5;

void setup() {
  Serial.begin(9600);  //Begin serial communcation
}

void loop() {
  Serial.print('#');
  Serial.print(analogRead(headphonePin));
  Serial.print(";");
  Serial.print(analogRead(ambientPin));
  delay(40);
}
