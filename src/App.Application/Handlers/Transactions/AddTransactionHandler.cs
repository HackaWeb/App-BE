using App.Application.Repositories;
using App.Domain.Enums;
using App.Domain.Models;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Transactions
{
    public record AddTransactionCommand(DateTime transactionDate, decimal amount, Guid userId, TransactionType type) : IRequest<ResultResponse>;
    class AddTransactionHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddTransactionCommand, ResultResponse>
    {
        public async Task<ResultResponse> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
        {
            var userTransaction = await unitOfWork.TransactionRepository.Find(x => x.UserId == request.userId);
            var lastTransaction = userTransaction.OrderByDescending(x => x.TransactionDate).FirstOrDefault();

            var transaction = new Transaction
            {
                Amount = request.amount,
                TransactionDate = request.transactionDate,
                Balance = request.type == TransactionType.Deposit 
                    ? lastTransaction?.Balance ?? 0 + request.amount 
                    : lastTransaction?.Balance ?? 0 - request.amount,
                Type = request.type,
                UserId = request.userId,
            };

            await unitOfWork.TransactionRepository.Add(transaction);
            await unitOfWork.SaveChangesAsync();

            return new ResultResponse() { IsSuccess = true, StatusCode = 200, };
        }
    }
}
