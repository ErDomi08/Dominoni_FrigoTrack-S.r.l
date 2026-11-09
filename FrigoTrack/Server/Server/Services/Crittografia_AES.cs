using System.Security.Cryptography;

namespace Server.Services
{
    public class Crittografia_AES
    {
        public (string TestoCifrato, string Tag) Cifra(byte[] chiave, string testo)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] dati = System.Text.Encoding.UTF8.GetBytes(testo);

            byte[] cifrato = new byte[dati.Length];
            byte[] tag = new byte[16];

            using (AesGcm aes = new AesGcm(chiave, 16))
            {
                aes.Encrypt(nonce, dati, cifrato, tag);
            }

            // Uniamo nonce e testo cifrato
            byte[] risultato = new byte[nonce.Length + cifrato.Length];

            Buffer.BlockCopy(nonce, 0, risultato, 0, nonce.Length);
            Buffer.BlockCopy(cifrato, 0, risultato, nonce.Length, cifrato.Length);

            return (
                Convert.ToBase64String(risultato),
                Convert.ToBase64String(tag)
            );
        }

        public string Decifra(
            byte[] chiave,
            string testoCifrato,
            string tagBase64)
        {
            byte[] dati = Convert.FromBase64String(testoCifrato);
            byte[] tag = Convert.FromBase64String(tagBase64);

            if (dati.Length < 12 || tag.Length != 16)
                throw new CryptographicException("Busta non valida.");

            byte[] nonce = dati[..12];
            byte[] cifrato = dati[12..];
            byte[] testo = new byte[cifrato.Length];

            using (AesGcm aes = new AesGcm(chiave, 16))
            {
                aes.Decrypt(nonce, cifrato, tag, testo);
            }

            return System.Text.Encoding.UTF8.GetString(testo);
        }
    }
}
