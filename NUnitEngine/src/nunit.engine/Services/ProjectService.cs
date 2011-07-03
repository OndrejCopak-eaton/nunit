// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************
using System;
using System.IO;

namespace NUnit.Engine.Services
{
	/// <summary>
	/// Summary description for ProjectService.
	/// </summary>
	public class ProjectService : IProjectLoader, IService
	{
		/// <summary>
		/// Seed used to generate names for new projects
		/// </summary>
        //private int projectSeed = 0;

		/// <summary>
		/// The extension used for test projects
		/// </summary>
        //private static readonly string nunitExtension = ".nunit";

		/// <summary>
		/// Array of all installed ProjectLoaders
		/// </summary>
		IProjectLoader[] loaders = new IProjectLoader[] 
		{
            new NUnitProjectLoader(),
			//new VisualStudioLoader()
		};

		#region Instance Methods

        public TestPackage MakeTestPackage(IProject project)
        {
            return MakeTestPackage(project, null);
        }

        public TestPackage MakeTestPackage(IProject project, string configName)
        {
            TestPackage package = new TestPackage(project.ProjectPath);

            if (project.Configs.Count == 0)
                return package;

            foreach (string assembly in project.ActiveConfig.Assemblies)
                package.Add(new TestPackage(assembly));

            return package;
        }

        ///// <summary>
        ///// Creates a project to wrap a list of assemblies
        ///// </summary>
        //public IProject WrapAssemblies( string[] assemblies )
        //{
        //    // if only one assembly is passed in then the configuration file
        //    // should follow the name of the assembly. This will only happen
        //    // if the LoadAssembly method is called. Currently the console ui
        //    // does not differentiate between having one or multiple assemblies
        //    // passed in.
        //    if ( assemblies.Length == 1)
        //        return WrapAssembly(assemblies[0]);


        //    NUnitProject project = ServiceContext.ProjectService.EmptyProject();
        //    ProjectConfig config = new ProjectConfig( "Default" );
        //    foreach( string assembly in assemblies )
        //    {
        //        string fullPath = Path.GetFullPath( assembly );

        //        if ( !File.Exists( fullPath ) )
        //            throw new FileNotFoundException( string.Format( "Assembly not found: {0}", fullPath ) );
				
        //        config.Assemblies.Add( fullPath );
        //    }

        //    project.Configs.Add( config );

        //    // TODO: Deduce application base, and provide a
        //    // better value for loadpath and project path
        //    // analagous to how new projects are handled
        //    string basePath = Path.GetDirectoryName( Path.GetFullPath( assemblies[0] ) );
        //    project.ProjectPath = Path.Combine( basePath, project.Name + ".nunit" );

        //    project.IsDirty = true;

        //    return project;
        //}

        ///// <summary>
        ///// Creates a project to wrap an assembly
        ///// </summary>
        //public IProject WrapAssembly( string assemblyPath )
        //{
        //    if ( !File.Exists( assemblyPath ) )
        //        throw new FileNotFoundException( string.Format( "Assembly not found: {0}", assemblyPath ) );

        //    string fullPath = Path.GetFullPath( assemblyPath );

        //    NUnitProject project = new NUnitProject( fullPath );
			
        //    ProjectConfig config = new ProjectConfig( "Default" );
        //    config.Assemblies.Add( fullPath );
        //    project.Configs.Add( config );

        //    project.IsAssemblyWrapper = true;
        //    project.IsDirty = false;

        //    return project;
        //}

        //public string GenerateProjectName()
        //{
        //    return string.Format( "Project{0}", ++projectSeed );
        //}

        //public IProject EmptyProject()
        //{
        //    return new NUnitProject( GenerateProjectName() );
        //}

        //public IProject NewProject()
        //{
        //    NUnitProject project = EmptyProject();

        //    project.Configs.Add( "Debug" );
        //    project.Configs.Add( "Release" );
        //    project.IsDirty = false;

        //    return project;
        //}

        //public void SaveProject( IProject project )
        //{
        //    project.Save();
        //}
		#endregion

		#region IProjectLoader Members

		public bool IsProjectFile(string path)
		{
			foreach( IProjectLoader loader in loaders )
				if ( loader.IsProjectFile(path) )
					return true;

			return false;
		}

		public IProject LoadProject(string path)
		{
			foreach( IProjectLoader loader in loaders )
			{
				if ( loader.IsProjectFile( path ) )
					return loader.LoadProject( path );
			}

			return null;
		}

        /// <summary>
        /// Recursively expands any TestPackages based on a known
        /// project format. The package settings are set from
        /// the project settings and subpackages are created for
        /// each assembly.
        /// </summary>
        /// <param name="package">The TestPackage to be expanded</param>
        public void ExpandProjectPackages(TestPackage package)
        {
            if (package.HasSubPackages)
            {
                foreach (TestPackage subPackage in package.SubPackages)
                    ExpandProjectPackages(subPackage);
            }
            else if (IsProjectFile(package.FilePath))
            {
                IProject project = LoadProject(package.FilePath);

                string configName = package.Settings["ActiveConfig"] as string;
                IProjectConfig config = configName != null
                    ? project.Configs[configName]
                    : project.ActiveConfig;

                foreach (string key in config.Settings.Keys)
                    package.Settings[key] = config.Settings[key];

                foreach (string assembly in config.Assemblies)
                {
                    TestPackage subPackage = new TestPackage(assembly);

                    //foreach (string key in package.Settings.Keys)
                    //    subPackage.Settings[key] = package.Settings[key];

                    package.Add(subPackage);
                }
            }
        }

        #endregion

		#region IService Members

        private ServiceContext services;
        public ServiceContext ServiceContext
        {
            get { return services; }
            set { services = value; }
        }

        public void InitializeService()
		{
			// TODO:  Add ProjectLoader.InitializeService implementation
		}

		public void UnloadService()
		{
			// TODO:  Add ProjectLoader.UnloadService implementation
		}

		#endregion
	}
}