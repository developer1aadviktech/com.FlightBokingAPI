using Com.Common.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Com.Common.Utility.AllEnums;

namespace Com.AuthProvider
{
    public class IdentityAuthProvider:IIdentityAuthProvider
    {
        private UserManager<IdentityUser<int>> _userManager;
        private SignInManager<IdentityUser<int>> _signInManager;
        public IConfiguration _configuration;

        public IdentityAuthProvider(UserManager<IdentityUser<int>> userManager, SignInManager<IdentityUser<int>> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser<int> user, UserRoleEnum role)
        {
            IdentityResult result = new IdentityResult();
            try
            {
                result = await _userManager.CreateAsync(user, user.PasswordHash);
                if (result.Succeeded)
                {
                    if (role == UserRoleEnum.Admin)
                        await _userManager.AddToRoleAsync(user, UserRoleEnum.Admin.ToString());
                    else if (role != null)
                        await _userManager.AddToRoleAsync(user, role.ToString());
                    else
                        await _userManager.AddToRoleAsync(user, UserRoleEnum.User.ToString());
                }

                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }
        public async Task<SignInResult> PasswordSignInAsync(IdentityUser<int> user)
        {
            SignInResult result = new SignInResult();
            try
            {
                result = await _signInManager.PasswordSignInAsync(user.UserName, user.PasswordHash, false, false);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<IdentityUser<int>> FindByNameAsync(string userName)
        {
            IdentityUser<int> result = new IdentityUser<int>();
            try
            {
                result = await _userManager.FindByNameAsync(userName);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<IdentityUser<int>> FindByEmailAsync(string email)
        {
            IdentityUser<int> result = new IdentityUser<int>();
            try
            {
                result = await _userManager.FindByEmailAsync(email);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<IdentityUser<int>> FindByIdAsync(string id)
        {
            IdentityUser<int> result = new IdentityUser<int>();
            try
            {
                result = await _userManager.FindByIdAsync(id);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(IdentityUser<int> user, string currentPassword, string newPassword)
        {
            IdentityResult result = new IdentityResult();
            try
            {
                result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<Tuple<string, DateTime>> GetJwtToken(string userName)
        {
            string jwtToken = "";
            DateTime? expireTime = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:DurationInHours"]));

            try
            {

                IdentityUser<int> loggedinUser = await _userManager.FindByNameAsync(userName);
                IdentityUserDTO identityUserDTO = new IdentityUserDTO() { Id = loggedinUser.Id.ToString(), UserName = loggedinUser.UserName, Email = loggedinUser.Email };
                var roles = await _userManager.GetRolesAsync(loggedinUser);

                var x = _configuration["JwtSettings:SecretKey"];

                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes((_configuration["JwtSettings:SecretKey"])));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key1 = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, identityUserDTO.UserName),
                    new Claim(ClaimTypes.NameIdentifier, identityUserDTO.Id.ToString()),
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = new JwtSecurityToken(
                   _configuration["JwtSettings:Issuer"],
                   _configuration["JwtSettings:Audience"],
                   claims,
                   expires: expireTime.Value,
                   signingCredentials: credentials
               );

                jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                return Tuple.Create(jwtToken, expireTime.Value);
            }
            catch (Exception ex)
            {
                return Tuple.Create(jwtToken, expireTime.Value);
            }
        }
        public async Task<bool> SignOutAsync()
        {
            bool isSignOut = false;
            try
            {
                await _signInManager.SignOutAsync();
                isSignOut = true;
                return isSignOut;
            }
            catch (Exception ex)
            {
                return isSignOut;
            }
        }
        public async Task<IdentityResult> UpdateAsync(IdentityUser<int> user)
        {
            IdentityResult result = new IdentityResult();
            try
            {
                result = await _userManager.UpdateAsync(user);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }
        public async Task<IList<string>> GetUserRole(IdentityUser<int> user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<IdentityResult> ForgetPassword(IdentityUser<int> user, string newPassword)
        {
            IdentityResult result = new IdentityResult();
            try
            {
                await _userManager.RemovePasswordAsync(user);
                result = await _userManager.AddPasswordAsync(user, newPassword);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> GeneratePasswordResetToken(IdentityUser<int> user)
        {
            try
            {
                return await _userManager.GeneratePasswordResetTokenAsync(user);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<IdentityResult> ResetPasswordAsync(IdentityUser<int> user, string token, string newPassword)
        {
            try
            {
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
