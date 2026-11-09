# FrigoTrack - Monitoraggio Telemetria Sicura Client-Server

FrigoTrack è un'applicazione Client-Server scritta in C# su .NET(.NET 8.0/.NET 9.0) progettata per inviare e certificare i dati di telemetria da parte di sensori ambientali.

## Caratteristiche Principali
- **Scambio Chiavi Sicuro:** uso algoritmo Diffie-Hellman per concordare la chiave di sessione.
- **Cifratura Avanzata:** Utilizzo di AES (Authenticated Encryption) per garantire riservatezza e integrità.
- **Validazione JSON Rigida:** Controllo dei payload in transito tramite JSON Schema.

## Struttura del Progetto
- `Server/`: Web APP ASP.NET Core che valida gli schemi, gestisce le sessioni e decifra le letture.
- `Client/`: Applicazione console che simula il sensore, esegue l'handshake e invia i dati cifrati.
- `Schemas/`: Contiene i file di definizione (json) per garantire la corretta struttura dei messaggi.

## Prerequisiti
- .NET 8.0 o .NET 9.0
- Visual Studio 2022 o Visual Studio Code

## Installazione e Configurazione
1. scaricare il codice della soluzione.
2. Aprire la cartella principale del progetto nel terminale o caricare il file `.sln` su Visual Studio.
3. Ripristinare i pacchetti NuGet necessari lanciando il comando:
   ```bash
   dotnet restore
   ```

## Come Avviare l'Applicazione

### 1. Avviare il Server
andare nella cartella del server ed eseguire il progetto:
```bash
cd Server/Server
dotnet run
```
Il server rimarrà in ascolto sugli endpoint predefiniti (es. `https://localhost:7233` o `http://localhost:5233`).

### 2. Avviare il Client
Aprire un secondo terminale, spostarsi nella cartella del client ed avviare l'eseguibile:
```bash
cd Client/Client
dotnet run
```

## Test Integrata
All'avvio, il Client eseguirà automaticamente una sequenza composta da **5 scenari di test** per verificare l'applicazione:
1. **Handshake Test:** Verifica dello scambio chiavi Diffie-Hellman.
2. **Valid Reading Test:** Invio di una telemetria cifrata corretta (esito positivo).
3. **Invalid JSON Test:** Invio di un messaggio malformato (rifiutato dal validatore JSON Schema del server).
4. **Wrong Key Test:** Invio di un pacchetto cifrato con una chiave errata (rifiutato dal controllo di integrità AES-GCM del server).
5. **Close Session Test:** Invio del comando di chiusura e invalidazione della sessione crittografica.
