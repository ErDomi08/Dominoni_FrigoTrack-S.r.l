using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
    internal class Diffie_Hellman
    {
        private readonly BigInteger p;
        private readonly BigInteger g;
        private readonly BigInteger a;

        public string P => p.ToString();
        public string G => g.ToString();

        public string ChiavePubblica { get; }

        public Diffie_Hellman()
        {
            string numeroEsadecimale =
                "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1" +
                "29024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
                "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245" +
                "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
                "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D" +
                "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F" +
                "83655D23DCA3AD961C62F356208552BB9ED529077096966D" +
                "670C354E4ABC9804F1746C08CA237327FFFFFFFFFFFFFFFF";

            byte[] pBytes = Convert.FromHexString(numeroEsadecimale);

            p = new BigInteger(
                pBytes,
                isUnsigned: true,
                isBigEndian: true
            );

            g = new BigInteger(2);

            // Generazione della chiave privata del client
            byte[] casuali = RandomNumberGenerator.GetBytes(32);

            a = new BigInteger(
                casuali,
                isUnsigned: true,
                isBigEndian: true
            );

            a = (a % (p - 3)) + 2;

            // Chiave pubblica A = g^a mod p
            BigInteger A = BigInteger.ModPow(g, a, p);

            ChiavePubblica = A.ToString();
        }

        public byte[] GeneraChiaveSegreta(string chiavePubblicaServer)
        {
            BigInteger B = BigInteger.Parse(chiavePubblicaServer);

            if (B <= 1 || B >= p)
                throw new ArgumentException("Chiave pubblica del server non valida.");

            // Segreto condiviso K = B^a mod p
            BigInteger K = BigInteger.ModPow(B, a, p);

            // Derivazione della chiave AES tramite SHA256
            byte[] chiave = SHA256.HashData(
                Encoding.UTF8.GetBytes(K.ToString())
            );

            return chiave;
        }
    }
}
