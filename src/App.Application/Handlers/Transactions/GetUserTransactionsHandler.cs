using App.Application.Repositories;
using App.Domain.Models;
using MediatR;

namespace App.Application.Handlers.Transactions
{
    public record GetUserTransactionsCommand(Guid userId) : IRequest<List<Transaction>>;
    public record GetAllTransactionsCommand() : IRequest<List<Transaction>>;

    public class GetUserTransactionsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUserTransactionsCommand, List<Transaction>>
    {
        public async Task<List<Transaction>> Handle(GetUserTransactionsCommand request, CancellationToken cancellationToken)
        {
            var transactions = await unitOfWork.TransactionRepository.Find(x => x.UserId == request.userId);

            return transactions.OrderByDescending(x => x.TransactionDate).ToList();
        }
    }

    public class GetAllTransactionsHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllTransactionsCommand, List<Transaction>>
    {
        public async Task<List<Transaction>> Handle(GetAllTransactionsCommand request, CancellationToken cancellationToken)
        {
            var transactions = await unitOfWork.TransactionRepository.GetAll();

            return transactions.OrderByDescending(x => x.TransactionDate).ToList();
        }
    }
}
