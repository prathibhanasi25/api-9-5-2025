using InsuranceClaimsAPI.DTO;
using InsuranceClaimsAPI.Models;
using Microsoft.AspNetCore.Mvc;
namespace InsuranceClaimsAPI.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<UserDto?> GetUserById(int userId);

        Task<UserDto> SaveUserAsync(UserDto userDto);

        Task<MemberDto?> GetMemberById(int userId);
    
        Task<MemberDto?> GetMemberByUserEmailAsync(int userId, string email);
        Task<AuthResponseDto?> LoginAsync(string mobileno,string email, string password);
        Task<AuthResponseDto> VerifyToken(int userId, Boolean IswebToken, string Token);
        Task<UserDto> RegisterUserAsync(UserDto userDto);
        Task<MemberDto> SaveMemberAsync(MemberDto MemberDto);

        Task<MemberInsuranceDTO?> GetMemberInsuranceByID(int memberInsuranceId);
        Task<List<FamilyInsuranceDTO>> GetMemberInsuranceByMemberID(int memberId);
        Task<MemberInsuranceDTO> DeleteMemberInsuranceAsync(int MemberInsuranceId, int memberId);
        Task<MemberInsuranceDTO> SaveInsuranceAsync(MemberInsuranceDTO MemberInsuranceDto, string storagepath);

        Task<UserDto> UpdatePasswordLinkShow(int UserID);

        Task<MemberDto> UpdateMemberImageUrl(int MemberID, string filename);
        Task<MemberDto> DeleteMemberAsync(int MemberId);
        Task<UserDto> DeleteUserAsync(int UserId);
        Task<List<MemberDto>> GetFamilyDepedent(int memberID);
        Task<List<MemberDto>> GetFamilyDepedentFormattedData(int memberID);

        Task<string> ForgotPasswordAsync(string email);

        Task<ChangePasswordDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

        Task<bool> EmailExistsAsync(string email);
        Task<bool> MemberEmailExistsAsync(string email);

        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string sessionToken);
        Task<bool> ValidateTokenAsync(string token);

        Task<bool> LockUserAsync(int userId);

        Task<bool> UnlockUserAsync(int userId);
      

        Task<List<string>> GetDistinctState();
        Task<List<ZipcodeDTO>> GetDistinctZipbyState(string strState);

        Task<ZipcodeDTO> GetZipDatabyCode(string zipcode);

        Task<List<CodeDto>> GetCodeType(string codeType);
        Task<List<UserListDto>> SearchAdmin_UserManagement(string? query);





    }

}
