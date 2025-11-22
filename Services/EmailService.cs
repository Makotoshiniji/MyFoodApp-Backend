using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace My_FoodApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // 1. อ่านค่าจาก appsettings.json
            var emailSettings = _config.GetSection("EmailSettings");

            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"]);
            var fromEmail = emailSettings["FromEmail"];
            var password = emailSettings["Password"];

            // 2. ตั้งค่า SMTP Client
            var client = new SmtpClient(host, port)
            {
                EnableSsl = true,

                // 🔴 สำคัญมาก: ต้องสั่งปิด UseDefaultCredentials ก่อนบรรทัด Credentials เสมอ
                // เพื่อแก้ปัญหา 5.7.0 Authentication Required ของ Gmail
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(fromEmail, password)
            };

            // 3. สร้างอีเมล
            var mailMessage = new MailMessage(from: fromEmail!, to: toEmail, subject, message)
            {
                IsBodyHtml = true // รองรับ HTML (ตัวหนา, สี)
            };

            // 4. ส่งอีเมล
            await client.SendMailAsync(mailMessage);
        }
    }
}