using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBank.Data;
using OnlineBank.DTOs;
using OnlineBank.Models;
using System.Security.Claims;

namespace OnlineBank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class TransactionController : Controller
    {
        private AppDbContext _context;

        public TransactionController(AppDbContext context) 
        {
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMoney([FromBody] SendMoneyDto model)
        {
            var senderBankAccount = User.FindFirstValue("BankAccountNumber");

            if(senderBankAccount == null)
                return Unauthorized("Bank account number not found");

            var sender = await  _context.Users.FirstOrDefaultAsync(u => u.BankAccountNumber == senderBankAccount);
            if (sender == null)
                return NotFound("Sender not found");
            var recipient = await _context.Users.FirstOrDefaultAsync(u =>
            u.BankAccountNumber == model.RecipientBankAccountNumber &&
            (u.Name + " " + u.Surname) == model.RecipientFullName);

            if(recipient == null)
                return NotFound("Recipient not found.");

            if (model.Amount <= 0)
                return BadRequest("Amount must be greater than 0.");

            if (sender.Balance < model.Amount)
                return BadRequest("Insufficient balance.");

            sender.Balance -= model.Amount;
            recipient.Balance += model.Amount;

            var transaction = new Transaction
            {
                SenderBankAccountNumber = sender.BankAccountNumber,
                RecipientBankAccountNumber = recipient.BankAccountNumber,
                RecipientFullName = model.RecipientFullName,
                Description = model.Description,
                Amount = model.Amount,
                Timestamp = DateTime.UtcNow 
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok("Transaction successful.");
        }

        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var BankAccountNumber = User.FindFirstValue("BankAccountNumber");
            if (BankAccountNumber == null)
                return Unauthorized("Bank account number not found.");

            var transactions = await _context.Transactions
                .Where(t => t.SenderBankAccountNumber == BankAccountNumber || t.RecipientBankAccountNumber == BankAccountNumber)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
