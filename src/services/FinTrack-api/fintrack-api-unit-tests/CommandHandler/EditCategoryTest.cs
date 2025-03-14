using fintrack_api_business_logic.Handlers.CategoryHandlers;
using fintrack_common.DTO.CategoryDTO;
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

namespace fintrack_api_unit_tests.CommandHandler
{
    public class EditCategoryTest
    {
        private readonly ICategoryRepository _categoryRepository;

        public EditCategoryTest()
        {
            _categoryRepository = Substitute.For<ICategoryRepository>();
        }

        private EditCategoryCommandHandler GetCommandHandler ()
        {
            return new EditCategoryCommandHandler(_categoryRepository);
        }

        [Fact]
        public async Task HandlerShouldEditCategory()
        {
            // Arrange

            _categoryRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new Category
            {
                Id = 1,
                UserId = 1,
                Name = "oldName"
            });

            EditCategoryRequest request = new EditCategoryRequest
            {
                Name = "newName",
            };

            EditCategoryCommand command = new EditCategoryCommand (request)
            {
                CategoryId = 1,
                UserId = 1
            };

            var handler = GetCommandHandler();

            // Act
            await handler.Handle(command, new CancellationToken());

            // Assert
            _categoryRepository.Received(1).Update(Arg.Is<Category>(x => x.Name == "newName"));
        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundException()
        {
            // Arrange
            _categoryRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
            EditCategoryRequest request = new EditCategoryRequest
            {
                Name = "newName",
            };
            EditCategoryCommand command = new EditCategoryCommand(request)
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act & Assert
            await Assert.ThrowsAsync<RecordNotFoundException>(() => handler.Handle(command, new CancellationToken()));
        }

        [Fact]
        public async Task HandlerShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            _categoryRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new Category
            {
                Id = 1,
                UserId = 2,
                Name = "oldName"
            });
            EditCategoryRequest request = new EditCategoryRequest
            {
                Name = "newName",
            };
            EditCategoryCommand command = new EditCategoryCommand(request)
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, new CancellationToken()));
        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundExceptionWhenCategoryIsNull()
        {
            // Arrange
            _categoryRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
            EditCategoryRequest request = new EditCategoryRequest
            {
                Name = "newName",
            };
            EditCategoryCommand command = new EditCategoryCommand(request)
            {
                CategoryId = 1,
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act & Assert
            await Assert.ThrowsAsync<RecordNotFoundException>(() => handler.Handle(command, new CancellationToken()));
        }
    }
}
