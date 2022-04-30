using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Actions.Dashboard;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using SiraUtil.Converters;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Actions
{
    internal class Config
    {
        public event Action<Config>? Updated;

        [NonNullable, UseConverter(typeof(VersionConverter))]
        public virtual Hive.Versioning.Version Version { get; set; } = new Hive.Versioning.Version("0.0.0");

        public virtual bool Enabled { get; set; } = true;

        [NonNullable, UseConverter(typeof(ListConverter<Macro>))]
        public virtual List<Macro> Macros { get; set; } = new List<Macro>();

        public virtual bool AllowModsToCreate { get; set; } = false;

        public virtual bool PrefixForTTS { get; set; } = true;

        public virtual bool ShowInGame { get; set; } = false;

        public virtual string? ChannelId { get; set; } = null;

        public Vector3 MacroDashboardPosition { get; set; } = new Vector3(1.2f, 1.5f, -1.5f);
        public Vector3 MacroDashboardRotation { get; set; } = new Vector3(0f, 180f, 0f);
        public Vector3 UserManagerDashboardPosition { get; set; } = new Vector3(-1.2f, 1.5f, -1.5f);
        public Vector3 UserManagerDashboardRotation { get; set; } = new Vector3(0f, 180f, 0f);

        public virtual void Changed()
        {
            Updated?.Invoke(this);
        }
    }
}