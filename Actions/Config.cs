using Actions.Dashboard;
using IPA.Config.Stores;
using SiraUtil.Converters;
using Version = SemVer.Version;
using System.Collections.Generic;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Actions
{
    internal class Config
    {

        [NonNullable, UseConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = new Version("0.0.0");

        [NonNullable, UseConverter(typeof(ListConverter<Macro>))]
        public virtual List<Macro> Macros { get; set; } = new List<Macro>();

        public bool PrefixForTTS { get; set; } = true;

        public string Channel { get; set; } = "";
    }
}