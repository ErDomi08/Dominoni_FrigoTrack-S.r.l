using System;
using System.IO;
using System.Text.Json;
using Json.Schema;

namespace Client.Services
{
    public class Controllo_JSON
    {
        public bool Valida(string nomeSchema, string json)
        {
            string percorso = Path.Combine(
                AppContext.BaseDirectory,
                "schemas",
                nomeSchema
            );

            string contenutoSchema = File.ReadAllText(percorso);

            JsonSchema schema = JsonSchema.FromText(contenutoSchema);

            using JsonDocument documento = JsonDocument.Parse(json);

            EvaluationResults risultato =
                schema.Evaluate(documento.RootElement);

            return risultato.IsValid;
        }
    }
}