namespace Widwickyy.SaveSystem
{
    public interface IStringCipher
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
