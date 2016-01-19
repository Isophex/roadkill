﻿using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Roadkill.Core.AmazingConfig;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution.StructureMap;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	public class MocksAndStubsContainer
	{
		public IConfiguration Configuration { get; set; }
		public ConfigurationStoreMock ConfigurationStoreMock { get; set; }

		public ConfigReaderWriterStub ConfigReaderWriter { get; set; }
		public IUserContext UserContext { get; set; }

		public MemoryCache MemoryCache { get; set; }
		public ListCache ListCache { get; set; }
		public SiteCache SiteCache { get; set; }
		public PageViewModelCache PageViewModelCache { get; set; }
		
		public PluginFactoryMock PluginFactory { get; set; }
		public MarkupConverter MarkupConverter { get; set; }
		public EmailClientMock EmailClient { get; set; }

		public UserServiceMock UserService { get; set; }
		public PageService PageService { get; set; }
		public SearchServiceMock SearchService { get; set; }
		public PageHistoryService HistoryService { get; set; }
		public IFileService FileService { get; set; }

		public RepositoryFactoryMock RepositoryFactory { get; set; }
		public UserRepositoryMock UserRepository { get; set; }
		public PageRepositoryMock PageRepository { get; set; }
		public InstallerRepositoryMock InstallerRepository { get; set; }
		public DatabaseTesterMock DatabaseTester { get;set; }
		public Container StructureMapContainer { get; set; }
		public StructureMapServiceLocator Locator { get; set; }
		public InstallationService InstallationService { get; set; }

		/// <summary>
		/// Creates a new instance of MocksAndStubsContainer.
		/// </summary>
		/// <param name="useCacheMock">The 'Roadkill' MemoryCache is used by default, but as this is static it can have problems with 
		/// the test runner unless you clear the Container.MemoryCache on setup each time, but then doing that doesn't give a realistic 
		/// reflection of how the MemoryCache is used inside an ASP.NET environment.</param>
		public MocksAndStubsContainer(bool useCacheMock = false)
		{
			// New config
			ConfigurationStoreMock = new ConfigurationStoreMock();
			Configuration = ConfigurationStoreMock.Load();
			Configuration.Installed = true;
			Configuration.AttachmentSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Cache
			MemoryCache = useCacheMock ? new CacheMock() : CacheMock.RoadkillCache;
			ListCache = new ListCache(ConfigurationStoreMock, MemoryCache);
			SiteCache = new SiteCache(MemoryCache);
			PageViewModelCache = new PageViewModelCache(ConfigurationStoreMock, MemoryCache);

			// Repositories
			UserRepository = new UserRepositoryMock();
			PageRepository = new PageRepositoryMock();
			InstallerRepository = new InstallerRepositoryMock();

			RepositoryFactory = new RepositoryFactoryMock()
			{
				UserRepository = UserRepository,
				PageRepository = PageRepository,
				InstallerRepository = InstallerRepository
			};
			DatabaseTester = new DatabaseTesterMock();

			// Plugins
			PluginFactory = new PluginFactoryMock();
			MarkupConverter = new MarkupConverter(ConfigurationStoreMock, PageRepository, PluginFactory);

			// Services
			// Dependencies for PageService. Be careful to make sure the class using this Container isn't testing the mock.
			UserService = new UserServiceMock(ConfigurationStoreMock, UserRepository);
			UserContext = new UserContext(UserService);
			SearchService = new SearchServiceMock(ConfigurationStoreMock, PageRepository, PluginFactory);
			SearchService.PageContents = PageRepository.PageContents;
			SearchService.Pages = PageRepository.Pages;
			HistoryService = new PageHistoryService(ConfigurationStoreMock, PageRepository, UserContext,
				PageViewModelCache, PluginFactory);
			FileService = new FileServiceMock();
			PageService = new PageService(ConfigurationStoreMock, PageRepository, SearchService, HistoryService,
				UserContext, ListCache, PageViewModelCache, SiteCache, PluginFactory);

			StructureMapContainer = new Container(x =>
			{
				x.AddRegistry(new TestsRegistry(this));
			});

			Locator = new StructureMapServiceLocator(StructureMapContainer, false);

			InstallationService = new InstallationService((databaseName, connectionString) =>
			{
				InstallerRepository.DatabaseName = databaseName;
				InstallerRepository.ConnectionString = connectionString;

				return InstallerRepository;
			}, Locator);

			// EmailTemplates
			EmailClient = new EmailClientMock();
		}

		public void ClearCache()
		{
			foreach (string key in MemoryCache.Select(x => x.Key))
				MemoryCache.Remove(key);
		}

		public class TestsRegistry : Registry
		{
			public TestsRegistry(MocksAndStubsContainer mockContainer)
			{
				For<IFileService>().Use(mockContainer.FileService);
				For<IRepositoryFactory>().Use(mockContainer.RepositoryFactory);
			}
		}
	}
}
