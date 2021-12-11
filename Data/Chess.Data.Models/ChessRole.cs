// ReSharper disable VirtualMemberCallInConstructor
namespace Chess.Data.Models
{
    using System;

    using Chess.Data.Common.Models;
    using Microsoft.AspNetCore.Identity;

    public class ChessRole : IdentityRole, IAuditInfo, IDeletableEntity
    {
        public ChessRole()
            : this(null)
        {
        }

        public ChessRole(string name)
            : base(name)
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
