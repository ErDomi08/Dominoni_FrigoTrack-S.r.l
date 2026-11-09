using System.Text.Json;
using Server.Model;

namespace Server.Services
{
    public class Salvataggiomisura
    {
        private readonly string cartella;
        private readonly string file;
        private readonly SemaphoreSlim blocco = new(1, 1);

        public Salvataggiomisura(IWebHostEnvironment env)
        {
            cartella = Path.Combine(env.ContentRootPath, "Data", "Readings");

            Directory.CreateDirectory(cartella);

            file = Path.Combine(cartella, "misure.jsonl");
        }

        public async Task Salva(MisuraSensore misura)
        {
            string json = JsonSerializer.Serialize(misura);

            await blocco.WaitAsync();

            try
            {
                await File.AppendAllTextAsync(file, json + Environment.NewLine);
            }
            finally
            {
                blocco.Release();
            }
        }

        public async Task<List<MisuraSensore>> Leggi()
        {
            List<MisuraSensore> misure = new();

            await blocco.WaitAsync();

            try
            {
                if (!File.Exists(file))
                    return misure;

                string[] righe = await File.ReadAllLinesAsync(file);

                foreach (string riga in righe)
                {
                    if (!string.IsNullOrWhiteSpace(riga))
                    {
                        MisuraSensore? misura =
                            JsonSerializer.Deserialize<MisuraSensore>(riga);

                        if (misura != null)
                            misure.Add(misura);
                    }
                }
            }
            finally
            {
                blocco.Release();
            }
            return misure;
        }
    }
}
