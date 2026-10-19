using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;

namespace ProjectFlow.Domain.Entities
{

    

    public class ProjectAssignment : Entity
    {
        public Guid WorkItemId { get; private set; }

        public Guid UserId { get; private set; }

        private ProjectAssignment() { }

        public ProjectAssignment(Guid workItemId, Guid userId)
        {
            if (workItemId == Guid.Empty)
                throw new ArgumentException("Work item ID is required.");

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required.");

            WorkItemId = workItemId;
            UserId = userId;
        }
    }

}
