using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Server.Services
{
    public class Controllo_JSON
    {
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

            JsonDocument documento = JsonDocument.Parse(json);

            EvaluationOptions opzioni = new()
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true
            };

            var risultato = schema.Evaluate(
                documento.RootElement,
                opzioni
            );

            return risultato.IsValid;
        }

    }
}

