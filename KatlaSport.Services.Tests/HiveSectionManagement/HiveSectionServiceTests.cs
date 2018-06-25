using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using KatlaSport.DataAccess.ProductStoreHive;
using KatlaSport.Services.HiveManagement;
using Moq;
using Xunit;

namespace KatlaSport.Services.Tests.HiveSectionManagement
{
    public class HiveSectionServiceTests
    {
        public HiveSectionServiceTests()
        {
            Mapper.Reset();
            Mapper.Initialize(x => x.AddProfile<HiveManagementMappingProfile>());
        }

        [Theory]
        [AutoMoqData]
        public void Constructor_PassesCorrectArguments_ExpectsServiceInstanceNotNull(
            [Frozen] IMock<IUserContext> userContextMock,
            [Frozen] IMock<IProductStoreHiveContext> productStoreHiveContextMock)
            => Assert.NotNull(new HiveSectionService(productStoreHiveContextMock.Object, userContextMock.Object));

        [Fact]
        public void Constructor_PassesNullAsIProductStoreHiveContext_ThrowsArgumentNullException()
            => Assert.Throws<ArgumentNullException>(() => new HiveSectionService(null, null));

        [Theory]
        [AutoMoqData]
        public void Constructor_PassesNullAsIUserContext_ThrowsArgumentNullException(
            [Frozen] IMock<IProductStoreHiveContext> contextMock)
            => Assert.Throws<ArgumentNullException>(() => new HiveSectionService(contextMock.Object, null));

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSectionsAsync_Create3FakeHiveSection_ExpectsGetting3HiveSections(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            var hiveSections = await hiveSectionService.GetHiveSectionsAsync();

            Assert.Equal(3, hiveSections.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSectionAsync_PassesCorrectHiveSectionId_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            var expectedHiveSection = storeHiveSections[1];
            var actualHiveSection = await hiveSectionService.GetHiveSectionAsync(storeHiveSections[1].Id);

            Assert.Equal(expectedHiveSection.Id, actualHiveSection.Id);
            Assert.Equal(expectedHiveSection.Name, actualHiveSection.Name);
            Assert.Equal(expectedHiveSection.StoreHiveId, actualHiveSection.HiveId);
            Assert.Equal(expectedHiveSection.Code, actualHiveSection.Code);
            Assert.Equal(expectedHiveSection.IsDeleted, actualHiveSection.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveSectionAsync_PassesHiveSectionId_ThrowsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture,
            int hiveSectionId)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(0).ToArray();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveSectionService.GetHiveSectionAsync(hiveSectionId));
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSectionAsync_PassesUpdateHiveSectionRequest_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            contextMock.Setup(x => x.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(x => x.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveSectionRequest { Name = "qwerty", Code = "12345", HiveId = storeHives[0].Id };

            var expectedHiveSection = await hiveSectionService.CreateHiveSectionAsync(createRequest);
            var actualHiveSection = await hiveSectionService.GetHiveSectionAsync(expectedHiveSection.Id);

            Assert.Equal(expectedHiveSection.Name, actualHiveSection.Name);
            Assert.Equal(expectedHiveSection.Code, actualHiveSection.Code);
            Assert.Equal(expectedHiveSection.HiveId, actualHiveSection.HiveId);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSectionAsync_PassesUpdateHiveSectionRequest_ThrowsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            string code = "12345";
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHiveSections[0].Code = code;
            var createRequest = new UpdateHiveSectionRequest { Code = code, Name = "qwerty" };

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveSectionService.CreateHiveSectionAsync(createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveSectionAsync_PassesUpdateHiveSectionRequest_ThrowsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveSectionRequest { Code = "12345", Name = "qwerty", HiveId = 123 };

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveSectionService.CreateHiveSectionAsync(createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            contextMock.Setup(x => x.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(x => x.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveSectionRequest { Name = "qwerty", Code = "12345", HiveId = storeHives[0].Id };

            var expectedHiveSection = await hiveSectionService.UpdateHiveSectionAsync(storeHiveSections[1].Id, createRequest);
            var actualHiveSection = await hiveSectionService.GetHiveSectionAsync(expectedHiveSection.Id);

            Assert.Equal(expectedHiveSection.Name, actualHiveSection.Name);
            Assert.Equal(expectedHiveSection.Code, actualHiveSection.Code);
            Assert.Equal(expectedHiveSection.HiveId, actualHiveSection.HiveId);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            string code = "12345";
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHiveSections[0].Code = code;
            var createRequest = new UpdateHiveSectionRequest { Code = code, Name = "qwerty" };

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveSectionService.UpdateHiveSectionAsync(1, createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveSectionRequest { Code = "12345", Name = "qwerty", HiveId = 123 };

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveSectionService.UpdateHiveSectionAsync(1, createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatusAsync_PassesHiveSectionIdAndDeleteStatus_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            bool actualDeletedStatus = false;
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            await hiveSectionService.SetStatusAsync(storeHiveSections[2].Id, actualDeletedStatus);
            var expectedDeletedStatus = (await hiveSectionService.GetHiveSectionAsync(storeHiveSections[2].Id)).IsDeleted;

            Assert.Equal(expectedDeletedStatus, actualDeletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatusAsync_PassesHiveSectionIdAndDeleteStatus_ExpectsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveSectionService.SetStatusAsync(123, false));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSectionAsync_PassesHiveSectionId_ExpectsThatCollectionDoesNotContainDeletedHiveSection(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var deletedHiveSection = storeHiveSections[1];

            await hiveSectionService.DeleteHiveSectionAsync(storeHiveSections[1].Id);

            Assert.DoesNotContain(deletedHiveSection, storeHiveSections);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSectionAsync_PassesHiveSectionId_ThrowsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var fakeId = 123;
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveSectionService.DeleteHiveSectionAsync(fakeId));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveSectionAsync_PassesHiveSectionId_ThrowsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveSectionService hiveSectionService,
            IFixture fixture)
        {
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHiveSections[0].IsDeleted = false;

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveSectionService.DeleteHiveSectionAsync(storeHiveSections[0].Id));
        }
    }
}
