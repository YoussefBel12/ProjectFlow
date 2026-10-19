using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Domain.Entities
{

    

    public class ProjectMember : Entity
    {
        public Guid ProjectId { get; private set; }

        public Guid UserId { get; private set; }

        public ProjectRole Role { get; private set; }

        private ProjectMember() { }

        public ProjectMember(Guid projectId, Guid userId, ProjectRole role)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID is required.");

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            ProjectId = projectId;
            UserId = userId;
            Role = role;
        }

        public void ChangeRole(ProjectRole role)
        {
            if (!Enum.IsDefined(role))
                throw new ArgumentOutOfRangeException(nameof(role));

            Role = role;
            MarkAsUpdated();
        }
    }

}
