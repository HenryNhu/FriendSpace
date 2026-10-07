using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.Abstractions
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    }
}
