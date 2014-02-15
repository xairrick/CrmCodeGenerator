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
        //public static bool CheckIfFileExistsInProject(ProjectItems projectItems, string projectFile, string folder = "")
        //{
        //    // initial value
        //    bool fileExists = false;

        //    projectFile = projectFile.Replace(@"\", "/");

        //    // iterate project items
        //    foreach (ProjectItem projectItem in projectItems)
        //    {
        //        // if the name matches
        //        if (folder + projectItem.Name == projectFile)
        //        {
        //            // abort this add, file already exists
        //            fileExists = true;

        //            // break out of loop
        //            break;
        //        }
        //        else if ((projectItem.ProjectItems != null) && (projectItem.ProjectItems.Count > 0))
        //        {
        //            // check if the file exists in the project
        //            fileExists = CheckIfFileExistsInProject(projectItem.ProjectItems,  projectFile,  folder + projectItem.Name + "/");  

        //            // if the file does exist
        //            if (fileExists)
        //            {
        //                // abort this add, file already exists
        //                fileExists = true;

        //                // break out of loop
        //                break;
        //            }
        //        }
        //    }
        //    return fileExists;
        //}

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
                    // check if the file exists in the project
                    result = GetProjectItemRecursive(projectItem.ProjectItems, projectFile, folder + projectItem.Name + "/");

                    // if the file does exist
                    if (result != null)
                    {
                        // break out of loop
                        break;
                    }
                }
            }
            return result;
        }
        private static object GetFullPath(ProjectItem projectItem)
        {
            if (projectItem.Document == null)
                return null;

            string fullname;
            var myType = projectItem.Document.GetType();
            try
            {
                fullname = projectItem.Document.FullName;
            }
            catch (Exception)
            {
                try
                {
                    fullname = System.IO.Path.Combine(projectItem.Document.Path, projectItem.Document.Name);
                }
                catch (Exception)
                {
                    return null;
                }
                
            }
            return fullname;
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
        public static Project GetSelectedProject(this EnvDTE.DTE dte)
        {
            Array projects = dte.ActiveSolutionProjects as Array;
            if (projects.Length > 0)
            {
                return projects.GetValue(0) as Project;
            }
            return null;
        }
        public static Project GetProject(this EnvDTE80.DTE2 dte, string UniqueName)
        {
            foreach (Project p in dte.Solution.Projects)
            {
                if (p.UniqueName.Equals(UniqueName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return p;
                }
            }
            return null;
        }
    }
}
