using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace SISPK.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
    public class VIEW_MENU_HIRARKI
    {
        public decimal MENU_ID { get; set; }
        public Nullable<decimal> MENU_PARENT_ID { get; set; }
        public string MENU_NAME { get; set; }
        public Nullable<decimal> MENU_ANAK { get; set; }
        public Nullable<decimal> MENU_LEVEL { get; set; }
    }
    public class VIEW_NOTIFIKASI
    {
        public decimal NOTIF_ID { get; set; }
        public string JENIS_NOTIF { get; set; }
        public decimal JUMLAH { get; set; }
        public decimal NOTIF_SORT { get; set; }
        public decimal NOTIF_TYPE { get; set; }
        public string NOTIF_LINK { get; set; }
        public decimal KOMTEK_ID { get; set; }
        
    }
    public class VIEW_POLLING_KOMTEK
    {
        public Nullable<decimal> POLLING_PROPOSAL_ID { get; set; }
        public Nullable<decimal> POLLING_TYPE { get; set; }
        public Nullable<decimal> SETUJU_PERSEN { get; set; }
        public Nullable<decimal> TIDAK_SETUJU_PERSEN { get; set; }
        public Nullable<decimal> JML_SETUJU { get; set; }
        public Nullable<decimal> JML_TIDAK_SETUJU { get; set; }
        public Nullable<decimal> JML_ABSTAIN { get; set; }
        public Nullable<decimal> JML_ABSTAIN_PERSEN { get; set; }
        public Nullable<decimal> JML_ANGGOTA { get; set; }
        public Nullable<decimal> POLLING_RESULT { get; set; }
        public string  POLLING_RESULT_NAME { get; set; }
    }
    public class GROUP_DASHBOARD
    {
        public string PROPOSAL_TAHAPAN { get; set; }
        public Nullable<decimal> PROPOSAL_STATUS { get; set; }
        public Nullable<decimal> PROPOSAL_YEAR { get; set; }
    }
    public class SELECT2
    {
        public Nullable<decimal> id { get; set; }
        public string text { get; set; }
    }
}
