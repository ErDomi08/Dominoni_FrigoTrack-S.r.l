using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Server.Services
{
    public class Controllo_JSON{ 
        private readonly string percorso;

        public Controllo_JSON(IWebHostEnvironment env)
        {
            percorso = Path.Combine(env.ContentRootPath, "schemas");
        }

        public bool Valida(string nomeSchema, string json)
        {
            string file = Path.Combine(percorso, nomeSchema);

            if (!File.Exists(file))
                throw new FileNotFoundException("Schema non trovato: " + file);

            string testoSchema = File.ReadAllText(file);

            JsonSchema schema = JsonSchema.FromText(testoSchema);
            JsonNode? dati = JsonNode.Parse(json);

            if (dati == null)
                return false;

            // Conversione da JsonNode a JsonElement
            JsonElement elemento = dati.AsObject().AsValue().GetValue<JsonElement>();

            EvaluationOptions opzioni = new()
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true
            };

            var risultato = schema.Evaluate(elemento, opzioni);

            return risultato.IsValid;
        }
    }

}

