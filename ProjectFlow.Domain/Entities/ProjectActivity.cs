using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Domain.Entities
{

    

    public class ProjectActivity : Entity
    {
        public Guid ProjectId { get; private set; }

        public Guid ActorUserId { get; private set; }

        public Guid? WorkItemId { get; private set; }

        public ActivityType Type { get; private set; }

        public string Details { get; private set; } = string.Empty;

        private ProjectActivity() { }

        public ProjectActivity(
            Guid projectId,
            Guid actorUserId,
            ActivityType type,
            string details,
            Guid? workItemId = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID is required.");

            if (actorUserId == Guid.Empty)
                throw new ArgumentException("Actor ID is required.");

            if (!Enum.IsDefined(type))
                throw new ArgumentOutOfRangeException(nameof(type));

            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Activity details are required.");

            ProjectId = projectId;
            ActorUserId = actorUserId;
            Type = type;
            Details = details.Trim();
            WorkItemId = workItemId;
        }
    }

}
