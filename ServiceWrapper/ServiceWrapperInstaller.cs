using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace ServiceWrapper
{
	[RunInstaller(true)]
	public class ServiceWrapperInstaller : Installer
	{
		public ServiceWrapperInstaller()
		{
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
		    string exeConfigFilename = Assembly.GetAssembly(typeof (ServiceWrapperInstaller)).Location + ".config";
		    fileMap.ExeConfigFilename = exeConfigFilename;
		    var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
		        //ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(typeof (ServiceWrapperInstaller)).Location);
            if (!config.HasFile)
                throw new Exception("File not found at " + exeConfigFilename);

		    string propertiesSettings = "applicationSettings/ServiceWrapper.Properties.Settings";
		    ConfigurationSection configurationSection = config.GetSection(propertiesSettings);
		    if (configurationSection == null)
		    {
                throw new Exception("could not find section " + propertiesSettings);
		    }
		    var settings = ((ClientSettingsSection)configurationSection).Settings;
		    var getValue = new Func<string, string>((string k) => settings.Get(k).Value.ValueXml.InnerText);
            
            
			var processInstaller = new ServiceProcessInstaller
			{
                Account = (ServiceAccount)Enum.Parse(typeof(ServiceAccount), getValue("ServiceAccount"), false),
                Username = getValue("AccountUsername") == string.Empty
					? null
                    : getValue("AccountUsername"),
                Password = getValue("AccountPassword") == string.Empty
					? null
                    : getValue("AccountPassword")
			};

			var installer = new ServiceInstaller
			{
                DisplayName = getValue("ServiceName"),
                StartType = (ServiceStartMode)Enum.Parse(typeof(ServiceStartMode), getValue("ServiceStartMode"), false),
                ServiceName = getValue("ServiceName"),
				Description = getValue("ServiceDescription")
			};

			Installers.Add(processInstaller);
			Installers.Add(installer);
		}
	}
}