using System;

namespace My_FoodApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }

        // cod, bank_transfer, qr, credit_card, wallet
        public string Method { get; set; } = "cod";

        public decimal Amount { get; set; }

        // path สลิป เช่น /payment_slips/order_5_xxx.jpg
        public string? SlipImagePath { get; set; }

        public DateTime? SlipUploadedAt { get; set; }

        // pending, paid, failed, refunded, waiting_confirm
        public string Status { get; set; } = "pending";

        public string? TxnRef { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
