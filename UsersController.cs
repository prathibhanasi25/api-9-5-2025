using AutoMapper.Execution;
using Azure.Core;
using InsuranceClaimsAPI.DTO;
using InsuranceClaimsAPI.Models;
using InsuranceClaimsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace InsuranceClaimsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly string _profilestoragePath;
        private readonly string _documentstoragePath;

        public UsersController(IUserService userService, IOptions<FileStorageOptions> options)
        {
            _userService = userService;
            _profilestoragePath = Path.Combine(options.Value.ProfilePhotoPath, "Profile");
            _documentstoragePath = Path.Combine(options.Value.ProfilePhotoPath, "Documents");
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto login)
        {
            
            if (string.IsNullOrEmpty(login.userId))
            {
                if(login.selectedTab == "web")
                {
                    if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
                        return Ok(new AuthResponseDto { status = "Error", message = "Invalid login data" });
                }
                else
                {
                    if (string.IsNullOrEmpty(login.Mobile) || string.IsNullOrEmpty(login.OTP))
                        return Ok(new AuthResponseDto { status = "Error", message = "Invalid login data" });
                }
                var result = await _userService.LoginAsync(login.Mobile, login.Email, (login.selectedTab == "web") ? login.Password : login.OTP);
                return Ok(result);
            }
            else
            {
                //Verify the token for the member and continue login               
                if (int.TryParse(login.userId, out var userid))
                {
                   var verifyTokenResult = await _userService.VerifyToken(userid, (login.selectedTab == "web")?true:false, (login.selectedTab == "web") ? login.Password : login.OTP);
                   return Ok(verifyTokenResult);
                }
            }
            return null;
        }

        [HttpPost("VerifyEmail")]
        public async Task<ActionResult<AuthResponseDto>> VerifyEmail([FromBody] TokenDto tokenDTO)
        {
            string validationMessage  = string.Empty;string status = "Error";
            if (tokenDTO == null)
                return BadRequest("Invalid Token data");
            else if (string.IsNullOrEmpty(tokenDTO.memberId))
                return BadRequest(" MemberId is required");
            else if (string.IsNullOrEmpty(tokenDTO.token))
                return BadRequest("Token is required");

            //Verify the token for the member and continue login               
            if (int.TryParse(tokenDTO.memberId, out var memberId))
            {
                var verifyTokenResult = await _userService.VerifyToken(memberId, true , tokenDTO.token);
                return Ok(verifyTokenResult);
            }
            return null;
        }

        [HttpPost("VerifyMobile")]
        public async Task<ActionResult<AuthResponseDto>> VerifyMobile([FromBody] TokenDto tokenDTO)
        {
            string validationMessage = string.Empty; string status = "Error";
            if (tokenDTO == null)
                return BadRequest("Invalid Token data");
            else if (string.IsNullOrEmpty(tokenDTO.memberId))
                return BadRequest(" MemberId is required");
            else if (string.IsNullOrEmpty(tokenDTO.token))
                return BadRequest("Token is required");

            //Verify the token for the member and continue login               
            if (int.TryParse(tokenDTO.memberId, out var memberId))
            {
                var verifyTokenResult = await _userService.VerifyToken(memberId, false, tokenDTO.token);
                return Ok(verifyTokenResult);
            }
            return null;
        }

        private bool validateDateFormat(DateTime date)
        {
            return Regex.IsMatch(date.ToString(), @"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$");
        }

        [HttpGet("GetMemberInsuranceByMemberID")]
        public async Task<IActionResult> GetMemberInsuranceByMemberID(int MemberID)
        {
            if (MemberID == 0)
                return BadRequest("Invalid Member ID");
            var result = await _userService.GetMemberInsuranceByMemberID(MemberID);
           if(result!=null && result.Count >0)
            foreach (var item in result)
            {
                item.InsFrontImgURL = string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("GetMemberInsuranceByMemberID", ""), item.InsFrontImgURL);
                item.InsBackImgURL = string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("GetMemberInsuranceByMemberID", ""), item.InsBackImgURL);
            }
            return Ok(result);
        }

        [HttpPost("DeleteMemberInsurance")]
        public async Task<IActionResult> DeleteMemberInsurance(int MemberInsuranceId,int memberId)
        {
            string validationMessage = "Member Insurance is not stored"; string status = "Error";
            if (memberId == 0)
                return Ok(new { status = status, message = "Invalid Member ID" });

            if (MemberInsuranceId <= 0)
                return Ok(new { status = status, message = "Invalid Member Insurance ID" });

            var result = await _userService.DeleteMemberInsuranceAsync(MemberInsuranceId, memberId);
                if (result != null)
                    return Ok(new { status = result.Status, message = result.Message });

            return Ok(new { status = status, message = validationMessage });
        }




        [HttpGet("GetMemberInsuranceById")]
        public async Task<IActionResult> GetMemberInsuranceById(int MemInsID)
        {
           
            var result = await _userService.GetMemberInsuranceByID(MemInsID);
            if (result != null)
            {
                result.InsFrontImgURL = string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("GetMemberInsuranceById", ""), result.InsFrontImgURL);
                result.InsBackImgURL = string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("GetMemberInsuranceById", ""), result.InsBackImgURL);
            }
            else            
                return BadRequest("No Member Insurance found");

            return Ok(result);

        }

        [HttpPost("SaveUserAsync")]       
        public async Task<IActionResult> SaveUserAsync(UserDto userDto)
        {
            string status = "Error";
            if (userDto == null)
                return Ok(new { status = status, message = "Invalid User data" });

            var result = await _userService.SaveUserAsync(userDto);
            if (result != null)
                return Ok(result);

            return null;

        }


        [HttpPost("SaveInsuranceAsync")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveInsuranceAsync([FromForm] MemberInsuranceDTO insurance)
        {
            string validationMessage = "Member Insurance is not stored"; string status = "Error";
            if (insurance == null)
                return Ok(new { status = status, message = "Invalid member Insurance data" });

            if (insurance.MemInsID <= 0)
            {
                if (insurance.FrontImage == null || insurance.FrontImage.Length == 0)
                    return Ok(new { status = status, message = "Insurance Front Image is required" });
                if (insurance.BackImage == null || insurance.BackImage.Length == 0)
                    return Ok(new { status = status, message = "Insurance Back Image is required" });
            }
            var result = await _userService.SaveInsuranceAsync(insurance, _documentstoragePath);
            if (result != null)
                return Ok(new { 
                    status = result.Status, 
                    message = result.Message, 
                    InsuranceId = result.MemInsID,
                    BackImgURL =(string.IsNullOrEmpty(result.InsBackImgURL))?null: string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("SaveInsuranceAsync", ""), result.InsBackImgURL),
                    FrontImgURL = (string.IsNullOrEmpty(result.InsFrontImgURL)) ? null : string.Format("{0}://{1}{2}insurancephoto/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("SaveInsuranceAsync", ""), result.InsFrontImgURL),
                });
            return Ok(new { status = status, message = validationMessage , MemInsID =0});
        }

            [HttpPost("SaveMember")]
       
        public async Task<IActionResult> SaveMemberAsync([FromBody] MemberDto MemberDto)
        {
            string validationMessage = "Member is not stored"; string status = "Error";
            if (MemberDto == null)
                return Ok(new { status = status, message = "Invalid member data" });

            if(string.IsNullOrEmpty(MemberDto.FirstName.Trim()))
                return Ok(new { status = status, message = "Member's First Name is required" });

            if (string.IsNullOrEmpty(MemberDto.LastName.Trim() ))
                return Ok(new { status = status, message = "Member's Last Name is required" });

      
           if (MemberDto.DOB !=new DateTime() && MemberDto.DOB.Year < 1950)
                return Ok(new { status = status, message = "Member DOB's Year must be 1970 or later" });
            


                var result = await _userService.SaveMemberAsync(MemberDto);
            if (result != null)
                return Ok(new { status = result.Status, message = result.Message , MemberID = result.MemberID});

            return Ok(new { status = status, message = validationMessage });
        }

        [HttpPost("DeleteMember")]
        public async Task<IActionResult> DeleteMemberAsync(string MemberId)
        {
            string validationMessage = "Member is not stored"; string status = "Error";
            if (int.TryParse(MemberId, out var memeberId))
            {               
                var result = await _userService.DeleteMemberAsync(memeberId);
                if (result != null)
                    return Ok(new { status = result.Status, message = result.Message });
               
            }
            return Ok(new { status = status, message = validationMessage });
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            string validationMessage = "Member registration failed"; string status = "Error";
            if (registerDto == null)
               return  Ok(new { status = status, message = "Invalid registration data" });  

            if (await _userService.MemberEmailExistsAsync(registerDto.Email))
                return Ok(new { status = status, message = "Memeber Email exists" });  

            var userDto=new UserDto {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                EmailID = registerDto.Email,
                MobileNo = registerDto.Mobileno,
                DOB = registerDto.DOB,
                Gender = registerDto.Gender               
            };

            var result = await _userService.RegisterUserAsync(userDto);
            if (result != null)
                return Ok(new { status = result.Status, message = result.Message });


            return Ok(new { status = status, message = validationMessage });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {          
            var strMessage = await _userService.ForgotPasswordAsync(email);          
            return Ok(new { message = strMessage });

        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            string validationMessage = "Member Password functionality is failed"; string status = "Error";
            var result = await _userService.ChangePasswordAsync(changePasswordDto);
            if (result != null)
                return Ok(new { status = result.status, message = result.message });

            return Ok(new { status = status, message = validationMessage });
        }


        [HttpPost("LockUserAsync")]
        public async Task<IActionResult> LockUserAsync(int userId)
        {
            string strMessage= string.Empty;
            var isValid = await _userService.LockUserAsync(userId);
            strMessage = isValid ? "User locked successfully" : "Failed to lock user";
            return Ok(new { message = strMessage });

        }

        [HttpPost("UnlockUserAsync")]
        public async Task<IActionResult> UnlockUserAsync(int userId)
        {
            string strMessage = string.Empty;
            var isValid = await _userService.UnlockUserAsync(userId);
            strMessage = isValid ? "User Unlocked successfully" : "Failed to Unlock user";
            return Ok(new { message = strMessage });

        }

        [HttpGet("GetCodeType")]
        public async Task<IActionResult> GetCodeType(string codeType)
        {
            var result = await _userService.GetCodeType(codeType);
            if (result == null)
                return BadRequest("No code records");
            return Ok(result);

        }





        [HttpGet("GetDistinctState")]
        public async Task<IActionResult> GetDistinctState()
        {
            var result = await _userService.GetDistinctState();
            if (result == null)
                return BadRequest("No State records");
            
            return Ok(result);

        }

        [HttpGet("GetDistinctZipbyState")]
        public async Task<IActionResult> GetDistinctZipbyState(string State)
        {
            var result = await _userService.GetDistinctZipbyState(State);
            if (result == null)
                return BadRequest("No Zip records");
            return Ok(result.Select(x => x.ZIPCode).Distinct().ToArray());

        }

        [HttpGet("GetZipDatabyCode")]
        public async Task<IActionResult> GetZipDatabyCode(string zipcode)
        {
            var result = await _userService.GetZipDatabyCode(zipcode);
            if (result == null)
                return BadRequest("No Zip records");
            return Ok(result);
        }

        [HttpGet("GetMemberByIdAsync")]
        public async Task<IActionResult> GetMemberById(int memberId)
        {
            var result = await _userService.GetMemberById(memberId);
            if (result == null)
                return BadRequest("No Members found");
            return Ok(result);

        }

       
        [HttpGet("GetMemberByUserEmailAsync")]
    

        public async Task<ActionResult<MemberDto>> GetMemberByUserEmailAsync(int userId,string email)
        {
            var result = await _userService.GetMemberByUserEmailAsync(userId, email);
            if (result == null)
                return BadRequest("No Members found");
            return Ok(result);

        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (int.TryParse(userID, out var newint))
            {
                var result = await _userService.GetUserById(newint);
                if (result == null)
                    return BadRequest("No User found");
                return Ok(new { result.FirstName,result.IsPasswordlinkShow, result.EmailID, imageUrl =  string.Format("{0}://{1}{2}photo/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("profile", ""), string.IsNullOrEmpty(result.ProfilePhoto) ? "user.jpg" : result.ProfilePhoto) });
            }
            return BadRequest("Invalid user ID on JWT Tokens");
        }

        

        [HttpGet("GetFamilyDepedent")]
        [Authorize]
        public async Task<IActionResult> GetFamilyDepedent()
        {
            var logInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (int.TryParse(logInUserId, out var newint))
            {
                var result = await _userService.GetFamilyDepedent(newint);

                var final = result.Select(b => new
                {
                    MemberID = b.MemberID,
                    FirstName = b.FirstName,
                    LastName = b.LastName,
                    Gender = b.Gender,
                    EmailID = b.EmailID,
                    DOB = b.DOB,
                    MobileNo = b.MobileNo,
                    Address1 = b.Address1,
                    Address2 = b.Address2,
                    City = b.City,
                    State = b.State,
                    ZipIntID = b.ZipIntID,
                    ZipCode = b.ZipCode,
                    IsPrimaryMem = b.IsPrimaryMem,
                    RelationshipTypeID = b.RelationshipTypeID,
                    RelationshipType=b.RelationshipType,
                    MemberPhotoURL = string.Format("{0}://{1}{2}photo/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("GetFamilyDepedent", ""), b.MemberPhotoURL),
                    HasLogin = b.HasLogin
                }).ToList();

                if (result == null)
                    return BadRequest("No Members found");
                return Ok(final);
            }
            return BadRequest("Invalid user ID on JWT Token");

        }

        [HttpGet("GetFamilyDepedentFormattedData")]
        [Authorize]
        public async Task<IActionResult> GetFamilyDepedentFormattedData()
        {
            var LoginuserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (int.TryParse(LoginuserId, out var newint))
            {
                
                var result = await _userService.GetFamilyDepedentFormattedData(newint);

                var final= result.Select(b => new
                {
                    id = b.MemberID,
                    name = string.Format("{0} {1}", b.FirstName, b.LastName),
                    relation = b.RelationshipType,
                    imageUrl = string.Format("{0}://{1}{2}photo/{3}", Request.Scheme, Request.Host,Request.Path.ToString().Replace("GetFamilyDepedentFormattedData", ""),b.MemberPhotoURL)
                }).ToList();

                if (result == null)
                    return BadRequest("No Members found");               
                return Ok(final);
            }
            return BadRequest("Invalid user ID on JWT Token");

        }


        [HttpPost("UpdatePasswordLinkShow")]
        public async Task<IActionResult> UpdatePasswordLinkShow(int userID)
        {
            var result = await _userService.UpdatePasswordLinkShow(userID);
            
            if (result == null)
                return BadRequest("No Members found");

            return Ok(new { message = result.Message, Status = result.Status });
        }

            [HttpPost("TestUploadfile")]
        public async Task<IActionResult> TestUploadfile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");
            return null;
        }

            [HttpPost("uploadfile")]
        public async Task<IActionResult> Uploadfile([FromForm] FileUploadDto model)
        {
            string storagepath= (model.IsProfileFileType) ? _profilestoragePath : _documentstoragePath;
            if (model.File == null || model.File.Length == 0)
                return BadRequest("No file uploaded.");
            try
            {
                if (!Directory.Exists(storagepath))
                    Directory.CreateDirectory(storagepath); // Ensure it exists

                var uniqueFileName = string.Format("{0}_{1}", model.memberID, model.File.FileName.Replace(Path.GetExtension(model.File.FileName),string.Empty)).Trim() + Path.GetExtension(model.File.FileName);
                var filePath = Path.Combine(storagepath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                    if (model.memberID >0 && !string.IsNullOrEmpty(uniqueFileName))
                    { 
                        var existingMemeber = await _userService.GetMemberById(model.memberID);
                        if (existingMemeber != null)
                        {
                            if (!string.IsNullOrEmpty(existingMemeber.MemberPhotoURL))
                            {
                                if (System.IO.File.Exists(Path.Combine(storagepath, existingMemeber.MemberPhotoURL)))
                                {
                                    System.IO.File.Delete(Path.Combine(storagepath, existingMemeber.MemberPhotoURL));
                                }
                            }

                        }
                        var result = await _userService.UpdateMemberImageUrl(model.memberID, uniqueFileName);


                    }
                    //delete the previous profile photo
                  

                }

                return Ok(new { message = "file uploaded successfully", Status = "Success", filename= uniqueFileName,profilephoto= string.Format("{0}://{1}{2}photo/{3}", Request.Scheme, Request.Host, Request.Path.ToString().Replace("Uploadfile", ""), uniqueFileName) });

            }
            catch (Exception)
            {

                return Ok(new { message = "file is not uploaded", Status = "Error" });
            }

            return Ok(new { message = "file is not uploaded", Status = "Error" });

        }

        [HttpGet("photo/{fileName}")]
        public IActionResult GetProfilePhoto(string fileName)
        {
            var filePath = Path.Combine(_profilestoragePath, fileName);
            var contentType = "image/jpeg"; // Change based on file extension if needed
            if (!System.IO.File.Exists(filePath))
                return PhysicalFile(Path.Combine(_profilestoragePath, "user.jpg"), contentType);
            // return NotFound("File not found.");


            return PhysicalFile(filePath, contentType);
        }

        [HttpGet("insurancephoto/{fileName}")]
        public IActionResult GetInsuranceDocument(string fileName)
        {
            var filePath = Path.Combine(_documentstoragePath, fileName);
            var contentType = "image/jpeg"; // Change based on file extension if needed
            if (!System.IO.File.Exists(filePath))
                return PhysicalFile(Path.Combine(_profilestoragePath, "user.jpg"), contentType);
            // return NotFound("File not found.");


            return PhysicalFile(filePath, contentType);
        }


        [HttpGet("ValidateJWTToken")]
        public async Task<ActionResult<string>> ValidateJWTToken(string jWTtoken)
        {
            string strresult= string.Empty;
            if (string.IsNullOrEmpty(jWTtoken))
                return BadRequest("Invalid Token data");
            var result = await _userService.ValidateTokenAsync(jWTtoken);           
            strresult = (result) ? "JWT Token is valid" : "JWT Token is invalid";

            if (!result)
                return BadRequest("JWT Token is invalid");
            return Ok(strresult);
        }

        [HttpGet("RefreshTokenAsync")]
        public async Task<ActionResult<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            if (!string.IsNullOrEmpty(refreshToken))
                return BadRequest("Invalid Token data");
            var result = await _userService.RefreshTokenAsync(refreshToken);
            if (result == null)
                return BadRequest("refreshToken is not completed successfully");
            return Ok(result);
        }
        [HttpGet("SearchAdmin_UserManagementPage")]
        public async Task<IActionResult> SearchAdmin_UserManagement([FromQuery] string? query)
        {
            var result = await _userService.SearchAdmin_UserManagement(query);


            return Ok(result);
        }

        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUserAsync(string UserId)
        {
            string validationMessage = "User is not stored"; string status = "Error";
            if (int.TryParse(UserId, out var userId))
            {
                var result = await _userService.DeleteUserAsync(userId);
                if (result != null)
                    return Ok(new { status = result.Status, message = result.Message });

            }
            return Ok(new { status = status, message = validationMessage });
        }
    }
}
