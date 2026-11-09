using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Server.Services
{
    public class DiffieHellman
    {
        public (string ChiavePubblica, byte[] ChiaveSegreta)
        GeneraChiavi(string pText, string gText, string aText)
        {
            BigInteger p = BigInteger.Parse(pText);
            BigInteger g = BigInteger.Parse(gText);
            BigInteger A = BigInteger.Parse(aText);

            if (p <= 3 || g <= 1 || g >= p || A <= 1 || A >= p)
                throw new ArgumentException("Parametri del Diffie-Hellman non validi.");

            // Generazione del segreto privato del server
            byte[] casuali = RandomNumberGenerator.GetBytes(32);

            BigInteger b = new BigInteger(
                casuali,
                isUnsigned: true,
                isBigEndian: true
            );

            b = (b % (p - 2)) + 2;

            // Chiave pubblica del server: B = g^b mod p
            BigInteger B = BigInteger.ModPow(g, b, p);

            // Segreto condiviso: K = A^b mod p
            BigInteger K = BigInteger.ModPow(A, b, p);

            // Derivazione della chiave AES tramite SHA256
            byte[] chiave = SHA256.HashData(
                Encoding.UTF8.GetBytes(K.ToString())
            );

            return (B.ToString(), chiave);
        }
    }
}
