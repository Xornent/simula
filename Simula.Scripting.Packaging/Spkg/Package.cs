using Simula.Scripting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Simula.Scripting.Packaging.Spkg
{

    public class PackageSearchResult
    {
        public List<Package> Packages = new List<Package>();
        public int MaximumPage { get; set; }
        public int CurrentPage { get; set; }
    }

    public class Package
    {

        public static string SearchAutoCompleteService = "";
        public static string Registrations = "";
        public static string RegistrationsGZip360 = "";

        public static void Initialize()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.nuget.org/v3/index.json");
            using (var response = request.GetResponse()) {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();
                var service = JsonConvert.DeserializeObject<ServiceIndex>(json);

                foreach (var item in service.Resources) {
                    if (item.Type == "SearchAutocompleteService" && SearchAutoCompleteService == "")
                        SearchAutoCompleteService = item.Id;
                    if (item.Type == "RegistrationsBaseUrl" && Registrations == "")
                        Registrations = item.Id;
                    if (item.Type == "RegistrationsBaseUrl/3.6.0" && RegistrationsGZip360 == "")
                        RegistrationsGZip360 = item.Id;
                }
            }
        }

        public static PackageSearchResult Search(string exactId, int page = 0, bool prereleased = false)
        {
            var result = SearchResult.Create(exactId, SearchAutoCompleteService, page, prereleased);
            PackageSearchResult search = new PackageSearchResult();
            search.MaximumPage = result.MaximumPage;
            search.CurrentPage = page + 1;

            foreach (var item in result.Data) {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Registrations + item.ToLower() + "/index.json"); ;
                using (var response = request.GetResponse()) {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string json = reader.ReadToEnd();
                    var regs = JsonConvert.DeserializeObject<PackageMetadata>(json);

                    Package pkg = new Package();

                    int i = -1;
                    foreach (var leaves in regs.Catalogues) {
                        i++;
                        if (leaves.Items == null) {
                            HttpWebRequest leafReq = (HttpWebRequest)HttpWebRequest.Create(leaves.Id); ;
                            using (var leafRes = leafReq.GetResponse()) {
                                JsonSerializerSettings s = new JsonSerializerSettings();
                                StreamReader r = new StreamReader(leafRes.GetResponseStream());
                                string j = r.ReadToEnd();
                                try {
                                    var leaf = JsonConvert.DeserializeObject<PackageMetadataLeaf>(j);
                                    regs.Catalogues[i] = leaf;
                                } catch {
                                    string err = j;
                                    continue;
                                }
                            }
                        }

                        foreach (var subvers in regs.Catalogues[i].Items) {
                            Module module = new Module();
                            module.IconUrl = subvers.Entry.IconUrl;
                            module.LicenseUrl = subvers.Entry.LicenseUrl;
                            module.ProjectUrl = subvers.Entry.ProjectUrl;
                            module.Summary = subvers.Entry.Summary;
                            module.Description = subvers.Entry.Description;
                            module.Version = subvers.Entry.Version;
                            module.License = subvers.Entry.LicenseExpression;
                            module.AcceptanceRequired = subvers.Entry.RequireLicenseAcceptance;
                            module.Publish = DateTime.Parse(subvers.Entry.Published);
                            module.Listed = subvers.Entry.Listed;
                            module.Author = subvers.Entry.Authors;
                            pkg.Name = subvers.Entry.PackageId;
                            module.Name = subvers.Entry.PackageId;
                            module.Download = subvers.Entry.PackageContent;

                            foreach (var group in subvers.Entry.DependencyGroups) {
                                if (group.TargetFramework == ".NetCore3.1") {
                                    foreach (var dep in group.Dependencies) {
                                        DependencySink sink = new DependencySink();
                                        sink.Id = dep.PackageId;
                                        sink.Registration = dep.Registration;

                                        string ver = dep.VersionRange.Substring(1, dep.VersionRange.Length - 2);
                                        string[] vers = ver.Split(',');
                                        if (string.IsNullOrWhiteSpace(vers[0]))
                                            sink.Minimal = Version.Parse(vers[0]);
                                        if (string.IsNullOrWhiteSpace(vers[1]))
                                            sink.Maximum = Version.Parse(vers[1]);
                                        module.Dependency.Add(sink);
                                    }
                                }
                            }

                            pkg.Modules.Add(module);
                        }
                    }

                    search.Packages.Add(pkg);
                }
            }

            return search;
        }

        public Package GetPackage(string name)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Registrations + name.ToLower() + "/index.json"); ;
            request.Method = "GET";

            try {
                using (var response = request.GetResponse()) {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string json = reader.ReadToEnd();
                    var regs = JsonConvert.DeserializeObject<PackageMetadata>(json);

                    Package pkg = new Package();

                    foreach (var leaves in regs.Catalogues) {
                        foreach (var subvers in leaves.Items) {
                            Module module = new Module();
                            module.IconUrl = subvers.Entry.IconUrl;
                            module.LicenseUrl = subvers.Entry.LicenseUrl;
                            module.ProjectUrl = subvers.Entry.ProjectUrl;
                            module.Summary = subvers.Entry.Summary;
                            module.Description = subvers.Entry.Description;
                            module.Version = subvers.Entry.Version;
                            module.License = subvers.Entry.LicenseExpression;
                            module.AcceptanceRequired = subvers.Entry.RequireLicenseAcceptance;
                            module.Publish = DateTime.Parse(subvers.Entry.Published);
                            module.Listed = subvers.Entry.Listed;
                            module.Author = subvers.Entry.Authors;
                            pkg.Name = subvers.Entry.PackageId;
                            module.Name = subvers.Entry.PackageId;
                            module.Download = subvers.Entry.PackageContent;

                            foreach (var group in subvers.Entry.DependencyGroups) {
                                if (group.TargetFramework == ".NetCore3.1") {
                                    foreach (var dep in group.Dependencies) {
                                        DependencySink sink = new DependencySink();
                                        sink.Id = dep.PackageId;
                                        sink.Registration = dep.Registration;

                                        string ver = dep.VersionRange.Substring(1, dep.VersionRange.Length - 2);
                                        string[] vers = ver.Split(',');
                                        if (string.IsNullOrWhiteSpace(vers[0]))
                                            sink.Minimal = Version.Parse(vers[0]);
                                        if (string.IsNullOrWhiteSpace(vers[1]))
                                            sink.Maximum = Version.Parse(vers[1]);
                                        module.Dependency.Add(sink);
                                    }
                                }
                            }

                            pkg.Modules.Add(module);
                        }
                    }

                    return pkg;
                }
            } catch { return null; }
        }

        public string Name { get; set; }
        public List<Module> Modules { get; set; } = new List<Module>();
    }

    public class Module
    {
        public string Author { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }

        public string Summary { get; set; }
        public string Description { get; set; }

        public string Version { get; set; }
        public string License { get; set; }
        public bool AcceptanceRequired { get; set; }
        public DateTime Publish { get; set; }
        public bool Listed { get; set; }

        public List<DependencySink> Dependency = new List<DependencySink>();
        public string Name { get; set; }
        public string Download { get; set; }
    }

    public class DependencySink
    {
        public Version Minimal { get; set; } = new Version();
        public Version Maximum { get; set; } = new Version();
        public string Id { get; set; }
        public string Registration { get; set; }
    }
}
