using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebBankingAPI.Models;

namespace WebBankingAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        //Metodo Login in cui vado a controllare i dati inseriti from body e verifico se esiste o no
        #region Login
        [HttpPost("/Login")]
        public ActionResult Login([FromBody] User credentials) 
        {
            if (credentials.Username.Trim().Length == 0 || credentials.Username.Trim() == "") return Problem("Username non inserito correttamente");
            if(credentials.Password.Trim().Length == 0 || credentials.Password.Trim() == "") return Problem("Password non inserita correttamente");

            using (WebBankingContext model = new WebBankingContext())
            {
                User candidate = model.Users.FirstOrDefault(q => q.Username == credentials.Username && q.Password == credentials.Password);
                if (candidate == null) return NotFound("Username o password errati"); //in caso di login username e pass non coincidono allora faccio NotFound()

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor //Descrive come è fatto il token
                {
                    SigningCredentials = new SigningCredentials(SecurityKeyGenerator.GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature),
                    Expires = DateTime.UtcNow.AddDays(1), //Scade domani
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                        //ci passo i dati che mi servono
                        new Claim("Id", candidate.Id.ToString()),
                        new Claim("Username", candidate.Username),
                        new Claim("Is_Banker",candidate.IsBanker.ToString())
                    })
                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); //creiamo il token descrittoe mi restituisce un oggetto di classe SecurityToken
                candidate.LastLogin = DateTime.Now;
                model.SaveChanges();
                return Ok(tokenHandler.WriteToken(token)); //ritorna una stringa 
            }
        }
        #endregion

        //Metodo Logout in cui parte solo se lo user è autorizzato e vado ad apportare alcune modifiche in caso positivo
        #region Logout
        [Authorize]
        [HttpPost("/Logout")]
        public ActionResult Logout()
        {
            //così facendo prendo il primo valore della lista in cui ha come tipo id-cioè username
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);

                if (candidate == null) return NotFound("Impossibile trovare lo user");

                //Prendo lo user e imposto lastLogOut con data e ora attuale e salvo
                candidate.LastLogout = DateTime.Now;
                model.SaveChanges();

                return Ok();
            }
        }
        #endregion
    }
}
