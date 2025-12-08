// src/main.cpp
#include <Arduino.h>
#include <WiFi.h>
#include <WebServer.h>

// =====================================================
// CONFIGURAÇÃO DE WI-FI
// =====================================================
const char* WIFI_SSID     = "Infinix HOT 30";
const char* WIFI_PASSWORD = "joseronaldo02";

// =====================================================
// CONFIGURAÇÃO DO SENSOR NPK (RS485 MODBUS)
// =====================================================

#define BAUD_RATE 4800

// Pinos da UART2 do ESP32 (ajuste se necessário)
#define RS485_RX 16
#define RS485_TX 17

// Se o seu módulo RS485 tiver pino DE/RE, defina aqui
// e descomente a linha abaixo:
// #define RS485_DE_RE 4

// Requisição Modbus para o sensor NPK 7x1
// (endereço 0x01, função 0x03, registradores 0x0000, quantidade 7, CRC)
// Esses bytes vieram do seu código original.
uint8_t byteRequest[8] = {
  0x01, 0x03, 0x00, 0x00,
  0x00, 0x07, 0x04, 0x08
};

// Resposta esperada: 19 bytes
uint8_t byteResponse[19];

// Estrutura para guardar a última leitura válida
struct NpkData {
  float hum;        // Umidade (%)
  float temp;       // Temperatura (°C)
  uint16_t ec;      // Condutividade elétrica (µS/cm, conforme sensor)
  float ph;         // pH
  uint16_t n;       // Nitrogênio
  uint16_t p;       // Fósforo
  uint16_t k;       // Potássio
  bool valid;       // Se há leitura válida
  unsigned long ts; // Timestamp em millis()
};

NpkData lastNpk = {0};

// Intervalo entre leituras do sensor (ms)
const unsigned long NPK_INTERVAL = 1000;
unsigned long lastNpkRead = 0;

// =====================================================
// SERVIDOR HTTP
// =====================================================
WebServer server(80);

// Prototypes
void readNPKSensor();
void resetNPKSensor();
void handleRoot();
void handleReadings();

// =====================================================
// SETUP
// =====================================================
void setup() {
  Serial.begin(115200);
  delay(1000);

  Serial.println();
  Serial.println("=== ESP32 + NPK 7x1 + HTTP JSON ===");

  // Configura pino DE/RE, se existir
  #ifdef RS485_DE_RE
    pinMode(RS485_DE_RE, OUTPUT);
    digitalWrite(RS485_DE_RE, LOW); // começa em recepção
  #endif

  // Inicializa Serial2 para RS485
  Serial2.begin(BAUD_RATE, SERIAL_8N1, RS485_RX, RS485_TX);
  Serial.println("Serial2 (RS485) inicializada.");

  // Conecta no Wi-Fi
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  Serial.printf("Conectando ao Wi-Fi: %s\n", WIFI_SSID);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println();
  Serial.println("Wi-Fi conectado!");
  Serial.print("IP: ");
  Serial.println(WiFi.localIP());

  // Configura rotas do servidor HTTP
  server.on("/", HTTP_GET, handleRoot);
  server.on("/api/readings", HTTP_GET, handleReadings);
  server.onNotFound([]() {
    server.send(404, "application/json", "{\"error\":\"not_found\"}");
  });

  server.begin();
  Serial.println("Servidor HTTP iniciado na porta 80.");

  // Leitura inicial do sensor para já ter algum dado
  readNPKSensor();
}

// =====================================================
// LOOP
// =====================================================
void loop() {
  // Trata requisições HTTP
  server.handleClient();

  // Faz leitura periódica do sensor
  unsigned long now = millis();
  if (now - lastNpkRead >= NPK_INTERVAL) {
    readNPKSensor();
    lastNpkRead = now;
  }
}

// =====================================================
// HANDLERS HTTP
// =====================================================

void handleRoot() {
  String msg = "ESP32 NPK online. Acesse /api/readings para JSON.\n";
  msg += "IP: " + WiFi.localIP().toString() + "\n";
  server.send(200, "text/plain", msg);
}

