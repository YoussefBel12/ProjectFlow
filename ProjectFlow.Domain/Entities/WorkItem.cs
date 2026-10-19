using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Domain.Entities
{



   

    public class WorkItem : Entity
    {
        public Guid ProjectId { get; private set; }

        public string Title { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public WorkItemStatus Status { get; private set; } = WorkItemStatus.Todo;

        public WorkItemPriority Priority { get; private set; } =
            WorkItemPriority.Medium;

        public DateTime? DueDateUtc { get; private set; }

        private WorkItem() { }

        public WorkItem(
            Guid projectId,
            string title,
            string? description = null,
            DateTime? dueDateUtc = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID is required.", nameof(projectId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required.", nameof(title));

            ProjectId = projectId;
            Title = title.Trim();
            Description = description;
            DueDateUtc = dueDateUtc;
        }

        public void ChangeStatus(WorkItemStatus status)
        {
            if (!Enum.IsDefined(status))
                throw new ArgumentOutOfRangeException(nameof(status));

            Status = status;
            MarkAsUpdated();
        }

        public void ChangePriority(WorkItemPriority priority)
        {
            if (!Enum.IsDefined(priority))
                throw new ArgumentOutOfRangeException(nameof(priority));

            Priority = priority;
            MarkAsUpdated();
        }

        public void UpdateDetails(
            string title,
            string? description,
            DateTime? dueDateUtc)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Task title is required.", nameof(title));

            Title = title.Trim();
            Description = description;
            DueDateUtc = dueDateUtc;

            MarkAsUpdated();
        }
    }
}