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

            // Simulate one top-level category (its Id is used by the handler)
            var topCategories = new List<Category>
            {
                new Category { Id = 1 }
            };
            _categoryRepository
                .GetCategoriesByUserIdWhereParentIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(topCategories);

            // Simulate category details for id = 1
            // Create a category with one income record (+100) and one expense record (–50)
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

            // Simulate no uncategorised records
            _recordRepository
                .GetRecordByUserIdWhereCategoryIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Record>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert:
            // Expect nodes from the BuildSankeyDataWithRecordFilter:
            //  • Income node with IdText "1-i"
            //  • Expense node with IdText "1-x"
            // Then, since there are nodes, a budget node ("b") is added.
            // Then, netValueOfBudget is computed as 100 - 50 = 50, so an extra node ("b-extra") is added.
            Assert.Equal(4, result.Nodes.Count);
            Assert.Contains(result.Nodes, n => n.IdText == "1-i" && n.Name == "Food");
            Assert.Contains(result.Nodes, n => n.IdText == "1-x" && n.Name == "Food");
            Assert.Contains(result.Nodes, n => n.IdText == "b" && n.Name == "budget");
            Assert.Contains(result.Nodes, n => n.IdText == "b-extra" && n.Name == "income not spent");

            // Expected links:
            //  • From "1-i" -> "b" with value 100
            //  • From "b" -> "1-x" with value 50
            //  • Extra link: from "b" -> "b-extra" with value 50
            Assert.Equal(3, result.Links.Count);
            Assert.Contains(result.Links, l => l.SourceText == "1-i" && l.TargetText == "b" && l.Value == 100);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "1-x" && l.Value == 50);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "b-extra" && l.Value == 50);

            // Verify that each link's Source and Target indices match the index of the corresponding node.
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

            // No top-level categories
            _categoryRepository
                .GetCategoriesByUserIdWhereParentIsNull(userId, Arg.Any<CancellationToken>())
                .Returns(new List<Category>());

            // Simulate uncategorised records: one income record (+80) and one expense record (–30)
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

            // Assert:
            // Since no categories exist, the if (sankeyData.Nodes.Count > 0) branch does not add a budget node.
            // But the uncategorised blocks will add:
            //  • A node with IdText "b-iu" and a link (SourceText "b-iu" -> TargetText "b") with value 80.
            //  • A node with IdText "b-xu" and a link (SourceText "b-xu" -> TargetText "b") with value 30.
            // Then netValueOfBudget is computed (80 + (80+30) - 30 = 160) so an extra node ("b-extra") is added with:
            //  • A link from "b" -> "b-extra" with value 160.
            // Expected nodes: "b-iu", "b-xu", "b-extra"
            // Expected links: three links referencing "b" as target or source.
            Assert.Equal(3, result.Nodes.Count);
            Assert.Contains(result.Nodes, n => n.IdText == "b-iu" && n.Name == "Uncategorised");
            Assert.Contains(result.Nodes, n => n.IdText == "b-xu" && n.Name == "Uncategorised");
            Assert.Contains(result.Nodes, n => n.IdText == "b-extra" && n.Name == "income not spent");

            Assert.Equal(3, result.Links.Count);
            Assert.Contains(result.Links, l => l.SourceText == "b-iu" && l.TargetText == "b" && l.Value == 80);
            Assert.Contains(result.Links, l => l.SourceText == "b-xu" && l.TargetText == "b" && l.Value == 30);
            Assert.Contains(result.Links, l => l.SourceText == "b" && l.TargetText == "b-extra" && l.Value == 160);

            // Verify that each link's indices are set according to the nodes.
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
