using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;

namespace ProjectFlow.Domain.Entities
{

  

    public class WorkItemComment : Entity
    {
        public Guid WorkItemId { get; private set; }

        public Guid AuthorUserId { get; private set; }

        public string Content { get; private set; } = string.Empty;

        private WorkItemComment() { }

        public WorkItemComment(
            Guid workItemId,
            Guid authorUserId,
            string content)
        {
            if (workItemId == Guid.Empty)
                throw new ArgumentException("Work item ID is required.");

            if (authorUserId == Guid.Empty)
                throw new ArgumentException("Author ID is required.");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment cannot be empty.");

            WorkItemId = workItemId;
            AuthorUserId = authorUserId;
            Content = content.Trim();
        }

        public void Edit(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment cannot be empty.");

            Content = content.Trim();
            MarkAsUpdated();
        }
    }

}
