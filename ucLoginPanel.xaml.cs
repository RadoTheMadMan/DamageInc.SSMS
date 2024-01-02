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



    public partial class ucLoginPanel : System.Windows.Controls.UserControl
    {

        private string _UserName = "";
        private string _Password = "";
        private string _LoginLabelText = "Login";
        private string _UserNameLabelText = "Username: ";
        private string _PasswordLabelText = "Password: ";
        private bool _LoginCredentialsReady = false;
        private DBManager _manager = null;
        public string UserName { get { return _UserName; } set { _UserName = value; } }
        public string Password { get { return _Password; } set { _Password = value; } }
        public string LoginLabelText { get { return _LoginLabelText; } set { _LoginLabelText = value; } }
        public string UserNameLabelText { get { return _UserNameLabelText; } set { _UserNameLabelText= value; } } 
        public string PasswordLabelText { get { return _PasswordLabelText; } set { _PasswordLabelText = value; } }

        public DBManager manager { get { return _manager; } set { _manager = value; } }

        public ucLoginPanel()
        {
            if (Parent.GetType() == typeof(windowMain))
            {
                windowMain parent_window = (windowMain)Parent;
                _manager = parent_window.getManager();
            }
            _LoginLabelText = "Login";
            _UserNameLabelText = "Username: ";
            _PasswordLabelText = "Password: ";
            _UserName = "";
            _Password = "";
            if(!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(Password))
            {
                _LoginCredentialsReady = true;
            }
            else
            {
                _LoginCredentialsReady=false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucLoginPanel(string LoginLabel, string UserNameLabel, string PasswordLabel, DBManager Manager)
        {
            _manager = Manager;
            _LoginLabelText = LoginLabel;
            _UserNameLabelText = UserNameLabel;
            _PasswordLabelText = PasswordLabel;
            _UserName = "";
            _Password = "";
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password))
            {
                _LoginCredentialsReady = true;
            }
            else
            {
                _LoginCredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        public ucLoginPanel(string LoginLabel, string UserNameLabel, string PasswordLabel, string UserName, string Password, DBManager Manager)
        {
            _manager = Manager;
            _LoginLabelText = LoginLabel;
            _UserNameLabelText = UserNameLabel;
            _PasswordLabelText = PasswordLabel;
            _UserName = UserName;
            _Password = Password;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password))
            {
                _LoginCredentialsReady = true;
            }
            else
            {
                _LoginCredentialsReady = false;
            }
            InitializeComponent();
            this.txtPassword.Password = Password;
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _UserName = txtUserName.Text;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password))
            {
                _LoginCredentialsReady = true;
            }
            else
            {
                _LoginCredentialsReady = false;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _Password = txtPassword.Password;
            if (!String.IsNullOrEmpty(_UserName) && !String.IsNullOrEmpty(_Password))
            {
                _LoginCredentialsReady = true;
            }
            else
            {
                _LoginCredentialsReady = false;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if(_LoginCredentialsReady)
            {
                manager.Login(_UserName, _Password);
            }
        }
    }
}
