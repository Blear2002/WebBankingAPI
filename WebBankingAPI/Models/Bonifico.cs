using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebBankingAPI.Models
{
    public class Bonifico
    {
        public string Iban { get; set; }
        public float Importo { get; set; }

        public string Descrizione { get; set; }
    
    }
}
