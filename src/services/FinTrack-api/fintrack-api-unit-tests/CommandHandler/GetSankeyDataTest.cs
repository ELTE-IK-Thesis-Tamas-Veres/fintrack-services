using fintrack_api_business_logic.Handlers.SankeyHandlers;
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
    public class GetSankeyDataTest
    {
        private readonly IRecordRepository _recordRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly GetSankeyDataCommandHandler _handler;

        public GetSankeyDataTest()
        {
            _recordRepository = Substitute.For<IRecordRepository>();
            _categoryRepository = Substitute.For<ICategoryRepository>();
            _handler = new GetSankeyDataCommandHandler(_recordRepository, _categoryRepository);
        }

        [Fact]
        public async Task Handle_NoCategoriesAndNoUncategorisedRecords_ReturnsEmptySankeyData()
        {
            // Arrange
            uint userId = 1;
            var command = new GetSankeyDataCommand { UserId = userId, Month = null, Year = null };

            _categoryRepository
                .GetCategoriesByUserIdWhereParentIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category>());
            _recordRepository
                .GetRecordByUserIdWhereCategoryIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Record>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(result.Nodes);
            Assert.Empty(result.Links);
        }

        [Fact]
        public async Task Handle_WithCategoryData_ReturnsExpectedSankeyData()
        {
            // Arrange
            uint userId = 1;
            var command = new GetSankeyDataCommand { UserId = userId, Month = null, Year = null };

            var topCategories = new List<Category>
            {
                new Category { Id = 1 }
            };
            _categoryRepository
                .GetCategoriesByUserIdWhereParentIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(topCategories);

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var category = new Category
            {
                Id = 1,
                Name = "Food",
                ParentCategoryId = null,
                Records = new List<Record>
                {
                    new Record { Amount = 100, Date = today },
                    new Record { Amount = -50, Date = today }
                },
                ChildCategories = new List<Category>()
            };
            _categoryRepository
                .GetCategoryByIdWithRecordsAndChildren(1, Arg.Any<CancellationToken>())
                .Returns(category);

            _recordRepository
                .GetRecordByUserIdWhereCategoryIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Record>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(4, result.Nodes.Count);
            Assert.Contains(result.Nodes, n => n.IdText == "1-i" && n.Name == "Food");
            Assert.Contains(result.Nodes, n => n.IdText == "1-x" && n.Name == "Food");
            Assert.Contains(result.Nodes, n => n.IdText == "b" && n.Name == "[budget]");
            Assert.Contains(result.Nodes, n => n.IdText == "b-extra" && n.Name == "[not spent]");

            Assert.Equal(3, result.Links.Count);
            Assert.Contains(result.Links, l => l.SourceText == "1-i" && l.TargetText == "b" && l.Value == 100);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "1-x" && l.Value == 50);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "b-extra" && l.Value == 50);

            foreach (var link in result.Links)
            {
                int sourceIndex = result.Nodes.FindIndex(n => n.IdText == link.SourceText);
                int targetIndex = result.Nodes.FindIndex(n => n.IdText == link.TargetText);
                Assert.Equal(sourceIndex, link.Source);
                Assert.Equal(targetIndex, link.Target);
            }
        }

        [Fact]
        public async Task Handle_WithOnlyUncategorisedRecords_ReturnsUncategorisedSankeyData()
        {
            // Arrange
            uint userId = 1;
            var command = new GetSankeyDataCommand { UserId = userId, Month = null, Year = null };

            _categoryRepository
                .GetCategoriesByUserIdWhereParentIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category>());

            var uncategorisedRecords = new List<Record>
            {
                new Record { Amount = 80, Date = DateOnly.FromDateTime(DateTime.Now) },
                new Record { Amount = -30, Date = DateOnly.FromDateTime(DateTime.Now) }
            };
            _recordRepository
                .GetRecordByUserIdWhereCategoryIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(uncategorisedRecords);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(4, result.Nodes.Count);
            Assert.Contains(result.Nodes, n => n.IdText == "b-iu" && n.Name == "[Uncategorised]");
            Assert.Contains(result.Nodes, n => n.IdText == "b-xu" && n.Name == "[Uncategorised]");
            Assert.Contains(result.Nodes, n => n.IdText == "b-extra" && n.Name == "[not spent]");
            Assert.Contains(result.Nodes, n => n.IdText == "b" && n.Name == "[budget]");

            Assert.Equal(3, result.Links.Count);
            Assert.Contains(result.Links, l => l.SourceText == "b-iu" && l.TargetText == "b" && l.Value == 80);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "b-xu" && l.Value == 30);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "b-extra" && l.Value == 50);

            foreach (var link in result.Links)
            {
                int sourceIndex = result.Nodes.FindIndex(n => n.IdText == link.SourceText);
                int targetIndex = result.Nodes.FindIndex(n => n.IdText == link.TargetText);
                Assert.Equal(sourceIndex, link.Source);
                Assert.Equal(targetIndex, link.Target);
            }
        }
    }
}
