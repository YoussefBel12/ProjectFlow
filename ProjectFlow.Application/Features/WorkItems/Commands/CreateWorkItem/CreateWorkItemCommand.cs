using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ProjectFlow.Application.Features.WorkItems.Commands.CreateWorkItem
{

    

    public record CreateWorkItemCommand(
        Guid ProjectId,
        string Title,
        string? Description,
        DateTime? DueDateUtc) : IRequest<Guid>;

}
