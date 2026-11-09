using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Server.Model;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("/api/telemetry")]
    public class Telemetria : ControllerBase
    {
        private readonly Gestionesessioni sessioni;
        private readonly Crittografia_AES cifratura;
        private readonly Controllo_JSON schemi;
        private readonly Salvataggiomisura archivio;

        public Telemetria(
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
        public async Task<IActionResult> Telemetry([FromBody] BustaCifrata busta)
        {
            if (!schemi.Valida(
                "busta_cifrata.json",
                JsonSerializer.Serialize(busta)))
            {
                return BadRequest(new { messaggio = "Busta non valida." });
            }

            if (!sessioni.Cerca(busta.SessionId, out Sessione sessione))
                return Unauthorized(new { messaggio = "Sessione inesistente." });

            try
            {
                // Prima decifriamo il messaggio
                string json = cifratura.Decifra(
                    sessione.Chiave,
                    busta.TestoCifrato,
                    busta.Tag
                );

                // Poi validiamo il contenuto
                if (!schemi.Valida("misura_sensore.json", json))
                    return BadRequest(new { messaggio = "Misura non valida secondo lo schema." });

                MisuraSensore? misura =
                    JsonSerializer.Deserialize<MisuraSensore>(json);

                if (misura == null)
                    return BadRequest();

                await archivio.Salva(misura);

                bool allarme = misura.CargoType switch
                {
                    "pharma" => misura.TemperatureC < 2 || misura.TemperatureC > 8,
                    "frozen" => misura.TemperatureC > -18,
                    "food" => misura.TemperatureC < 0 || misura.TemperatureC > 8,
                    _ => true
                };

                string risposta = JsonSerializer.Serialize(new
                {
                    stato = "OK",
                    allarme = allarme,
                    messaggio = allarme
                        ? "Temperatura fuori dall'intervallo di sicurezza."
                        : "Misura ricevuta correttamente."
                });

                var cifrato = cifratura.Cifra(sessione.Chiave, risposta);

                return Ok(new BustaCifrata
                {
                    SessionId = busta.SessionId,
                    TestoCifrato = cifrato.TestoCifrato,
                    Tag = cifrato.Tag
                });
            }
            catch (Exception)
            {
                return BadRequest(new { messaggio = "Decifratura fallita o dati non validi." });
            }
        }
    }
}
