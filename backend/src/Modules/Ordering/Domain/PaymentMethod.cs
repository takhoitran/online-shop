namespace OnlineShop.Modules.Ordering.Domain;

/// <summary>MVP scope per mentor decision: COD or manual bank transfer — no payment gateway
/// integration (VNPay/Momo) yet.</summary>
public enum PaymentMethod
{
    Cod,
    BankTransfer
}
