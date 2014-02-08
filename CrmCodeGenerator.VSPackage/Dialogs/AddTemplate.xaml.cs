using CrmCodeGenerator.VSPackage.Helpers;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrmCodeGenerator.VSPackage.Dialogs
{
    /// <summary>
    /// Interaction logic for AddTemplate.xaml
    /// </summary>
    public partial class AddTemplate : System.Windows.Window
    {
        private EnvDTE80.DTE2 dte; 
        private EnvDTE.Project project;
        private string templateSamplesPath;
        private AddTemplateProp props;

        public AddTemplate(EnvDTE80.DTE2 dte, Project project)
        {
            InitializeComponent();
            this.dte = dte;
            this.project = project;


            props = new AddTemplateProp();
            this.DataContext = props;

            templateSamplesPath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates");
            var dir = new DirectoryInfo(templateSamplesPath);
            props.TemplateList = new ObservableCollection<String>(dir.GetFiles().Select(x => x.Name).Where(x => !x.Equals("Blank.tt")).ToArray());
            props.Template = "CrmSchema.tt";
            props.Folder = project.GetProjectDirectory();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DefaultTemplate.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var templatePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(project.GetProjectDirectory(), props.Template));  //GetFullpath removes un-needed relative paths  (ie if you are putting something in the solution directory)


            //if (System.IO.File.Exists(templatePath))
            
           var defaultTemplatePath = System.IO.Path.Combine(templateSamplesPath, this.DefaultTemplate.SelectedValue.ToString());
            if (!System.IO.File.Exists(defaultTemplatePath))
            {
                throw new UserException("T4Path: " + defaultTemplatePath + " is missing or you can access it.");
            }
            UpdateStatus("Copying Template to project....", true);
            var dir = System.IO.Path.GetDirectoryName(templatePath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            // When you add a TT file to visual studio, it will try to automatically compile it, 
            // if there is error (and there will be error because we have custom generator) 
            // the error will persit until you close Visual Studio. The solution is to add 
            // a blank file, then overwrite it
            // http://stackoverflow.com/questions/17993874/add-template-file-without-custom-tool-to-project-programmatically
            var blankTemplatePath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\Blank.tt");
            System.IO.File.Copy(blankTemplatePath, templatePath, true);

            Console.Write("Adding " + templatePath + " to project");
            var p = project.ProjectItems.AddFromFile(templatePath);
            p.Properties.SetValue("CustomTool", typeof(CrmCodeGenerator2011).Name);

            System.IO.File.Copy(defaultTemplatePath, templatePath, true);

            this.Close();
        }
        private void UpdateStatus(string param1, bool param2)
        {
            // Blah
        }
    }

    public class AddTemplateProp : INotifyPropertyChanged
    {
        public AddTemplateProp()
        {
            Dirty = false;
        }

        // boiler-plate
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        //protected bool SetField<T>(ref T field, T value, string propertyName)
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
       
        
        private string _Template;
        public string Template
        {
            get
            {
                return _Template;
            }
            set
            {
                SetField(ref _Template, value);
                NewTemplate = !System.IO.File.Exists(System.IO.Path.Combine(_Folder, _Template));
            }
        }
        private string _Folder = "";
        public string Folder
        {
            get
            {
                return _Folder;
            }
            set
            {
                SetField(ref _Folder, value);
            }
        }

        private bool _NewTemplate;
        public bool NewTemplate
        {
            get
            {
                return _NewTemplate;
            }
            set
            {
                SetField(ref _NewTemplate, value);
            }
        }
        
        private string _OutputPath;
        public string OutputPath
        {
            get
            {
                return _OutputPath;
            }
            set
            {
                SetField(ref _OutputPath, value);
            }
        }

        private ObservableCollection<String> _TemplateList = new ObservableCollection<String>();
        public ObservableCollection<String> TemplateList
        {
            get
            {
                return _TemplateList;
            }
            set
            {
                SetField(ref _TemplateList, value);
            }
        }
        public bool Dirty { get; set; }


    }
}
