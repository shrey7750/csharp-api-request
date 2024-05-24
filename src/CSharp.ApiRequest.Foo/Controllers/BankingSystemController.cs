using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingSystem.Controllers
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
    }

    public class Account
    {
        public int AccountId { get; set; }
        public double Balance { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly List<User> _users = new List<User>();
        private int _userIdCounter = 1;

        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            if (user == null) {
                return BadRequest("User details are required, please provide valid user details");
            }

            user.UserId = _userIdCounter++;
            _users.Add(user);
            return Ok(user);
        }

        [HttpGet("{userId}")]
        public IActionResult GetUser(int userId)
        {
            if (userId == null || userId <= 0) {
                return BadRequest("User Id can not be null, please provide valid user ID");}

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found with this UserId:");

            return Ok(user);
        }
    }
   

    [Route("api/users/{userId}/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly List<User> _users;

        public AccountsController(List<User> users)
        {
            _users = users;
        }

        [HttpPost]
        public IActionResult CreateAccount(int userId, [FromBody] Account account)
        {
            if (account == null || userId == null || userId <= 0) {
                return BadRequest("Account details/valid user ID are required.");
           }

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            if (account.Balance < 100)
                return BadRequest("Account balance must be at least $100.");

            account.AccountId = user.Accounts.Count + 1;
            user.Accounts.Add(account);
            return Ok(account);
        }

        [HttpDelete("{accountId}")]
        public IActionResult DeleteAccount(int userId, int accountId)
        {
           if (accountId == null || userId == null || userId <= 0) {
                return BadRequest("Account ID/valid user ID are required.");
         }

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var account = user.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
                return NotFound();

            user.Accounts.Remove(account);
            return NoContent();
        }

        [HttpPost("{accountId}/deposit")]
        public IActionResult Deposit(int userId, int accountId, [FromBody] double depositAmount)
        {
        if (accountId == null || accountId <= 0 || userId == null || userId <= 0) {
                return BadRequest("Account ID/valid user ID are required.");
         }

         if (depositAmount <= 0) {
                return BadRequest("Deposit amount must be greater than zero.");
           }
         if (depositAmount > 10000) {
                return BadRequest("Deposit amount cannot exceed $10,000.");
           }


            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var account = user.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
                return NotFound();      

            account.Balance += depositAmount;
            return Ok(new { accountId, newBalance = account.Balance });
        }

        [HttpPost("{accountId}/withdraw")]
        public IActionResult Withdraw(int userId, int accountId, [FromBody] double amount)
        {
         if (accountId == null || accountId <= 0 || userId == null || userId <= 0 ) {
                return BadRequest("Account ID/valid user ID are required.");
         }
           if (amount <= 0) {
                return BadRequest("Withdrawal amount must be greater than zero.");
           }

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var account = user.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
                return NotFound();

            if (amount > account.Balance)
                return BadRequest("Insufficient funds.");

            double maxWithdrawAmount = account.Balance * 0.90;
            if (amount > maxWithdrawAmount)
                return BadRequest($"Withdrawal amount exceeds 90% of the total balance. Maximum withdrawal amount: {maxWithdrawAmount}");

            account.Balance -= amount;  //subtract the withdrawn amount from main balance
            return Ok(new { accountId, newBalance = account.Balance });
        }
    }
}
