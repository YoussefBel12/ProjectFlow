using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectFlow.Domain.Common;

namespace ProjectFlow.Domain.Entities
{

 

    public class Notification : Entity
    {
        public Guid RecipientUserId { get; private set; }

        public string Title { get; private set; } = string.Empty;

        public string Message { get; private set; } = string.Empty;

        public DateTime? ReadAtUtc { get; private set; }

        private Notification() { }

        public Notification(
            Guid recipientUserId,
            string title,
            string message)
        {
            if (recipientUserId == Guid.Empty)
                throw new ArgumentException("Recipient ID is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Notification title is required.");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message is required.");

            RecipientUserId = recipientUserId;
            Title = title.Trim();
            Message = message.Trim();
        }

        public void MarkAsRead()
        {
            if (ReadAtUtc.HasValue)
                return;

            ReadAtUtc = DateTime.UtcNow;
            MarkAsUpdated();
        }
    }

}
