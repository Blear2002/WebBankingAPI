using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace WebBankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class conti_correntiController : ControllerBase
    {
        #region 3.conti-correnti Metodo autorizzato per banchieri e correntisti

        [Authorize]
        [HttpGet("/conti-correnti")]
        public ActionResult ContiCorrenti()
        {
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");
                #endregion
                if (candidate.IsBanker) //se è bancario 
                {
                    var AllBanksAccounts = model.BankAccounts.ToList(); //prendo tutti i conti bancari

                    if (AllBanksAccounts == null) //se è null genero notFound
                        return NotFound("Problema nel trovare i conti bancari");
                    else if (AllBanksAccounts.Count == 0) //se è vuoto geneero notFound
                        return NotFound("Non ci sono attualmente conti bancari");
                    else
                    {
                        //se non si verificano le condizioni negli if allora ritorno ok
                        return Ok(AllBanksAccounts);
                    }
                }
                else if (!candidate.IsBanker) //se è false quindi correntista
                {
                    //prendo i conti bancari del correntista 
                    var UserBankAccounts = model.BankAccounts.Where(o => o.FkUser == candidate.Id).Select(o => new { o.Id, o.Iban }).ToList();

                    if (UserBankAccounts == null) //se è null genero notFound
                        return NotFound("Non hai attualmente conti bancari");
                    else
                    {
                        //se non si verificano le condizioni negli if allora ritorno ok
                        return Ok(UserBankAccounts);
                    }
                }
                else //se non è ne un bancario o correntista do problem
                    return Problem("Problema durante l'accesso alla pagina");
            }
        }

        #endregion

        #region 4.conti-correnti/ID Metodo autorizzato per banchieri e correntisti
        [Authorize]
        [HttpGet("/conti-correnti/{id}")]
        public ActionResult ContiCorrenti(int id)
        {
            if (id <= 0) return Problem("L'ID passato non è accettato, id deve essere > 0");
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");
                #endregion

                if (candidate.IsBanker)
                {
                    var contoBancarioScelto = model.BankAccounts.Where(o => o.Id == id).ToList();
                    if (contoBancarioScelto.Count == 0 || contoBancarioScelto == null)
                        return NotFound("Conto bancario non trovato");
                    else
                        return Ok(contoBancarioScelto);
                }
                else if (!candidate.IsBanker)
                {
                    var contoBancarioScelto_Utente = model.BankAccounts.Where(o => o.Id == id && o.FkUser == candidate.Id).Select(o => new { o.Id, o.Iban }).ToList();

                    if (contoBancarioScelto_Utente.Count == 0 || contoBancarioScelto_Utente == null)
                        return NotFound("Conto bancario non trovato");
                    else
                        return Ok(contoBancarioScelto_Utente);
                }
                else
                    return Problem("Problema durante l'accesso alla pagina");
            }
        }
        #endregion

        #region 5.conti-correnti/ID/Movimenti ordinati per data -- Metodo autorizzato per banchieri e correntisti
        [Authorize]
        [HttpGet("/conti-correnti/{id}/Movimenti")]
        public ActionResult Movimenti(int id)
        {
            if (id <= 0) return Problem("L'ID passato non è accettato, id deve essere > 0");
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");
                #endregion

                if (candidate.IsBanker)
                {
                    var movimento_conto = model.AccountMovements.Where(o => o.FkBankAccount == id)
                        .Select(o => new { o.Id, o.Date, o.FkBankAccount, o.In, o.Out, o.Description })
                        .OrderBy(o => o.Date).ToList();

                    if (movimento_conto.Count == 0 || movimento_conto == null) return NotFound("Non c'è stato alcun movimento per questo conto");
                    else
                    {
                        return Ok(movimento_conto);
                    }
                }
                else if (!candidate.IsBanker)
                {
                    var movimento_conto_utente = model.AccountMovements
                        .Where(o => o.FkBankAccount == id && o.FkBankAccountNavigation.FkUser == candidate.Id)
                        .Select(o => new { o.Id, o.Date, o.FkBankAccount, o.In, o.Out, o.Description })
                        .OrderBy(o => o.Date).ToList();
                    if (movimento_conto_utente.Count == 0 || movimento_conto_utente == null) return NotFound("Non c'è stato alcun movimento per questo conto");
                    else
                    {
                        return Ok(movimento_conto_utente);
                    }
                }
                else
                    return Problem("Problema durante l'accesso alla pagina");
            }
        }
        #endregion

        #region 6.conti-correnti/ID/Movimenti/id_movimento  -- Metodo autorizzato per banchieri e correntisti
        [Authorize]
        [HttpGet("/conti-correnti/{id}/Movimenti/{id_movimento}")]
        public ActionResult Movimenti(int id, int id_movimento)
        {
            if (id <= 0) return Problem("Id conto corrente non accettato, inserire un id > 0");
            if (id_movimento <= 0) return Problem("Id movimento non accettato, inserire un id > 0");
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");
                #endregion

                if (candidate.IsBanker)
                {
                    var movimento_conto = model.AccountMovements.Where(o => o.FkBankAccount == id && o.Id == id_movimento)
                        .Select(o => new { o.Id, o.Date, o.FkBankAccount, o.In, o.Out, o.Description }).ToList();

                    if (movimento_conto.Count == 0 || movimento_conto == null) return NotFound("Non è stato trovato alcun movimento per questo conto");
                    else
                    {
                        return Ok(movimento_conto);
                    }
                }
                else if (!candidate.IsBanker)
                {
                    var movimento_conto_utente = model.AccountMovements
                        .Where(o => o.FkBankAccount == id && o.FkBankAccountNavigation.FkUser == candidate.Id && o.Id == id_movimento)
                        .Select(o => new { o.Id, o.Date, o.FkBankAccount, o.In, o.Out, o.Description }).ToList();
                    if (movimento_conto_utente.Count == 0 || movimento_conto_utente == null) return NotFound("Non 'è stato trovato alcun movimento");
                    else
                    {
                        return Ok(movimento_conto_utente);
                    }
                }
                else
                    return Problem("Problema durante l'accesso alla pagina");
            }
        }
        #endregion

        #region 7.Inserimento bonifico
        [Authorize]
        [HttpPost("/conti-correnti/{id}/bonifico")]
        public ActionResult Bonifico(int id, [FromBody] Bonifico bonifico)
        {
            //controllo i dati passati 
            if (id <= 0) return Problem("Id conto mittente non disponibile o non valido");
            if (bonifico.Iban.Trim() == "" || bonifico.Iban.Trim().Length == 0) return Problem("Iban destinatario non impostato");
            if (bonifico.Importo <= 0) return Problem("Importo non valido");

            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);
                #endregion

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");


                if (candidate.IsBanker)
                {
                    var contoMittente = model.BankAccounts.Include(o => o.AccountMovements).Where(O => O.Id == id).FirstOrDefault();
                    var ibanDestinatario = bonifico.Iban;

                    //controllo se esistono
                    if (contoMittente == null)
                        return NotFound("Conto corrente mittente non trovato");
                    else
                    {
                        //qua non faccio controlli sul saldo perchè  l'utente deposita soldi per se stesso come per esempio soldi che ha ottentuto 
                        //e poi va a depositare nel suo conto
                        if (ibanDestinatario == contoMittente.Iban) //se l'iban è lo stesso del contoMittente allora eseguo un model.add su id del mittente 
                        {
                            model.AccountMovements.Add(new AccountMovement { Date = DateTime.Now, In = bonifico.Importo, Out = null, Description = bonifico.Descrizione, FkBankAccount = contoMittente.Id });
                            model.SaveChanges();
                            return Ok(bonifico);
                        }
                        else //se l'iban non è uguale al conto mittente cerco nel db altri conti cioè il destinatario
                        {
                            //check saldo con importo bonifico
                            double checkSaldoAttuale = OttieniSaldoMittente(contoMittente);
                            if (checkSaldoAttuale < bonifico.Importo) return Problem("Non hai abbasanza soldi per completare il bonifico!");

                            var contoDestinatario = model.BankAccounts.Where(o => o.Iban == ibanDestinatario).FirstOrDefault();

                            if (contoDestinatario != null) //se trovo il conto destinatario
                            {
                                model.AccountMovements.Add(new AccountMovement
                                { //movimento sia per mittente che destinatario
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoMittente.Id
                                });

                                model.AccountMovements.Add(new AccountMovement //destinatario
                                {
                                    Date = DateTime.Now,
                                    In = bonifico.Importo,
                                    Out = null,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoDestinatario.Id
                                });

                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                            else //se destinatario non è stato trovato allora eseguo solo l'add per il mittente
                            {
                                model.AccountMovements.Add(new AccountMovement
                                { //movimento  per mittente mentre destinatario di un altra banca non lo conosco qundi non lo creo
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoMittente.Id
                                });
                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                        }
                    }
                }
                else if (!candidate.IsBanker)
                {
                    var contoMittenteUser = model.BankAccounts.Include(o=>o.AccountMovements).Where(O => O.Id == id && O.FkUser == candidate.Id).FirstOrDefault();
                    var ibanDestinatario = bonifico.Iban;

                    //controllo se esistono
                    if (contoMittenteUser == null)
                        return NotFound("Conto corrente mittente non trovato");
                    else
                    {
                        
                        //qua non faccio controlli sul saldo perchè  l'utente deposita soldi per se stesso come per esempio soldi che ha ottentuto 
                        //e poi va a depositare nel suo conto
                        if (ibanDestinatario == contoMittenteUser.Iban) //se l'iban è lo stesso del contoMittente allora eseguo un model.add su id del mittente 
                        {
                            model.AccountMovements.Add(new AccountMovement { Date = DateTime.Now, In = bonifico.Importo, Out = null, Description = bonifico.Descrizione, FkBankAccount = contoMittenteUser.Id });
                            model.SaveChanges();
                            return Ok(bonifico);
                        }
                        else //se l'iban non è uguale al conto mittente cerco nel db altri conti cioè il destinatario
                        {
                            //check saldo con importo bonifico
                            double checkSaldoAttuale = OttieniSaldoMittente(contoMittenteUser);
                            if (checkSaldoAttuale < bonifico.Importo) return Problem("Non hai abbasanza soldi per completare il bonifico!");

                            var contoDestinatario = model.BankAccounts.Where(o => o.Iban == ibanDestinatario).FirstOrDefault();

                            if (contoDestinatario != null) //se trovo il conto destinatario
                            {
                                model.AccountMovements.Add(new AccountMovement
                                { //movimento sia per mittente che destinatario
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoMittenteUser.Id
                                });

                                model.AccountMovements.Add(new AccountMovement //destinatario
                                {
                                    Date = DateTime.Now,
                                    In = bonifico.Importo,
                                    Out = null,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoDestinatario.Id
                                });

                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                            else //se destinatario non è stato trovato allora eseguo solo l'add per il mittente
                            {
                                model.AccountMovements.Add(new AccountMovement
                                { //movimento  per mittente mentre destinatario di un altra banca non lo conosco qundi non lo creo
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    Description = bonifico.Descrizione,
                                    FkBankAccount = contoMittenteUser.Id
                                });
                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                        }
                    }
                }
                else //in caso di problemi
                {
                    return Problem("Errore nell'eseguire il bonifico");
                }
            }
        }
        #endregion

        #region 8./conti-correnti  Banchieri  Crea un nuovo conto corrente
        [Authorize]
        [HttpPost("/conti-correnti")]
        public ActionResult CreazioneContoCorrente([FromBody] BankAccount bankAccount)
        {
            if (bankAccount.Iban.Trim().Length == 0) return Problem("Conto corrente impostato male non favorisce i requisiti necessari");

            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);
                #endregion

                //controllo ancora se lo user c'è
                if (candidate == null) return NotFound("Impossibile trovare lo user");

                if (candidate.IsBanker)
                {
                    var checkContoEsistente = model.BankAccounts.Where(o => o.Iban == bankAccount.Iban).FirstOrDefault();
                    if (checkContoEsistente == null)
                    {
                        var checkIdUser = model.Users.Where(o => o.Id == bankAccount.FkUser).FirstOrDefault();
                        if (checkIdUser == null) return Problem("Id user non esistente");

                        model.BankAccounts.Add(new BankAccount
                        {
                            Iban = bankAccount.Iban,
                            FkUser = bankAccount.FkUser,
                        });
                        model.SaveChanges();
                        return Ok(bankAccount);
                    }
                    else
                        return Problem("Conto corrente con stesso iban già esistente!");
                }
                else
                    return Unauthorized("Accesso negato! Non hai le credenziali necessarie per entrare in questo url");
            }
        }
        #endregion

        #region 9. PUT  /conti-correnti/{id}  Banchieri  Aggiorna un conto corrente       
        [Authorize]
        [HttpPut("/conti-correnti/{id}")]
        public ActionResult ModificaContoCorrente(int id, [FromBody] BankAccount bankAccountAggiornato)
        {
            if (id <= 0) return Problem("Id non valido");
            if (bankAccountAggiornato.Iban == null) return Problem("Iban non inserito");
            if (bankAccountAggiornato.FkUser == null) return Problem("id user non valido");
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);
                #endregion

                if (candidate.IsBanker)
                {
                    var contoCandidate = model.BankAccounts.Where(o => o.Id == id).FirstOrDefault();
                    if (contoCandidate == null) return NotFound("Id contobancario da modificare non trovato");

                    var checkNomeEsistente = model.BankAccounts.Where(o => o.Iban == bankAccountAggiornato.Iban).FirstOrDefault();

                    if (checkNomeEsistente != null) return Problem("Iban già esistente");

                    var checkIdUser = model.Users.Where(o => o.Id == bankAccountAggiornato.FkUser).FirstOrDefault();
                    if (checkIdUser == null) return Problem("Id user non esistente");

                    contoCandidate.Iban = bankAccountAggiornato.Iban;
                    contoCandidate.FkUser = bankAccountAggiornato.FkUser;

                    model.SaveChanges();
                    return Ok(contoCandidate);
                }
                else
                {
                    return Unauthorized("Non sei autorizzato ad entrare in questa pagina");
                }
            }                
        }
        #endregion

        #region 10. DELETE  /conti-correnti/{id}  Banchieri  Cancella un conto corrente (e tutti i movimenti associati) 
        [HttpDelete("/conti-correnti/{id}")]
        public ActionResult EliminaConto(int id)
        {
            if (id <= 0) return Problem("Id non valido");
            #region verificaUtente e prendo da database l'intero user
            //prendo attraverso lo user e i claims i valori 
            var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

            //prendo l'intero user con model
            using (WebBankingContext model = new WebBankingContext())
            {
                //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                //tanto è un metodo in cui serve il token di autorizzazione
                User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);
                #endregion

                if (candidate.IsBanker)
                {
                    var candidateDelete = model.BankAccounts.Where(a => a.Id == id).FirstOrDefault();
                    if (candidateDelete == null) return NotFound("Conto bancario non trovato");

                    var listaMovements = model.AccountMovements.Where(o => o.FkBankAccount == candidateDelete.Id).ToList();

                    model.AccountMovements.RemoveRange(listaMovements);

                    model.BankAccounts.Remove(candidateDelete);
                    model.SaveChanges();
                    return Ok("cancellato");
                }
                else
                {
                    return Unauthorized("Non sei autorizzato ad entrare in questa pagina");
                }

            }
        }
            #endregion

        #region 11. /conti-correnti/id/saldo
            [Authorize]
            [HttpGet("/conti-correnti/{id}/saldo")]
            public ActionResult CalcolaSaldo(int id)
            {
                if (id <= 0) return Problem("Id non valido");
                #region verificaUtente e prendo da database l'intero user
                //prendo attraverso lo user e i claims i valori 
                var ID_utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
                var Name_Utente = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Username").Value;

                //prendo l'intero user con model
                using (WebBankingContext model = new WebBankingContext())
                {
                    //Ricontrollo l'untente -- lo prendo e lo modifico per il last logout, mi basta solo id e username perchè
                    //tanto è un metodo in cui serve il token di autorizzazione
                    User candidate = model.Users.FirstOrDefault(q => q.Id.ToString() == ID_utente && q.Username == Name_Utente);
                #endregion

                if (candidate.IsBanker)
                {
                    BankAccount ba = model.BankAccounts.Include(o=> o.AccountMovements).Where(o => o.Id == id).FirstOrDefault();
                    if (ba == null) return NotFound("Conto bancario non trovato");

                    var calcoloSaldo = OttieniSaldoMittente(ba);
                    var result = new { Iban = ba.Iban, Saldo = calcoloSaldo};
                    
                    return Ok(result);
                       
                    }            
                    else if (!candidate.IsBanker)
                    {
                    BankAccount ba = model.BankAccounts.Include(o => o.AccountMovements).Where(o => o.Id == id && o.FkUser == candidate.Id).FirstOrDefault();
                    if (ba == null) return NotFound("Non cè un conto bancario con id cercato");

                    var importoIn = ba.AccountMovements.Sum(o => o.In);
                    var importoOut = ba.AccountMovements.Sum(o => o.Out);

                    var calcoloSaldo = importoIn - importoOut;
                    var result = new { Iban = ba.Iban, Saldo = calcoloSaldo };

                    return Ok(result);
                    }
                    else
                        return Problem("Problema nel visualizzare il saldo");
                    }                  
            }
        #endregion

        public double OttieniSaldoMittente(BankAccount userAccount)
        {
            double saldoAttuale = 0;

            using(WebBankingContext model = new WebBankingContext())
            {
                double saldoIn = userAccount.AccountMovements.Sum(i => i.In).Value;
                double saldoOut = userAccount.AccountMovements.Sum(o => o.Out).Value;
                saldoAttuale = saldoIn + saldoOut;
            }

            return saldoAttuale;
        }
    }
}