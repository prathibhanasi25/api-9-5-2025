namespace InsuranceClaimsAPI.DTO
{
    public class UserDTO
    {
    }
    public class UserListDto
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastLoginDate { get; set; } = DateTime.MinValue; // Fixed the type mismatch issue
    }
}
