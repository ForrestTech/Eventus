using System;
using System.Threading.Tasks;
using Eventus.Samples.Web.Features.BankAccount.DTO;
using Eventus.Samples.Web.Features.BankAccount.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Samples.Web.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly IMediator _mediator;

        public BankAccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get bank account summary
        /// </summary>
        /// <param name="id">Bank Account ID</param>
        /// <response code="200">Returns the bank account summary</response>
        [HttpGet("api/bankaccount/{id:guid}")]
        [ProducesResponseType(typeof(BankAccountSummary), 200)]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            var account = await _mediator.Send(new GetBankAccountQuery
            {
                Id = id
            }).ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return new ObjectResult(account);
        }
    }
}