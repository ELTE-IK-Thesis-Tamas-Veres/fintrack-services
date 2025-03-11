using fintrack_common.DTO.ImportDTO;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_api_business_logic.Handlers.ImportHandlers
{
    public class ImportTransactionsCommand : IRequest
    {
        public uint UserId { get; set; }
        public List<ImportTransaction> Transactions { get; set; } = new List<ImportTransaction>();
    }

    public class ImportTransactionsCommandHandler : IRequestHandler<ImportTransactionsCommand>
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ImportTransactionsCommandHandler(IRecordRepository recordRepository, ICategoryRepository categoryRepository)
        {
            _recordRepository = recordRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task Handle(ImportTransactionsCommand command, CancellationToken cancellationToken)
        {
            Dictionary<string, Category> categoryMap = GetUserCategoriesDictionary(command.UserId, cancellationToken);

            List<Category> categoriesToInsert = new List<Category>();

            foreach (ImportTransaction transaction in command.Transactions)
            {
                string? transactionCategoryName = transaction.Categories.FirstOrDefault();

                if (transactionCategoryName is not null && !categoryMap.ContainsKey(transactionCategoryName))
                {
                    Category category = new Category
                    {
                        Name = transactionCategoryName,
                        UserId = command.UserId
                    };

                    categoriesToInsert.Add(category);
                    categoryMap.Add(category.Name, category);
                }
            }

            foreach (Category categoryToInsert in categoriesToInsert)
            {
                _categoryRepository.Insert(categoryToInsert);
            }

            await _categoryRepository.SaveAsync(cancellationToken);

            if (categoriesToInsert.Count > 0)
            {
                categoryMap = GetUserCategoriesDictionary(command.UserId, cancellationToken);
            }

            foreach (ImportTransaction transaction in command.Transactions)
            {
                string? transactionCategoryName = transaction.Categories.FirstOrDefault();

                Category? category = null;

                if (transactionCategoryName is not null && categoryMap.ContainsKey(transactionCategoryName))
                {
                    category = categoryMap[transactionCategoryName];
                }

                Record record = new Record
                {
                    Amount = transaction.Amount.Value,
                    Date = DateOnly.FromDateTime((transaction.TransactionDateTime ?? transaction.Booking).DateTime),
                    Description = transaction.Note ?? "",
                    CategoryId = category?.Id,
                    UserId = command.UserId
                };
                _recordRepository.Insert(record);
            }

            await _recordRepository.SaveAsync(cancellationToken);
        }

        private Dictionary<string, Category> GetUserCategoriesDictionary(uint userId, CancellationToken cancellationToken)
        {
            List<Category> categories = _categoryRepository.GetCategoriesByUserId(userId, cancellationToken).Result;
            return categories.ToDictionary(c => c.Name, c => c);
        }
    }
}
