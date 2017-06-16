using System;
using MediatR;

namespace Eventus.Samples.Web.Features
{
    public abstract class BaseCommand : IRequest
    {
        protected BaseCommand()
        {
            CorrelationId = Guid.NewGuid();
        }

        public Guid CorrelationId { get; set; }
    }
}