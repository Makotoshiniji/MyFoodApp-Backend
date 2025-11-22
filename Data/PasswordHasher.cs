using System;
using System.Security.Cryptography;

namespace My_FoodApp.Data
{
    // คลาสนี้ใช้สำหรับ Hash และ Verify รหัสผ่าน
    // โดยใช้ PBKDF2 (เป็นมาตรฐานที่ปลอดภัย)
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        // เมธอดสำหรับ "เข้ารหัส" รหัสผ่าน
        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            // รวม Salt และ Hash เข้าด้วยกันเพื่อเก็บลง DB
            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        // เมธอดสำหรับ "ตรวจสอบ" รหัสผ่าน
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // แยก Salt และ Hash ออกจากกัน
                var parts = hashedPassword.Split(Delimiter);
                if (parts.Length != 2)
                {
                    return false; // รูปแบบไม่ถูกต้อง
                }

                var salt = Convert.FromBase64String(parts[0]);
                var hash = Convert.FromBase64String(parts[1]);

                // ทำการ Hash รหัสผ่านที่ผู้ใช้ป้อนเข้ามาใหม่ โดยใช้ Salt เดิม
                var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

                // เปรียบเทียบผลลัพธ์ (ต้องใช้ CryptographicOperations.FixedTimeEquals เพื่อป้องกัน Timing Attacks)
                return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
            }
            catch
            {
                // ถ้ามี Error (เช่น Base64 ผิด) ให้ถือว่าไม่ผ่าน
                return false;
            }
        }
    }
}