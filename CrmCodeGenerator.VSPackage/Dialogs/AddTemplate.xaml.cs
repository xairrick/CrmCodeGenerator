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
        private AddTemplateProp _Props;
        public AddTemplateProp Props
        {
            get
            {
                return _Props;
            }
        }

        private bool _Canceled = true;
        public bool Canceled {
            get
            {
                return _Canceled;
            }
        }

        public AddTemplate(EnvDTE80.DTE2 dte, Project project)
        {
            InitializeComponent();

            var main = dte.GetMainWindow();
            this.Owner = main;
            //Loaded += delegate { this.CenterWindow(main); };

            _Props = new AddTemplateProp();
            this.DataContext = Props;

            var samplesPath = System.IO.Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates");
            var dir = new DirectoryInfo(samplesPath);
            Props.TemplateList = new ObservableCollection<String>(dir.GetFiles().Select(x => x.Name).Where(x => !x.Equals("Blank.tt")).ToArray());
            Props.Template = "CrmSchema.tt";
            Props.Folder = project.GetProjectDirectory();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HideMinimizeAndMaximizeButtons();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DefaultTemplate.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            _Canceled = true;
            this.Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;
            _Canceled = false;
            this.Close();
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
