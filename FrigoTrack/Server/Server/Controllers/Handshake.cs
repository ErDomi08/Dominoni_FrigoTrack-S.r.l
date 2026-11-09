using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Server.Model;
using Server.Services;
namespace Server.Controllers
{
    [ApiController]
    [Route("api/handshake")]
    public class Handshake : ControllerBase
    {
        private readonly DiffieHellman dh;
        private readonly Gestionesessioni sessioni;
        private readonly Controllo_JSON schemi;

        public Handshake(
            DiffieHellman dh,
            Gestionesessioni sessioni,
            Controllo_JSON schemi)
        {
            this.dh = dh;
            this.sessioni = sessioni;
            this.schemi = schemi;
        }

        [HttpPost]
        public IActionResult handshake([FromBody] JsonElement richiesta)
        {
            string json = richiesta.GetRawText();

            if (!schemi.Valida("richiesta_handshake.json", json))
                return BadRequest(new { messaggio = "Richiesta handshake non valida." });

            try
            {
                string p = richiesta.GetProperty("p").GetString()!;
                string g = richiesta.GetProperty("g").GetString()!;
                string A = richiesta.GetProperty("chiave_pubblica_client_A").GetString()!;

                var risultato = dh.GeneraChiavi(p, g, A);

                string id = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(4)
                ).ToLowerInvariant();

                sessioni.Aggiungi(new Sessione
                {
                    SessionId = id,
                    Chiave = risultato.ChiaveSegreta
                });

                return Ok(new
                {
                    session_id = id,
                    server = risultato.ChiavePubblica
                });
            }
            catch
            {
                return BadRequest(new { messaggio = "Parametri Diffie-Hellman non validi." });
            }
        }
    }
}
