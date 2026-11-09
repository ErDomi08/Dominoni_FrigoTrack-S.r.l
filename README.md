# Dominoni_FrigoTrack-S.r.l.
---
## Introduzione e Obiettivo

Il progetto realizza un sistema in architettura Client-Server, chiamato **FrigoTrack**, sviluppato su **.NET 8.0/9.0**. L'obiettivo è simulare monitoraggio e trasmissione protetta delle telemetrie di un sensore di temperatura ambientale (es. un frigorifero industriale).

---

## Architettura e Flusso Logico

Il sistema si articola in due componenti che comunicano tramite HTTP:

1. **Client (Sensore):** Genera le letture di telemetria, si occupa della cifratura e spedisce i pacchetti protetti.
2. **Server (Web API):** Mostra gli endpoint per l'handshake e la ricezione dati, valida la struttura dei messaggi e memorizza le letture.

Il flusso:
* **Fase 1 (Handshake):** Client e Server concordano una chiave di sessione segreta simmetrica tramite l'algoritmo *Diffie-Hellman*.
* **Fase 2 (Trasmissione Dati):** Il Client genera una misura, la serializza in JSON, la cifra all'interno di una busta e la invia al Server tramite POST HTTP.

---

## Tecnologie e Scelte Implementative

* **Linguaggio e Framework:** C# su .NET moderno: elevate prestazioni e supporto a primitive crittografiche.
* **Sicurezza e Crittografia:**
  * **Scambio chiavi:** *Diffie-Hellman* per evitare la trasmissione della chiave in chiaro.
  * **Cifratura simmetrica:** *AES-GCM (Galois/Counter Mode)*, assicura sia riservatezza che l'autenticità del dato cifrato, prevenendo possibili attacchi.
* **Gestione dei Dati e Validazione:** I messaggi sono definiti da file *JSON Schema*. La validazione sintattica a runtime è affidata alla libreria `JsonSchema.Net`.

---

## Validazione e Test

Il corretto funzionamento dell'applicazione è stato verificato tramite **5 Test fondamentali** integrati nell' avvio del Client:

1. **Test 1: Handshake iniziale.** Verifica il corretto scambio dei parametri del Diffie-Hellman e la generazione della chiave segreta tra Client e Server.
2. **Test 2: Invio di una misura corretta.** Dimostra il flusso: generazione dati, cifratura AES-GCM, invio, ricezione sul Server, decifratura e corretta validazione JSON.
3. **Test 3: Invio di un JSON non formattato correttamente.** Il Client invia un JSON sintatticamente volontariamente errato. Il Server intercetta l'anomalia tramite il suo JSON Schema e rifiuta la richiesta Client restituendo un erroere.
4. **Test 4: Invio di una busta cifrata con chiave errata.** Il Client invia dati cifrati con una chiave differente da quella concordata. Il Server fallisce la decifratura e scarta il pacchetto infetto.
5. **Test 5: Chiusura della sessione.** Verifica il comando di terminazione della sessione attiva, invalidando la chiave di cifratura sul Server.
