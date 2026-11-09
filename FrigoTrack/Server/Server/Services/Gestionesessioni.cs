using System.Collections.Concurrent;
using Server.Model;

namespace Server.Services
{
    public class Gestionesessioni
    {
        private readonly ConcurrentDictionary<string, Sessione> sessioni = new();

        public void Aggiungi(Sessione sessione)
        {
            sessioni[sessione.SessionId] = sessione;
        }

        public bool Cerca(string id, out Sessione sessione)
        {
            return sessioni.TryGetValue(id, out sessione!);
        }

        public void Rimuovi(string id)
        {
            sessioni.TryRemove(id, out _);
        }
    }
}
