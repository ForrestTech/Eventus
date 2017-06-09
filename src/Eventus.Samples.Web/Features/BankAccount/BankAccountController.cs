using System;
using System.Threading.Tasks;
using Eventus.Samples.Core.Commands;
using Eventus.Samples.Core.ReadModel;
using Eventus.Samples.Web.Features.Account;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Samples.Web.Features.BankAccount
{
    //[Authorize] todo add identity server 4
    public class BankAccountController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public BankAccountController(IMediator mediator, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        /// <summary>
        /// Get bank account summary
        /// </summary>
        /// <param name="accountId">Bank Account ID</param>
        /// <response code="200">Returns the bank account summary</response>
        [HttpGet("api/bankaccount/{accountId:guid}")]
        [ProducesResponseType(typeof(BankAccountSummary), 200)]
        public async Task<IActionResult> GetAccountAsync(Guid accountId)
        {
            var account = await _mediator.Send(new Get.Query
            {
                Id = accountId
            }).ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return new ObjectResult(account);
        }

        /// <summary>
        /// Gety Bank Account Summary
        /// </summary>
        /// <response code="200">Screen for bank account</response>
        [HttpGet("bankaccount/{accountId:guid}")]
        public Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Create Bank Account
        /// </summary>
        /// <response code="200">Create bank account details</response>
        [HttpGet("bankaccount")]
        public Task<IActionResult> Create()
        {
            return View();
        }
      
        /// <summary>
        /// Create Bank Account
        /// </summary>
        /// <param name="command">Command to create bank account</param>
        /// <response code="303">Redirect to the bank account summary</response>
        [HttpPost("api/bankaccount")]
        public async Task<IActionResult> Create(CreateAccountViewModel viewModel)
        {
            var account = await _userManager.FindByNameAsync(User.Identity.Name);

            var command = new CreateAccountCommand(Guid.NewGuid(), account.Id, viewModel.AccountName);

            await _mediator.Send(command).ConfigureAwait(false);

            //todo create a transaction status endpoint
            return RedirectToAction(nameof(GetAccountAsync), new { accountId = command.AggregateId });
        }

        /// <summary>
        /// Deposit money into Bank Account
        /// </summary>
        /// <param name="accountId">Bank Account ID</param>
        /// <response code="303">Returns the bank account summary</response>
        [HttpPost("api/bankaccount/{accountId:guid}/deposit")]
        public async Task<IActionResult> DepositAsync(Guid accountId, DepositFundsCommand command)
        {
            await _mediator.Send(command).ConfigureAwait(false);

            //todo create a transaction status endpoint
            return RedirectToAction(nameof(GetAccountAsync), new { accountId = command.AggregateId });
        }
    }
}