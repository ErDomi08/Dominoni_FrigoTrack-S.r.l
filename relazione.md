# Relazione su FrigoTrack.

## 1. Scelte Tecniche e Motivazioni
Il progetto mentiene i vincoli stringenti imposti dalla consegna (canale HTTP in chiaro, nessuna chiave in rete, struttura messaggio fissa) adottando queste soluzioni architetturali:

*   **Scambio Chiavi (Diffie-Hellman)**: Implementato in modo nativo sfruttando la classe `System.Numerics.BigInteger` di .NET e la sua funzione di potenza modulare `ModPow`. Questa scelta garantisce la massima efficienza matematica anche con numeri primi di grandi dimensioni, rispettando il vincolo di non delegare l'intero handshake a librerie esterne di terze parti.
*   **Cifratura Autenticata (AES-GCM)**: Invece di usare modalità basilari come AES-CBC (che richiede un vettore di inizializzazione separato e non protegge dall'alterazione dei dati), si è optato per la modalità **GCM (Galois/Counter Mode)** tramite la classe nativa `System.Security.Cryptography.AesGcm`. Questa scelta è cruciale: fornisce contemporaneamente la riservatezza (`testo_cifrato`) e l'autenticità dei dati (`tag`). Se un concorrente o un tecnico malintenzionato altera anche un singolo bit della busta in transito, il server fallisce la verifica del tag prima di elaborare il testo, neutralizzando l'attacco all'origine.
*   **Validazione Condizionale Rigida**: I messaggi decifrati vengono validati secondo le regole descritte nei file `schemas/`. Nel codice C#, questo controllo formale viene mappato attraverso controlli condizionali stringenti e deserializzazione tipizzata con `System.Text.Json`, garantendo il rifiuto immediato con codice HTTP 400 in caso di discrepanze strutturali.

## 2. Limiti Noti della Soluzione e Possibili Evoluzioni

Nonostante il sistema soddisfi i requisiti del prototipo, un ambiente di produzione reale evidenzierebbe due limiti di sicurezza intrinseci legati all'assenza di HTTPS:

1.  **Vulnerabilità ad Attacchi Man-in-the-Middle (MitM) durante l'Handshake**: Diffie-Hellman protegge efficacemente da un utente malintenzionato che si limita ad ascoltare passivamente il traffico di rete (sniffing). Tuttavia, se l'attaccante intercetta attivamente la richiesta iniziale di handshake, può porsi nel mezzo modificando i parametri e fingendosi il server agli occhi del client, e viceversa. 
    *   *Soluzione futura*: Integrare un'autenticazione del server a chiave asimmetrica (Firma Digitale) con una chiave pubblica del server pre-condivisa in modo sicuro nei sensori in fase di fabbricazione.
2.  **Attacco di Replay (Replay Attack)**: Poiché la struttura della busta cifrata è deterministica per una specifica lettura, un utente malintenzionato potrebbe catturare una busta valida registrata su `/api/telemetry` (ad esempio una lettura che certifica una temperatura corretta) e inviarla nuovamente al server in un secondo momento, ingannando il sistema di tracciabilità sulla freschezza del dato.
    *   *Soluzione futura*: Introdurre all'interno del JSON applicativo un numero progressivo monouso (*Nonce applicativo*) o verificare rigidamente sul server che la `marca_temporale` interna non differisca di oltre pochi secondi dall'orario di ricezione del server, scartando i messaggi obsoleti.
