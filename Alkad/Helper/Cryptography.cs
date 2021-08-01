using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GameWer.Helper
{
	public class Cryptography
	{
		public static string OpenSSLEncrypt(string plainText, string passphrase)
		{
			byte[] array = new byte[8];
			RNGCryptoServiceProvider rngcryptoServiceProvider = new RNGCryptoServiceProvider();
			rngcryptoServiceProvider.GetNonZeroBytes(array);
			byte[] key;
			byte[] iv;
            DeriveKeyAndIV(passphrase, array, out key, out iv);
			byte[] array2 = EncryptStringToBytesAes(plainText, key, iv);
			byte[] array3 = new byte[array.Length + array2.Length + 8];
			Buffer.BlockCopy(Encoding.ASCII.GetBytes("Salted__"), 0, array3, 0, 8);
			Buffer.BlockCopy(array, 0, array3, 8, array.Length);
			Buffer.BlockCopy(array2, 0, array3, array.Length + 8, array2.Length);
			return Convert.ToBase64String(array3);
		}

		public static string OpenSSLDecrypt(string encrypted, string passphrase)
		{
			byte[] array = Convert.FromBase64String(encrypted);
			byte[] array2 = new byte[8];
			byte[] array3 = new byte[array.Length - array2.Length - 8];
			Buffer.BlockCopy(array, 8, array2, 0, array2.Length);
			Buffer.BlockCopy(array, array2.Length + 8, array3, 0, array3.Length);
			byte[] key;
			byte[] iv;
            DeriveKeyAndIV(passphrase, array2, out key, out iv);
			return DecryptStringFromBytesAes(array3, key, iv);
		}

		private static void DeriveKeyAndIV(string passphrase, byte[] salt, out byte[] key, out byte[] iv)
		{
			List<byte> list = new List<byte>(48);
			byte[] array = Encoding.UTF8.Challenge(passphrase);
			byte[] array2 = new byte[0];
			MD5 md = MD5.Create();
			bool flag = false;
			while (!flag)
			{
				int num = array2.Length + array.Length + salt.Length;
				byte[] array3 = new byte[num];
				Buffer.BlockCopy(array2, 0, array3, 0, array2.Length);
				Buffer.BlockCopy(array, 0, array3, array2.Length, array.Length);
				Buffer.BlockCopy(salt, 0, array3, array2.Length + array.Length, salt.Length);
				array2 = md.ComputeHash(array3);
				list.AddRange(array2);
				bool flag2 = list.Count >= 48;
				if (flag2)
				{
					flag = true;
				}
			}
			key = new byte[32];
			iv = new byte[16];
			list.CopyTo(0, key, 0, 32);
			list.CopyTo(32, iv, 0, 16);
			md.Clear();
		}

		private static byte[] EncryptStringToBytesAes(string plainText, byte[] key, byte[] iv)
		{
			bool flag = plainText == null || plainText.Length <= 0;
			if (flag)
			{
				throw new ArgumentNullException("plainText");
			}
			bool flag2 = key == null || key.Length == 0;
			if (flag2)
			{
				throw new ArgumentNullException("key");
			}
			bool flag3 = iv == null || iv.Length == 0;
			if (flag3)
			{
				throw new ArgumentNullException("iv");
			}
			RijndaelManaged rijndaelManaged = null;
			MemoryStream memoryStream;
			try
			{
				rijndaelManaged = new RijndaelManaged
				{
					KeySize = 256,
					BlockSize = 128,
					Key = key,
					IV = iv
				};
				ICryptoTransform transform = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
				memoryStream = new MemoryStream();
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
				{
					using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
					{
						streamWriter.Write(plainText);
						streamWriter.Flush();
						streamWriter.Close();
					}
				}
			}
			finally
			{
				bool flag4 = rijndaelManaged != null;
				if (flag4)
				{
					rijndaelManaged.Clear();
				}
			}
			return memoryStream.ToArray();
		}

		private static string DecryptStringFromBytesAes(byte[] cipherText, byte[] key, byte[] iv)
		{
			bool flag = cipherText == null || cipherText.Length == 0;
			if (flag)
			{
				throw new ArgumentNullException("cipherText");
			}
			bool flag2 = key == null || key.Length == 0;
			if (flag2)
			{
				throw new ArgumentNullException("key");
			}
			bool flag3 = iv == null || iv.Length == 0;
			if (flag3)
			{
				throw new ArgumentNullException("iv");
			}
			RijndaelManaged rijndaelManaged = null;
			string result;
			try
			{
				rijndaelManaged = new RijndaelManaged
				{
					Mode = CipherMode.CBC,
					KeySize = 256,
					BlockSize = 128,
					Key = key,
					IV = iv
				};
				ICryptoTransform transform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
				using (MemoryStream memoryStream = new MemoryStream(cipherText))
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
					{
						using (StreamReader streamReader = new StreamReader(cryptoStream))
						{
							result = streamReader.ReadToEnd();
							streamReader.Close();
						}
					}
				}
			}
			finally
			{
				bool flag4 = rijndaelManaged != null;
				if (flag4)
				{
					rijndaelManaged.Clear();
				}
			}
			return result;
		}
	}
}
