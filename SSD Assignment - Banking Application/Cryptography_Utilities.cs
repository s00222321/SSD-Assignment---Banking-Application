﻿using Banking_Application;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    internal sealed class Cryptography_Utilities
    {
        internal readonly Aes aes;
        private readonly HMACSHA256 hmac;
        private static string KeyFilePath = Environment.GetEnvironmentVariable("KEY_FILE_PATH");

        public Cryptography_Utilities()
        {
            aes = Aes.Create();
            byte[] key = RetrieveOrGenerateKey(KeyFilePath);
            aes.KeySize = 128;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            hmac = new HMACSHA256(key);
        }

        public static byte[] GenerateSymmetricKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                return aesAlg.Key;
            }
        }

        // Store the key securely using DPAPI
        public static void StoreKey(byte[] key, string filePath)
        {
            byte[] protectedKey = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(filePath, protectedKey);
        }

        // Retrieve the key: check if it exists, otherwise create and store a new one
        public static byte[] RetrieveOrGenerateKey(string filePath)
        {
            if (File.Exists(filePath))
            {
                // If the key file exists, retrieve and unprotect the key
                return RetrieveKey(filePath);
            }
            else
            {
                // If the key file does not exist, generate a new key and store it
                byte[] newKey = GenerateSymmetricKey();
                StoreKey(newKey, filePath);
                return newKey;
            }
        }

        // Retrieve the key securely using DPAPI
        private static byte[] RetrieveKey(string filePath)
        {
            byte[] protectedKey = File.ReadAllBytes(filePath);
            byte[] key = ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
            return key;
        }


        public byte[] Encrypt(string plaintextData)
        {
            byte[] byteString = Encoding.UTF8.GetBytes(plaintextData);

            // Generate a random IV
            byte[] iv = new byte[aes.BlockSize / 8]; // AES block size is 128 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv); // Fill the IV with random data
            }

            using var encryptor = aes.CreateEncryptor(aes.Key, iv); // Use AES with key and IV
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(byteString, 0, plaintextData.Length);
            }

            byte[] ciphertextData = msEncrypt.ToArray();

            // Combine the IV and ciphertext
            byte[] combinedOutput = new byte[iv.Length + ciphertextData.Length];
            Buffer.BlockCopy(iv, 0, combinedOutput, 0, iv.Length); // Copy IV to the beginning
            Buffer.BlockCopy(ciphertextData, 0, combinedOutput, iv.Length, ciphertextData.Length); // Copy the ciphertext data

            return combinedOutput; // Return combined IV + ciphertext
        }


        public byte[] Decrypt(byte[] combinedData)
        {
            // Separate IV and ciphertext
            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            byte[] ciphertextData = new byte[combinedData.Length - ivLength];

            // Extract the IV and ciphertext from the combined data
            Buffer.BlockCopy(combinedData, 0, iv, 0, ivLength); // Extract the IV
            Buffer.BlockCopy(combinedData, ivLength, ciphertextData, 0, ciphertextData.Length); // Extract the ciphertext

            // Decrypt the ciphertext using the extracted IV
            using var decryptor = aes.CreateDecryptor(aes.Key, iv); // Use AES with the key and IV
            using var msDecrypt = new MemoryStream();
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
            {
                csDecrypt.Write(ciphertextData, 0, ciphertextData.Length);
            }

            return msDecrypt.ToArray(); // Return the decrypted data
        }


        public byte[] ComputeHash(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException(nameof(inputData));

            return hmac.ComputeHash(inputData); // Compute and return the hash using the HMAC algorithm
        }

        public byte[] ComputeHash(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            byte[] inputData = Encoding.UTF8.GetBytes(input);
            return ComputeHash(inputData); // Compute and return the hash using the HMAC algorithm
        }


        public bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length) return false; // If the hash lengths are different, they are not equal
            // Compare each byte of the hashes
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }
            return true; // If no differences were found, the hashes are equal
        }

        public byte[] CombineByteArrays(params byte[][] arrays)
        {
            // Calculate the total length of the combined array by summing the lengths of non-null arrays
            int totalLength = arrays.Where(a => a != null).Sum(a => a.Length);
            byte[] combined = new byte[totalLength]; // Create a new byte array to hold the combined data

            int offset = 0;
            // Iterate through each array and copy its content into the combined array
            foreach (var array in arrays)
            {
                if (array != null)
                {
                    Buffer.BlockCopy(array, 0, combined, offset, array.Length);
                    offset += array.Length;
                }
            }

            return combined;
        }

        public byte[] GenerateHashForSendingToDB(Bank_Account account)
        {
            // Compute the hash value directly using plaintext fields
            byte[] updatedHashValue = ComputeHash(
                CombineByteArrays(
                    Encoding.UTF8.GetBytes(account.AccountNo),
                    Encoding.UTF8.GetBytes(account.Name),
                    account.AddressLine1 != null ? Encoding.UTF8.GetBytes(account.AddressLine1) : new byte[0],
                    account.AddressLine2 != null ? Encoding.UTF8.GetBytes(account.AddressLine2) : new byte[0],
                    account.AddressLine3 != null ? Encoding.UTF8.GetBytes(account.AddressLine3) : new byte[0],
                    Encoding.UTF8.GetBytes(account.Town),
                    BitConverter.GetBytes(account.Balance),
                    BitConverter.GetBytes(account.GetType() == typeof(Current_Account) ? 1 : 2),
                    BitConverter.GetBytes((account is Current_Account ca) ? ca.OverdraftAmount : 0.0), // Handle nulls
                    BitConverter.GetBytes((account is Savings_Account sa) ? sa.InterestRate : 0.0),
                    ComputeHash(account.AccountNo) // Compute hash of account number
                )
            );
            return updatedHashValue;
        }

    }
}
