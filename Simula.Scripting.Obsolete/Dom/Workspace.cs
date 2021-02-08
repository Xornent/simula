using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Simula.Scripting.Contexts;
using System.Xml;

namespace Simula.Scripting.Dom
{
    public class Workspace
    {
        // initialize a workspace from a directory path, recursing to all of its sub-folders
        // and files. and detects for workspace settings and other files under the folder.

        public Workspace(string path)
        {
            try {
                this.Directory = new DirectoryInfo(path);
            } catch {
                DynamicRuntime.PostExecutionError(StringTableIndex.WorkspaceFolderIllegal);
                return;
            }

            // enumerate all files readable into the filepaths.

            LoadDirectory(this.Directory);

            // search for config file. a config file is a file named config.xml directly under the
            // base directory. it defines the extensive properties and behaviors the workspace
            // loader should follow to load and executes.

            // the file structure contains standard xml nodes as follows:
            // project           : the base node of specified xml, with attribute 'version'.
            // metadata          : the container of metadata nodes.
            // meta              : a metadata node item, with attribute 'name' and 'value'.
            //                     available meta names are 'name', 'version', 'description', 'authors', and 'moreinfo'
            // dependencies      : the container of dependency nodes.
            // dependency        : a dependency node. containing the package, workspace and library dependencies.
            //                     with attribute 'type' and 'href'. available types are 'workspace', 'library', 'clr'
            // properties        : the container of property nodes.
            // prop              : a property, with attribute 'name' and 'value'

            // a legal example of config.xml (version 1) 
            // <config version='1'>
            //   <metadata>
            //     <meta name='name' value=''/>
            //     <meta name='version' value='0.0.0.0'/>
            //     <meta name='description' value=''/>
            //     <meta name='authors' value=''/>
            //     <meta name='moreinfo' value=''/>
            //   </metadata>
            //   <dependencies>
            //     <dependency type='workspace' href='d:/source/numerics/'/>
            //     <dependency type='clr' href='d:/libs/CSharp.Library.dll'/>
            //   </dependencies>
            //   <properties>
            //     <prop name='sysclr' value='1'/>
            //     <prop name='systools' value='1'/>
            //     <prop name='noalias' value='0'/>
            //   </properties>
            // </config>

            foreach (var item in this.Directory.GetFiles()) {
                if (item.Name.Split('.')[0].ToLower() == "config") {
                    if(!HasConfigFile) {
                        HasConfigFile = true;
                        XmlDocument doc = new XmlDocument();
                        doc.Load(item.FullName);
                        this.Configuration = doc;
                        break;
                    }
                }
            }

            if (HasConfigFile) {

            }
        }

        public DirectoryInfo Directory;
        public List<string> FilePaths = new List<string>();
        public Dictionary<string, Source> LoadedSources = new Dictionary<string, Source>();

        public bool HasConfigFile = false;
        public XmlDocument Configuration;

        // configuration properties:

        // the system clr libraries are add-ons provided by system to enhance the features of language.
        // they are precompiled c-sharp dynamic link libraries that are stored by default under the
        // directory ./libraries/ with extension name '.dll' or '.sdl'

        public bool LoadSystemClr { get; set; } = true;

        // the system toolsets are workspaces provided by system in the form of script source files.
        // they are stored by default under ./toolsets/xxx/

        public bool LoadSystemToolsets { get; set; } = true;

        // for example 'double' is an alias (reference by name) of 'sys.double'. this is created by 
        // runtime context by default, if this is turned on, you cannot omit the 'sys.' prefix when 
        // refering to 'class double'.

        public bool NoAlias { get; set; } = false;

        // metadatas

        public string Name { get; set; }
        public Version FileVersion { get; set; } = new Version("1.0.0.0");
        public Version PackageVersion { get; set; } = new Version("0.0.0.0");
        public string Description { get; set; }
        public List<string> Authors = new List<string>();
        public string MoreInfo { get; set; }

        public DynamicRuntime Runtime;
        public IDictionary<string, object> Variables {
            get {
                if (this.Runtime.Scopes.Count == 0) return new Dictionary<string, object>();
                else return (IDictionary<string,object>)this.Runtime.Scopes[0].Store;
            }
        }

        private void LoadDirectory(DirectoryInfo info)
        {
            foreach (var item in info.GetFiles()) {
                if(item.Extension.Replace(".","").ToLower() == "ss") {
                    FilePaths.Add(item.FullName);
                }
            }

            foreach (var item in info.GetDirectories()) {
                LoadDirectory(info);
            }
        }

        public void Run(string source)
        {

        }

        public void Clear()
        {

        }
    }

    public enum DependencyType
    {
        Workspace,
        Clr,
        Library
    }

    public class DependencyRecord
    {
        public DependencyType Type;
        public string Location = "";
    }

    public class WorkspaceDependency : DependencyRecord
    {

    }

    public class ClrDependency : DependencyRecord
    {

    }
}
