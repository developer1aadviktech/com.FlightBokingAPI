using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Com.Common.Utility.AllEnums;

namespace Com.AuthProvider
{
    public interface IIdentityAuthProvider
    {
        Task<IdentityResult> CreateAsync(IdentityUser<int> user, UserRoleEnum role);

        Task<SignInResult> PasswordSignInAsync(IdentityUser<int> user);

        Task<IdentityUser<int>> FindByNameAsync(string userName);

        Task<IdentityUser<int>> FindByEmailAsync(string email);

        Task<IdentityUser<int>> FindByIdAsync(string id);

        Task<IdentityResult> ChangePasswordAsync(IdentityUser<int> user, string currentPassword, string newPassword);
        Task<bool> SignOutAsync();
        Task<IdentityResult> UpdateAsync(IdentityUser<int> user);
        Task<Tuple<string, DateTime>> GetJwtToken(string userNme);
        Task<IList<string>> GetUserRole(IdentityUser<int> user);
        Task<IdentityResult> ForgetPassword(IdentityUser<int> user, string newPassword);
        Task<string> GeneratePasswordResetToken(IdentityUser<int> user);
        Task<IdentityResult> ResetPasswordAsync(IdentityUser<int> user, string token, string newPassword);
    }
}
