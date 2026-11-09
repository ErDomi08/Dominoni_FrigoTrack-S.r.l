using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Client.Model;
using Client.Services;

namespace Client
{
    internal class Program
    {
        private static readonly HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };

        private static readonly Diffie_Hellman dh =
            new Diffie_Hellman();

        private static readonly CrittografiaAES aes =
            new CrittografiaAES();

        private static readonly Controllo_JSON schemi =
            new Controllo_JSON();

        private static byte[] chiave = Array.Empty<byte>();

        private static string sessionId = "";

        static async Task Main(string[] args)
        {
            Console.WriteLine("FRIGOTRACK - CLIENT");
            Console.WriteLine("====================");

            await TestHandshake();

            if (string.IsNullOrEmpty(sessionId))
                return;

            await TestMisuraValida();

            await TestMisuraNonValida();

            await TestBustaManomessa();

            await TestSessioneInesistente();

            Console.WriteLine("\nTEST COMPLETATI.");

            Console.WriteLine("\nAvvio simulazione sensori...");

            await SimulaSensori();
        }

        // TEST 1: handshake
        static async Task TestHandshake()
        {
            Console.WriteLine("\nTEST 1 - Handshake");

            var richiesta = new
            {
                p = dh.P,
                g = dh.G,
                chiave_pubblica_client_A = dh.ChiavePubblica
            };

            string json = JsonSerializer.Serialize(richiesta);

            if (!schemi.Valida("richiesta_handshake.json", json))
            {
                Console.WriteLine("ERRORE: richiesta handshake non valida.");
                return;
            }

            HttpResponseMessage risposta =
                await http.PostAsJsonAsync("api/handshake", richiesta);

            if (!risposta.IsSuccessStatusCode)
            {
                Console.WriteLine("TEST FALLITO: handshake rifiutato.");
                return;
            }

            string contenuto = await risposta.Content.ReadAsStringAsync();

            using JsonDocument documento = JsonDocument.Parse(contenuto);

            sessionId = documento.RootElement
                .GetProperty("session_id").GetString()!;

            string B = documento.RootElement
                .GetProperty("server").GetString()!;

            chiave = dh.GeneraChiaveSegreta(B);

            Console.WriteLine("TEST SUPERATO: handshake completato.");
            Console.WriteLine("Sessione: " + sessionId);
        }

        // TEST 2: invio misura valida
        static async Task TestMisuraValida()
        {
            Console.WriteLine("\nTEST 2 - Misura valida");

            MisuraSensore misura = CreaMisura(
                "FRIGO-BG-001",
                "food",
                4.5
            );

            HttpResponseMessage risposta =
                await InviaMisura(misura);

            if (risposta.StatusCode == HttpStatusCode.OK)
            {
                string json = await risposta.Content.ReadAsStringAsync();

                Busta_cifrata? busta =
                    JsonSerializer.Deserialize<Busta_cifrata>(json);

                if (busta != null)
                {
                    string ack = aes.Decifra(
                        chiave,
                        busta.Ciphertext,
                        busta.Tag
                    );

                    Console.WriteLine("Risposta server: " + ack);
                }

                Console.WriteLine("TEST SUPERATO.");
            }
            else
            {
                Console.WriteLine("TEST FALLITO: " + risposta.StatusCode);
            }
        }

        // TEST 3: misura non valida
        static async Task TestMisuraNonValida()
        {
            Console.WriteLine("\nTEST 3 - Misura non valida");

            MisuraSensore misura = CreaMisura(
                "FRIGO-BG-001",
                "food",
                100
            );

            // Si bypassa intenzionalmente la validazione client
            // per verificare che il server rifiuti la misura.

            string json = JsonSerializer.Serialize(misura);

            Busta_cifrata busta = CifraMessaggio(json);

            HttpResponseMessage risposta =
                await http.PostAsJsonAsync("api/telemetry", busta);

            if (risposta.StatusCode == HttpStatusCode.BadRequest)
                Console.WriteLine("TEST SUPERATO: misura rifiutata.");
            else
                Console.WriteLine("TEST FALLITO: " + risposta.StatusCode);
        }

