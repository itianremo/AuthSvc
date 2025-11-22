using System;

namespace AuthService.Application.DTOs
{
    // === UserController DTOs ===

    public class UserListDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserBasicInfoDto
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        //public bool IsEmailVerified { get; set; }
        //public bool IsPhoneVerified { get; set; }
    }

    public class VerifyEmailDto
    {
        public Guid UserId { get; set; }
        public string OtpToken { get; set; } = string.Empty;
    }

    public class VerifyPhoneDto
    {
        public Guid UserId { get; set; }
        public string OtpToken { get; set; } = string.Empty;
    }

    public class UpdateStatusDto
    {
        public string StatusValue { get; set; } = string.Empty;
    }
}
