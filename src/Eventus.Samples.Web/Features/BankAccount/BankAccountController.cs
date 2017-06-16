using System;
using System.Threading.Tasks;
using Eventus.Samples.Contracts.BankAccount;
using Eventus.Samples.Web.Features.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Eventus.Samples.Web.Features.BankAccount
{
    [Authorize]
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
        [HttpGet("bankaccount/{accountId:guid}")]
        [ProducesResponseType(typeof(BankAccountSummary), 200)]
        public async Task<IActionResult> BankAccount([FromRoute]Get.Query query)
        {
            var account = await _mediator.Send(query).ConfigureAwait(false);

            if (account == null)
            {
                return RedirectToAction(nameof(Create));
            }

            if (Request.IsForView())
            {
                return View("Details", account);
            }

            return Ok(account);
        }

        /// <summary>
        /// Create Bank Account
        /// </summary>
        /// <response code="200">Create bank account details</response>
        [HttpGet("bankaccount")]
        public async Task<IActionResult> Create()
        {
            var account = await _userManager.FindByNameAsync(User.Identity.Name)
                .ConfigureAwait(false);

            var viewModel = new Create.Command
            {
                AccountId = Guid.Parse(account.Id)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Create Bank Account
        /// </summary>
        /// <param name="baseCommand">Command to create bank account</param>
        /// <response code="303">Redirect to the bank account summary</response>
        [HttpPost("bankaccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Create.Command baseCommand)
        {
            await _mediator.Send(baseCommand)
                .ConfigureAwait(false);
            
            return RedirectToAction(nameof(BankAccount), new { accountId = baseCommand.AccountId });
        }

        /// <summary>
        /// View for depositing 
        /// </summary>
        /// <param name="accountId">The id of the bank account</param>
        /// <response code="200">View for depositing</response>
        [HttpGet("bankaccount/{accountId:guid}/deposit")]
        public async Task<IActionResult> Deposit(Guid accountId)
        {
            var account = await _mediator.Send(new Get.Query
            {
                AccountId = accountId
            }).ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return View();
        }

        /// <summary>
        /// Deposit money into Bank Account
        /// </summary>
        /// <param name="accountId">Bank Account ID</param>
        /// <param name="command">The deposit commands</param>
        /// <response code="303">Redirect to the bank account summary</response>
        [HttpPost("bankaccount/{accountId:guid}/deposit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Deposit.Command command)
        {
            await _mediator.Send(command).ConfigureAwait(false);

            //todo create a transaction status endpoint
            return RedirectToAction(nameof(BankAccount), new { accountId = command.AccountId });
        }

        /// <summary>
        /// View for withdrawing 
        /// </summary>
        /// <param name="accountId">The id of the bank account</param>
        /// <response code="200">View for withdrawal</response>
        [HttpGet("bankaccount/{accountId:guid}/withdrawal")]
        public async Task<IActionResult> Withdrawal(Guid accountId)
        {
            var account = await _mediator.Send(new Get.Query
            {
                AccountId = accountId
            }).ConfigureAwait(false);

            if (account == null)
            {
                return NotFound();
            }

            return View();
        }

        /// <summary>
        /// Withdrawal money from Bank Account
        /// </summary>
        /// <param name="accountId">Bank Account ID</param>
        /// <param name="command">The withdrawal command</param>
        /// <response code="303">Redirect to the bank account summary</response>
        [HttpPost("bankaccount/{accountId:guid}/withdrawal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(Withdrawal.Command command)
        {
            await _mediator.Send(command).ConfigureAwait(false);

            //todo create a transaction status endpoint
            return RedirectToAction(nameof(BankAccount), new { accountId = command.AccountId });
        }
    }
}