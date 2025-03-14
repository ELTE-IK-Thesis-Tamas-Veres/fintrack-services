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

namespace fintrack_api_unit_tests.CommandHandler
{
    public class CreateCategoryTest
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryTest()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _categoryRepository = Substitute.For<ICategoryRepository>();
        }

        private CreateCategoryCommandHandler GetCommandHandler()
        {
            return new CreateCategoryCommandHandler(_userRepository, _categoryRepository);
        }

        [Fact]
        public async Task HandlerShouldCreateCategory()
        {
            _userRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns(new User
            {
                Id = 1,
                Sub = "sub"
            });

            CreateCategoryCommand command = new CreateCategoryCommand
            {
                UserId = 1,
                Name = "name"
            };

            var handler = GetCommandHandler();

            await handler.Handle(command, new CancellationToken());

            _categoryRepository.Received(1).Insert(Arg.Is<Category>(x => x.Name == "name"));
        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundException()
        {
            _userRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns((User?)null);
            CreateCategoryCommand command = new CreateCategoryCommand
            {
                UserId = 1,
                Name = "name"
            };
            var handler = GetCommandHandler();
            await Assert.ThrowsAsync<RecordNotFoundException>(() => handler.Handle(command, new CancellationToken()));
        }

        [Fact]
        public async Task HandlerShouldThrowRecordNotFoundExceptionWhenUserIsNull()
        {
            _userRepository.FindAsync(Arg.Any<uint>(), Arg.Any<CancellationToken>()).Returns((User?)null);
            CreateCategoryCommand command = new CreateCategoryCommand
            {
                UserId = 1,
                Name = "name"
            };
            var handler = GetCommandHandler();
            await Assert.ThrowsAsync<RecordNotFoundException>(() => handler.Handle(command, new CancellationToken()));
        }
    }
}
