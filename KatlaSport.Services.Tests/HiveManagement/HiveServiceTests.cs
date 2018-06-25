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

namespace KatlaSport.Services.Tests.HiveManagement
{
    public class HiveServiceTests
    {
        public HiveServiceTests()
        {
            Mapper.Reset();
            Mapper.Initialize(x => x.AddProfile<HiveManagementMappingProfile>());
        }

        [Theory]
        [AutoMoqData]
        public void Constructor_PassesCorrectArguments_ExpectsServiceInstanceNotNull(
            [Frozen] IMock<IUserContext> userContextMock,
            [Frozen] IMock<IProductStoreHiveContext> productStoreHiveContextMock)
            => Assert.NotNull(new HiveService(productStoreHiveContextMock.Object, userContextMock.Object));

        [Fact]
        public void Constructor_PassesNullAsIProductStoreHiveContext_ThrowsArgumentNullException()
            => Assert.Throws<ArgumentNullException>(() => new HiveService(null, null));

        [Theory]
        [AutoMoqData]
        public void Constructor_PassesNullAsIUserContext_ThrowsArgumentNullException(
            [Frozen] IMock<IProductStoreHiveContext> contextMock)
            => Assert.Throws<ArgumentNullException>(() => new HiveService(contextMock.Object, null));

        [Theory]
        [AutoMoqData]
        public async Task GetHivesAsync_Create3FakeHives_ExpectsGetting3Hives(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToArray();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            var hives = await hiveService.GetHivesAsync();

            Assert.Equal(3, hives.Count);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveAsync_PassesCorrectHiveId_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToArray();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            var expectedHive = storeHives[1];
            var actualHive = await hiveService.GetHiveAsync(storeHives[1].Id);

            Assert.Equal(expectedHive.Id, actualHive.Id);
            Assert.Equal(expectedHive.Name, actualHive.Name);
            Assert.Equal(expectedHive.Address, actualHive.Address);
            Assert.Equal(expectedHive.Code, actualHive.Code);
            Assert.Equal(expectedHive.IsDeleted, actualHive.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetHiveAsync_PassesHiveId_ThrowsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture,
            int hiveId)
        {
            var storeHives = fixture.CreateMany<StoreHive>(0).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveService.GetHiveAsync(hiveId));
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveAsync_PassesUpdateHiveRequest_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveRequest { Address = "Kuprevicha 1-1", Code = "12345", Name = "qwerty" };

            var expectedHive = await hiveService.CreateHiveAsync(createRequest);
            var actualHive = await hiveService.GetHiveAsync(expectedHive.Id);

            Assert.Equal(expectedHive.Id, actualHive.Id);
            Assert.Equal(expectedHive.Name, actualHive.Name);
            Assert.Equal(expectedHive.Address, actualHive.Address);
            Assert.Equal(expectedHive.Code, actualHive.Code);
            Assert.Equal(expectedHive.IsDeleted, actualHive.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateHiveAsync_PassesUpdateHiveRequest_ThrowsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            string code = "12345";
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHives[0].Code = code;
            var createRequest = new UpdateHiveRequest { Address = "Kuprevicha 1-1", Code = code, Name = "qwerty" };

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveService.CreateHiveAsync(createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToArray();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveRequest { Address = "Kuprevicha 1-1", Code = "12345", Name = "qwerty" };

            var expectedHive = await hiveService.UpdateHiveAsync(storeHives[1].Id, createRequest);
            var actualHive = await hiveService.GetHiveAsync(expectedHive.Id);

            Assert.Equal(expectedHive.Id, actualHive.Id);
            Assert.Equal(expectedHive.Name, actualHive.Name);
            Assert.Equal(expectedHive.Address, actualHive.Address);
            Assert.Equal(expectedHive.Code, actualHive.Code);
            Assert.Equal(expectedHive.IsDeleted, actualHive.IsDeleted);
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            string code = "12345";
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHives[0].Code = code;
            var createRequest = new UpdateHiveRequest { Address = "Kuprevicha 1-1", Code = code, Name = "qwerty" };

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveService.UpdateHiveAsync(1, createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task UpdateHiveAsync_PassesHiveIdAndUpdateHiveRequest_ExpectsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var createRequest = new UpdateHiveRequest { Address = "Kuprevicha 1-1", Code = "12345", Name = "qwerty" };

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveService.UpdateHiveAsync(123, createRequest));
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatusAsync_PassesHiveIdAndDeleteStatus_ExpectsSuccessfullEqualityAssertion(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            bool actualDeletedStatus = false;
            var storeHives = fixture.CreateMany<StoreHive>(3).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);

            await hiveService.SetStatusAsync(storeHives[2].Id, actualDeletedStatus);
            var expectedDeletedStatus = (await hiveService.GetHiveAsync(storeHives[2].Id)).IsDeleted;

            Assert.Equal(expectedDeletedStatus, actualDeletedStatus);
        }

        [Theory]
        [AutoMoqData]
        public async Task SetStatusAsync_PassesHiveIdAndDeleteStatus_ExpectsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToArray();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(() => hiveService.SetStatusAsync(123, false));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveAsync_PassesHiveId_ExpectsThatCollectionDoesNotContainDeletedHive(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            var deletedHive = storeHives[1];

            await hiveService.DeleteHiveAsync(storeHives[1].Id);

            Assert.DoesNotContain(deletedHive, storeHives);
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveAsync_PassesHiveId_ThrowsRequestedResourceNotFoundException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var fakeId = 123;
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);

            await Assert.ThrowsAsync<RequestedResourceNotFoundException>(
                () => hiveService.DeleteHiveAsync(fakeId));
        }

        [Theory]
        [AutoMoqData]
        public async Task DeleteHiveAsync_PassesHiveId_ThrowsRequestedResourceHasConflictException(
            [Frozen] Mock<IProductStoreHiveContext> contextMock,
            HiveService hiveService,
            IFixture fixture)
        {
            var storeHives = fixture.CreateMany<StoreHive>(3).ToList();
            var storeHiveSections = fixture.CreateMany<StoreHiveSection>(3).ToList();
            contextMock.Setup(c => c.Hives).ReturnsEntitySet(storeHives);
            contextMock.Setup(c => c.Sections).ReturnsEntitySet(storeHiveSections);
            storeHives[0].IsDeleted = false;

            await Assert.ThrowsAsync<RequestedResourceHasConflictException>(
                () => hiveService.DeleteHiveAsync(storeHives[0].Id));
        }
    }
}
