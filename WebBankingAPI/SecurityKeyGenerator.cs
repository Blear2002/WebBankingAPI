using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBankingAPI
{
    public class SecurityKeyGenerator
    {
        //metodo per generare una chiave complessa 
        public static SecurityKey GetSecurityKey()
        {
            var key = Encoding.ASCII.GetBytes(Startup.MasterKey);
            return new SymmetricSecurityKey(key);
        }
    }
}