// Endpoint que o app .NET MAUI vai consumir:
// GET http://IP_DO_ESP32/api/readings
// Resposta JSON compatível com ReadingDto do seu app:
// { "N":..., "P":..., "K":..., "PH":..., "CE":..., "Temp":..., "Umid":... }
void handleReadings() {
  if (!lastNpk.valid) {
    server.send(503, "application/json", "{\"error\":\"no_data\"}");
    return;
  }

  String json = "{";
  json += "\"N\":"     + String(lastNpk.n);
  json += ",\"P\":"    + String(lastNpk.p);
  json += ",\"K\":"    + String(lastNpk.k);
  json += ",\"PH\":"   + String(lastNpk.ph, 1);
  json += ",\"CE\":"   + String(lastNpk.ec);
  json += ",\"Temp\":" + String(lastNpk.temp, 1);
  json += ",\"Umid\":" + String(lastNpk.hum, 1);
  json += "}";

  server.send(200, "application/json", json);
}

// =====================================================
// LEITURA DO SENSOR NPK VIA RS485/MODBUS
// =====================================================

void readNPKSensor() {
  memset(byteResponse, 0, sizeof(byteResponse));

  // Envia requisição Modbus
  #ifdef RS485_DE_RE
    digitalWrite(RS485_DE_RE, HIGH); // modo transmissão
    delay(2);
  #endif

  Serial2.write(byteRequest, sizeof(byteRequest));
  Serial2.flush();

  #ifdef RS485_DE_RE
    digitalWrite(RS485_DE_RE, LOW); // volta para recepção
  #endif

  int bytesRead = 0;
  unsigned long t0 = millis();

  // Aguarda até 1s pela resposta ou até ler 19 bytes
  while ((millis() - t0) < 1000 && bytesRead < 19) {
    if (Serial2.available()) {
      byteResponse[bytesRead++] = Serial2.read();
    }
  }

  if (bytesRead == 19) {
    // Aqui daria pra checar CRC também, se quiser ser mais rígido.

    uint16_t soilHumidity    = (byteResponse[3]  << 8) | byteResponse[4];
    uint16_t soilTemperature = (byteResponse[5]  << 8) | byteResponse[6];
    uint16_t soilEC          = (byteResponse[7]  << 8) | byteResponse[8];
    uint16_t soilPH          = (byteResponse[9]  << 8) | byteResponse[10];
    uint16_t nitrogen        = (byteResponse[11] << 8) | byteResponse[12];
    uint16_t phosphorus      = (byteResponse[13] << 8) | byteResponse[14];
    uint16_t potassium       = (byteResponse[15] << 8) | byteResponse[16];

    lastNpk.hum  = soilHumidity    / 10.0f;
    lastNpk.temp = soilTemperature / 10.0f;
    lastNpk.ec   = soilEC;
    lastNpk.ph   = soilPH / 10.0f;
    lastNpk.n    = nitrogen;
    lastNpk.p    = phosphorus;
    lastNpk.k    = potassium;
    lastNpk.valid = true;
    lastNpk.ts = millis();

    Serial.print("NPK OK | ");
    Serial.print("Hum=");  Serial.print(lastNpk.hum);  Serial.print("%, ");
    Serial.print("Temp="); Serial.print(lastNpk.temp); Serial.print("C, ");
    Serial.print("EC=");   Serial.print(lastNpk.ec);   Serial.print(", ");
    Serial.print("pH=");   Serial.print(lastNpk.ph);   Serial.print(", ");
    Serial.print("N=");    Serial.print(lastNpk.n);    Serial.print(", ");
    Serial.print("P=");    Serial.print(lastNpk.p);    Serial.print(", ");
    Serial.print("K=");    Serial.println(lastNpk.k);

  } else {
    Serial.print("Erro/timeout NPK. Recebidos ");
    Serial.print(bytesRead);
    Serial.println(" bytes.");

    resetNPKSensor();
    lastNpk.valid = false;
  }
}

// =====================================================
// RESET DA PORTA SERIAL DO SENSOR
// =====================================================

void resetNPKSensor() {
  Serial.println("Reiniciando porta Serial2 do sensor NPK...");
  Serial2.end();
  delay(100);
  Serial2.begin(BAUD_RATE, SERIAL_8N1, RS485_RX, RS485_TX);
  delay(500);

  #ifdef RS485_DE_RE
    digitalWrite(RS485_DE_RE, LOW); // volta pra recepção
  #endif
}
