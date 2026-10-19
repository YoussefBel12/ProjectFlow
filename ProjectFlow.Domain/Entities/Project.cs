using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;

namespace ProjectFlow.Domain.Entities
{






    public class Project : Entity
    {
        private readonly List<WorkItem> _workItems = [];

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public DateTime? DueDateUtc { get; private set; }

        public IReadOnlyCollection<WorkItem> WorkItems => _workItems.AsReadOnly();

        private Project() { }

        public Project(
            string name,
            string? description = null,
            DateTime? dueDateUtc = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name is required.", nameof(name));

            Name = name.Trim();
            Description = description;
            DueDateUtc = dueDateUtc;
        }

        public WorkItem AddWorkItem(
            string title,
            string? description = null,
            DateTime? dueDateUtc = null)
        {
            var workItem = new WorkItem(
                Id,
                title,
                description,
                dueDateUtc);

            _workItems.Add(workItem);

            MarkAsUpdated();

            return workItem;
        }

        public void UpdateDetails(
            string name,
            string? description,
            DateTime? dueDateUtc)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name is required.", nameof(name));

            Name = name.Trim();
            Description = description;
            DueDateUtc = dueDateUtc;

            MarkAsUpdated();
        }
    }
}
