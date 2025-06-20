using System;
using System.Collections.Generic;

namespace OnlineGallery.Models;

public partial class User
{
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; }
    public string? ExpirationDate { get; set; }
    public string? CVV { get; set; }
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }
    public string? RoleName { get; set; }

    public DateTime? DateCreated { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    public virtual ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();

}
