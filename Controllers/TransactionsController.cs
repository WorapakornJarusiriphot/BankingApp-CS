using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankingApp.Data;
using BankingApp.Models;

namespace BankingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ILogger<TransactionsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("deposit")]
        public IActionResult Deposit([FromBody] TransactionModel transaction)
        {
            var user = _context.Users.Find(transaction.UserId);
            if (user == null) return NotFound("User not found");

            user.Balance += transaction.Amount;
            _context.Transactions.Add(new Transaction
            {
                UserId = user.UserId,
                Type = TransactionType.Deposit,
                Amount = transaction.Amount,
                TransactionDate = DateTime.Now
            });
            _context.SaveChanges();

            return Ok(new { Balance = user.Balance });
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferModel transferModel)
        {
            var sender = _context.Users.Find(transferModel.SenderId);
            var receiver = _context.Users.Find(transferModel.ReceiverId);

            if (sender == null || receiver == null) return NotFound("User not found");

            if (sender.Balance < transferModel.Amount)
            {
                return BadRequest("Insufficient balance.");
            }

            sender.Balance -= transferModel.Amount;
            receiver.Balance += transferModel.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = sender.UserId,
                Type = TransactionType.Transfer,
                Amount = -transferModel.Amount,
                TransactionDate = DateTime.Now
            });

            _context.Transactions.Add(new Transaction
            {
                UserId = receiver.UserId,
                Type = TransactionType.Transfer,
                Amount = transferModel.Amount,
                TransactionDate = DateTime.Now
            });

            _context.SaveChanges();

            return Ok(new { SenderBalance = sender.Balance, ReceiverBalance = receiver.Balance });
        }

        [HttpGet("history/{userId}")]
        public IActionResult TransactionHistory(int userId)
        {
            var transactions = _context.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.TransactionDate,
                    t.Type,
                    t.Amount
                })
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return Ok(transactions);
        }

        [HttpGet("balance")]
        public IActionResult GetBalance([FromQuery] int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("User not found");

            return Ok(new { Balance = user.Balance });
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] TransactionModel transaction)
        {
            var user = _context.Users.Find(transaction.UserId);
            if (user == null) return NotFound("User not found");

            if (user.Balance < transaction.Amount)
            {
                return BadRequest("Insufficient balance");
            }

            user.Balance -= transaction.Amount;
            _context.Transactions.Add(new Transaction
            {
                UserId = user.UserId,
                Type = TransactionType.Withdraw,
                Amount = transaction.Amount,
                TransactionDate = DateTime.Now
            });
            _context.SaveChanges();

            return Ok(new { Balance = user.Balance });
        }

        [HttpGet("history")]
        public IActionResult GetTransactionHistory([FromQuery] int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("User not found");

            var transactions = _context.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.TransactionDate,
                    t.Type,
                    t.Amount
                }).ToList();

            return Ok(transactions);
        }

    }
}
