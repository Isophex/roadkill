﻿using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class ConfigReaderWriterStub : ConfigReaderWriter
	{
		public ApplicationSettings ApplicationSettings { get; set; }
		public string UILanguageCode { get; set; }
		public bool InstallStateReset { get; set; }
		public bool Saved { get; set; }
		public string TestWebConfigResult { get; set; }

		public ConfigReaderWriterStub()
		{
			ApplicationSettings = new ApplicationSettings();
			ApplicationSettings.AttachmentsFolder = "~/App_Data/Attachments";
			TestWebConfigResult = "OK";
		}

		public override void UpdateLanguage(string uiLanguageCode)
		{
			UILanguageCode = uiLanguageCode;
		}

		public override void Save(SettingsViewModel settings)
		{
			Saved = true;

			// The bare minimum needed to test the installer
			ApplicationSettings.ConnectionString = settings.ConnectionString;
			ApplicationSettings.DatabaseName = settings.DatabaseName;
			ApplicationSettings.UseBrowserCache = settings.UseBrowserCache;
			ApplicationSettings.UseObjectCache = settings.UseObjectCache;
		}

		public override RoadkillSection Load()
		{
			return new RoadkillSection();
		}

		public override ApplicationSettings GetApplicationSettings()
		{
			return ApplicationSettings;
		}

		public override void ResetInstalledState()
		{
			InstallStateReset = true;
		}

		public override string TestSaveWebConfig()
		{
			return TestWebConfigResult;
		}
	}
}
