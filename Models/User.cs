using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace BloodDonorApplication.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class User
    {
        
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AlternatePhone { get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public string? BloodGroup { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public string? PinCode { get; set; }
        public bool HasTattoos { get; set; }
        public bool HasDisability { get; set; }
        public DateTime? LastDonationDate { get; set; }

        //LastDonation date not required to connect with data base.

        public bool IsAvailable => 
            !LastDonationDate.HasValue ||
            LastDonationDate.Value <= DateTime.Now.AddMonths(-6);

    }
}
