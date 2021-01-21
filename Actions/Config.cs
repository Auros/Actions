using UnityEngine;
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
        public virtual Version Version { get; set; } = new Version("0.0.0");

        [NonNullable, UseConverter(typeof(ListConverter<Macro>))]
        public virtual List<Macro> Macros { get; set; } = new List<Macro>();

        public virtual bool AllowModsToCreate { get; set; } = false;

        public virtual bool PrefixForTTS { get; set; } = true;

        public virtual bool ShowInGame { get; set; } = false;

        public virtual string Channel { get; set; } = "";

        [UseConverter(typeof(Vector3Converter))] public Vector3 MacroDashboardPosition { get; set; } = new Vector3(1.2f, 1.5f, -1.5f);
        [UseConverter(typeof(Vector3Converter))] public Vector3 MacroDashboardRotation { get; set; } = new Vector3(0f, 180f, 0f);
        [UseConverter(typeof(Vector3Converter))] public Vector3 UserManagerDashboardPosition { get; set; } = new Vector3(-1.2f, 1.5f, -1.5f);
        [UseConverter(typeof(Vector3Converter))] public Vector3 UserManagerDashboardRotation { get; set; } = new Vector3(0f, 180f, 0f);
    }
}