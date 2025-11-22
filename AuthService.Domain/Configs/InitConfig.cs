namespace AuthService.Domain.Configs
{
    public class InitPermissions
    {
        public string SuperAccess { get; set; }
        public string ManageApps { get; set; }
        public string ManageAssign { get; set; }
        public string ManageUsers { get; set; }
        public string ManageRoles { get; set; }
        public string ManagePermissions { get; set; }
    }

    public class InitSeeds
    {
        public Guid AppId { get; set; }
        public string AppName { get; set; }
        public string AppRedirectUrls { get; set; }
        public string AppScopes { get; set; }

        public Guid RoleId { get; set; }
        public string RoleName { get; set; }

        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public string UserPasswordHash { get; set; }
        public AppAccountStatus UserGlobalAccountStatus { get; set; }
        public bool UserIsEmailVerified { get; set; }
        public bool UserIsPhoneVerified { get; set; }
        public DateTime UserCreatedAt { get; set; }

        public Guid AdminUserAppId { get; set; }
        public Guid AdminUserRoleId { get; set; }

        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }

        public Guid RolePermissionId { get; set; }
    }

    public class InitConfig
    {
        public InitPermissions Permissions { get; set; }
        public InitSeeds Seeds { get; set; }
    }

}
