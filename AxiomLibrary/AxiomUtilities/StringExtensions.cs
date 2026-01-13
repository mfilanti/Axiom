using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Axiom.Utilities
{
    public static class StringExtensions
    {
		/// <summary>
		/// Cripta una stringa utilizzando AES con una chiave fornita.
		/// </summary>
		/// <param name="plainText">Testo</param>
		/// <param name="key">Chiave</param>
		/// <returns>testo criptato</returns>
		public static string Encrypt(this string plainText, string key)
		{
			using var aes = Aes.Create();
			// 1. Prepariamo la chiave (deve essere di 32 byte per AES-256)
			aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

			// 2. Generiamo un IV casuale nuovo ogni volta
			aes.GenerateIV();
			byte[] iv = aes.IV;

			using var encryptor = aes.CreateEncryptor(aes.Key, iv);
			using var ms = new MemoryStream();

			// 3. Scriviamo l'IV non cifrato all'inizio dello stream
			ms.Write(iv, 0, iv.Length);

			using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
			using (var sw = new StreamWriter(cs))
			{
				sw.Write(plainText);
			}

			// Il risultato contiene: [16 byte di IV] + [Dati Cifrati]
			return Convert.ToBase64String(ms.ToArray());
		}

		/// <summary>
		/// Decripta una stringa AES con la chiave fornita.
		/// </summary>
		/// <param name="cipherText">Testo criptato</param>
		/// <param name="key">Chiave</param>
		/// <returns>Testo in chiaro</returns>
		public static string Decrypt(this string cipherText, string key)
		{
			// 1. Convertiamo la stringa Base64 in byte
			byte[] fullCipher = Convert.FromBase64String(cipherText);

			using var aes = Aes.Create();
			// Prepariamo la chiave (stessa logica usata nell'Encrypt)
			aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

			// 2. Estraiamo l'IV (i primi 16 byte)
			byte[] iv = new byte[16];
			byte[] cipherData = new byte[fullCipher.Length - 16];

			Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
			Buffer.BlockCopy(fullCipher, 16, cipherData, 0, cipherData.Length);

			aes.IV = iv;

			// 3. Decriptiamo il resto dei dati
			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream(cipherData);
			using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			using var sr = new StreamReader(cs);

			return sr.ReadToEnd();
		}
	}
}
