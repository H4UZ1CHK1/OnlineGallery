using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineGallery.Models;

public partial class Transactions
{
    public int TransactionId { get; set; }

    public int UserId { get; set; }

    public int ImageId { get; set; }

    public DateTime TransactionDate { get; set; }

    public virtual User? User { get; set; }

    public virtual Image? Image { get; set; }

}
