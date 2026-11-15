
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Server.Model;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/readings")]
    public class Readings : ControllerBase
    {
        private readonly Gestionesessioni sessioni;
        private readonly Crittografia_AES cifratura;
        private readonly Controllo_JSON schemi;
        private readonly Salvataggiomisura archivio;

        public Readings(
            Gestionesessioni sessioni,
            Crittografia_AES cifratura,
            Controllo_JSON schemi,
            Salvataggiomisura archivio)
        {
            this.sessioni = sessioni;
            this.cifratura = cifratura;
            this.schemi = schemi;
            this.archivio = archivio;
        }

        [HttpPost]
        public async Task<IActionResult> readings(
            [FromBody] BustaCifrata busta)
        {
            Console.WriteLine("1 - Richiesta ricevuta.");

            // Validazione della busta cifrata
            if (!schemi.Valida(
                "busta_cifrata.json",
                JsonSerializer.Serialize(busta)))
            {
                return BadRequest();
            }

            Console.WriteLine("2 - Busta validata.");

            // Verifica dell'esistenza della sessione
            if (!sessioni.Cerca(
                busta.SessionId,
                out Sessione sessione))
            {
                return Unauthorized();
            }

            Console.WriteLine("3 - Sessione trovata.");

            try
            {
                // Decifratura della richiesta
                string json = cifratura.Decifra(
                    sessione.Chiave,
                    busta.Ciphertext,
                    busta.Tag
                );

                Console.WriteLine("4 - Richiesta decifrata.");

                // Validazione della richiesta
                if (!schemi.Valida(
                    "richieste_reading.json",
                    json))
                {
                    return BadRequest(new
                    {
                        messaggio = "Richiesta non valida."
                    });
                }

                Console.WriteLine("5 - Richiesta validata.");

                // Lettura dei parametri
                using JsonDocument doc =
                    JsonDocument.Parse(json);

                string id = doc.RootElement
                    .GetProperty("id_sensore")
                    .GetString()!;

                DateTime inizio = doc.RootElement
                    .GetProperty("ora_inizio")
                    .GetDateTime();

                DateTime fine = doc.RootElement
                    .GetProperty("ora_fine")
                    .GetDateTime();

                if (inizio > fine)
                {
                    return BadRequest(new
                    {
                        messaggio = "Intervallo temporale non valido."
                    });
                }

                Console.WriteLine("6 - Parametri letti.");

                // Lettura delle misure archiviate
                List<MisuraSensore> tutte =
                    await archivio.Leggi();

                // Ricerca delle misure richieste
                var risultato = tutte.Where(m =>
                    m.SensorId == id &&
                    m.Timestamp >= inizio &&
                    m.Timestamp <= fine
                ).ToList();

                Console.WriteLine(
                    "7 - Misure trovate: " + risultato.Count
                );

                // Creazione della risposta JSON
                string risposta = JsonSerializer.Serialize(new
                {
                    misure = risultato
                });

                // Cifratura della risposta
                var cifrato = cifratura.Cifra(
                    sessione.Chiave,
                    risposta
                );

                // Invio della risposta cifrata
                return Ok(new BustaCifrata
                {
                    SessionId = busta.SessionId,
                    Ciphertext = cifrato.TestoCifrato,
                    Tag = cifrato.Tag
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "ERRORE READINGS: " + ex.Message
                );

                return BadRequest(new
                {
                    messaggio = "Richiesta non decifrabile."
                });
            }
        }
    }
}
