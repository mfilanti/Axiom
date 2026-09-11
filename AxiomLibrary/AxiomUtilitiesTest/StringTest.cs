using System;
using System.Security.Cryptography;
using Axiom.Utilities;

namespace AxiomUtilitiesTest
{
    [TestClass]
    public sealed class StringTest
    {
        [TestMethod]
        public void TestEncryptDecrypt()
        {
            string original = "Hello, World!";
            string key = "123456789";
            string encrypted = StringExtensions.Encrypt(original, key);
            string decrypted = StringExtensions.Decrypt(encrypted, key);
            Assert.AreEqual(original, decrypted);
        }

        [TestMethod]
        public void TestEncryptIsNonDeterministic()
        {
            // Sale e nonce casuali => stesso testo e chiave producono blob diversi.
            string original = "messaggio ripetuto";
            string key = "chiave";
            string a = StringExtensions.Encrypt(original, key);
            string b = StringExtensions.Encrypt(original, key);
            Assert.AreNotEqual(a, b, "La cifratura dovrebbe essere non deterministica.");
            Assert.AreEqual(original, a.Decrypt(key));
            Assert.AreEqual(original, b.Decrypt(key));
        }

        [TestMethod]
        public void TestWrongKeyIsRejected()
        {
            string encrypted = "dato riservato".Encrypt("chiave-giusta");
            Assert.Throws<CryptographicException>(
                () => encrypted.Decrypt("chiave-sbagliata"));
        }

        [TestMethod]
        public void TestTamperedCipherIsRejected()
        {
            string key = "chiave";
            string encrypted = "payload autentico".Encrypt(key);

            // Manomissione di un byte del blob => il tag GCM non combacia più.
            byte[] blob = Convert.FromBase64String(encrypted);
            blob[blob.Length - 1] ^= 0xFF;
            string tampered = Convert.ToBase64String(blob);

            Assert.Throws<CryptographicException>(() => tampered.Decrypt(key));
        }

        [TestMethod]
        public void TestTruncatedCipherIsRejected()
        {
            Assert.Throws<CryptographicException>(
                () => Convert.ToBase64String(new byte[8]).Decrypt("chiave"));
        }
    }
}
