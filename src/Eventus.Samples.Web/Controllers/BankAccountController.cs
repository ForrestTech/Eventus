using System;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Samples.Web.Controllers
{
    public class BankAccountController : Controller
    {
        /// <summary>
        /// Get bank account summary
        /// </summary>
        /// <param name="id">Bank Account ID</param>
        /// <response code="200">Returns the bank account summary</response>
        [HttpGet("/bankaccount/{id:guid}")]
        [ProducesResponseType(typeof(BankAccountSummary), 200)]
        public IActionResult GetAccount(Guid id)
        {
            return new ObjectResult(new BankAccountSummary
            {
                Id = id,
                AccountName = "Joe",
                CurrentBalance = 100
            });
        }
    }

    public class BankAccountSummary
    {
        public Guid Id { get; set; }

        public string AccountName { get; set; }

        public int CurrentBalance { get; set; }
    }
}