using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Domain.Enums
{

    

    public enum ActivityType
    {
        ProjectCreated = 1,
        ProjectUpdated = 2,
        WorkItemCreated = 3,
        WorkItemStatusChanged = 4,
        WorkItemPriorityChanged = 5,
        WorkItemAssigned = 6,
        CommentAdded = 7,
        WorkItemUnassigned = 8
    }

}
