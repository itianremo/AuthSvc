using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Application.Services
{
    public class UserRoleBiz : IUserRoleBiz
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRoleBiz(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Retrieval
        public async Task<IEnumerable<UserRoleDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetWithRolesAsync(userId, cancellationToken);
            if (user == null) return Enumerable.Empty<UserRoleDto>();

            return user.Roles.Select(r => new UserRoleDto
            {
                UserId = user.UserId,
                UserEmail = user.Email,
                RoleId = r.RoleId,
                RoleName = r.RoleName
            });
        }

        public async Task<IEnumerable<UserRoleDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _unitOfWork.Roles.GetWithUsersAsync(roleId, cancellationToken);
            if (role == null) return Enumerable.Empty<UserRoleDto>();

            return role.Users.Select(u => new UserRoleDto
            {
                UserId = u.UserId,
                UserEmail = u.Email,
                RoleId = role.RoleId,
                RoleName = role.RoleName
            });
        }

        // Single assignment
        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetWithRolesAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId, cancellationToken)
                ?? throw new InvalidOperationException("Role not found.");

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be manually assigned.");

            if (user.Roles.Any(r => r.RoleId == roleId))
                return true;

            user.Roles.Add(role);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Single unassignment
        public async Task<bool> UnassignRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetWithRolesAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            var role = user.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null) return false;

            if (role.IsSystemDefined)
                throw new InvalidOperationException("System-defined roles cannot be unassigned.");

            user.Roles.Remove(role);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Batch assignment
        public async Task<bool> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            foreach (var roleId in roleIds)
            {
                await AssignRoleToUserAsync(userId, roleId, cancellationToken);
            }
            return true;
        }

        // Batch unassignment
        public async Task<bool> UnassignRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            foreach (var roleId in roleIds)
            {
                await UnassignRoleFromUserAsync(userId, roleId, cancellationToken);
            }
            return true;
        }
    }
}
