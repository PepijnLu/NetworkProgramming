using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class MD5Helper
{
    public static string EncryptToMD5(string input)
    {
        Debug.Log($"MD5Helper: input = {input}");
        using (MD5 md5 = MD5.Create())
        {
            // Convert the input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Compute the hash
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert hash bytes to hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2")); // lowercase hex

            Debug.Log($"MD5Helper: output = {sb.ToString()}");
            return sb.ToString();
        }
    }
}
