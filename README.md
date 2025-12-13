# MultiSoil-.NET-MAUI-ESP32-IA

Este é um projeto de monitoramento inteligente do solo, utilizando a plataforma .NET MAUI, ESP32 e Inteligência Artificial para análise de dados. O objetivo deste projeto é criar uma solução multiplataforma para monitorar e analisar o solo, integrando sensores de umidade e temperatura do solo com dispositivos ESP32, e gerenciar os dados utilizando uma interface móvel construída com .NET MAUI.

![photo_5037804670809738009_y](https://github.com/user-attachments/assets/e1f8314d-caed-4d43-a512-796a669e2493)

![photo_5037804670809738012_y](https://github.com/user-attachments/assets/2546c2ec-cc76-479c-baf1-c8232e0aaa19)


![photo_5037804670809738010_y](https://github.com/user-attachments/assets/d11063dd-5a32-40e4-8fb6-e74c3c59fd48)

![photo_5037804670809738011_y](https://github.com/user-attachments/assets/505664d3-a39b-46a8-a4d6-73c59d3fdb3e)


O projeto está em fase de desenvolvimento, o uso de IA é apenas exemplificado com um pobre modelo de regressão linear.

## Estrutura do Projeto

O projeto está dividido nas seguintes pastas principais:

- **app**: Contém o código-fonte da aplicação móvel criada com .NET MAUI.
- **esp32_maui_firmware**: Firmware para o ESP32 que comunica com os sensores e envia dados para a aplicação MAUI.


## Requisitos

- **ESP32**: Para comunicação com os sensores.
- **.NET MAUI**: Para a criação da aplicação multiplataforma (Android/iOS/Windows).
- **Sensor de umidade e temperatura do solo**: Para coleta de dados ambientais.
- **IDE**: Visual Studio 2022 ou superior, com suporte a .NET MAUI e ferramentas de desenvolvimento para ESP32.

## Instalação

### Passo 1: Preparar o Ambiente de Desenvolvimento

1. **Instalar o .NET 6 ou superior**:
   - [Download do .NET](https://dotnet.microsoft.com/download)

2. **Instalar o Visual Studio**:
   - Baixe e instale o Visual Studio 2022 com a carga de trabalho para **.NET MAUI** e **desenvolvimento para dispositivos móveis**.

3. **Instalar o ESP32 Toolchain**:
   - Siga as instruções de instalação no [site oficial do ESP32](https://docs.espressif.com/projects/esp-idf/en/latest/get-started/).

### Passo 2: Configurar o Firmware no ESP32

1. Conecte o ESP32 ao computador.
2. Abra o projeto de firmware localizado na pasta **esp32_maui_firmware**.
3. Compile e faça o upload do firmware para o ESP32.

### Passo 3: Rodar a Aplicação .NET MAUI

1. Abra o projeto na pasta **app** no Visual Studio.
2. Selecione o dispositivo de execução (Android, iOS, Windows).
3. Execute a aplicação para testar a comunicação com o ESP32 e os sensores.



```xml
<Image Source="Images/your-image.png" />
