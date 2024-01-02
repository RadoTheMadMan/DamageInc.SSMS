using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DMGINC
{
    /// <summary>
    /// Interaction logic for ucLoginPanel.xaml
    /// This will have public properties to set credentials like Login and Password
    /// </summary>



    public partial class ucRegisterPanel : System.Windows.Controls.UserControl
    {

        private string _UserName = "";
        private string _Password = "";
        private string _DisplayName = "";
        private string _Email = "";
        private string _Phone = "";
        private Bitmap _ProfilePic = new Bitmap(16, 16);
        private bool _IsAdmin = false;
        private bool _IsWorker = false;
        private bool _IsClient = false;
        private string _RegisterLabelText = "Login";
        private string _UserNameLabelText = "Username: ";
        private string _DisplayNameLabelText = "Display Name: ";
        private string _EmailLabelText = "Email: ";
        private string _PasswordLabelText = "Password: ";
        private string _PhoneLabelText = "Phone: ";
        private string _ProfilePictureLabelText = "Profile Picture: ";
        private string _IsWorkerText = "Worker";
        private string _IsAdminText = "Admin";
        private string _IsClientText = "Client";
        
        private bool _RegisterCredentialsReady = false;
        private DBManager _manager = null;
        public string UserName { get { return _UserName; } set { _UserName = value; } }
        public string DisplayName { get { return _DisplayName; } set { _DisplayName = value; } }
        public string Email { get { return _Email; } set { _Email = value; } }
        public string Password { get { return _Password; } set { _Password = value; } }
        public string Phone { get { return _Phone; } set { _Phone = value; } }
        public Bitmap ProfilePic { get { return _ProfilePic; } set { _ProfilePic = value; } }
        public bool IsAdmin { get { return _IsAdmin; } set { _IsAdmin = value; } }
        public bool IsWorker { get { return _IsWorker; } set { _IsWorker = value; } }
        public bool IsClient { get { return _IsClient; } set { _IsClient = value; } }
        public string RegisterLabelText { get { return _RegisterLabelText; } set { _RegisterLabelText = value; } }
        public string UserNameLabelText { get { return _UserNameLabelText; } set { _UserNameLabelText= value; } }
        public string DisplayNameLabelText { get { return _DisplayNameLabelText; } set { _DisplayNameLabelText = value; } }
        public string EmailLabelText { get { return _EmailLabelText; } set { _EmailLabelText = value; } }
        public string PasswordLabelText { get { return _PasswordLabelText; } set { _PasswordLabelText = value; } }
        public string PhoneLabelText { get { return _PhoneLabelText; } set { _PhoneLabelText = value; } }
        public string ProfilePictureLabelText { get { return _ProfilePictureLabelText; } set { _ProfilePictureLabelText = value; } }
        public string IsWorkerText { get { return _IsWorkerText; } set { _IsWorkerText = value; } }
        public string IsAdminText { get { return _IsAdminText; } set { _IsWorkerText= value; } }
        public string IsClientText { get { return _IsClientText; } set { _IsClientText= value; } }

        public DBManager manager { get { return _manager; } set { _manager = value; } }

        public ucRegisterPanel()
        {
            if (Parent.GetType() == typeof(windowMain))
            {
                windowMain parent_window = (windowMain)Parent;
                _manager = parent_window.getManager();
            }
            _RegisterLabelText = "Register";
            _UserNameLabelText = "Username: ";
            _DisplayNameLabelText = "Display Name: ";
            _EmailLabelText = "Email: ";
            _PasswordLabelText = "Password: ";
            _PhoneLabelText = "Phone: ";
            _ProfilePictureLabelText = "Profile Picture: ";
            _IsAdminText = "Admin";
            _IsWorkerText = "Worker";
            _IsClientText = "Client";
            _UserName = "";
            _DisplayName = "";
            _Email = "";
            _Password = "";
            _Phone = "";
            _ProfilePic = new Bitmap(16,16);
            _IsAdmin = false;
            _IsWorker = false;
            _IsClient = false;
            if(!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady=false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucRegisterPanel(string RegisterLabel, string UserNameLabel, string DisplayNameLabel, string EmailLabel, string PasswordLabel, string PhoneLabel, string ProfilePicLabel, string IsAdminLabel, string IsWorkerLabel, string IsClientLabel, DBManager Manager)
        {
            _manager = Manager;
            _RegisterLabelText = RegisterLabel;
            _UserNameLabelText = UserNameLabel;
            _DisplayNameLabelText = DisplayNameLabel;
            _EmailLabelText = EmailLabel;
            _PasswordLabelText = PasswordLabel;
            _PhoneLabelText = PhoneLabel;
            _ProfilePictureLabelText = ProfilePicLabel;
            _IsAdminText = IsAdminLabel;
            _IsWorkerText = IsWorkerLabel;
            _IsClientText = IsClientLabel;
            _UserName = "";
            _DisplayName = "";
            _Email = "";
            _Password = "";
            _Phone = ""; 
            _ProfilePic = new Bitmap(16, 16);
            _IsAdmin = false;
            _IsWorker = false;
            _IsClient = false;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucRegisterPanel(string RegisterLabel, string UserNameLabel, string DisplayNameLabel, string EmailLabel, string PasswordLabel, string PhoneLabel, string ProfilePicLabel, string IsAdminLabel, string IsWorkerLabel, string IsClientLabel, string UserName, string DisplayName, string Email, string Password, string Phone, Bitmap ProfilePic, bool IsAdmin, bool IsWorker, bool IsClient, DBManager Manager)
        {
            _manager = Manager;
            _RegisterLabelText = RegisterLabel;
            _UserNameLabelText = UserNameLabel;
            _DisplayNameLabelText = DisplayNameLabel;
            _EmailLabelText = EmailLabel;
            _PasswordLabelText = PasswordLabel;
            _PhoneLabelText = PhoneLabel;
            _ProfilePictureLabelText = ProfilePicLabel;
            _IsAdminText = IsAdminLabel;
            _IsWorkerText = IsWorkerLabel;
            _IsClientText = IsClientLabel;
            _UserName = UserName;
            _DisplayName = DisplayName;
            _Email = Email;
            _Password = Password;
            _Phone = Phone; 
            _ProfilePic = ProfilePic;
            _IsAdmin = IsAdmin;
            _IsWorker = IsWorker;
            _IsClient = IsClient;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _UserName = txtUserName.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _Password = txtPassword.Password;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_RegisterCredentialsReady)
            {
                manager.Register(_UserName, _DisplayName, _Email, _Password, _Phone, _ProfilePic, _IsAdmin, _IsWorker, _IsClient);
            }
        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Phone = txtPhone.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Email = txtEmail.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void txtDisplayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _DisplayName = txtDisplayName.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void rbIsAdmin_Checked(object sender, RoutedEventArgs e)
        {
            _IsAdmin = (bool)rbIsAdmin.IsChecked;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void rbIsWorker_Checked(object sender, RoutedEventArgs e)
        {
            _IsWorker = (bool)rbIsWorker.IsChecked;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }

        private void rbIsClient_Checked(object sender, RoutedEventArgs e)
        {
            _IsClient = (bool)rbIsClient.IsChecked;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone) && _ProfilePic != null)
            {
                _RegisterCredentialsReady = true;
            }
            else
            {
                _RegisterCredentialsReady = false;
            }
        }
    }
}
