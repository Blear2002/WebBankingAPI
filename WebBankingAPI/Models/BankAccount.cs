using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable

namespace WebBankingAPI.Models
{
    public partial class BankAccount
    {
        public BankAccount()
        {
            AccountMovements = new HashSet<AccountMovement>();
        }

        public int Id { get; set; }
        public string Iban { get; set; }
        public int? FkUser { get; set; }
        
        [JsonIgnore]
        public virtual User FkUserNavigation { get; set; }
        [JsonIgnore]
        public virtual ICollection<AccountMovement> AccountMovements { get; set; }
    }
}
