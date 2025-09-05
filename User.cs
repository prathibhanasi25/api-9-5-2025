using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InsuranceClaimsAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required] 
        [EmailAddress]
        [MaxLength(50)]
        public string EmailID { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string MobileNo { get; set; } = string.Empty;

        public string? Gender { get; set; }

        public DateTime DOB { get; set; }

        public Boolean EmailConfirmed { get; set; }

        public string EmailConfirmationToken { get; set; } = string.Empty;

        public DateTime EmailConfirmationTokenExpiry { get; set; }

        public Boolean MobileNoConfirmed { get; set; }

        public string MobileConfirmationToken { get; set; } = string.Empty;

        public DateTime MobileConfirmationTokenExpiry { get; set; }

        public Boolean IsPasswordlinkShow { get; set; }

        public int? RoleId { get; set; }


        public int FaiedLoginAttempt { get; set; }

        [MaxLength(100)]
        public string? ProfilePhoto { get; set; }


        [MaxLength(700)]
        public string SessionToken { get; set; } = string.Empty;

      
        [MaxLength(700)]
        public string RefreshToken { get; set; } = string.Empty;

        public bool IsLocked { get; set; } = false;   

        public DateTime ExpiresAt { get; set; }


        public DateTime LastLoginDate { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public Boolean IsActive { get; set; }
        public bool IsDelete { get; set; }

        public ICollection<Member>? Member { get; set; } 

        [ForeignKey("RoleId")]
        public virtual Codes Role { get; set; } = null!;



    }
}
