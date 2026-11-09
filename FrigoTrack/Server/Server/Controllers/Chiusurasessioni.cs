using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Server.Model;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/handshake")]
    public class Chiusurasessioni : ControllerBase
    {
        private readonly Gestionesessioni sessioni;
        private readonly Crittografia_AES cifratura;

        public Chiusurasessioni(
            Gestionesessioni sessioni,
            Crittografia_AES cifratura)
        {
            this.sessioni = sessioni;
            this.cifratura = cifratura;
        }

        [HttpPost]
        public IActionResult Close([FromBody] BustaCifrata busta)
        {
            if (!sessioni.Cerca(busta.SessionId, out Sessione sessione))
                return Unauthorized();

            try
            {
                // Verifica che la richiesta sia autentica
                cifratura.Decifra(
                    sessione.Chiave,
                    busta.TestoCifrato,
                    busta.Tag
                );

                // Elimina la sessione
                sessioni.Rimuovi(busta.SessionId);

                return Ok(new { messaggio = "Sessione chiusa correttamente." });
            }
            catch
            {
                return BadRequest(new { messaggio = "Richiesta non valida." });
            }
        }
    }
}
