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
    public class EditParentOfCategoriesTest
    {
        private readonly ICategoryRepository _categoryRepository;

        public EditParentOfCategoriesTest()
        {
            _categoryRepository = Substitute.For<ICategoryRepository>();
        }

        public EditParentOfCategoriesCommandHandler GetCommandHandler()
        {
            return new EditParentOfCategoriesCommandHandler(_categoryRepository);
        }

        [Fact]
        public async Task HandlerShouldEditParentOfCategories()
        {
            // Arrange
            _categoryRepository
                .GetCategoriesByCategoryIdList(Arg.Any<List<uint>>(), Arg.Any<CancellationToken>())
                .Returns(new List<Category>()
                {
                    new Category () {
                        Id = 1,
                        UserId = 1,
                    }
                });

            _categoryRepository
                .FindAsync(Arg.Is<uint>(id => id == 2), Arg.Any<CancellationToken>())
                .Returns(new Category { Id = 2 });

            var command = new EditParentOfCategoriesCommand(
                new EditParentOfCategoriesRequest()
                {
                    CategoryIds = new List<uint> { 1 },
                    ParentId = 2
                }
            )
            {
                UserId = 1
            };

            var handler = GetCommandHandler();

            // Act
            await handler.Handle(command, new CancellationToken());

            _categoryRepository.Received(1).Update(
                Arg.Is<Category>(x => x.Id == 1 && x.ParentCategoryId != null && x.ParentCategoryId == 2)
            );


        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundException()
        {
            // Arrange
            _categoryRepository.GetCategoriesByCategoryIdList(Arg.Any<List<uint>>(), Arg.Any<CancellationToken>()).Returns(new List<Category>());
            EditParentOfCategoriesCommand command = new EditParentOfCategoriesCommand(new EditParentOfCategoriesRequest() { CategoryIds = [1], ParentId = 2 })
            {
                UserId = 1
            };
            var handler = GetCommandHandler();
            // Act
            Func<Task> act = async () => await handler.Handle(command, new CancellationToken());
            // Assert
            await Assert.ThrowsAsync<RecordNotFoundException>(act);
        }

        [Fact]
        public async Task HandlerShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            _categoryRepository.GetCategoriesByCategoryIdList(Arg.Any<List<uint>>(), Arg.Any<CancellationToken>()).Returns(new List<Category>()
            {
                new Category ()
                {

                    Id = 1,
                    UserId = 2
                }
            });

            EditParentOfCategoriesCommand command = new EditParentOfCategoriesCommand(new EditParentOfCategoriesRequest() { CategoryIds = [1], ParentId = 2 })
            {
                UserId = 1
            };

            var handler = GetCommandHandler();

            // Act

            // Assert

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, new CancellationToken()));
        }
    }
}
