using BobikServer.Data;
using BobikServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BobikServer.Controllers
{

    [ApiController]
    [Route("api/gameserver")]
    public class GameServerController : ControllerBase
    {
        private readonly LiteDatabase _database;

        public GameServerController(LiteDatabase database)
        {
            _database = database;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> CheckConnectTokenAsync([FromBody] ConnectRequest request)
        {
            try
            {
                ConnectGameserverToken connectToken = await _database.ConnectGameserverTokens.Include(t => t.AccountProfile)
                .ThenInclude(a => a.GameProfiles)
                .FirstOrDefaultAsync(x => x.Token == request.ClientToken);

                if (connectToken == null || connectToken.ExpiresAt < DateTime.UtcNow)
                {
                    return BadRequest();
                }

                GameProfile gameProfile = connectToken.AccountProfile.GameProfiles.First();

                string nickname = gameProfile.Nickname;
                int coins = gameProfile.Coins;

                return Ok(new { nickname = nickname, coins = coins });
            }
            catch
            {
                return StatusCode(500);
            }
        }


    }
}
