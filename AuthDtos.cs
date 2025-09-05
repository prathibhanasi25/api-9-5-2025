using System.ComponentModel.DataAnnotations;

namespace InsuranceClaimsAPI.DTO
{
    public class AuthDtos
    {

    }
    public class LoginDto
    {
        public string Mobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string selectedTab { get; set; } = string.Empty;

        public string userId { get; set; } = string.Empty;
        



    }

    public class TokenDto
    {
        public string memberId { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;

    }


    


    public class MemberDto
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int MemberID { get; set; }

        public int MappedMemberID { get; set; }
     
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Gender { get; set; }
      
        public DateTime DOB { get; set; }

        public Boolean EmailConfirmed { get; set; }

        public string EmailConfirmationToken { get; set; } = string.Empty;

        public DateTime EmailConfirmationTokenExpiry { get; set; }


        public Boolean MobileNoConfirmed { get; set; }

        public string MobileConfirmationToken { get; set; } = string.Empty;

        public DateTime MobileConfirmationTokenExpiry { get; set; }


        public string MobileNo { get; set; } = string.Empty;

        public string EmailID { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Address1 { get; set; }

       
        public string? Address2 { get; set; }

    
        public string? City { get; set; }

    
        public string? State { get; set; }


        public string? ZipCode { get; set; }

   
        public string? Country { get; set; }

        public int? ZipIntID { get; set; }

        public Guid IsMemberLinkID { get; set; }

        public Boolean IsPasswordlinkShow { get; set; }

        public int? RelationshipTypeID { get; set; }

        public string? RelationshipType { get; set; }


        public Boolean IsPrimaryMem { get; set; }

        public Boolean HasLogin { get; set; }

        public string? MemberPhotoURL { get; set; }

        public int RoleId { get; set; }

        public string Role { get; set; }=string.Empty;

        public DateTime AddDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public Boolean IsActive { get; set; }
        public bool IsDelete { get; set; }
    }

    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string Mobileno { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public bool Status { get; set; }


    }


    public class UserDto
    {
        public int UserID { get; set; }
        public string EmailID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime DOB { get; set; }

        public string? ProfilePhoto { get; set; }

        public string? Gender { get; set; } = string.Empty;

        public Boolean IsPasswordlinkShow { get; set; }


        public int RoleId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public Boolean IsActive { get; set; }

        public DateTime AddDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? AddedBy { get; set; }
        public string? ModifiedBy { get; set; }


    }

    public class AuthResponseDto
    {
        public string? UserId { get; set; }
        public int MemberId { get; set; } 
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

     
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        public Boolean IsPasswordlinkShow { get; set; }

        public int RoleId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string message { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;


    }

    public class ChangePasswordDto
    {       
        public string Email { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public string message { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class CodeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class StateDto
    {
        public string Statename { get; set; } = string.Empty;
        public string stateAbbr { get; set; } = string.Empty;
    }

    public class ZipcodeDTO
    {

        public int ZipIntID { get; set; }
    
        public string ZIPCode { get; set; } = string.Empty;
                      
        public string CityName { get; set; } = string.Empty;     
     
        public string StateName { get; set; } = string.Empty;
      
        public string StateAbbr { get; set; } = string.Empty;

    }

    public class FileUploadDto
    {
        public int memberID { get; set; }
        public bool IsProfileFileType { get; set; }
        public IFormFile File  { get; set; }

      
    }

}
