using AutoMapper.Execution;
using InsuranceClaimsAPI.Data;
using InsuranceClaimsAPI.DTO;
using InsuranceClaimsAPI.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace InsuranceClaimsAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService emailService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IEmailService emailService, IPasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            this.emailService = emailService;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.User
                .FirstOrDefaultAsync(u => u.EmailID == email && u.IsActive);
        }

        public async Task<UserDto?> GetUserById(int userId)
        {
            var userdetails = await _context.User.FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive);
            if (userdetails == null)
                return null;

            return new UserDto
            {

                UserID = userdetails.UserID,               
                FirstName = userdetails.FirstName,
                LastName = userdetails.LastName,             
                EmailID = userdetails.EmailID,
                IsPasswordlinkShow = userdetails.IsPasswordlinkShow,
                ProfilePhoto = userdetails.ProfilePhoto
            };
        }

        public async Task<MemberDto?> GetMemberByUserEmailAsync(int userId,string email)
        {

            var memberdetails = await _context.Member
                  .FirstOrDefaultAsync(u => u.MappedMemberID == userId && u.EmailID==email && u.IsActive && u.HasLogin);
            if (memberdetails == null)
                return null;

            return new MemberDto
            {
                MemberID = memberdetails.MemberID,
                FirstName = memberdetails.FirstName,
                LastName = memberdetails.LastName,
                Gender = memberdetails.Gender,
                EmailID = memberdetails.EmailID,
                DOB = memberdetails.DOB,
                MobileNo = memberdetails.MobileNo,              
                IsActive = true,               
                AddDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

        }


        public async Task<List<CodeDto>> GetCodeType(string CodeType)
        {

            var codeType = await _context.Codes.Where(x=>x.CodeType== CodeType && x.IsActive).Select(b => new CodeDto() { Id = b.CodeID, Type = b.CodeValue }).ToListAsync(); 

            return codeType;
        }

       


        public async Task<List<string>> GetDistinctState()
        {

            var state = await _context.Zipcodes.Select(b => b.StateAbbr).Distinct().ToListAsync();

            return state;
        }

        public async Task<List<ZipcodeDTO>> GetDistinctZipbyState(string strState)
        {

            var zipcode = await _context.Zipcodes.Where(x=>x.StateAbbr == strState).Select(b => new ZipcodeDTO() { ZipIntID  = b.ZipIntID, ZIPCode= b.ZIPCode }).Distinct().ToListAsync();

            return zipcode;
        }


        public async Task<ZipcodeDTO> GetZipDatabyCode(string zipcode)
        {
            var result = await _context.Zipcodes.Where(x => x.ZIPCode == zipcode).Select(b => new ZipcodeDTO() { ZipIntID = b.ZipIntID, ZIPCode = b.ZIPCode, CityName =b.CityName, StateAbbr =b.StateAbbr, StateName=b.StateName }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<MemberDto>> GetFamilyDepedentFormattedData(int userId)
        {           
            var existingmember = await _context.Member
                 .FirstOrDefaultAsync(u => u.IsMemberLinkID == userId && u.IsActive);
            if (existingmember != null)
            {
                var memberdetails = await _context.Member.Include(r => r.RelationshipType).Where(u => u.MappedMemberID == existingmember.MappedMemberID && u.IsActive).Select(b => new MemberDto
                {

                    MemberID = b.MemberID,
                    FirstName = b.FirstName,
                    LastName = b.LastName,
                    Gender = b.Gender,
                    RelationshipTypeID = b.RelationshipTypeID,
                    MemberPhotoURL = string.IsNullOrEmpty(b.MemberPhoto)?"user.jpg": b.MemberPhoto,
                    RelationshipType = b.RelationshipType.CodeValue

                }).ToListAsync();


                return memberdetails;
            }
            return new List<MemberDto>(); 

        }

        public async Task<List<MemberDto>> GetFamilyDepedent(int userId)
        {
           
            var existingmember = await _context.Member
              .FirstOrDefaultAsync(u => u.IsMemberLinkID == userId && u.IsActive);

            if (existingmember != null)
            {
                var memberdetails = await _context.Member.Include(r => r.RelationshipType).Where(u => u.MappedMemberID == existingmember.MappedMemberID && u.IsActive).Select(b => new MemberDto
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
                    ZipCode =b.ZipCode,
                    IsPrimaryMem =b.IsPrimaryMem,
                    RelationshipTypeID = b.RelationshipTypeID,
                    RelationshipType=(b.RelationshipType!=null)? b.RelationshipType.CodeValue :string.Empty,
                    MemberPhotoURL = string.IsNullOrEmpty(b.MemberPhoto) ? "user.jpg" : b.MemberPhoto,
                    HasLogin=b.HasLogin
                   

                }).ToListAsync();


                return memberdetails;
            }
            return new List<MemberDto>();
        }



        public async Task<MemberInsuranceDTO> DeleteMemberInsuranceAsync(int MemberInsuranceId,int memberId)
        {
            try
            {
                var memberinsurance = await _context.MemberInsurance.FirstOrDefaultAsync(m => m.MemInsID == MemberInsuranceId && m.IsActive);
                if (memberinsurance != null)
                {
                    memberinsurance.IsActive = false;
                    memberinsurance.ModifiedBy = memberId.ToString();
                    memberinsurance.ModifiedDate = DateTime.Now;
                    _context.MemberInsurance.Update(memberinsurance);
                    await _context.SaveChangesAsync();               

                    return new MemberInsuranceDTO
                    {
                        Message = "Member Insurance deleted successfully.",
                        Status = "Success"
                    };
                }

            }
            catch (Exception exec)
            {
                return new MemberInsuranceDTO
                {
                    Message = "Member Insurance is not deleted",
                    Status = "Error"
                };

            }
            return new MemberInsuranceDTO
            {
                Message = "Member Insurance is not deleted",
                Status = "Error"
            };
        }


        public Task<List<FamilyInsuranceDTO>> GetMemberInsuranceByMemberID(int memberId)
        {
            /*
                var memberInsurance = _context.PolicyType
                .GroupJoin(
                _context.MemberInsurance
                .Where(i => i.MemberID == memberId && i.IsActive)
                .Include(m => m.PolicyType)
                .Include(m => m.BenefitType)
                .Include(m => m.MemberInsEligibility)
                .OrderBy(m => m.PolicyTypeID),
                pt => pt.Id,
                i => i.PolicyTypeID,
                (pt, insurances) => new { PolicyType = pt, Insurances = insurances.DefaultIfEmpty() }
                )
                .SelectMany(
                x => x.Insurances,
                (x, insurance) => new FamilyInsuranceDTO
                {
                    MemInsID = insurance != null ? insurance.MemInsID :0 ,                
                    InsFrontImgURL = insurance != null ? insurance.InsFrontImgURL:string.Empty,
                    InsBackImgURL = insurance != null ? insurance.InsBackImgURL:string.Empty,                  
                    PolicyTypeID = x.PolicyType.Id,
                    PolicyType = x.PolicyType.Type,
                    BenefitTypeID = insurance != null ? insurance.BenefitTypeID:0,
                    BenefitType = insurance != null ? insurance.BenefitType.Type : string.Empty,
                    SubscriberID = insurance != null ? insurance.SubscriberID:Guid.Empty,
                    SubscriberName = insurance != null ? (insurance.Member.FirstName + " " + insurance.Member.LastName):string.Empty,//Alter required.
                    Status = insurance != null ? (insurance.MemberInsEligibility != null ? (insurance.MemberInsEligibility.IsInsuranceActive? "Active": "Inactive") : "Inactive") : "Inactive",
                    PayerName = insurance != null ? insurance.PayerName:string.Empty,
                    strActiveDateFromTo = insurance != null ? (insurance.MemberInsEligibility!=null ? insurance.MemberInsEligibility.InsActiveDateFrom.ToShortDateString() : ""): "",
                    EligibilityChkDate = insurance != null ? (insurance.MemberInsEligibility != null ? insurance.MemberInsEligibility.EligibilityChkDate : "") : "",
                }).ToListAsync();
            */
          
            var memberInsurance = _context.MemberInsurance
                .Where(i=>i.MemberID == memberId && i.IsActive)
                .Include(m => m.PolicyType)
                .Include(m=>m.BenefitType)
                .Include(m => m.Subscriber)
                .Include(m=>m.MemberInsEligibility)
                .OrderBy(m=>m.PolicyTypeID)
                .Select(u => new FamilyInsuranceDTO
                {
                    MemInsID = u.MemInsID,                
                    InsFrontImgURL = u.InsFrontImgURL,
                    InsBackImgURL = u.InsBackImgURL,                  
                    PolicyTypeID = u.PolicyTypeID,
                    PolicyType = u.PolicyType.CodeValue,
                    BenefitTypeID = u.BenefitTypeID,
                    BenefitType= u.BenefitType.CodeValue,
                    SubscriberID = u.SubscriberID,
                    SubscriberName= u.Subscriber.FirstName,
                    Status= (u.MemberInsEligibility) == null ? "Inactive": (u.MemberInsEligibility.IsInsuranceActive)?"Active" :"Inactive",
                    PayerName = u.PayerName,
                    strActiveDateFromTo = (u.MemberInsEligibility)==null? "": String.Format("{0} - {1}",u.MemberInsEligibility.InsActiveDateFrom.ToShortDateString(), u.MemberInsEligibility.InsActiveDateTo.ToShortDateString()),                   
                    EligibilityChkDate = (u.MemberInsEligibility) == null ? "" : u.MemberInsEligibility.EligibilityChkDate
                }).ToListAsync();
         
            if (memberInsurance == null)
                return new Task<List<FamilyInsuranceDTO>>(() => new List<FamilyInsuranceDTO>());
            return memberInsurance;
        }

        public async Task<MemberInsuranceDTO?> GetMemberInsuranceByID(int memberInsuranceId)
        {
            var memberInsurance = await _context.MemberInsurance
                .Include(m => m.Member)
                .FirstOrDefaultAsync(u => u.MemInsID == memberInsuranceId && u.IsActive);
            if (memberInsurance == null)
                return null;
            return new MemberInsuranceDTO
            {
                MemInsID = memberInsurance.MemInsID,
                MemberID = memberInsurance.MemberID,               
                InsFrontImgURL = memberInsurance.InsFrontImgURL,
                InsBackImgURL = memberInsurance.InsBackImgURL,
                DependentsID = memberInsurance.DependentsID,
                PolicyTypeID = memberInsurance.PolicyTypeID,
                PolicyID = memberInsurance.PolicyID,
                BenefitTypeID = memberInsurance.BenefitTypeID,
                SubscriberID = memberInsurance.SubscriberID                
            };
        }

        public async Task<MemberDto?> GetMemberById(int memberId)
        {
            var memberdetails= await _context.Member
                .FirstOrDefaultAsync(u => u.MemberID == memberId && u.IsActive);
            if (memberdetails == null)
                return null;

                return new MemberDto
            {

                MemberID = memberdetails.MemberID,
                MappedMemberID=memberdetails.MappedMemberID,
                FirstName = memberdetails.FirstName,
                LastName = memberdetails.LastName,
                Gender = memberdetails.Gender,
                EmailID = memberdetails.EmailID,
                DOB = memberdetails.DOB,
                MobileNo = memberdetails.MobileNo,
                Address1 = memberdetails.Address1,
                Address2 = memberdetails.Address2,
                City = memberdetails.City,
                State=memberdetails.State,
                ZipIntID = memberdetails.ZipIntID,   
                ZipCode = memberdetails.ZipCode,                    
                IsActive = true,
                AddDate = DateTime.Now,
                AddedBy = memberdetails.AddedBy,
                ModifiedDate = DateTime.Now,
                ModifiedBy = memberdetails.ModifiedBy                
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(string mobileno, string email, string password)
        {

            var userdetails = await _context.User.Include(x => x.Role).FirstOrDefaultAsync(m => m.EmailID == email || m.MobileNo== mobileno);

            if (userdetails == null) return new AuthResponseDto { message = "User is not found", status = "Error" };

            // Check if account is locked
            if (userdetails.IsLocked)
                return new AuthResponseDto{ message= "User Locked.Please reset the password" , status="Error"};
            // Verify password

            if (!_passwordService.VerifyPassword(password, userdetails.Password, userdetails.PasswordSalt))
            {
                // Increment failed attempts
                userdetails.FaiedLoginAttempt++;
                if (userdetails.FaiedLoginAttempt >= 3)
                {
                    userdetails.IsLocked = true;
                    await _context.SaveChangesAsync();
                    return new AuthResponseDto { message = "User Locked.Please reset the password", status="Error" };
                }
                await _context.SaveChangesAsync();
                return  new AuthResponseDto { message = "User credential is not matching" ,status="Error"}; ;
            }

            var memberdetails = await _context.Member
                 .SingleOrDefaultAsync(m => m.EmailID ==userdetails.EmailID && m.IsMemberLinkID==userdetails.UserID && m.HasLogin );

            if (userdetails != null)
            {
                // Generate tokens
                var jwtToken = GenerateJwtToken(userdetails);
                var refreshToken = GenerateRandomToken();
                var expiresAt = DateTime.UtcNow.AddDays(1);

                // Reset failed attempts on successful login
                userdetails.FaiedLoginAttempt = 0;
                userdetails.IsLocked = false;
                userdetails.LastLoginDate = DateTime.UtcNow;
                userdetails.SessionToken = jwtToken;
                userdetails.RefreshToken = refreshToken;
                userdetails.ExpiresAt = expiresAt;
                _context.User.Update(userdetails);
                await _context.SaveChangesAsync();


          
                return new AuthResponseDto
                {
                    UserId = Convert.ToString(userdetails.UserID),
                    MemberId= (memberdetails==null)? 0:memberdetails.MemberID ,
                    Email = userdetails.EmailID,
                    Token = userdetails.SessionToken,
                    RoleId=(userdetails.RoleId==null)? 0: (int)userdetails.RoleId,
                    Role=(userdetails.Role==null)? string.Empty: userdetails.Role.CodeValue,
                    RefreshToken = userdetails.RefreshToken,
                    FirstName = userdetails.FirstName,
                    LastName = userdetails.LastName,
                    ExpiresAt = userdetails.ExpiresAt,                 
                    IsPasswordlinkShow=userdetails.IsPasswordlinkShow,
                    ProfileImageUrl=userdetails.ProfilePhoto,
                    message = "Login successful",
                    status="Success"
                };
            }
            return null;
           
        }



        public async Task<AuthResponseDto> VerifyToken(int userId, Boolean IswebToken, string Token)
        {
            string validateMsg = string.Empty;string status= "Error";
            try
            {
                int memberId = 0;
                var existinguser = await _context.User.Include(x => x.Role).SingleOrDefaultAsync(m => m.UserID == userId);
                var userRole = await _context.Codes.FirstOrDefaultAsync(r => r.CodeType == "Role" && r.CodeValue == "User");

                if (existinguser != null)
                {
                    if (existinguser.IsActive == false)
                    {                        
                        return new AuthResponseDto { message = "User is not active", status = "Error" };
                    }
                    else if (existinguser.EmailConfirmed == true || existinguser.MobileNoConfirmed == true)                     
                            return new AuthResponseDto { message =  (existinguser.EmailConfirmed) ? "Already web Token is registered" : "Already web Token is registered", status = "Error" };

                    if (((IswebToken) ? existinguser.EmailConfirmationToken: existinguser.MobileConfirmationToken) != Token)
                            return new AuthResponseDto { message = String.Format("{0} Token is not matching", (IswebToken) ? "web" : "mobile"), status = "Error" };

                    if (((IswebToken) ? existinguser.EmailConfirmationTokenExpiry : existinguser.MobileConfirmationTokenExpiry) < DateTime.Now)
                    {                       
                            return new AuthResponseDto { message = String.Format("{0} Token is expired", (IswebToken) ? "web" : "mobile"), status = "Error" };
                    }

                    var salt = _passwordService.GenerateSalt();
                    var passwordHash = _passwordService.HashPassword(Token, salt);

                    // Generate tokens
                    var jwtToken = GenerateJwtToken(existinguser);
                    var refreshToken = GenerateRandomToken();
                    var expiresAt = DateTime.UtcNow.AddDays(1);
                    if (existinguser.RoleId == userRole?.CodeID)//Normal User
                    {
                        var memberdetails = await _context.Member
                   .FirstOrDefaultAsync(u => u.IsMemberLinkID == existinguser.UserID && u.IsActive);

                        if (memberdetails == null)
                        {
                            //Create Self Member 
                            var relationship = await _context.Codes.FirstOrDefaultAsync(r => r.CodeValue == "Self");
                            var member = new Models.Member
                            {
                                FirstName = existinguser.FirstName.Trim(),
                                LastName = existinguser.LastName.Trim(),
                                MobileNo = existinguser.MobileNo,
                                DOB = existinguser.DOB,
                                Gender = existinguser.Gender,
                                EmailID = existinguser.EmailID.Trim(),
                                RelationshipTypeID = relationship?.CodeID ?? 0,
                                FamilyID = await GenarateFamilyId(),
                                MemberPhoto = existinguser.ProfilePhoto,
                                IsPrimaryMem = true,
                                HasLogin = true,
                                IsMemberLinkID = userId,
                                IsActive = true,
                                AddDate = DateTime.Now,
                                AddedBy = existinguser.UserID.ToString()
                            };
                            _context.Member.Add(member);
                            await _context.SaveChangesAsync();
                            memberId = member.MemberID;
                            //Map the primary memberId
                            member.MappedMemberID = memberId;
                            _context.Member.Update(member);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            memberId = memberdetails.MemberID;

                            if (memberdetails.IsMemberLinkID == 0)
                                memberdetails.IsMemberLinkID = memberId;
                            memberdetails.HasLogin = true;

                            _context.Member.Update(memberdetails);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (existinguser.IsLocked)
                        existinguser.IsLocked = false;
                    if (existinguser.FaiedLoginAttempt > 0)
                        existinguser.FaiedLoginAttempt = 0;
                    existinguser.Password = passwordHash;
                    existinguser.PasswordSalt = salt;
                    existinguser.SessionToken = jwtToken;
                    existinguser.RefreshToken = refreshToken;
                    existinguser.ExpiresAt = expiresAt;
                    existinguser.ModifiedDate = DateTime.Now;
                    existinguser.LastLoginDate = DateTime.Now;

                    if (IswebToken)
                        existinguser.EmailConfirmed = true;
                    else
                        existinguser.MobileNoConfirmed = true;
                    _context.User.Update(existinguser);
                    await _context.SaveChangesAsync();
               
                    
                    return new AuthResponseDto
                    {
                        UserId = Convert.ToString(existinguser.UserID),
                        MemberId = memberId,
                        Email = existinguser.EmailID,
                        Token = jwtToken,
                        RefreshToken = refreshToken,
                        RoleId = (existinguser.RoleId == null) ? 0 : (int)existinguser.RoleId,
                        Role = (existinguser.Role == null) ? string.Empty : existinguser.Role.CodeValue,
                        FirstName = existinguser.FirstName.Trim(),
                        LastName = existinguser.LastName.Trim(),
                        ProfileImageUrl=existinguser.ProfilePhoto,
                        ExpiresAt = (existinguser == null) ? DateTime.Now  : existinguser.ExpiresAt,                     
                        message = String.Format("{0} token is verified successfully", (IswebToken) ? "web" : "mobile"),
                        status = "Success"
                    };

                }

            }
            catch (Exception)
            {

                return new AuthResponseDto { message = String.Format("{0} token is not verified", (IswebToken) ? "web" : "mobile"), status = "Error" };
            }
            return null;

        }

        private async Task<string> GenarateFamilyId()
        {
            string familyId = string.Empty; int nextNumber = 1;
                           // Generate new FamilyID
            var lastFamilyId = await _context.Member
                .Where(m => m.FamilyID.StartsWith("FAB-"))
                .OrderByDescending(m => m.MemberID)
                .Select(m => m.FamilyID)
                .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(lastFamilyId))
                {
                    var numberPart = lastFamilyId.Substring(5); // Get the number part after "FAB-"
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                    return $"FAB-{nextNumber:D4}";
                }
                else
                    return $"FAB-{nextNumber:D4}";//First Time
        }

        public async Task<ChangePasswordDto> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
           
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.EmailID == changePasswordDto.Email && u.IsActive);
                if (user != null)
                {
                    // Verify old password
                    if (!_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.Password, user.PasswordSalt))
                        return new ChangePasswordDto { message = "Current password is incorrect", status="Error" };
                    // Hash new password
                    var salt = _passwordService.GenerateSalt();
                    var newPasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword, salt);
                    // Update user password
                    user.Password = newPasswordHash;
                    user.PasswordSalt = salt;
                    user.ModifiedDate = DateTime.Now;
                    _context.User.Update(user);
                    await _context.SaveChangesAsync();
                 //   validateMsg = "User password is changed. please proceed with login functionality";
                   
                    return new ChangePasswordDto { message = "User password is changed.", status = "Success" };
                }
                else
                {                  
                    return new ChangePasswordDto { message = "User not found.", status = "Error" };
                }
            }
            catch (Exception)
            {
                return new ChangePasswordDto { message = "Member change password functionality is failed", status = "Error" };            
            }
            return null;
        }
        public async Task<string> ForgotPasswordAsync(string email)
        {
            string validateMsg = string.Empty; string? appurl = string.Empty; string? mobileappurl = string.Empty;
            try
            {

                var userdetails = await _context.User
                   .FirstOrDefaultAsync(u => u.EmailID == email && u.IsActive);
                if (userdetails != null)
                {
                    userdetails.EmailConfirmationToken = PasswordGenerator.Generate();
                    userdetails.EmailConfirmationTokenExpiry = DateTime.Now.AddMinutes(60);
                    userdetails.MobileConfirmationToken = Generaterandomnumber();
                    userdetails.MobileConfirmationTokenExpiry = DateTime.Now.AddMinutes(60);
                    userdetails.EmailConfirmed = false;
                    userdetails.MobileNoConfirmed = false;
                    _context.User.Update(userdetails);
                 
                    await _context.SaveChangesAsync();

                    //web Token
                    appurl = GenerateWebLink(userdetails.UserID, userdetails.EmailConfirmationToken, "0", userdetails.EmailID);
                    mobileappurl = GenerateWebLink(userdetails.UserID, userdetails.MobileConfirmationToken, "1", userdetails.MobileNo);
                    await SendEail(userdetails.EmailID, userdetails.EmailConfirmationToken, userdetails.MobileConfirmationToken, string.Format("{0} {1}", userdetails.FirstName, userdetails.LastName), appurl, mobileappurl);

                    /*
                    //web Token
                    appurl = GenerateWebLink(memberdetails.MemberID, memberdetails.EmailConfirmationToken, "0", memberdetails.EmailID);
                    await SendOtpMail(memberdetails.EmailID, memberdetails.EmailConfirmationToken, string.Format("{0} {1}", memberdetails.FirstName, memberdetails.LastName), appurl);
                    //OTP
                    appurl = GenerateWebLink(memberdetails.MemberID, memberdetails.MobileConfirmationToken, "1", memberdetails.MobileNo);
                    await SendOtpSMS(memberdetails.EmailID, memberdetails.MobileConfirmationToken, string.Format("{0} {1}", memberdetails.FirstName, memberdetails.LastName), appurl);
                    */
                    validateMsg = "User password reset is done. Please check your email for confirmation link or OTP for mobile verification.";

                }
                else
                    validateMsg = "User email is not found";

            }
            catch (Exception)
            {

                validateMsg = "User password reset is failed";
            }


            return validateMsg;
        }

        public async Task<MemberDto> DeleteMemberAsync(int MemberId)
        {
            try
            {
                var member = await _context.Member.FirstOrDefaultAsync(m => m.MemberID == MemberId && m.IsActive);
                if(member!=null)
                {
                    member.IsActive = false;
                    member.IsDelete = true;                  
                    _context.Member.Update(member);
                    await _context.SaveChangesAsync(); 
                    
                    if(member.HasLogin )
                    {
                        var loginuser = await _context.User.FirstOrDefaultAsync(m => m.UserID == member.IsMemberLinkID);
                        if (loginuser != null)
                        {                         
                            _context.User.Remove(loginuser);
                            await _context.SaveChangesAsync();
                        }
                    }

                    return new MemberDto
                    {
                        Message = "Member deleted successfully.",
                        Status = "Success"
                    };
                }

            }
            catch (Exception exec)
            {
                return new MemberDto
                {
                    Message = "Member is not deleted",
                    Status = "Error"
                };

            }
            return new MemberDto
            {
                Message = "Member is not deleted",
                Status = "Error"
            };
        }

        public async Task<MemberDto> UpdateMemberImageUrl(int MemberID, string filename)
        {
            var existingmember = await _context.Member.FirstOrDefaultAsync(u => u.MemberID == MemberID && u.IsActive);

            //validation

            if (existingmember != null)
            {
                existingmember.MemberPhoto = filename;
                _context.Member.Update(existingmember);
                await _context.SaveChangesAsync();

                if(existingmember.HasLogin && existingmember.IsMemberLinkID >0)
                {
                    var existingUser = await _context.User.FirstOrDefaultAsync(u => u.UserID == existingmember.IsMemberLinkID && u.IsActive);
                    if(existingUser!=null)
                    {
                        existingUser.ProfilePhoto = filename;
                        _context.User.Update(existingUser);
                        await _context.SaveChangesAsync();

                    }
                }

                return new MemberDto
                {

                    MemberID = existingmember.MemberID,
                    MemberPhotoURL = existingmember.MemberPhoto,
                    Message = "Member Image url uploaded successfully",
                    Status = "Success"
                };
            }
            return new MemberDto
            {     
                Message = "Member is not found",
                Status = "Error"
            };

        }

        public async Task<UserDto> UpdatePasswordLinkShow(int UserID)
        {
            var existinguser = await _context.User.FirstOrDefaultAsync(u => u.UserID == UserID && u.IsActive);

            //validation

            if (existinguser != null)
            {
                existinguser.IsPasswordlinkShow = false;
                _context.User.Update(existinguser);
                await _context.SaveChangesAsync();


                return new UserDto
                {

                    UserID  = existinguser.UserID,
                    ProfilePhoto = existinguser.ProfilePhoto,
                    Message = "Member Password link updated successfully",
                    Status = "Success"
                };
            }
            return new UserDto
            {
                Message = "User is not found",
                Status = "Error"
            };

        }

        public async Task<MemberInsuranceDTO> SaveInsuranceAsync(MemberInsuranceDTO MemberInsuranceDto,string storagepath)
        {
            string validateMsg = string.Empty; string status = "Error";int memInsID = 0;
            try
            {

           
            if (MemberInsuranceDto != null)
            {
                var existingmember = await _context.Member.FirstOrDefaultAsync(u => u.MemberID == MemberInsuranceDto.MemberID && u.IsActive);
                if (existingmember != null)
                {                    
                   var defaultBenfitType = await _context.Codes.FirstOrDefaultAsync(u => u.CodeType == "BenefitType" && u.IsActive);
                        if (MemberInsuranceDto.MemInsID <= 0)
                    {

                            //validation
                                                      
                            if (await _context.MemberInsurance.AnyAsync(u => u.PolicyTypeID == MemberInsuranceDto.PolicyTypeID && u.MemberID ==MemberInsuranceDto.MemberID && u.IsActive))
                            {
                                return new MemberInsuranceDTO
                                {
                                    Message = "Member Insurance Policy Type exists",
                                    Status = "Error"
                                };
                            }
                            //New Memeber Insurance
                            var memberinsurance = new Models.MemberInsurance
                        {
                            MemberID = MemberInsuranceDto.MemberID,
                            DependentsID = MemberInsuranceDto.DependentsID,
                            BenefitTypeID = (MemberInsuranceDto.BenefitTypeID == 0) ? defaultBenfitType?.CodeID ?? 0 : MemberInsuranceDto.BenefitTypeID,                                
                            PolicyTypeID = MemberInsuranceDto.PolicyTypeID,
                            SubscriberID = MemberInsuranceDto.SubscriberID,
                            PolicyID = MemberInsuranceDto.PolicyID,
                            QuestionnaireId = MemberInsuranceDto.QuestionnaireId,
                            IsActive = true,
                            AddDate = DateTime.Now,
                            AddedBy = MemberInsuranceDto.UserID.ToString(),
                            };
                        _context.MemberInsurance.Add(memberinsurance);
                        await _context.SaveChangesAsync();

                        //Testing Purpose
                        MemberInsEligibility memberInsEligibility = new MemberInsEligibility
                        {
                            MemInsID = memberinsurance.MemInsID,
                            IsInsuranceActive = true,
                            InsActiveDateFrom = DateTime.Now,
                            InsActiveDateTo = DateTime.Now.AddYears(1),
                            EligibilityChkDate = DateTime.Now.ToShortDateString(),
                            IsActive=true,
                            AddDate = DateTime.Now,
                            AddedBy = MemberInsuranceDto.UserID.ToString()
                        };
                            _context.MemberInsEligibility.Add(memberInsEligibility);
                            await _context.SaveChangesAsync();

                            memInsID = memberinsurance.MemInsID;
                            if (memInsID > 0 && MemberInsuranceDto.FrontImage != null && MemberInsuranceDto.BackImage != null)
                            {
                                var fileupload = await Uploadfile(memInsID, MemberInsuranceDto, storagepath);
                                MemberInsuranceDto.InsBackImgURL= fileupload.InsBackImgURL;
                                MemberInsuranceDto.InsFrontImgURL = fileupload.InsFrontImgURL;
                            }

                        validateMsg = "Member Insurance added successfully.";
                        status = "Success";

                        return new MemberInsuranceDTO
                        {
                            MemInsID = memberinsurance.MemInsID,
                            Message = validateMsg,
                            Status = status,
                            InsBackImgURL = MemberInsuranceDto.InsBackImgURL,
                            InsFrontImgURL = MemberInsuranceDto.InsFrontImgURL
                        };
                    }
                    else
                    {

                        var existingmemberInsurance = await _context.MemberInsurance.FirstOrDefaultAsync(u => u.MemInsID == MemberInsuranceDto.MemInsID && u.IsActive);

                        //validation

                        if (existingmemberInsurance != null)
                        {
                          
                            if (await _context.MemberInsurance.AnyAsync(u => u.PolicyTypeID == MemberInsuranceDto.PolicyTypeID && u.MemberID ==MemberInsuranceDto.MemberID && u.MemInsID != MemberInsuranceDto.MemInsID && u.IsActive))
                            {
                                return new MemberInsuranceDTO
                                {
                                    Message = "Member Insurance Policy Type exists",
                                    Status = "Error"
                                };
                            }
                            memInsID = existingmemberInsurance.MemInsID;
                            existingmemberInsurance.DependentsID = MemberInsuranceDto.DependentsID;
                            existingmemberInsurance.BenefitTypeID = MemberInsuranceDto.BenefitTypeID;
                            existingmemberInsurance.PolicyTypeID = MemberInsuranceDto.PolicyTypeID;
                            existingmemberInsurance.SubscriberID = MemberInsuranceDto.SubscriberID;
                                existingmemberInsurance.PolicyID = MemberInsuranceDto.PolicyID;
                            existingmemberInsurance.ModifiedDate = DateTime.Now;
                            existingmemberInsurance.ModifiedBy = MemberInsuranceDto.UserID.ToString();
                                _context.MemberInsurance.Update(existingmemberInsurance);
                            await _context.SaveChangesAsync();
                            if (MemberInsuranceDto.FrontImage != null && MemberInsuranceDto.BackImage != null)
                            {
                                var fileupload = await Uploadfile(memInsID, MemberInsuranceDto, storagepath);
                                MemberInsuranceDto.InsBackImgURL = fileupload.InsBackImgURL;
                                MemberInsuranceDto.InsFrontImgURL = fileupload.InsFrontImgURL;
                            }
                            else
                            {
                                MemberInsuranceDto.InsBackImgURL = existingmemberInsurance.InsBackImgURL;
                                MemberInsuranceDto.InsFrontImgURL = existingmemberInsurance.InsFrontImgURL;
                                    
                            }
                                    validateMsg = "Member Insurance updated successfully.";
                            status = "Success";
                                //Testing Purpose
                                var memEligibility = await _context.MemberInsEligibility.FirstOrDefaultAsync(u => u.MemInsID == existingmemberInsurance.MemInsID && u.IsActive);
                                if (memEligibility == null)
                                {
                                    MemberInsEligibility memberInsEligibility = new MemberInsEligibility
                                    {
                                        MemInsID = memInsID,
                                        IsInsuranceActive = true,
                                        InsActiveDateFrom = DateTime.Now,
                                        InsActiveDateTo = DateTime.Now.AddYears(1),
                                        EligibilityChkDate = DateTime.Now.ToShortDateString(),
                                        AddDate = DateTime.Now,
                                        AddedBy = MemberInsuranceDto.UserID.ToString()
                                    };
                                    _context.MemberInsEligibility.Add(memberInsEligibility);
                                    await _context.SaveChangesAsync();

                                }
                                return new MemberInsuranceDTO
                            {
                                MemInsID = existingmemberInsurance.MemInsID,
                                Message = validateMsg,
                                Status = status,
                                InsBackImgURL = MemberInsuranceDto.InsBackImgURL,
                                InsFrontImgURL = MemberInsuranceDto.InsFrontImgURL
                            };
                        }
                    }
                 
                }
            }
            }
            catch (Exception)
            {

                validateMsg = "Member Insurance is not saved";
                status = "Error";
            }
            return new MemberInsuranceDTO
                {                  
                    Message = validateMsg,
                    Status = status
                };
            
        }

        private async Task<MemberInsuranceDTO> Uploadfile(int memInsID,MemberInsuranceDTO memberInsurance,string storagepath)
        {
            try
            {
                if (memberInsurance != null && memInsID > 0)
                {
                    var policytype   = await _context.Codes.FirstOrDefaultAsync(u => u.CodeID == memberInsurance.PolicyTypeID);
                    // Ensure the directory exists
                    if (!Directory.Exists(storagepath))
                    {
                        Directory.CreateDirectory(storagepath);
                    }
                    // Create a unique filename
                    if (memberInsurance.FrontImage != null && memberInsurance.FrontImage.Length > 0)
                    {
                          var frontFileName = string.Format("{0}_{1}_{2}{3}", string.Concat("MemInsurance",memInsID.ToString()), policytype.CodeType, "frontimage",Path.GetExtension(memberInsurance.FrontImage.FileName));
                            string frontFilePath = Path.Combine(storagepath, frontFileName);
                            using (var stream = new FileStream(frontFilePath, FileMode.Create))
                            {
                                await memberInsurance.FrontImage.CopyToAsync(stream);
                                memberInsurance.InsFrontImgURL = frontFileName; // Save the file name to the DTO
                            }
                        
                    }
                    if (memberInsurance.BackImage != null && memberInsurance.BackImage.Length > 0)
                    {
                        var backFileName = string.Format("{0}_{1}_{2}{3}", string.Concat("MemInsurance", memInsID.ToString()), policytype.CodeType, "backimage",Path.GetExtension(memberInsurance.BackImage.FileName));
                        string backFilePath = Path.Combine(storagepath, backFileName);
                        using (var stream = new FileStream(backFilePath, FileMode.Create))
                        {
                            await memberInsurance.BackImage.CopyToAsync(stream);
                            memberInsurance.InsBackImgURL = backFileName; // Save the file name to the DTO
                        }
                    }
                    if (!string.IsNullOrEmpty(memberInsurance.InsFrontImgURL) || !string.IsNullOrEmpty(memberInsurance.InsBackImgURL))
                    {
                        var existingmemberInsurance = await _context.MemberInsurance.FirstOrDefaultAsync(u => u.MemInsID == memInsID && u.IsActive);
                        if (existingmemberInsurance != null)
                        {
                            existingmemberInsurance.InsFrontImgURL = string.IsNullOrEmpty(memberInsurance.InsFrontImgURL) ? string.Empty : memberInsurance.InsFrontImgURL;
                            existingmemberInsurance.InsBackImgURL = string.IsNullOrEmpty(memberInsurance.InsBackImgURL) ? string.Empty : memberInsurance.InsBackImgURL;
                            _context.MemberInsurance.Update(existingmemberInsurance);
                            await _context.SaveChangesAsync();

                        }

                       
                    }

                }
                return memberInsurance;
            }
            catch (Exception ex)
            {
               
            }
            return memberInsurance;

        }

        public async Task<MemberDto> SaveMemberAsync(MemberDto MemberDto)
        {
            string validateMsg = string.Empty; string status = "Error";
            string? appurl = string.Empty; string? mobileappurl = string.Empty;
            int newMemberID = 0;
            Boolean haslogin=false;
           
            try
            {
               

                var relationship = await _context.Codes.FirstOrDefaultAsync(r => r.CodeValue == "Self");

                var selfmember = await _context.Member.FirstOrDefaultAsync(m => m.MemberID == MemberDto.MappedMemberID && m.HasLogin);

                if (MemberDto.MemberID == 0)
                {

                    //validation
                    if (await _context.Member.AnyAsync(u => u.EmailID == MemberDto.EmailID && u.IsActive ))
                    {
                        return new MemberDto
                        {
                            Message = "Member Email exists",
                            Status = "Error"
                        };
                    }

                    if (await _context.Member.AnyAsync(u => u.MobileNo == MemberDto.MobileNo && u.IsActive))
                    {
                        return new MemberDto
                        {
                            Message = "Member Mobile number exists",
                            Status = "Error"
                        };
                    }

                    if (relationship!=null && relationship.CodeID == MemberDto.RelationshipTypeID)
                        return new MemberDto
                        {
                            Message = "You can't create the primary member again!",
                            Status = "Error"
                        };
                    //New Memeber
                    var member = new Models.Member
                    {
                        MappedMemberID = selfmember.MappedMemberID,
                        FirstName = MemberDto.FirstName.Trim(),
                        LastName = MemberDto.LastName.Trim(),
                        DOB = MemberDto.DOB,                     
                        MobileNo = MemberDto.MobileNo,
                        EmailID = MemberDto.EmailID.Trim(),
                        Address1 = MemberDto.Address1,
                        Address2 = MemberDto.Address2,
                        City = MemberDto.City,
                        State = MemberDto.State,
                        Gender = MemberDto.Gender,      
                        ZipCode= MemberDto.ZipCode,
                        RelationshipTypeID=MemberDto.RelationshipTypeID,                     
                        FamilyID= selfmember.FamilyID,
                        HasLogin= MemberDto.HasLogin,
                        IsActive = true,                       
                        AddDate = DateTime.Now,
                        AddedBy= selfmember.MemberID.ToString()
                    };

                    _context.Member.Add(member);
                    await _context.SaveChangesAsync();
                    newMemberID = member.MemberID;

                    if (newMemberID > 0 && MemberDto.HasLogin)
                    {
                        var userDto = new UserDto
                        {
                            FirstName = MemberDto.FirstName,
                            LastName = MemberDto.LastName,
                            EmailID = MemberDto.EmailID,
                            MobileNo = MemberDto.MobileNo,
                            DOB = MemberDto.DOB,
                            Gender = MemberDto.Gender
                        };
                        
                        var result=await SaveUserAsync(userDto);

                        if(result!=null && result.UserID>0)
                        {
                            var updatemember = await _context.Member.FirstOrDefaultAsync(m => m.MemberID == newMemberID && m.IsActive);
                            if (updatemember != null)
                            {
                                updatemember.IsMemberLinkID = result.UserID;
                                _context.Member.Update(updatemember);
                                await _context.SaveChangesAsync();
                            }
                        }
                        // validateMsg = "Member registered successfully. Please check your email for confirmation link or OTP for mobile verification.";
                        //  status = "Success";
                    }
                   
                    return new MemberDto
                    {

                        MemberID = newMemberID,
                        FirstName = MemberDto.FirstName.Trim(),
                        LastName = MemberDto.LastName.Trim(),
                        Gender = MemberDto.Gender,
                        EmailID = MemberDto.EmailID.Trim(),
                        DOB = MemberDto.DOB,
                        MobileNo = MemberDto.MobileNo,
                        IsActive = true,
                        AddDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Message = "Member added successfully",
                        Status = "Success"
                    };

                }
                else
                {
                    Boolean IsMemeberhaslogin = false;
                    var existingmember = await _context.Member.FirstOrDefaultAsync(u => u.MemberID == MemberDto.MemberID && u.IsActive);

                    //validation
                 
                    if (existingmember != null)
                    {
                       
                        if (relationship != null && relationship.CodeID == MemberDto.RelationshipTypeID && selfmember.IsPrimaryMem && existingmember.RelationshipTypeID!= MemberDto.RelationshipTypeID)
                            return new MemberDto
                            {
                                Message = "You can't create the primary member again!",
                                Status = "Error",
                                MemberID = existingmember.MemberID
                            };

                        if (await _context.Member.AnyAsync(u => u.EmailID == MemberDto.EmailID) && existingmember.EmailID != MemberDto.EmailID)
                        {
                            return new MemberDto
                            {
                                Message = "Member Email exists",
                                Status = "Error",
                                MemberID = existingmember.MemberID

                            };
                        }

                        if (await _context.Member.AnyAsync(u => u.MobileNo == MemberDto.MobileNo) && existingmember.MobileNo != MemberDto.MobileNo)
                        {
                            return new MemberDto
                            {
                                Message = "Member Mobile number exists",
                                Status = "Error",
                                MemberID = existingmember.MemberID
                            };
                        }

                        IsMemeberhaslogin =existingmember.HasLogin;
                        existingmember.FirstName = MemberDto.FirstName;
                        existingmember.LastName = MemberDto.LastName;
                        existingmember.DOB = MemberDto.DOB;
                        existingmember.MobileNo = MemberDto.MobileNo;
                        existingmember.EmailID = MemberDto.EmailID;
                        existingmember.Address1 = MemberDto.Address1;
                        existingmember.Address2 = MemberDto.Address2;
                        existingmember.City = MemberDto.City;
                        existingmember.State = MemberDto.State;
                        existingmember.Gender = MemberDto.Gender;
                        existingmember.ZipCode = MemberDto.ZipCode;
                        existingmember.RelationshipTypeID = MemberDto.RelationshipTypeID;                       
                        existingmember.IsActive = true;
                        existingmember.ModifiedDate = DateTime.Now;
                        existingmember.ModifiedBy = MemberDto.MappedMemberID.ToString();
                        existingmember.HasLogin = MemberDto.HasLogin;
                        
                        _context.Member.Update(existingmember);
                        await _context.SaveChangesAsync();

                        if(!IsMemeberhaslogin && MemberDto.HasLogin)
                        {
                            var userDto = new UserDto
                            {
                                FirstName = MemberDto.FirstName,
                                LastName = MemberDto.LastName,
                                EmailID = MemberDto.EmailID,
                                MobileNo = MemberDto.MobileNo,
                                DOB = MemberDto.DOB,
                                Gender = MemberDto.Gender
                            };

                            var result = await SaveUserAsync(userDto);

                            if (result != null && result.UserID > 0)
                            {
                                var updatemember = await _context.Member.FirstOrDefaultAsync(m => m.MemberID == newMemberID && m.IsActive);
                                if (updatemember != null)
                                {
                                    updatemember.IsMemberLinkID = result.UserID;
                                    _context.Member.Update(updatemember);
                                    await _context.SaveChangesAsync();
                                }
                            }

                        }
                        return new MemberDto
                        {

                            MemberID = existingmember.MemberID,
                            FirstName = MemberDto.FirstName,
                            LastName = MemberDto.LastName,
                            Gender = MemberDto.Gender,
                            EmailID = MemberDto.EmailID,
                            DOB = MemberDto.DOB,
                            MobileNo = MemberDto.MobileNo,
                            IsActive = true,
                            AddDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Message = "Member updated successfully",
                            Status = "Success"
                        };

                    }

                }


            }
            catch (Exception exec)
            {
                return new MemberDto
                {
                    Message = "Member is not saved",
                    Status = "Error"
                };

            }
            return new MemberDto
            {              
                Message = "Member is not saved",
                Status = "Error"
            };
        }

      /*  public async Task<UserDto> SaveUserAsync(UserDto userDto)
        {
            string validateMsg = string.Empty; string status = "Error";
            string? appurl = string.Empty; string? Mobileappurl = string.Empty;int newUserID = 0;int roleId=0;
            // Assign default role
            var userRole = await _context.Codes.FirstOrDefaultAsync(r => r.CodeType == "Role" && r.CodeValue == "User");

            if (userDto != null)
            {
                //Basic validation
                if (string.IsNullOrEmpty(userDto.EmailID)) 
                {
                    return new UserDto
                    {
                        Message = "User Email is required",
                        Status = "Error"
                    };
                }
                roleId= userDto.RoleId == 0 ? userRole?.CodeID ?? 0 : userDto.RoleId;
                if (roleId == userRole?.CodeID)//Normal User
                {
                    if (string.IsNullOrEmpty(userDto.MobileNo))
                    {
                        return new UserDto
                        {
                            Message = "User Mobile number is required",
                            Status = "Error"
                        };
                    }
                }


                try
                    {
                        if (userDto.UserID == 0)
                        {

                            if (await _context.User.AnyAsync(u => u.EmailID == userDto.EmailID && u.IsActive))
                            {
                                return new UserDto
                                {
                                    Message = "User Email exists",
                                    Status = "Error"
                                };
                            }

                            if (roleId == userRole?.CodeID && await _context.User.AnyAsync(u => u.MobileNo == userDto.MobileNo && u.IsActive))
                            {
                                return new UserDto
                                {
                                    Message = "User Mobile number exists",
                                    Status = "Error"
                                };
                            }

                            //create  user
                            var user = new Models.User
                            {
                                FirstName = userDto.FirstName,
                                LastName = userDto.LastName,
                                EmailID = userDto.EmailID,
                                MobileNo = userDto.MobileNo,
                                DOB = userDto.DOB,
                                Gender = userDto.Gender,
                                EmailConfirmationToken = PasswordGenerator.Generate(),
                                EmailConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),
                                MobileConfirmationToken = Generaterandomnumber(),
                                MobileConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),
                                IsActive = true,
                                IsPasswordlinkShow = true,
                                RoleId = (userDto.RoleId == 0) ? userRole?.CodeID ?? 0 : userDto.RoleId,
                                AddDate = DateTime.Now

                            };

                            _context.User.Add(user);
                            await _context.SaveChangesAsync();
                            newUserID= user.UserID;
                        if (user.UserID == 0)
                            {
                                validateMsg = "User registration failed";
                            }
                            else
                            {

                                //web Token
                                if (!string.IsNullOrEmpty(user.EmailID))
                                    appurl = GenerateWebLink(user.UserID, user.EmailConfirmationToken, "0", user.EmailID);
                                if (!string.IsNullOrEmpty(user.MobileNo))
                                    Mobileappurl = GenerateWebLink(user.UserID, user.MobileConfirmationToken, "1", user.MobileNo);
                                await SendEail(user.EmailID, user.EmailConfirmationToken, user.MobileConfirmationToken, string.Format("{0} {1}", user.FirstName, user.LastName), appurl, Mobileappurl);
                                validateMsg = "User registered successfully. Please check your email for confirmation link or OTP for mobile verification.";
                                status = "Success";
                            }
                        }
                        else
                        {

                            var existinguser = await _context.User.FirstOrDefaultAsync(u => u.UserID == userDto.UserID);
                            //validation
                            if (existinguser != null)
                            {
                            newUserID = existinguser.UserID;
                            if (await _context.User.AnyAsync(u => u.EmailID == userDto.EmailID) && existinguser.EmailID != userDto.EmailID)
                                {
                                    return new UserDto
                                    {
                                        Message = "User Email exists",
                                        Status = "Error"
                                    };
                                }
                                if (roleId == userRole?.CodeID && await _context.User.AnyAsync(u => u.MobileNo == userDto.MobileNo) && existinguser.MobileNo != userDto.MobileNo)
                                {
                                    return new UserDto
                                    {
                                        Message = "User Mobile number exists",
                                        Status = "Error"
                                    };
                                }
                                existinguser.FirstName = userDto.FirstName;
                                existinguser.LastName = userDto.LastName;
                                existinguser.DOB = userDto.DOB;
                                existinguser.MobileNo = userDto.MobileNo;
                                existinguser.EmailID = userDto.EmailID;
                                existinguser.ModifiedDate = DateTime.Now;
                                existinguser.Gender = userDto.Gender;
                                existinguser.RoleId = (userDto.RoleId == 0) ? userRole?.CodeID ?? 0 : userDto.RoleId;
                                existinguser.IsActive = userDto.IsActive;
                                _context.User.Update(existinguser);
                                await _context.SaveChangesAsync();

                            }
                        }

                    }
                    catch (Exception)
                    {

                        validateMsg = "User registration failed";
                    }

                return new UserDto
                {

                    UserID = newUserID,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Gender = userDto.Gender,
                    EmailID = userDto.EmailID,
                    DOB = userDto.DOB,
                    MobileNo = userDto.MobileNo,
                    IsActive = userDto.IsActive,
                    AddDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Message = "User registered successfully",
                    Status = "Success"

                };
            }
            return new UserDto
            {
                Message = "User data is empty",
                Status = "Error"
            };
        }*/

        public async Task<UserDto> RegisterUserAsync(UserDto userDto)
        {
            string validateMsg=string.Empty; string status = "Error";
            string? appurl = string.Empty; string? Mobileappurl = string.Empty;           
            // Assign default role
            var userRole = await _context.Codes.FirstOrDefaultAsync(r => r.CodeType == "Role" && r.CodeValue == "User");           
            try
            {
                //create  user
                var user = new Models.User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    EmailID = userDto.EmailID,
                    MobileNo = userDto.MobileNo,
                    DOB = userDto.DOB,
                    Gender = userDto.Gender,
                    EmailConfirmationToken = PasswordGenerator.Generate(),
                    EmailConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),
                    MobileConfirmationToken = Generaterandomnumber(),
                    MobileConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),                   
                    IsActive = true,
                    IsPasswordlinkShow=true,                  
                    RoleId= (userDto.RoleId==0)? userRole?.CodeID ?? 0 : userDto.RoleId,
                    AddDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                if (user.UserID == 0)
                {
                    validateMsg = "User registration failed";
                }
                else
                {

                    //web Token
                    if (!string.IsNullOrEmpty(user.EmailID))
                        appurl = GenerateWebLink(user.UserID, user.EmailConfirmationToken, "0", user.EmailID);
                    if(!string.IsNullOrEmpty(user.MobileNo))                  
                        Mobileappurl = GenerateWebLink(user.UserID, user.MobileConfirmationToken, "1", user.MobileNo);
                    await SendEail(user.EmailID, user.EmailConfirmationToken, user.MobileConfirmationToken, string.Format("{0} {1}", user.FirstName, user.LastName), appurl, Mobileappurl);

                    /*
                    appurl = GenerateWebLink(newMemberID, member.EmailConfirmationToken, "0", member.EmailID);
                    await SendOtpMail(member.EmailID, member.EmailConfirmationToken, string.Format("{0} {1}", member.FirstName, member.LastName),appurl);
                    //OTP
                    appurl = GenerateWebLink(newMemberID, member.MobileConfirmationToken, "1", member.MobileNo);
                    await SendOtpSMS(member.EmailID, member.MobileConfirmationToken, string.Format("{0} {1}", member.FirstName, member.LastName), appurl);
                    */
                    validateMsg = "User registered successfully. Please check your email for confirmation link or OTP for mobile verification.";
                    status= "Success";
                }


            }
            catch (Exception)
            {

                validateMsg = "User registration failed";
            }

            return new UserDto
            {

                UserID = userDto.UserID,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Gender = userDto.Gender,
                EmailID = userDto.EmailID,
                DOB = userDto.DOB,
                MobileNo = userDto.MobileNo,
                IsActive = true,
                AddDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Message = validateMsg,
                Status = status

            };
        }


        public async Task<bool> LockUserAsync(int userId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null) return false;

            user.IsLocked = true;           

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null) return false;

            user.IsLocked = false;          
            user.FaiedLoginAttempt = 0;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.User.AnyAsync(u => u.EmailID == email);
        }

        public async Task<bool> MemberEmailExistsAsync(string email)
        {
            return await _context.Member.AnyAsync(u => u.EmailID == email && u.IsActive);
        }


        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.IsActive && s.ExpiresAt > DateTime.UtcNow);

            if (user == null) return null;


            if (!user.IsActive || user.IsLocked) return null;

         
            // Generate new tokens
            var newJwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRandomToken();
            var newExpiresAt = DateTime.UtcNow.AddDays(1);

            // Update session
            user.SessionToken = newJwtToken;
            user.RefreshToken = newRefreshToken;
            user.ExpiresAt = newExpiresAt;

            await _context.SaveChangesAsync();
            return new AuthResponseDto
            {
                UserId = Convert.ToString(user.UserID),
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                Email = user.EmailID,
                ExpiresAt = newExpiresAt
              
            };
        }

        public async Task<bool> LogoutAsync(string sessionToken)
        {
            var session = await _context.User
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

            if (session == null) return false;

            session.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Check if session exists and is active
                var session = await _context.User
                    .FirstOrDefaultAsync(s => s.SessionToken == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow);

                return session != null;
            }
            catch
            {
                return false;
            }
        }


        private async Task SendOtpMail(string useremail, string OtpText, string Name,string appUrl)
        {
            var mailrequest = new Mailrequest();
            mailrequest.Email = useremail;
            mailrequest.Subject = "Thanks for registering : OTP";
            mailrequest.Emailbody = GenerateEmailBody(Name, OtpText, appUrl,false);
            await this.emailService.SendEmailAsync(mailrequest);

        }

        private async Task SendEail(string useremail,string webtoken, string OtpText, string Name, string appUrl, string MobileappUrl)
        {
            var mailrequest = new Mailrequest();
            mailrequest.Email = useremail;
            mailrequest.Subject = "Thanks for registering Activa Patients";
            mailrequest.Emailbody = GenerateEmail(Name, webtoken,OtpText, appUrl, MobileappUrl);
            await this.emailService.SendEmailAsync(mailrequest);

        }

        private async Task SendOtpSMS(string useremail, string OtpText, string Name, string appUrl)
        {
            var mailrequest = new Mailrequest();
            mailrequest.Email = useremail;
            mailrequest.Subject = "Thanks for registering : OTP";
            mailrequest.Emailbody = GenerateEmailBody(Name, OtpText, appUrl, true);
            await this.emailService.SendEmailAsync(mailrequest);

        }

        private string GenerateWebLink(int userId,string token,string IsMobile,string strEmailMobile)
        {
            string applink=string.Empty;
            var clientUrl = _configuration["client:AppUrl"];
            if (clientUrl != null)
            {
                applink = clientUrl.Replace("userIdInfo", userId.ToString());
                applink = applink.Replace("tokenIdInfo", token).Replace("IsMobileInfo", IsMobile).Replace("useremailInfo", strEmailMobile);
            }
            return applink;
        }
        private string Generaterandomnumber()
        {
            Random random = new Random();
            string randomno = random.Next(0, 1000000).ToString("D6");
            return randomno;
        }


        private string GenerateEmail(string name, string webtoken,string otptext, string appUrl, string MobileappUrl)
        {
           
            string emailbody = "<div style='width:100%;background-color:grey'>";
            emailbody += "<h1>Hi " + name + ", Thanks for registering into Activa Patients </h1>";
            emailbody += "<h2>Please enter OTP password and complete the registration</h2>";
            emailbody += "<h2>Password :" + webtoken + "</h2>";
            emailbody += "<h2>OTP password :" + otptext + "</h2>";
            if (!string.IsNullOrEmpty(appUrl))
            {
                emailbody += "<h2>Click here to confirm your<a href='" + appUrl + "'> email </a></h2>";
            }
            if (!string.IsNullOrEmpty(MobileappUrl))
            {
                emailbody += "<h2>Click here to confirm your<a href='" + MobileappUrl + "'> Mobile number</a></h2>";
            }
            emailbody += "</div>";

            return emailbody;
        }

        private string GenerateEmailBody(string name, string otptext,string appUrl,Boolean isMobile)
        {
            string deviceType = isMobile ? "Mobile number" : "email";
            string emailbody = "<div style='width:100%;background-color:grey'>";
            emailbody += "<h1>Hi " + name + ", Thanks for registering </h1>";
            emailbody += "<h2>Please enter OTP text and complete the registration</h2>";
            emailbody += "<h2>OTP Text is :" + otptext + "</h2>";          
            if (!string.IsNullOrEmpty(appUrl))
            {
                emailbody += "<h2>Click here to confirm your<a href='" + appUrl + "'> "+ deviceType + "</a></h2>";
            }
            emailbody += "</div>";

            return emailbody;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Email, user.EmailID),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRandomToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[64];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes);
            }
        }

        public async Task<List<UserListDto>> SearchAdmin_UserManagement(string? query)
        {
            query = query?.ToLower()?.Trim();

            var UserQuery = from a in _context.User
                             join c in _context.Codes
                               on a.RoleId equals c.CodeID
                             where c.CodeType == "Role"
                                   && c.IsActive && !c.IsDelete && c.CodeValue!= "User" && a.EmailConfirmed==true
                            select new
                             {
                                 a.UserID,
                                 a.FirstName,
                                 a.LastName,
                                 a.EmailID,
                                 a.RoleId,
                                 a.IsActive,
                                 a.LastLoginDate,
                                 RoleName = c.CodeValue
                             };

            if (!string.IsNullOrWhiteSpace(query))
            {
                if (query == "active")
                {
                    UserQuery = UserQuery.Where(a => a.IsActive);
                }
                else if (query == "inactive")
                {
                    UserQuery = UserQuery.Where(a => !a.IsActive);
                }
                else
                {
                    UserQuery = UserQuery.Where(a =>
                        (a.FirstName != null && a.FirstName.ToLower().Contains(query)) ||
                        (a.LastName != null && a.LastName.ToLower().Contains(query)) ||
                        (a.EmailID != null && a.EmailID.ToLower().Contains(query)) ||
                        (a.RoleName != null && a.RoleName.ToLower().Contains(query))
                    );
                }
            }

            var results = await UserQuery
                .Select(u => new UserListDto
                {
                    UserID = u.UserID,
                    Email = u.EmailID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    RoleId = (int)u.RoleId,
                    RoleName = u.RoleName,
                    IsActive = u.IsActive,
                    LastLoginDate = u.LastLoginDate
                })
                .ToListAsync();

            return results;
        }
      
        public async Task<UserDto> DeleteUserAsync(int UserId)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u=> u.UserID== UserId && u.IsActive);
                if (user != null)
                {
                    user.IsActive = false;
                    user.IsDelete = true;
                    _context.User.Update(user);
                    await _context.SaveChangesAsync();

                    

                    return new UserDto
                    {
                        Message = "User deleted successfully.",
                        Status = "Success"
                    };
                }

            }
            catch (Exception exec)
            {
                return new UserDto
                {
                    Message = "User is not deleted",
                    Status = "Error"
                };

            }
            return new UserDto
            {
                Message = "User is not deleted",
                Status = "Error"
            };
        }
        public async Task<UserDto> SaveUserAsync(UserDto userDto)
        {
            if (userDto == null)
            {
                return new UserDto
                {
                    Message = "User data is empty",
                    Status = "Error"
                };
            }

            string validateMsg = string.Empty;
            string status = "Error";
            int newUserID = 0;
            Models.User? existingUser = null;

            // Get default role "User"
            var userRole = await _context.Codes
                .FirstOrDefaultAsync(r => r.CodeType == "Role" && r.CodeValue == "User");

            int roleId = userDto.RoleId == 0 ? userRole?.CodeID ?? 0 : userDto.RoleId;

            // Basic validation
            if (string.IsNullOrEmpty(userDto.EmailID))
            {
                return new UserDto
                {
                    Message = "User Email is required",
                    Status = "Error"
                };
            }

            // Require MobileNo only for "User" role
            if (roleId == userRole?.CodeID && string.IsNullOrEmpty(userDto.MobileNo))
            {
                return new UserDto
                {
                    Message = "User Mobile number is required",
                    Status = "Error"
                };
            }

            try
            {
                // CREATE NEW USER
                if (userDto.UserID == 0)
                {
                    // Check duplicates
                    if (await _context.User.AnyAsync(u => u.EmailID == userDto.EmailID && u.IsActive))
                    {
                        return new UserDto
                        {
                            Message = "User Email already exists",
                            Status = "Error"
                        };
                    }

                    if (roleId == userRole?.CodeID &&
                        await _context.User.AnyAsync(u => u.MobileNo == userDto.MobileNo && u.IsActive))
                    {
                        return new UserDto
                        {
                            Message = "User Mobile number already exists",
                            Status = "Error"
                        };
                    }

                    // Create new user
                    var user = new Models.User
                    {
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        EmailID = userDto.EmailID,
                        MobileNo = userDto.MobileNo,
                        DOB = userDto.DOB,
                        Gender = userDto.Gender,
                        EmailConfirmationToken = PasswordGenerator.Generate(),
                        EmailConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),
                        MobileConfirmationToken = Generaterandomnumber(),
                        MobileConfirmationTokenExpiry = DateTime.Now.AddMinutes(60),
                        IsActive = true,
                        IsPasswordlinkShow = true,
                        RoleId = roleId,
                        AddDate = DateTime.Now
                    };

                    _context.User.Add(user);
                    await _context.SaveChangesAsync();
                    newUserID = user.UserID;

                    if (user.UserID == 0)
                    {
                        validateMsg = "User registration failed";
                    }
                    else
                    {
                        string appurl = !string.IsNullOrEmpty(user.EmailID)
                            ? GenerateWebLink(user.UserID, user.EmailConfirmationToken, "0", user.EmailID)
                            : string.Empty;

                        string mobileAppUrl = !string.IsNullOrEmpty(user.MobileNo)
                            ? GenerateWebLink(user.UserID, user.MobileConfirmationToken, "1", user.MobileNo)
                            : string.Empty;

                        await SendEail(user.EmailID, user.EmailConfirmationToken, user.MobileConfirmationToken,
                            $"{user.FirstName} {user.LastName}", appurl, mobileAppUrl);

                        validateMsg = "User registered successfully. Please check your email for confirmation link or OTP for mobile verification.";
                        status = "Success";
                    }
                }
                // UPDATE EXISTING USER
                else
                {
                    existingUser = await _context.User
                        .FirstOrDefaultAsync(u => u.UserID == userDto.UserID);

                    if (existingUser == null)
                    {
                        return new UserDto
                        {
                            Message = "User not found",
                            Status = "Error"
                        };
                    }

                    newUserID = existingUser.UserID;

                    // Duplicate checks
                    if (await _context.User.AnyAsync(u => u.EmailID == userDto.EmailID && u.UserID != existingUser.UserID))
                    {
                        return new UserDto
                        {
                            Message = "User Email already exists",
                            Status = "Error"
                        };
                    }

                    if (roleId == userRole?.CodeID &&
                        await _context.User.AnyAsync(u => u.MobileNo == userDto.MobileNo && u.UserID != existingUser.UserID))
                    {
                        return new UserDto
                        {
                            Message = "User Mobile number already exists",
                            Status = "Error"
                        };
                    }

                    // Update fields
                    existingUser.FirstName = userDto.FirstName;
                    existingUser.LastName = userDto.LastName;
                    existingUser.DOB = userDto.DOB;
                    existingUser.MobileNo = userDto.MobileNo;
                    existingUser.EmailID = userDto.EmailID; // frontend keeps readonly, backend safe
                    existingUser.ModifiedDate = DateTime.Now;
                    existingUser.Gender = userDto.Gender;
                    existingUser.RoleId = roleId;
                    existingUser.IsActive = userDto.IsActive;

                    _context.User.Update(existingUser);
                    await _context.SaveChangesAsync();

                    validateMsg = "User updated successfully";
                    status = "Success";
                }
            }
            catch (Exception ex)
            {
                validateMsg = $"Operation failed: {ex.Message}";
                status = "Error";
            }

            return new UserDto
            {
                UserID = newUserID,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Gender = userDto.Gender,
                EmailID = userDto.EmailID,
                DOB = userDto.DOB,
                MobileNo = userDto.MobileNo,
                IsActive = userDto.IsActive,
                AddDate = userDto.UserID == 0
                    ? DateTime.Now
                    : (existingUser?.AddDate ?? DateTime.Now), // no nulls, always valid
                ModifiedDate = DateTime.Now,
                Message = validateMsg,
                Status = status
            };
        }


    }

}

