using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: CLSCompliant(true)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, // where theme specific resource dictionaries are located
                                     // (used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly // where the generic resource dictionary is located
                                              // (used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: NeutralResourcesLanguage("en-US")]

[assembly: XmlnsPrefix("http://xornent.org/simula/editor", "Editor")]

[assembly: XmlnsDefinition("http://xornent.org/simula/editor", "Simula.Editor")]
[assembly: XmlnsDefinition("http://xornent.org/simula/editor", "Simula.Editor.Editing")]
[assembly: XmlnsDefinition("http://xornent.org/simula/editor", "Simula.Editor.Rendering")]
[assembly: XmlnsDefinition("http://xornent.org/simula/editor", "Simula.Editor.Highlighting")]
[assembly: XmlnsDefinition("http://xornent.org/simula/editor", "Simula.Editor.Search")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly",
    Justification = "AssemblyInformationalVersion does not need to be a parsable version")]

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("Simula.Editor")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Xornent S.I.")]
[assembly: AssemblyProduct("Simula.Editor")]
[assembly: AssemblyCopyright("Copyright © Xornent S.I. 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("d74412fa-afed-46f8-98a8-2f1ea71d7ff3")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
//可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
//通过使用 "*"，如下所示:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.1.0")]
[assembly: AssemblyFileVersion("0.8.1.0")]
