using System;
using System.Security.Cryptography;
using System.Text;

namespace Axiom.Utilities
{
    public static class StringExtensions
    {
        // Parametri di formato per la cifratura autenticata.
        // Layout del blob (prima della codifica Base64):
        //   [ salt (16) ][ nonce (12) ][ tag (16) ][ ciphertext (N) ]
        private const int SaltSize = 16;   // sale casuale per la derivazione della chiave (KDF)
        private const int NonceSize = 12;  // dimensione standard del nonce per AES-GCM
        private const int TagSize = 16;    // dimensione del tag di autenticazione (max per AES-GCM)
        private const int KeySize = 32;    // AES-256
        private const int Pbkdf2Iterations = 100_000;

        /// <summary>
        /// Cripta una stringa con AES-256-GCM (cifratura <b>autenticata</b>).
        /// La chiave AES è derivata dalla passphrase con PBKDF2 (SHA-256, sale casuale),
        /// non con un semplice padding; ogni chiamata usa sale e nonce casuali, quindi lo
        /// stesso testo produce ogni volta un risultato diverso.
        /// </summary>
        /// <param name="plainText">Testo in chiaro.</param>
        /// <param name="key">Passphrase.</param>
        /// <returns>Blob Base64 contenente sale, nonce, tag e dati cifrati.</returns>
        public static string Encrypt(this string plainText, string key)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            if (key == null) throw new ArgumentNullException(nameof(key));

            // 1. Sale e nonce casuali, nuovi ad ogni chiamata.
            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(nonce);
            }

            // 2. Derivazione della chiave AES-256 dalla passphrase (KDF, non padding).
            byte[] aesKey = DeriveKey(key, salt);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSize];

            // 3. Cifratura autenticata.
            using (var aes = new AesGcm(aesKey))
            {
                aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            // 4. Composizione del blob: [salt][nonce][tag][ciphertext].
            byte[] blob = new byte[SaltSize + NonceSize + TagSize + cipherBytes.Length];
            int offset = 0;
            Buffer.BlockCopy(salt, 0, blob, offset, SaltSize); offset += SaltSize;
            Buffer.BlockCopy(nonce, 0, blob, offset, NonceSize); offset += NonceSize;
            Buffer.BlockCopy(tag, 0, blob, offset, TagSize); offset += TagSize;
            Buffer.BlockCopy(cipherBytes, 0, blob, offset, cipherBytes.Length);

            return Convert.ToBase64String(blob);
        }

        /// <summary>
        /// Decripta una stringa prodotta da <see cref="Encrypt"/>.
        /// Verifica il tag di autenticazione: se il testo cifrato (o la chiave) è stato
        /// manomesso, viene sollevata una <see cref="CryptographicException"/> anziché
        /// restituire dati corrotti.
        /// </summary>
        /// <param name="cipherText">Blob Base64 prodotto da <see cref="Encrypt"/>.</param>
        /// <param name="key">Passphrase.</param>
        /// <returns>Testo in chiaro.</returns>
        public static string Decrypt(this string cipherText, string key)
        {
            if (cipherText == null) throw new ArgumentNullException(nameof(cipherText));
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] blob = Convert.FromBase64String(cipherText);
            if (blob.Length < SaltSize + NonceSize + TagSize)
                throw new CryptographicException("Testo cifrato non valido o troncato.");

            // 1. Estrazione delle sezioni del blob.
            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            int cipherLen = blob.Length - SaltSize - NonceSize - TagSize;
            byte[] cipherBytes = new byte[cipherLen];

            int offset = 0;
            Buffer.BlockCopy(blob, offset, salt, 0, SaltSize); offset += SaltSize;
            Buffer.BlockCopy(blob, offset, nonce, 0, NonceSize); offset += NonceSize;
            Buffer.BlockCopy(blob, offset, tag, 0, TagSize); offset += TagSize;
            Buffer.BlockCopy(blob, offset, cipherBytes, 0, cipherLen);

            // 2. Ri-derivazione della stessa chiave dal sale memorizzato.
            byte[] aesKey = DeriveKey(key, salt);

            // 3. Decifratura autenticata (lancia se il tag non combacia).
            byte[] plainBytes = new byte[cipherLen];
            using (var aes = new AesGcm(aesKey))
            {
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Deriva una chiave AES-256 dalla passphrase usando PBKDF2 (SHA-256).
        /// </summary>
        private static byte[] DeriveKey(string key, byte[] salt)
        {
            using var kdf = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(key), salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(KeySize);
        }
    }
}
