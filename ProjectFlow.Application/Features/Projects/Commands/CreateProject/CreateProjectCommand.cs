using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ProjectFlow.Application.Features.Projects.Commands.CreateProject
{
 

    public record CreateProjectCommand(
        string Name,
        string? Description,
        DateTime? DueDateUtc
    ) : IRequest<Guid>;
}
