using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;

namespace GDG_SP.WinPhone
{
    /// <summary>
    /// Código por https://gist.github.com/rufusl/34f9c282089f7da0a8192080c52c58c8
    /// Notei que no Windows Phone acontece um erro com os arquivos .resx do Xamarin, fiz de tudo mas o aplicativo só funcionou com o idioma do sistema como inglês, e achei essa classe que resolve esse erro.
    /// </summary>
    public class WindowsRuntimeResourceManager : ResourceManager
    {
        private readonly ResourceLoader resourceLoader;
        private static readonly Regex resourcesExtensionRegex = new Regex("\\.resources$");

        private WindowsRuntimeResourceManager(string baseName, Assembly assembly)
            : base(baseName, assembly)
        {
            this.resourceLoader = ResourceLoader.GetForViewIndependentUse(baseName);
        }

        public static void PatchResourceManagersInAssembly(Type typeInAssembly)
        {
            var assembly = typeInAssembly.GetTypeInfo().Assembly;
            var resourceTypeNames = assembly.GetManifestResourceNames()
                .Where(n => resourcesExtensionRegex.IsMatch(n))
                .Select(n => resourcesExtensionRegex.Replace(n, string.Empty));

            foreach (var typeName in resourceTypeNames)
            {
                var resourcesGeneratedType = assembly.GetType(typeName);
                resourcesGeneratedType.GetRuntimeFields()
                    .First(f => f.IsStatic && f.FieldType == typeof(ResourceManager))
                    .SetValue(null, new WindowsRuntimeResourceManager(typeName, assembly));
            }
        }

        public override string GetString(string name, CultureInfo culture)
        {
            return this.resourceLoader.GetString(name);
        }
    }
}
