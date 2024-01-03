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



    public partial class ucUserCustomisationPanel : System.Windows.Controls.UserControl
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
        private string _UserCustomisationLabelText = "Login";
        private string _UserNameLabelText = "Username: ";
        private string _DisplayNameLabelText = "Display Name: ";
        private string _EmailLabelText = "Email: ";
        private string _PasswordLabelText = "Password: ";
        private string _PhoneLabelText = "Phone: ";
        private string _ProfilePictureLabelText = "Profile Picture: ";

        
        private bool _CredentialsReady = false;
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
        public string UserCustomisationLabelText { get { return _UserCustomisationLabelText; } set { _UserCustomisationLabelText = value; } }
        public string UserNameLabelText { get { return _UserNameLabelText; } set { _UserNameLabelText= value; } }
        public string DisplayNameLabelText { get { return _DisplayNameLabelText; } set { _DisplayNameLabelText = value; } }
        public string EmailLabelText { get { return _EmailLabelText; } set { _EmailLabelText = value; } }
        public string PasswordLabelText { get { return _PasswordLabelText; } set { _PasswordLabelText = value; } }
        public string PhoneLabelText { get { return _PhoneLabelText; } set { _PhoneLabelText = value; } }
        public string ProfilePictureLabelText { get { return _ProfilePictureLabelText; } set { _ProfilePictureLabelText = value; } }

        public DBManager manager { get { return _manager; } set { _manager = value; } }

        public ucUserCustomisationPanel()
        {
            if (Parent.GetType() == typeof(windowMain))
            {
                windowMain parent_window = (windowMain)Parent;
                _manager = parent_window.getManager();
            }
            _UserCustomisationLabelText = "Register";
            _UserNameLabelText = "Username: ";
            _DisplayNameLabelText = "Display Name: ";
            _EmailLabelText = "Email: ";
            _PasswordLabelText = "Password: ";
            _PhoneLabelText = "Phone: ";
            _ProfilePictureLabelText = "Profile Picture: ";
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
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady=false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucUserCustomisationPanel(string UserCustomisationLabel, string UserNameLabel, string DisplayNameLabel, string EmailLabel, string PasswordLabel, string PhoneLabel, string ProfilePicLabel, DBManager Manager)
        {
            _manager = Manager;
            _UserCustomisationLabelText = UserCustomisationLabel;
            _UserNameLabelText = UserNameLabel;
            _DisplayNameLabelText = DisplayNameLabel;
            _EmailLabelText = EmailLabel;
            _PasswordLabelText = PasswordLabel;
            _PhoneLabelText = PhoneLabel;
            _ProfilePictureLabelText = ProfilePicLabel;
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
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucUserCustomisationPanel(string UserCustomisationLabel, string UserNameLabel, string DisplayNameLabel, string EmailLabel, string PasswordLabel, string PhoneLabel, string ProfilePicLabel, string UserName, string DisplayName, string Email, string Password, string Phone, Bitmap ProfilePic, bool IsAdmin, bool IsWorker, bool IsClient, DBManager Manager)
        {
            _manager = Manager;
            _UserCustomisationLabelText = UserCustomisationLabel;
            _UserNameLabelText = UserNameLabel;
            _DisplayNameLabelText = DisplayNameLabel;
            _EmailLabelText = EmailLabel;
            _PasswordLabelText = PasswordLabel;
            _PhoneLabelText = PhoneLabel;
            _ProfilePictureLabelText = ProfilePicLabel;
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
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _UserName = txtUserName.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _Password = txtPassword.Password;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
        }

        private void btnDeleteCurrentUser_Click(object sender, RoutedEventArgs e)
        {
            if (_CredentialsReady)
            {
                manager.DeleteCurrentUser();
            }
        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Phone = txtPhone.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            _Email = txtEmail.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
        }

        private void txtDisplayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _DisplayName = txtDisplayName.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password) &&
               !String.IsNullOrEmpty(_Email) && !String.IsNullOrEmpty(_Phone))
            {
                _CredentialsReady = true;
            }
            else
            {
                _CredentialsReady = false;
            }
        }

       
        private void imgProfilePic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BitmapImage source;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select a profile picture..";
            dlg.Filter = "image files |*.png*,*.jpg*,*.gif*,*.jfif*,*.tiff*";
            if(dlg.ShowDialog() == DialogResult.OK) { 
                source = new BitmapImage(new Uri(dlg.FileName,UriKind.Absolute));
            }
        }

        private void btnUpdateUserData_Click(object sender, RoutedEventArgs e)
        {
            if (_CredentialsReady)
            {
                manager.UpdateCurrentUser(UserName,DisplayName,Email,Password,Phone,ProfilePic);
            }
        }

        private void btnRevertUserData_Click(object sender, RoutedEventArgs e)
        {
            User currentUser = (User)manager.CurrentUser;
            UserName = currentUser.UserName;
            DisplayName = currentUser.DisplayName;
            Email = currentUser.Email;
            Password = currentUser.Password;
            Phone = currentUser.Phone;
            ProfilePic = currentUser.ProfilePic;
            IsAdmin = currentUser.IsAdmin;
            IsWorker = currentUser.IsWorker;
            IsClient = currentUser.IsClient;
        }
    }
}
