using BobikServer.Data;
using BobikServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BobikServer.Controllers
{

    [ApiController]
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {

        private readonly LiteDatabase _database;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthorizationController(LiteDatabase database, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _database = database;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            ValidateResult loginValidation = LoginIsValid(request.Login);

            if (loginValidation != ValidateResult.Success)
            {
                return BadRequest(new {                 
                    error = RegisterResult.InvalidLogin.ToString(),
                    detail = loginValidation.ToString()
                });
            }

            ValidateResult nicknameValidation = NicknameIsValid(request.Nickname);

            if (nicknameValidation != ValidateResult.Success)
            {
                return BadRequest(new
                {
                    error = RegisterResult.InvalidNickname.ToString(),
                    detail = nicknameValidation.ToString()
                });
            }

            IdentityUser user = new IdentityUser
            {
                UserName = request.Login
            };

            using var transaction = await _database.Database.BeginTransactionAsync();

            try
            {
                IdentityResult result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        error = result.Errors.First().Code
                    });
                }

                AccountProfile createdAccount = new AccountProfile()
                {
                    IdentityId = user.Id
                };
                
                GameProfile gameProfile = new GameProfile
                {
                    AccountProfile = createdAccount,
                    Nickname = request.Nickname
                };
                
                _database.Accounts.Add(createdAccount);
                _database.GameProfiles.Add(gameProfile);

                await _database.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = RegisterResult.Success.ToString() });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = RegisterResult.ServerError.ToString() });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {

                LoginToken loginEnterToken = await GetAliveLoginToken(request.LoginToken);

                if (loginEnterToken != null)
                {
                    string connectToken = await CreateConnectGameserverToken(loginEnterToken.AccountId);
                    return Ok(new { message = LoginResult.Success.ToString(), loginToken = "", connectToken = connectToken});
                }
                else
                {
                    IdentityUser user = await _userManager.FindByNameAsync(request.Login);

                    if (user == null)
                    {
                        return BadRequest(new { error = LoginResult.Deny.ToString() });
                    }

                    var passwordSignInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

                    if (!passwordSignInResult.Succeeded)
                    {
                        return BadRequest(new { error = LoginResult.Deny.ToString() });
                    }

                    if (request.CreateToken)
                    {
                        AccountProfile accountProfile = await _database.Accounts.FirstAsync(x => x.IdentityId == user.Id);
                        string authToken = await CreateLoginToken(accountProfile.Id);
                        string connectToken = await CreateConnectGameserverToken(accountProfile.Id);

                        return Ok(new { message = LoginResult.Success.ToString(), loginToken = authToken, connectToken = connectToken});
                    }

                    return Ok(new { message = LoginResult.Success.ToString(), loginToken = "", connectToken = "" });
                }
            }
            catch
            {
                return StatusCode(500, new { error = LoginResult.ServerError.ToString() });
            }
        }

        #region LongLifeToken

        private async Task<LoginToken> GetAliveLoginToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                LoginToken tokenEntity = await _database.LoginTokens.FirstOrDefaultAsync(x => x.Token == token);
                
                if (tokenEntity != null && tokenEntity.ExpiresAt > DateTime.UtcNow)
                {
                    return tokenEntity;
                }
            }
                
            return null;
        }

        private async Task<string> CreateLoginToken(int accountId)
        {
            var tokenValue = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

            var token = new LoginToken
            {
                AccountId = accountId,
                Token = tokenValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(10)
            };

            _database.LoginTokens.Add(token);
            await _database.SaveChangesAsync();

            return tokenValue;
        }

        private async Task<string> CreateConnectGameserverToken(int accountId)
        {
            var tokenValue = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

            var token = new ConnectGameserverToken
            {
                AccountId = accountId,
                Token = tokenValue,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2)
            };

            _database.ConnectGameserverTokens.Add(token);
            await _database.SaveChangesAsync();

            return tokenValue;
        }

        #endregion

        private ValidateResult LoginIsValid(string login)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login))
                    return ValidateResult.Empty;

                if (login.Length < 6)
                    return ValidateResult.TooShort;

                if (login.Length > 14)
                    return ValidateResult.TooLong;

                return ValidateResult.Success; 
            }
            catch
            {
                return ValidateResult.Unknown; 
            }
        }
 
        private ValidateResult NicknameIsValid(string nickname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nickname))
                    return ValidateResult.Empty;

                if (nickname.Length < 3)
                    return ValidateResult.TooShort;

                if (nickname.Length > 14)
                    return ValidateResult.TooLong;

                if (!nickname.All(char.IsLetterOrDigit))
                    return ValidateResult.InvalidCharacters;

                return ValidateResult.Success;
            }
            catch
            {
                return ValidateResult.Unknown;
            }
        }

        public enum LoginResult
        {
           Success,
           Deny,
           ServerError
        }

        public enum RegisterResult
        {
            Success,
            LoginExists,
            InvalidLogin,
            InvalidPassword,
            InvalidNickname,
            ServerError
        }

        private enum ValidateResult
        {
            Success,
            Empty,
            TooShort,
            TooLong,
            InvalidCharacters,
            Unknown
        }
    }
}
