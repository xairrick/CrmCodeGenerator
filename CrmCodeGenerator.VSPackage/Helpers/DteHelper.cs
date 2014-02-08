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
            return GetProjectItemRecusive(project.ProjectItems, projectFile);
        }
        private static ProjectItem GetProjectItemRecusive(ProjectItems projectItems, string projectFile, string folder = "")
        {
            // initial value
            ProjectItem result = null;

            projectFile = projectFile.Replace(@"\", "/");

            // iterate project items
            foreach (ProjectItem projectItem in projectItems)
            {
                // if the name matches
                if (folder + projectItem.Name == projectFile)
                {
                    // abort this add, file already exists
                    result = projectItem;

                    // break out of loop
                    break;
                }
                else if ((projectItem.ProjectItems != null) && (projectItem.ProjectItems.Count > 0))
                {
                    // check if the file exists in the project
                    result = GetProjectItemRecusive(projectItem.ProjectItems, projectFile, folder + projectItem.Name + "/");

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