        // TEST 4: busta manomessa
        static async Task TestBustaManomessa()
        {
            Console.WriteLine("\nTEST 4 - Busta manomessa");

            MisuraSensore misura = CreaMisura(
                "FRIGO-BG-001",
                "food",
                4.5
            );

            string json = JsonSerializer.Serialize(misura);

            Busta_cifrata busta = CifraMessaggio(json);

            byte[] dati = Convert.FromBase64String(busta.Ciphertext);

            // Modifica un byte del ciphertext
            dati[dati.Length - 1] ^= 1;

            busta.Ciphertext = Convert.ToBase64String(dati);

            HttpResponseMessage risposta =
                await http.PostAsJsonAsync("api/telemetry", busta);

            if (risposta.StatusCode == HttpStatusCode.BadRequest)
                Console.WriteLine("TEST SUPERATO: manomissione rilevata.");
            else
                Console.WriteLine("TEST FALLITO: " + risposta.StatusCode);
        }

        // TEST 5: sessione inesistente
        static async Task TestSessioneInesistente()
        {
            Console.WriteLine("\nTEST 5 - Sessione inesistente");

            Busta_cifrata busta = new Busta_cifrata
            {
                SessionId = "deadbeef",
                Ciphertext = Convert.ToBase64String(new byte[20]),
                Tag = Convert.ToBase64String(new byte[16])
            };

            HttpResponseMessage risposta =
                await http.PostAsJsonAsync("api/telemetry", busta);

            if (risposta.StatusCode == HttpStatusCode.Unauthorized)
                Console.WriteLine("TEST SUPERATO: sessione inesistente rifiutata.");
            else
                Console.WriteLine("TEST FALLITO: " + risposta.StatusCode);
        }

        // Invio normale di una misura
        static async Task<HttpResponseMessage> InviaMisura(
            MisuraSensore misura)
        {
            string json = JsonSerializer.Serialize(misura);

            if (!schemi.Valida("misura_sensore.json", json))
                throw new Exception("Misura non valida secondo JSON Schema.");

            Busta_cifrata busta = CifraMessaggio(json);

            return await http.PostAsJsonAsync("api/telemetry", busta);
        }

        // Cifratura e creazione della busta
        static Busta_cifrata CifraMessaggio(string json)
        {
            var risultato = aes.Cifra(chiave, json);

            return new Busta_cifrata
            {
                SessionId = sessionId,
                Ciphertext = risultato.Ciphertext,
                Tag = risultato.Tag
            };
        }

        // Creazione di una misura
        static MisuraSensore CreaMisura(
            string id,
            string tipo,
            double temperatura)
        {
            return new MisuraSensore
            {
                SensorId = id,
                Timestamp = DateTime.UtcNow.ToString("O"),
                CargoType = tipo,
                TemperatureC = temperatura,
                Humidity = 65,
                Battery = 95
            };
        }

        // Simulazione dei tre sensori
        static async Task SimulaSensori()
        {
            MisuraSensore[] sensori =
            {
                CreaMisura("FRIGO-BG-001", "food", 4.5),
                CreaMisura("CAMION-MI-002", "pharma", 5.0),
                CreaMisura("FRIGO-TO-003", "frozen", -20.0)
            };

            while (true)
            {
                foreach (MisuraSensore sensore in sensori)
                {
                    sensore.Timestamp = DateTime.UtcNow.ToString("O");

                    try
                    {
                        HttpResponseMessage risposta =
                            await InviaMisura(sensore);

                        Console.WriteLine(
                            $"{sensore.SensorId} - HTTP {(int)risposta.StatusCode}"
                        );

                        if (risposta.IsSuccessStatusCode)
                        {
                            string contenuto =
                                await risposta.Content.ReadAsStringAsync();

                            Busta_cifrata? busta =
                                JsonSerializer.Deserialize<Busta_cifrata>(contenuto);

                            if (busta != null)
                            {
                                string ack = aes.Decifra(
                                    chiave,
                                    busta.Ciphertext,
                                    busta.Tag
                                );

                                Console.WriteLine("ACK: " + ack);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Errore: " + ex.Message);
                    }
                }

                await Task.Delay(5000);
            }
        }
    }
}