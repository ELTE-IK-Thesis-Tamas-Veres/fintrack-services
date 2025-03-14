using fintrack_api_business_logic.Handlers.CategoryHandlers;
using fintrack_common.Exceptions;
using fintrack_common.Repositories;
using fintrack_database.Entities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Record = fintrack_database.Entities.Record;

namespace fintrack_api_unit_tests.CommandHandler
{
    public class DeleteCategoryTest
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRecordRepository _recordRepository;

        public DeleteCategoryTest()
        {
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _recordRepository = Substitute.For<IRecordRepository>();
        }

        public DeleteCategoryCommandHandler GetCommandHandler()
        {
            return new DeleteCategoryCommandHandler(_categoryRepository, _recordRepository);
        }

        [Fact]
        public async Task HandlerShouldDeleteCategory ()
        {
            // Arrange
            _categoryRepository.GetCategoryByIdWithChildCategories(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new Category ()
            {
                Id = 1,
                UserId = 1,
                ParentCategory = new Category
                {
                    Id = 2
                },
                ChildCategories = new List<Category>
                {
                    new Category
                    {
                        Id = 3
                    }
                }
            });
            _recordRepository.GetRecordsByCategoryId(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new List<Record>
            {
                new Record
                {
                    Id = 1,
                    CategoryId = 1
                }
            });
            DeleteCategoryCommand command = new DeleteCategoryCommand
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act
            await handler.Handle(command, new CancellationToken());
            // Assert
            _recordRepository.Received(1).Update(Arg.Is<Record>(x => x.Category != null && x.Category.Id == 2));
            await _recordRepository.Received(1).SaveAsync(Arg.Any<CancellationToken>());
            _categoryRepository.Received(1).Update(Arg.Is<Category>(x => x.Id == 3));
            _categoryRepository.Received(1).Delete(Arg.Is<Category>(x => x.Id == 1));
        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundException()
        {
            // Arrange
            _categoryRepository.GetCategoryByIdWithChildCategories(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
            DeleteCategoryCommand command = new DeleteCategoryCommand
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act
            // Assert
            await Assert.ThrowsAsync<RecordNotFoundException>(() => handler.Handle(command, new CancellationToken()));
        }

        [Fact]
        public async Task HandlerShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            _categoryRepository.GetCategoryByIdWithChildCategories(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new Category
            {
                Id = 1,
                UserId = 2
            });
            DeleteCategoryCommand command = new DeleteCategoryCommand
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act
            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, new CancellationToken()));
        }
    }
}
