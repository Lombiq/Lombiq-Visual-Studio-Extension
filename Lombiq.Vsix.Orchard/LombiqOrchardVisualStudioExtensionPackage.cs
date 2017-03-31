﻿using EnvDTE;
using Lombiq.Vsix.Orchard.Constants;
using Lombiq.Vsix.Orchard.Models;
using Lombiq.Vsix.Orchard.Options;
using Lombiq.Vsix.Orchard.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace Lombiq.Vsix.Orchard
{
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(LogWatcherOptionsPage), "Lombiq Orchard Visual Studio Extension", "Orchard Log Watcher", 120, 121, true)]
    [Guid(PackageGuids.LombiqOrchardVisualStudioExtensionPackageGuidString)]
    public sealed class LombiqOrchardVisualStudioExtensionPackage : Package, ILogWatcherSettingsAccessor
    {
        private readonly IEnumerable<IPackageRegistrator> _packageRegistrators;


        public LombiqOrchardVisualStudioExtensionPackage()
        {
            var dte = GetGlobalService(typeof(SDTE)) as DTE;
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            var dependencyInjector = new DependencyInjector();
            var fieldNameGenerators = new IFieldNameFromDependencyGenerator[]
            {
                new DefaultFieldNameFromDependencyGenerator(),
                new DefaultFieldNameFromGenericTypeGenerator(),
                new FieldNameFromIEnumerableGenerator()
            };

            var dependencyInjectorPackageRegistrator = new DependencyInjectorPackageRegistrator(
                dte,
                menuCommandService,
                dependencyInjector,
                fieldNameGenerators);

            var logWatcherSettingsAccessor = this;
            var logWatcher = new OrchardErrorLogFileWatcher(logWatcherSettingsAccessor, dte);

            var logWatcherPackageRegistrator = new LogWatcherPackageRegistrator(
                dte,
                menuCommandService,
                logWatcherSettingsAccessor,
                logWatcher);

            _packageRegistrators = new IPackageRegistrator[] 
                {
                    dependencyInjectorPackageRegistrator,
                    logWatcherPackageRegistrator
                };
        }


        protected override void Initialize()
        {
            base.Initialize();

            foreach (var registrator in _packageRegistrators)
            {
                registrator.RegisterCommands();
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var registrator in _packageRegistrators)
            {
                registrator.Dispose();
            }

            base.Dispose(disposing);
        }
        

        #region ILogWatcherSettings Members

        ILogWatcherSettings ILogWatcherSettingsAccessor.GetSettings() => 
            (ILogWatcherSettings)GetDialogPage(typeof(LogWatcherOptionsPage));

        #endregion
    }
}
