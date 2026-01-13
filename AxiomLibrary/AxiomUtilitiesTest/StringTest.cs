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
            string encrypted = Axiom.Utilities.StringExtensions.Encrypt(original, key);
            string decrypted = Axiom.Utilities.StringExtensions.Decrypt(encrypted, key);
            Assert.AreEqual(original, decrypted);
		}
    }
}
