using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public static class DteHelper
    {
        public static bool HasProjectItem(this Project project, string projectFile)
        {
            var projectItem = project.GetProjectItem(projectFile);
            return projectItem == null ? false : true;
        }
        public static ProjectItem GetProjectItem(this Project project, string projectFile)
        {
            return GetProjectItemRecursive(project.ProjectItems, projectFile);
        }
        private static ProjectItem GetProjectItemRecursive(ProjectItems projectItems, string projectFile, string folder = "")
        {
            // initial value
            ProjectItem result = null;

            projectFile = projectFile.Replace(@"\", "/");

            // iterate project items
            foreach (ProjectItem projectItem in projectItems)
            {
                //var fullPath = GetFullPath(projectItem);
                // if the name matches
                Status.Update(folder + projectItem.Name);
                if (folder + projectItem.Name == projectFile)
                {
                    result = projectItem;
                    break;
                }
                else if ((projectItem.ProjectItems != null) && (projectItem.ProjectItems.Count > 0))
                {
                    // Drilldown on folders
                    result = GetProjectItemRecursive(projectItem.ProjectItems, projectFile, folder + projectItem.Name + "/");

                    // if the file does exist
                    if (result != null)
                    {
                        // break out of loop & Recursion
                        break;
                    }
                }
            }
            return result;
        }
        public static string MakeRelative(string fromAbsolutePath, string toDirectory)
        {
            if (!System.IO.Path.IsPathRooted(fromAbsolutePath))
                return fromAbsolutePath;  // we can't make a relative if it's not rooted(C:\)  so we'll assume we already have a relative path.

            if (!toDirectory[toDirectory.Length - 1].Equals("\\"))
                toDirectory += "\\";

            System.Uri from = new Uri(fromAbsolutePath);
            System.Uri to = new Uri(toDirectory);

            Uri relativeUri = to.MakeRelativeUri(from);
            return relativeUri.ToString();
        }


        public static Property SetValue(this Properties props, string name, object value)
        {
            foreach (Property p in props)
            {
                if (p.Name == name)
                {
                    p.Value = value;
                    return p;
                }
            }
            return null;
        }
        public static string AssemblyDirectory()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }
        public static string GetDefaultNamespace(this Project project)
        {
            return project.Properties.Item("DefaultNamespace").Value.ToString();
        }
        public static string GetProjectDirectory(this Project project)
        {
            return System.IO.Path.GetDirectoryName(project.FullName);
        }
        public static Project GetSelectedProject(this EnvDTE80.DTE2 dte)
        {
            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects.Length > 0)
            {
                return projects.GetValue(0) as Project;
            }
            return null;
        }
        public static Project GetSelectedProject(this DTE dte)
        {
            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects.Length > 0)
            {
                return projects.GetValue(0) as Project;
            }
            return null;
        }
    }
}
