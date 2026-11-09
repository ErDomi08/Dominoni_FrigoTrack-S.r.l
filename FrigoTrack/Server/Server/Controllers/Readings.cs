using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Server.Model;
using Server.Services;
namespace Server.Controllers
{
    [ApiController]
    [Route("api/readings")]
    public class Readings: ControllerBase
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
        public async Task<IActionResult> readings([FromBody] BustaCifrata busta)
        {
            if (!schemi.Valida(
                "busta_cifrata.json",
                JsonSerializer.Serialize(busta)))
                return BadRequest();

            if (!sessioni.Cerca(busta.SessionId, out Sessione sessione))
                return Unauthorized();

            try
            {
                string json = cifratura.Decifra(
                    sessione.Chiave,
                    busta.TestoCifrato,
                    busta.Tag
                );

                if (!schemi.Valida("richieste_reading.json", json))
                    return BadRequest(new { messaggio = "Richiesta non valida." });

                using JsonDocument doc = JsonDocument.Parse(json);

                string id = doc.RootElement.GetProperty("id_sensore").GetString()!;

                DateTime inizio = doc.RootElement
                    .GetProperty("ora_inizio").GetDateTime();

                DateTime fine = doc.RootElement
                    .GetProperty("ora_fine").GetDateTime();

                if (inizio > fine)
                    return BadRequest(new { messaggio = "Intervallo temporale non valido." });

                List<MisuraSensore> tutte = await archivio.Leggi();

                var risultato = tutte.Where(m =>
                    m.SensorId == id &&
                    m.Timestamp >= inizio &&
                    m.Timestamp <= fine
                ).ToList();

                string risposta = JsonSerializer.Serialize(new
                {
                    misure = risultato
                });

                var cifrato = cifratura.Cifra(sessione.Chiave, risposta);

                return Ok(new BustaCifrata
                {
                    SessionId = busta.SessionId,
                    TestoCifrato = cifrato.TestoCifrato,
                    Tag = cifrato.Tag
                });
            }
            catch
            {
                return BadRequest(new { messaggio = "Richiesta non decifrabile." });
            }
        }
    }
}
