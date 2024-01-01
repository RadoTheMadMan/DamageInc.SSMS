using ADOX;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using Microsoft.Win32;
using Microsoft.Reporting;
using Microsoft.Reporting.WinForms;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Drawing.Image;
using DMGINC.Properties.DataSources;
using System.Windows.Threading;

namespace DMGINC
{
    /// <summary>
    /// Interaction logic for frmMain.xaml
    /// </summary>
    /// 

    public static class ImageDecoderEncoder
    {
        public static Bitmap DecodeImage(byte[] array)
        {
            try
            {
                MemoryStream ms = new MemoryStream(array);
                ms.Position = 0;
                Bitmap image = (Bitmap)Bitmap.FromStream(ms);
                return image;
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap DecodeImage(string Base64)
        {
            try
            {
                byte[] array = Convert.FromBase64String(Base64);
                MemoryStream ms = new MemoryStream(array);
                ms.Position = 0;
                Bitmap image = (Bitmap)Bitmap.FromStream(ms);
                return image;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] EncodeImageToByte(Bitmap image)
        {
            try
            {
                byte[] result;
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);
                result = stream.ToArray();
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static string EncodeImageToBase64(Bitmap image)
        {
            try
            {
                string base64result = "";
                byte[] result;
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);
                result = stream.ToArray();
                base64result = Convert.ToBase64String(result);
                return base64result;
            }
            catch
            {
                return null;
            }
        }
    }

    public struct User
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int Balance { get; set; }
        public DateTime RegisterDate { get; set; }
        public Bitmap ProfilePic { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsWorker { get; set; }
        public bool IsClient { get; set; }

    }

    public class DBManager
    {
        public static string _ConnString = "";
        public static string _CompanyName = "";
        private static string _ReportDefinitionPath = "";
        public static List<string> _OrderReports = new List<string>();
        public static List<string> _DeliveryReports = new List<string>();
        public static Nullable<User> _CurrentUser;
        public static bool _EnableBulkInsert = false;
        public static bool _EnableBulkUpdate = false;
        public static bool _EnableBulkDelete = false;
        public Dictionary<string, DataSet> Tables = new Dictionary<string, DataSet>()
        {
            { "Select a table", new DataSet() },
            { "Users", new DataSet() },
            { "Clients", new DataSet() },
            { "ProductCategories", new DataSet() },
            { "ProductBrands" , new DataSet() },
            { "OrderTypes", new DataSet() },
            { "PaymentMethods", new DataSet() },
            { "DiagnosticTypes", new DataSet() },
            { "DeliveryServices", new DataSet() },
            { "Products", new DataSet() },
            { "ProductImages", new DataSet() },
            { "ProductOrders", new DataSet() },
            { "OrderDeliveries", new DataSet() }
        };
        public Dictionary<string, List<string>> Criterias = new Dictionary<string, List<string>>()
        {
            { "Select a criteria", new List<string>() },
            { "Users", new List<string>() { "ID", "UserName", "DisplayName", "Email", "Phone", "Balance", "Date" } },
            { "Clients", new List<string>() { "ID", "UserID","UserName" ,"Name", "Email", "Phone","Address", "Balance"} },
            { "ProductCategories", new List<string>() { "ID", "Name"} },
            { "ProductBrands", new List<string>() { "ID", "Name"} },
            { "OrderTypes", new List<string>() { "ID", "Name"} },
            { "PaymentMethods", new List<string>() { "ID", "Name"} },
            { "DiagnosticTypes", new List<string>() { "ID", "Name", "Price"} },
            { "DeliveryServices", new List<string>() { "ID", "Name", "Price"} },
            { "Products", new List<string> () {"ID", "CategoryID", "CategoryName", "BrandID", "BrandName","Name","Description","Quantity", "Price", "ArtID", "SerialNumber", "StorageLocation", "Date"} },
            { "ProductImages", new List<string> () {"ImageName","ProductID", "ProductName","ProductDescription","ProductQuantity", "ProductPrice", "ProductArtID", "ProductSerialNumber", "ProductStorageLocation", "ProductDate"} },
            { "ProductOrders", new List<string> () {"ID","ProductID", "ProductName", "OrderTypeID", "ReplacementProductID", "ReplacementProductName", "DiagnosticTypeID","Quantity","Price", "ClientID", "ClientName", "UserID", "UserDisplayName" , "Date","Status"} },
            { "OrderDeliveries", new List<string> () {"ID","OrderID",  "DeliveryServiceID", "DeliveryServiceName", "PaymentMethodID", "PaymentMethodName", "DeliveryCargoID", "Price", "ClientID", "ClientName","OrderDate", "Date","Status"} }
        };

        public Dictionary<string, int> ProductOrderStatusList = new Dictionary<string, int>()
        {
            { "Awaiting processing", 0 },
            { "Prepaid", 1 },
            { "Paid on delivery", 2 },
            { "Paid directly", 3 },
            { "Generating report", 4 },
            { "Generating invoice", 5 },
            { "Processing", 6 },
            { "Cancelled", 7 },
            { "Refunded", 8 },
            { "Completed", 9 },
            {"Product replacement", 10 },
            {"Product repair", 11 }
        };

        public Dictionary<string, int> OrderDeliveryStatusList = new Dictionary<string, int>()
        {
            { "Awaiting delivery", 0 },
            { "Prepaid", 1 },
            { "Paid directly", 2 },
            { "Paid on completion", 3 },
            { "Generating report", 4 },
            { "Generating invoice", 5 },
            { "On the move", 6 },
            { "Cancelled", 7 },
            { "Refunded", 8 },
            { "Completed", 9 }
        };

        public Dictionary<string, ObservableCollection<DataRow>> BulkAddList = new Dictionary<string, ObservableCollection<DataRow>>()
        {
            { "Select a table", new ObservableCollection<DataRow>() },
            { "Users", new ObservableCollection<DataRow>() },
            { "Clients", new ObservableCollection<DataRow>() },
            { "ProductCategories", new ObservableCollection<DataRow>() },
            { "ProductBrands" , new ObservableCollection<DataRow>() },
            { "OrderTypes", new ObservableCollection<DataRow>() },
            { "PaymentMethods", new ObservableCollection<DataRow>() },
            { "DiagnosticTypes", new ObservableCollection<DataRow>() },
            { "DeliveryServices", new ObservableCollection<DataRow>() },
            { "Products", new ObservableCollection<DataRow>() },
            { "ProductImages", new ObservableCollection<DataRow>() },
            { "ProductOrders", new ObservableCollection<DataRow>() },
            { "OrderDeliveries", new ObservableCollection<DataRow>() }
        };

        public Dictionary<string, ObservableCollection<DataRow>> BulkUpdateList = new Dictionary<string, ObservableCollection<DataRow>>()
        {
            { "Select a table", new ObservableCollection<DataRow>() },
            { "Users", new ObservableCollection<DataRow>() },
            { "Clients", new ObservableCollection<DataRow>() },
            { "ProductCategories", new ObservableCollection<DataRow>() },
            { "ProductBrands" , new ObservableCollection<DataRow>() },
            { "OrderTypes", new ObservableCollection<DataRow>() },
            { "PaymentMethods", new ObservableCollection<DataRow>() },
            { "DiagnosticTypes", new ObservableCollection<DataRow>() },
            { "DeliveryServices", new ObservableCollection<DataRow>() },
            { "Products", new ObservableCollection<DataRow>() },
            { "ProductImages", new ObservableCollection<DataRow>() },
            { "ProductOrders", new ObservableCollection<DataRow>() },
            { "OrderDeliveries", new ObservableCollection<DataRow>() }
        };

        public Dictionary<string, ObservableCollection<DataRow>> BulkDeleteList = new Dictionary<string, ObservableCollection<DataRow>>()
        {
            { "Select a table", new ObservableCollection<DataRow>() },
            { "Users", new ObservableCollection<DataRow>() },
            { "Clients", new ObservableCollection<DataRow>() },
            { "ProductCategories", new ObservableCollection<DataRow>() },
            { "ProductBrands" , new ObservableCollection<DataRow>() },
            { "OrderTypes", new ObservableCollection<DataRow>() },
            { "PaymentMethods", new ObservableCollection<DataRow>() },
            { "DiagnosticTypes", new ObservableCollection<DataRow>() },
            { "DeliveryServices", new ObservableCollection<DataRow>() },
            { "Products", new ObservableCollection<DataRow>() },
            { "ProductImages", new ObservableCollection<DataRow>() },
            { "ProductOrders", new ObservableCollection<DataRow>() },
            { "OrderDeliveries", new ObservableCollection<DataRow>() }
        };

        public string ConnString { get { return _ConnString; } set { _ConnString = value; } }
        public string CompanyName { get { return _CompanyName; } set { _CompanyName = value; } }

        public Nullable<User> CurrentUser { get { return _CurrentUser; } set { _CurrentUser =  value; } }

        public string ReportDefinitionPath { get { return _ReportDefinitionPath; } set { _ReportDefinitionPath = value; } }

        public List<string> OrderReports { get { return _OrderReports; } set { _OrderReports = value; } }

        public List<string> DeliveryReports { get { return _DeliveryReports; } set { _DeliveryReports = value; } }

        public bool EnableBulkInsert { get { return _EnableBulkInsert; } set { _EnableBulkInsert = value; } }

        public bool EnableBulkUpdate { get { return _EnableBulkUpdate; } set { _EnableBulkUpdate = value; } }

        public bool EnableBulkDelete { get { return _EnableBulkDelete; } set { _EnableBulkDelete = value; } }

        public DBManager()
        {
            ReportDefinitionPath = ConfigurationManager.AppSettings["REPORT_SOURCES_FOLDER"];
            CompanyName = ConfigurationManager.AppSettings["COMPANY_NAME"];
            bool.TryParse(ConfigurationManager.AppSettings["BULK_INSERT"], out _EnableBulkInsert);
            bool.TryParse(ConfigurationManager.AppSettings["BULK_UPDATE"], out _EnableBulkUpdate);
            bool.TryParse(ConfigurationManager.AppSettings["BULK_DELETE"], out _EnableBulkDelete);
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Server"] = $".\\{ConfigurationManager.AppSettings["DB_ADDRESS"]},{ConfigurationManager.AppSettings["DB_PORT"]}";
            builder["User ID"] = ConfigurationManager.AppSettings["DB_USER"];
            builder["Initial Catalog"] = ConfigurationManager.AppSettings["DB_NAME"];
            builder["Password"] = ConfigurationManager.AppSettings["DB_PASSWORD"];
            _ConnString = builder.ConnectionString.ToString();
            if (ConfigurationManager.ConnectionStrings.Count > 0)
            {
                Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                conf.ConnectionStrings.ConnectionStrings.Clear();
                conf.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("SERVER_CONN", _ConnString));
                conf.Save(ConfigurationSaveMode.Full);
            }
        }



        public void LoadReportDefinitions()
        {
            try
            {
                DirectoryInfo reportDirectoryInfo = new DirectoryInfo(System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory().ToString() + ReportDefinitionPath));
                if (reportDirectoryInfo.Exists)
                {
                    foreach (FileInfo file in reportDirectoryInfo.GetFiles())
                    {
                        if (file.Exists && file.Extension == ".rdlc")
                        {
                            if (file.Name.Contains("OrderReport") || file.Name.Contains("OrderInvoiceReport"))
                            {
                                _OrderReports.Add(System.IO.Path.GetFileNameWithoutExtension(file.Name));
                            }
                            else if (file.Name.Contains("DeliveryReport") || file.Name.Contains("DeliveryInvoiceReport"))
                            {
                                _DeliveryReports.Add(System.IO.Path.GetFileNameWithoutExtension(file.Name));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void GenerateReportDataSet(string Term, bool LookBelowTerm, string Table, string Criteria, out DataSet ds)
        {
            DataSet result = new DataSet();
            if (Table == "ProductOrders")
            {
                if (Criteria == "ID")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderByIDForReports(IntTerm, Table, out result);
                }
                if (Criteria == "ProductID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "ProductName")
                {
                    GetOrderByProductNameForReports(Term, Table, out result);
                }
                if (Criteria == "OrderTypeID")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderByTypeIDForReports(IntTerm, Table, out result);
                }
                if (Criteria == "ReplacementProductID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "ReplacementProductName")
                {
                    GetOrderByReplacementProductNameForReports(Term, Table, out result);
                }
                if (Criteria == "DiagnosticTypeID")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderByDiagnosticTypeIDForReports(IntTerm, Table, out result);
                }
                else if (Criteria == "Quantity")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    bool LookBelowQuantity = LookBelowTerm;
                    GetOrderByQuantityForReports(IntTerm, LookBelowQuantity, Table, out result);
                }
                else if (Criteria == "Price")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    bool LookBelowPrice = LookBelowTerm;
                    GetOrderByPriceForReports(IntTerm, LookBelowPrice, Table, out result);
                }
                if (Criteria == "ClientID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "ClientName")
                {
                    GetOrderByClientNameForReports(Term, Table, out result);
                }
                if (Criteria == "UserID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "UserDisplayName")
                {
                    GetOrderByUserDisplayNameForReports(Term, Table, out result);
                }
                else if (Criteria == "Date")
                {
                    DateTime TargetDateTime = DateTime.Now;
                    DateTime.TryParse(Term, out TargetDateTime);
                    bool LookBeforeDate = LookBelowTerm;
                    GetOrderByDateForReports(TargetDateTime, LookBeforeDate, Table, out result);
                }
                else if (Criteria == "Status")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderByStatusForReports(IntTerm, Table, out result);
                }
            }
            else if (Table == "OrderDeliveries")
            {
                if (Criteria == "ID")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderDeliveryByIDForReports(IntTerm, Table, out result);
                }
                else if (Criteria == "OrderID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "DeliveryServiceID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "DeliveryServiceName")
                {
                    GetOrderDeliveryByServiceNameForReports(Term, Table, out result);
                }
                else if (Criteria == "DeliveryCargoID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "PaymentMethodID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "PaymentMethodProductName")
                {
                    GetOrderDeliveryByPaymentMethodNameForReports(Term, Table, out result);
                }
                else if (Criteria == "Price")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    bool LookBelowPrice = LookBelowTerm;
                    GetOrderDeliveryByPriceForReports(IntTerm, LookBelowPrice, Table, out result);
                }
                else if (Criteria == "ClientID")
                {
                    System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                else if (Criteria == "ClientName")
                {
                    GetOrderDeliveryByClientNameForReports(Term, Table, out result);
                }
                else if (Criteria == "OrderDate")
                {
                    DateTime TargetDateTime = DateTime.Now;
                    DateTime.TryParse(Term, out TargetDateTime);
                    bool LookBeforeDate = LookBelowTerm;
                    GetOrderDeliveryByOrderDateForReports(TargetDateTime, LookBeforeDate, Table, out result);
                }
                else if (Criteria == "Date")
                {
                    DateTime TargetDateTime = DateTime.Now;
                    DateTime.TryParse(Term, out TargetDateTime);
                    bool LookBeforeDate = LookBelowTerm;
                    GetOrderDeliveryByDateForReports(TargetDateTime, LookBeforeDate, Table, out result);
                }
                else if (Criteria == "Status")
                {
                    int IntTerm = 0;
                    Int32.TryParse(Term, out IntTerm);
                    GetOrderDeliveryByStatusForReports(IntTerm, Table, out result);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("The selected table or criteria aren't supported for report generation.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            ds = result;
        }
        public void ExecuteStoredProcedure(string ProcedureName, string Table, List<SqlParameter> parameters, DataGrid grid)
        {
            if (!String.IsNullOrEmpty(ProcedureName) && parameters != null)
            {
                try
                {
                    SqlConnection conn = new SqlConnection(ConnString);
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        string cmdText = ProcedureName;
                        SqlCommand cmd = new SqlCommand(cmdText, conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters.ToArray());
                        SqlDataReader rd = cmd.ExecuteReader();
                        DataTable table = new DataTable();
                        table.TableName = Table;
                        table.Load(rd);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            Tables[Table].Tables.Clear();
                            Tables[Table].Tables.Add(table);
                        }
                        else
                        {
                            Tables[Table].Tables.Add(table);
                        }
                        grid.ItemsSource = Tables[Table].Tables[0].DefaultView;
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void ExecuteStoredProcedure(string ProcedureName, string Table, List<SqlParameter> parameters, out DataSet targetds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ProcedureName) && parameters != null)
            {
                try
                {
                    SqlConnection conn = new SqlConnection(ConnString);
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        string cmdText = ProcedureName;
                        SqlCommand cmd = new SqlCommand(cmdText, conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters.ToArray());
                        SqlDataReader rd = cmd.ExecuteReader();
                        DataTable table = new DataTable();
                        table.TableName = Table;
                        table.Load(rd);
                        if (resultds.Tables.Count > 0)
                        {
                            resultds.Tables.Clear();
                            resultds.Tables.Add(table);
                        }
                        else
                        {
                            resultds.Tables.Add(table);
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            targetds = resultds;
        }

        public void RegisterUser(string UserName, string DisplayName, string Email, string Password, string Phone, Bitmap ProfilePic, bool IsAdmin, bool IsWorker, bool IsClient, string TableName, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(DisplayName) && !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Phone) &&
                IsAdmin != null && IsWorker != null && !IsClient != null && ProfilePic != null && !String.IsNullOrEmpty(TableName) && targetgrid != null)
            {
                string ProcedureName = "RegisterUser";
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProfilePic);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@displayname", DisplayName));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@password", Password));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@registerdate", DateTime.Now));
                parameters.Add(new SqlParameter("@profilepic", imgbin));
                parameters.Add(new SqlParameter("@isadmin", IsAdmin));
                parameters.Add(new SqlParameter("@isclient", IsClient));
                parameters.Add(new SqlParameter("@isworker", IsWorker));
                ExecuteStoredProcedure(ProcedureName, TableName, parameters, targetgrid);
            }
        }

        public void RegisterAdmin(string UserName, string DisplayName, string Email, string Password, string Phone, Bitmap ProfilePic, string TableName, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(DisplayName) && !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Phone) &&
                ProfilePic != null && !String.IsNullOrEmpty(TableName) && targetgrid != null)
            {
                string ProcedureName = "RegisterAdmin";
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProfilePic);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@displayname", DisplayName));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@password", Password));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@registerdate", DateTime.Now));
                parameters.Add(new SqlParameter("@profilepic", imgbin));
                ExecuteStoredProcedure(ProcedureName, TableName, parameters, targetgrid);
            }
        }

        public void RegisterWorker(string UserName, string DisplayName, string Email, string Password, string Phone, Bitmap ProfilePic, string TableName, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(DisplayName) && !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Phone) &&
                ProfilePic != null && !String.IsNullOrEmpty(TableName) && targetgrid != null)
            {
                string ProcedureName = "RegisterWorker";
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProfilePic);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@displayname", DisplayName));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@password", Password));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@registerdate", DateTime.Now));
                parameters.Add(new SqlParameter("@profilepic", imgbin));
                ExecuteStoredProcedure(ProcedureName, TableName, parameters, targetgrid);
            }
        }

        public void RegisterClient(string UserName, string DisplayName, string Email, string Password, string Phone, string Address, Bitmap ProfilePic, string TableName, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(DisplayName) && !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Phone) &&
                !String.IsNullOrEmpty(Address) && ProfilePic != null && !String.IsNullOrEmpty(TableName) && targetgrid != null)
            {
                string ProcedureName = "RegisterClient";
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProfilePic);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@displayname", DisplayName));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@password", Password));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@address", Address));
                parameters.Add(new SqlParameter("@registerdate", DateTime.Now));
                parameters.Add(new SqlParameter("@profilepic", imgbin));
                ExecuteStoredProcedure(ProcedureName, TableName, parameters, targetgrid);
            }
        }

        public void AddClientWithoutRegistering(string ClientName, string Email, string Phone, string Address, Bitmap ProfilePic, string TableName, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Phone) &&
                !String.IsNullOrEmpty(Address) && ProfilePic != null && !String.IsNullOrEmpty(TableName) && targetgrid != null)
            {
                string ProcedureName = "AddClientWithoutRegistering";
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProfilePic);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@address", Address));
                parameters.Add(new SqlParameter("@profilepic", imgbin));
                ExecuteStoredProcedure(ProcedureName, TableName, parameters, targetgrid);
            }
        }

        public void AddDeliveryService(string Name, int Price, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && Price != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddDeliveryService";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@price", Price));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddDiagnosticType(string Name, int Price, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && Price != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddDiagnosticType";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@price", Price));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddOrderType(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddOrderType";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typename", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddPaymentMethod(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddPaymentMethod";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@method_name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProductCategory(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddProductCategory";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryname", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProductBrand(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddProductBrand";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@brandname", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProductImage(int TargetProductID, string ImageName, Bitmap ProductImage, string Table, DataGrid targetgrid)
        {
            if (TargetProductID != null && ProductImage != null && !String.IsNullOrEmpty(ImageName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProductImage);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                string ProcedureName = "AddProductImage";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", TargetProductID));
                parameters.Add(new SqlParameter("@image_name", ImageName));
                parameters.Add(new SqlParameter("@productimage", imgbin));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProductImageExtended(string TargetProductName, string ImageName, Bitmap ProductImage, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(TargetProductName) && ProductImage != null && !String.IsNullOrEmpty(ImageName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                byte[] imgbin = ImageDecoderEncoder.EncodeImageToByte(ProductImage);
                if (imgbin == null)
                {
                    Bitmap DefaultUserImage = new Bitmap(16, 16);
                    imgbin = ImageDecoderEncoder.EncodeImageToByte(DefaultUserImage);
                }
                string ProcedureName = "AddProductImageExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", TargetProductName));
                parameters.Add(new SqlParameter("@image_name", ImageName));
                parameters.Add(new SqlParameter("@productimage", imgbin));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProduct(int CategoryID, int BrandID, string ProductName, string ProductDescription, int Quantity, int Price, string ArticuleID, string SerialNumber, string StorageLocation, string Table, DataGrid targetgrid)
        {
            if (CategoryID != null && BrandID != null && !String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ProductDescription) && Quantity != null && Price != null && !String.IsNullOrEmpty(ArticuleID)
              && !String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(StorageLocation) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddProduct";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryid", CategoryID));
                parameters.Add(new SqlParameter("@brandid", BrandID));
                parameters.Add(new SqlParameter("@name", ProductName));
                parameters.Add(new SqlParameter("@description", ProductDescription));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@artid", ArticuleID));
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                parameters.Add(new SqlParameter("@storagelocation", StorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddProductExtended(string CategoryName, string BrandName, string ProductName, string ProductDescription, int Quantity, int Price, string ArticuleID, string SerialNumber, string StorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(CategoryName) && !String.IsNullOrEmpty(BrandName) && !String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ProductDescription) && Quantity != null && Price != null && !String.IsNullOrEmpty(ArticuleID)
              && !String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(StorageLocation) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddProductExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryname", CategoryName));
                parameters.Add(new SqlParameter("@brandname", BrandName));
                parameters.Add(new SqlParameter("@name", ProductName));
                parameters.Add(new SqlParameter("@description", ProductDescription));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@artid", ArticuleID));
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                parameters.Add(new SqlParameter("@storagelocation", StorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddOrder(int ProductID, int OrderTypeID, int ReplacementProductID, int DiagnosticTypeID, int DesiredQuantity, int Price, int ClientID, int UserID, bool SetTotalPrice, string Table, DataGrid targetgrid)
        {
            if (ProductID != null && OrderTypeID != null && ReplacementProductID != null && DiagnosticTypeID != null && DesiredQuantity != null && Price != null && ClientID != null &&
              UserID != null && SetTotalPrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddOrder";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", ProductID));
                parameters.Add(new SqlParameter("@ordertypeid", OrderTypeID));
                parameters.Add(new SqlParameter("@replacementproductid", ReplacementProductID));
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                parameters.Add(new SqlParameter("@desiredquantity", DesiredQuantity));
                parameters.Add(new SqlParameter("@desiredprice", Price));
                parameters.Add(new SqlParameter("@clientid", ClientID));
                parameters.Add(new SqlParameter("@userid", UserID));
                parameters.Add(new SqlParameter("@settotalprice", SetTotalPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddOrderExtended(string ProductName, string OrderTypeName, string ReplacementProductName, string DiagnosticTypeName, int DesiredQuantity, int Price, string ClientName, string UserDisplayName, bool SetTotalPrice, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(OrderTypeName) && !String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(DiagnosticTypeName) && DesiredQuantity != null && Price != null && !String.IsNullOrEmpty(ClientName)
              && !String.IsNullOrEmpty(UserDisplayName) && SetTotalPrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddOrderExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@typename", OrderTypeName));
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                parameters.Add(new SqlParameter("@diagnostictypename", DiagnosticTypeName));
                parameters.Add(new SqlParameter("@desiredquantity", DesiredQuantity));
                parameters.Add(new SqlParameter("@desiredprice", Price));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                parameters.Add(new SqlParameter("@settotalprice", SetTotalPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddOrderDelivery(int OrderID, int ServiceID, string DeliveryCargoID, int PaymentMethodID, string Table, DataGrid targetgrid)
        {;
            if (OrderID >= 0 && ServiceID >= 0 && !String.IsNullOrEmpty(DeliveryCargoID) && PaymentMethodID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddOrderDelivery";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@orderid", OrderID));
                parameters.Add(new SqlParameter("@serviceid", ServiceID));
                parameters.Add(new SqlParameter("@deliverycargoid", DeliveryCargoID));
                parameters.Add(new SqlParameter("@paymentmethodid", PaymentMethodID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void AddOrderDeliveryExtended(string ProductName, string ServiceName, string DeliveryCargoID, string PaymentMethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(DeliveryCargoID) && !String.IsNullOrEmpty(PaymentMethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "AddOrderDeliveryExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                parameters.Add(new SqlParameter("@deliverycargoid", DeliveryCargoID));
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteAllClients(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllClients";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllDeliveryServices(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllDeliveryServices";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllOrderDeliveries(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllOrderDeliveries";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllOrderTypes(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllOrderTypes";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllPaymentMethods(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllPaymentMethods";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllProductCategories(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllProductCategories";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllProductBrands(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllProductBrands";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllProductImages(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllProductImages";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllProductOrders(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllProductOrders";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteAllUsers(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "DeleteAllUsers";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void DeleteClientByAddress(string Address, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Address) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByAddress";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@address", Address));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByBalance(int Balance, bool IncludeBelowBalance, bool IncludeAboveBalance, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByAddress";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@also_delete_below_balance", IncludeBelowBalance));
                parameters.Add(new SqlParameter("@also_delete_above_balance", IncludeAboveBalance));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByEmail(string Email, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByEmail";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@email", Email));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByPhone(string Phone, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Phone) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByPhone";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@phone", Phone));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByUserID(int UserID, string Table, DataGrid targetgrid)
        {
            if (UserID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByUserID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userid", UserID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteClientByUserName(string UserName, bool AlsoDeleteUser, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && AlsoDeleteUser != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteClientByUserName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@delete_user_as_well", AlsoDeleteUser));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDeliveryServiceByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDeliveryServiceByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDeliveryServiceByName(string ServiceName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDeliveryServiceByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", ServiceName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDeliveryServiceByPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDeliveryServiceByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_price", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_price", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDiagnosticTypeByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDiagnosticTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDiagnosticTypeByName(string DiagnosticName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(DiagnosticName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDiagnosticTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", DiagnosticName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteDiagnosticTypeByPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteDiagnosticTypeByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_price", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_price", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByDate(DateTime Date, bool IncludeBeforeDate, bool IncludeAfterDate, string Table, DataGrid targetgrid)
        {
            if (Date != null && IncludeBeforeDate != null && IncludeAfterDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@also_delete_before_date", IncludeBeforeDate));
                parameters.Add(new SqlParameter("@also_delete_after_date", IncludeAfterDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByDeliveryServiceID(int ServiceID, string Table, DataGrid targetgrid)
        {
            if (ServiceID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByDeliveryServiceID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serviceid", ServiceID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByDeliveryServiceName(string ServiceName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByDeliveryServiceName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByOrderID(int OrderID, string Table, DataGrid targetgrid)
        {
            if (OrderID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByOrderID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@orderid", OrderID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByPaymentMethodID(int MethodID, string Table, DataGrid targetgrid)
        {
            if (MethodID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByPaymentMethodID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodid", MethodID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByPaymentMethodName(string MethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(MethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByPaymentMethodName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@methodname", MethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByStatus(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByDeliveryServiceID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderDeliveryByCargoID(string CargoID, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(CargoID) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderDeliveryByCargoID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@cargoid", CargoID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderTypeByID(int TypeID, string Table, DataGrid targetgrid)
        {
            if (TypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", TypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteOrderTypeByName(string TypeName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(TypeName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteOrderTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", TypeName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeletePaymentMethodByID(int MethodID, string Table, DataGrid targetgrid)
        {
            if (MethodID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeletePaymentMethodByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", MethodID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeletePaymentMethodByName(string MethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(MethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeletePaymentMethodByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", MethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductBrandByID(int BrandID, string Table, DataGrid targetgrid)
        {
            if (BrandID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductBrandByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", BrandID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductBrandByName(string BrandName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(BrandName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductBrandByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", BrandName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }


        public void DeleteProductByArtID(string ArtID, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByArtID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@artid", ArtID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByBrandID(int BrandID, string Table, DataGrid targetgrid)
        {
            if (BrandID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByBrandID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@brandid", BrandID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByBrandName(string BrandName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(BrandName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByBrandName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@brandname", BrandName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByCategoryID(int CategoryID, string Table, DataGrid targetgrid)
        {
            if (CategoryID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByCategoryID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryid", CategoryID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByCategoryName(string CategoryName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(CategoryName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByCategoryName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryname", CategoryName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByDate(DateTime Date, bool IncludeBeforeDate, bool IncludeAfterDate, string Table, DataGrid targetgrid)
        {
            if (Date != null && IncludeBeforeDate != null && IncludeAfterDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@also_delete_before_date", IncludeBeforeDate));
                parameters.Add(new SqlParameter("@also_delete_after_date", IncludeAfterDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByDescription(string Description, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Description) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByDescription";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@description", Description));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByQuantity(int Quantity, bool IncludeBelowQuantity, bool IncludeAboveQuantity, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && IncludeBelowQuantity != null && IncludeAboveQuantity != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowQuantity));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAboveQuantity));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductBySerialNumber(string SerialNumber, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductBySerialNumber";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductByStorageLocation(string Location, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Location) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductByStorageLocation";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@location", Location));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductCategoryByID(int CategoryID, string Table, DataGrid targetgrid)
        {
            if (CategoryID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductCategoryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", CategoryID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductCategoryByName(string CategoryName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(CategoryName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductCategoryByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", CategoryName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImage(string ImageName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ImageName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImage";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@image_name", ImageName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductArtID(string ArtID, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductArtID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@artid", ArtID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductDate(DateTime Date, bool IncludeBeforeDate, bool IncludeAfterDate, string Table, DataGrid targetgrid)
        {
            if (Date != null && IncludeBeforeDate != null && IncludeAfterDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@also_delete_if_before_date", IncludeBeforeDate));
                parameters.Add(new SqlParameter("@also_delete_if_after_date", IncludeAfterDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductDescription(string Description, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Description) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductDescription";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@description", Description));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductQuantity(int Quantity, bool IncludeBelowQuantity, bool IncludeAboveQuantity, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && IncludeBelowQuantity != null && IncludeAboveQuantity != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowQuantity));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAboveQuantity));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageBySerialNumber(string SerialNumber, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageBySerialNumber";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductImageByProductStorageLocation(string Location, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Location) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductImageByProductStorageLocation";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@location", Location));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByClientID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByClientID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientid", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByClientName(string ClientName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByClientName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByDate(DateTime Date, bool IncludeBeforeDate, bool IncludeAfterDate, string Table, DataGrid targetgrid)
        {
            if (Date != null && IncludeBeforeDate != null && IncludeAfterDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@also_delete_before_date", IncludeBeforeDate));
                parameters.Add(new SqlParameter("@also_delete_after_date", IncludeAfterDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByDiagnosticTypeID(int DiagnosticTypeID, string Table, DataGrid targetgrid)
        {
            if (DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByDiagnosticTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", DiagnosticTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByPrice(int Price, bool IncludeBelowPrice, bool IncludeAbovePrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && IncludeBelowPrice != null && IncludeAbovePrice != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowPrice));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAbovePrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByProductID(int ProductID, string Table, DataGrid targetgrid)
        {
            if (ProductID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByProductID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", ProductID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByProductName(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByQuantity(int Quantity, bool IncludeBelowQuantity, bool IncludeAboveQuantity, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && IncludeBelowQuantity != null && IncludeAboveQuantity != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@also_delete_below_amount", IncludeBelowQuantity));
                parameters.Add(new SqlParameter("@also_delete_above_amount", IncludeAboveQuantity));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByReplacementProductID(int ProductID, string Table, DataGrid targetgrid)
        {
            if (ProductID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByReplacementProductID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", ProductID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByReplacementProductName(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByReplacementProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByStatus(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByStatus";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByTypeID(int OrderTypeID, string Table, DataGrid targetgrid)
        {
            if (OrderTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", OrderTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByUserID(int UserID, string Table, DataGrid targetgrid)
        {
            if (UserID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByUserID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userid", UserID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteProductOrderByUserDisplayName(string UserDisplayName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteProductOrderByUserDisplayName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByBalance(int Balance, bool IncludeBelowBalance, bool IncludeAboveBalance, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && IncludeBelowBalance != null && IncludeAboveBalance != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByBalance";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@also_delete_below_that_balance", IncludeBelowBalance));
                parameters.Add(new SqlParameter("@also_delete_above_that_balance", IncludeAboveBalance));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByDate(DateTime Date, bool IncludeBeforeDate, bool IncludeAfterDate, string Table, DataGrid targetgrid)
        {
            if (Date != null && IncludeBeforeDate != null && IncludeAfterDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@also_delete_before_that_date", IncludeBeforeDate));
                parameters.Add(new SqlParameter("@also_delete_after_that_date", IncludeAfterDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByDisplayName(string UserDisplayName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByDisplayName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@displayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByEmail(string UserEmail, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserEmail) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByEmail";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@email", UserEmail));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByID(int UserID, string Table, DataGrid targetgrid)
        {
            if (UserID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", UserID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByPhone(string UserPhone, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserPhone) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByPhone";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@phone", UserPhone));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void DeleteUserByUserName(string UserName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "DeleteUserByUserName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", UserName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetAllClients(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllClients";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllDeliveryServices(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllDeliveryServices";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllDiagnosticTypes(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllDiagnosticTypes";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllOrderDeliveries(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllOrderDeliveries";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllOrderDeliveriesExtended(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllOrderDeliveriesExtended";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllOrders(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllOrders";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllOrdersExtended(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllOrdersExtended";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllOrderTypes(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllOrderTypes";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllPaymentMethods(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllPaymentMethods";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllProductBrands(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllProductBrands";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllProductCategories(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllProductCategories";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllProductImages(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllProductImages";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllProducts(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllProducts";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetAllUsers(string Table, DataGrid targetgrid)
        {
            string ProcedureName = "GetAllUsers";
            List<SqlParameter> parameters = new List<SqlParameter>();
            ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
        }

        public void GetClientByAddress(string Address, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Address) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByAddress";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientaddress", Address));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetClientByBalance(int Balance, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByBalance";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetClientByEmail(string Email, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByEmail";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientemail", Email));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetClientByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetClientByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetClientByPhone(string Phone, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Phone) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientByPhone";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientphone", Phone));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }


        public void GetClientByStrict(int ID, string Name, string Email, string Phone, string Address, int Balance, bool LookBelowBalance, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Phone) && !String.IsNullOrEmpty(Email) &&
                !String.IsNullOrEmpty(Address) && Balance >= 0 && LookBelowBalance != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetClientStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@address", Address));
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@look_below_balance", LookBelowBalance));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDeliveryServiceByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDeliveryServiceByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDeliveryServiceByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDeliveryServiceByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@servicename", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDeliveryServiceByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDeliveryServiceByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDeliveryServiceStrict(int ID, string Name, int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Name) && Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDeliveryServiceStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDiagnosticTypeByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDiagnosticTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDiagnosticTypeByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDiagnosticTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typename", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDiagnosticTypeByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDiagnosticTypeByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetDiagnosticTypeStrict(int ID, string Name, int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Name) && Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetDiagnosticTypeStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByClientName(string ClientName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByClientName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByClientNameExtended(string ClientName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByClientNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByClientNameForReports(string ClientName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByClientNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByDateExtended(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByDateForReports(DateTime Date, bool LookBefore, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByDiagnosticTypeID(int DiagnosticTypeID, string Table, DataGrid targetgrid)
        {
            if (DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByDiagnosticTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByDiagnosticTypeIDExtended(int DiagnosticTypeID, string Table, DataGrid targetgrid)
        {
            if (DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByDiagnosticTypeIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByDiagnosticTypeIDForReports(int DiagnosticTypeID, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByDiagnosticTypeIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByIDExtended(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByIDForReports(int ID, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (ID >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByPriceExtended(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByPriceExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByPriceForReports(int Price, bool LookBelow, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByPriceExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByProductName(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByProductNameExtended(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByProductNameForReports(string ProductName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }


        public void GetOrderByQuantity(int Quantity, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByQuantityExtended(int Quantity, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByQuantityExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByQuantityForReports(int Quantity, bool LookBelow, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Quantity >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByQuantityExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByReplacementProductName(string ReplacementProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByReplacementProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByReplacementProductNameExtended(string ReplacementProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByReplacementProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByReplacementProductNameForReports(string ReplacementProductName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByReplacementProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByStatus(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByStatus";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByStatusExtended(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByStatusExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByStatusForReports(int Status, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Status >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByStatusExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByTypeID(int OrderTypeID, string Table, DataGrid targetgrid)
        {
            if (OrderTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", OrderTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByTypeIDExtended(int OrderTypeID, string Table, DataGrid targetgrid)
        {
            if (OrderTypeID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByTypeIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", OrderTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByTypeIDForReports(int OrderTypeID, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (OrderTypeID >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByTypeIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", OrderTypeID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderByUserDisplayName(string UserDisplayName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByUserDisplayName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByUserDisplayNameExtended(string UserDisplayName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderByUserDisplayNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderByUserDisplayNameForReports(string UserDisplayName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderByUserDisplayNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderStrict(string ProductName, int TypeID, string ReplacementProductName, int DiagnosticTypeID, string ClientName, string UserDisplayName, int Quantity, int Price, DateTime Date, int Status, bool LookBelowQuantity, bool LookBelowPrice, bool LookBeforeDate, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && TypeID >= 0 && !String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(ReplacementProductName) && DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(UserDisplayName) && Quantity >= 0
                 && Price >= 0 && Date != null && Status >= 0 && LookBelowQuantity != null && LookBelowPrice != null && LookBeforeDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@typeid", TypeID));
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_quantity", LookBelowQuantity));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderStrictExtended(string ProductName, int TypeID, string ReplacementProductName, int DiagnosticTypeID, string ClientName, string UserDisplayName, int Quantity, int Price, DateTime Date, int Status, bool LookBelowQuantity, bool LookBelowPrice, bool LookBeforeDate, string Table, DataGrid targetgrid)
        {
            if ((!String.IsNullOrEmpty(ProductName) && TypeID >= 0 && !String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(ReplacementProductName) && DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(UserDisplayName) && Quantity >= 0
                 && Price >= 0 && Date != null && Status >= 0 && LookBelowQuantity != null && LookBelowPrice != null && LookBeforeDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null))
            {
                string ProcedureName = "GetOrderStrictExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@typeid", TypeID));
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_quantity", LookBelowQuantity));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderStrictForReports(string ProductName, int TypeID, string ReplacementProductName, int DiagnosticTypeID, string ClientName, string UserDisplayName, int Quantity, int Price, DateTime Date, int Status, bool LookBelowQuantity, bool LookBelowPrice, bool LookBeforeDate, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if ((!String.IsNullOrEmpty(ProductName) && TypeID >= 0 && !String.IsNullOrEmpty(ReplacementProductName) && !String.IsNullOrEmpty(ReplacementProductName) && DiagnosticTypeID >= 0 && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(UserDisplayName) && Quantity >= 0
                 && Price >= 0 && Date != null && Status >= 0 && LookBelowQuantity != null && LookBelowPrice != null && LookBeforeDate != null && !String.IsNullOrEmpty(Table)))
            {
                string ProcedureName = "GetOrderStrictExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@typeid", TypeID));
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_quantity", LookBelowQuantity));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByClientName(string ClientName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByClientName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByClientNameExtended(string ClientName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByClientNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByClientNameForReports(string ClientName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByClientNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByDateExtended(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByDateForReports(DateTime Date, bool LookBefore, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByIDExtended(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByIDForReports(int ID, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (ID >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByIDExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByOrderDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByOrderDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByOrderDateExtended(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByOrderDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByOrderDateForReports(DateTime Date, bool LookBefore, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByOrderDateExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByPaymentMethodName(string PaymentMethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(PaymentMethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByPaymentMethodName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByPaymentMethodNameExtended(string PaymentMethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(PaymentMethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByPaymentMethodNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByPaymentMethodNameForReports(string PaymentMethodName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(PaymentMethodName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByPaymentMethodNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByProductName(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByProductNameExtended(string ProductName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByProductNameForReports(string ProductName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByProductNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByServiceName(string ServiceName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByServiceName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByServiceNameExtended(string ServiceName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByServiceNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByServiceNameForReports(string ServiceName, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByServiceNameExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below_price", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByPriceExtended(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByPriceExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below_price", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByPriceForReports(int Price, bool LookBelow, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByPriceExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below_price", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryByStatus(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByStatus";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByStatusExtended(int Status, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryByStatusExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryByStatusForReports(int Status, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (Status >= 0 && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryByStatusExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderDeliveryStrict(string ProductName, string ClientName, string ServiceName, string PaymentMethodName, int Price, DateTime OrderDate, DateTime DeliveryDate, int Status, bool LookBelowPrice, bool LookBeforeOrderDate, bool LookBeforeDeliveryDate, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(PaymentMethodName) &&
                Price >= 0 && OrderDate != null && DeliveryDate != null && Status >= 0 && LookBelowPrice != null && LookBeforeOrderDate != null && LookBeforeDeliveryDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@order_date", OrderDate));
                parameters.Add(new SqlParameter("@delivery_date", DeliveryDate));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_order_date", LookBeforeOrderDate));
                parameters.Add(new SqlParameter("@look_before_delivery_date", LookBeforeDeliveryDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryStrictExtended(string ProductName, string ClientName, string ServiceName, string PaymentMethodName, int Price, DateTime OrderDate, DateTime DeliveryDate, int Status, bool LookBelowPrice, bool LookBeforeOrderDate, bool LookBeforeDeliveryDate, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(PaymentMethodName) &&
                Price >= 0 && OrderDate != null && DeliveryDate != null && Status >= 0 && LookBelowPrice != null && LookBeforeOrderDate != null && LookBeforeDeliveryDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderDeliveryStrictExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@order_date", OrderDate));
                parameters.Add(new SqlParameter("@delivery_date", DeliveryDate));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_order_date", LookBeforeOrderDate));
                parameters.Add(new SqlParameter("@look_before_delivery_date", LookBeforeDeliveryDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderDeliveryStrictForReports(string ProductName, string ClientName, string ServiceName, string PaymentMethodName, int Price, DateTime OrderDate, DateTime DeliveryDate, int Status, bool LookBelowPrice, bool LookBeforeOrderDate, bool LookBeforeDeliveryDate, string Table, out DataSet outputds)
        {
            DataSet resultds = new DataSet();
            if (!String.IsNullOrEmpty(ProductName) && !String.IsNullOrEmpty(ClientName) && !String.IsNullOrEmpty(ServiceName) && !String.IsNullOrEmpty(PaymentMethodName) &&
                Price >= 0 && OrderDate != null && DeliveryDate != null && Status >= 0 && LookBelowPrice != null && LookBeforeOrderDate != null && LookBeforeDeliveryDate != null && !String.IsNullOrEmpty(Table))
            {
                string ProcedureName = "GetOrderDeliveryStrictExtended";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@servicename", ServiceName));
                parameters.Add(new SqlParameter("@paymentmethodname", PaymentMethodName));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@order_date", OrderDate));
                parameters.Add(new SqlParameter("@delivery_date", DeliveryDate));
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_order_date", LookBeforeOrderDate));
                parameters.Add(new SqlParameter("@look_before_delivery_date", LookBeforeDeliveryDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, out resultds);
            }
            outputds = resultds;
        }

        public void GetOrderTypeByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetOrderTypeByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetOrderTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetPaymentMethodByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetPaymentMethodByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetPaymentMethodByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetPaymentMethodByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductBrandByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductBrandByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductBrandByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductBrandByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductCategoryByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductCategoryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductCategoryByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductCategoryByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByArtID(string ArtID, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByArtID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@artid", ArtID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByBrand(string Brand, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Brand) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByBrand";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@brand", Brand));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByCategory(string Category, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Category) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByCategory";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@category", Category));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByDescription(string Description, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Description) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByDescription";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@description", Description));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductBySerialNumber(string SerialNumber, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductBySerialNumber";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByStorageLocation(string StorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(StorageLocation) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByStorageLocation";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@location", StorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductByQuantity(int Quantity, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductStrict(int ID, string Category, string Brand, string Name, string Description, int Quantity, int Price, string ArtID, string SerialNumber, string StorageLocation, DateTime Date, bool LookBelowQuantity, bool LookBelowPrice, bool LookBeforeDate, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Category) && !String.IsNullOrEmpty(Brand) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description) &&
                Quantity >= 0 && Price >= 0 && !String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(StorageLocation) && Date != null && LookBelowQuantity != null && LookBelowPrice != null && LookBeforeDate != null
                && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@category", Category));
                parameters.Add(new SqlParameter("@brand", Brand));
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@description", Description));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@artid", ArtID));
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                parameters.Add(new SqlParameter("@storagelocation", StorageLocation));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_below_quantity", LookBelowQuantity));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByArtID(string ArtID, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByArtID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@artid", ArtID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByDescription(string Description, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Description) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByDescription";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@description", Description));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByID(int ID, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByLocation(string StorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(StorageLocation) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByLocation";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@location", StorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByName(string Name, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByPrice(int Price, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesByQuantity(int Quantity, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesBySerialNumber(string SerialNumber, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesBySerialNumber";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetProductImagesStrict(int ID, string Category, string Brand, string Name, string Description, int Quantity, int Price, string ArtID, string SerialNumber, string StorageLocation, DateTime Date, bool LookBelowQuantity, bool LookBelowPrice, bool LookBeforeDate, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(Category) && !String.IsNullOrEmpty(Brand) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Description) &&
                Quantity >= 0 && Price >= 0 && !String.IsNullOrEmpty(ArtID) && !String.IsNullOrEmpty(SerialNumber) && !String.IsNullOrEmpty(StorageLocation) && Date != null && LookBelowQuantity != null && LookBelowPrice != null && LookBeforeDate != null
                && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetProductImagesStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@category", Category));
                parameters.Add(new SqlParameter("@brand", Brand));
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@description", Description));
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@artid", ArtID));
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                parameters.Add(new SqlParameter("@storagelocation", StorageLocation));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_below_quantity", LookBelowQuantity));
                parameters.Add(new SqlParameter("@look_below_price", LookBelowPrice));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByBalance(int Balance, bool LookBelow, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && LookBelow != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByBalance";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@look_below", LookBelow));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByDate(DateTime Date, bool LookBefore, string Table, DataGrid targetgrid)
        {
            if (Date != null && LookBefore != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before", LookBefore));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByDisplayName(string UserDisplayName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByDisplayName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@displayname", UserDisplayName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByEmail(string UserEmail, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserEmail) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByEmail";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@email", UserEmail));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByID(int UserID, string Table, DataGrid targetgrid)
        {
            if (UserID >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", UserID));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByPhone(string UserPhone, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserPhone) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByPhone";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@phone", UserPhone));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserByUserName(string UserName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserByUserName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void GetUserStrict(int ID, string UserName, string UserDisplayName, string UserEmail, string UserPhone, int Balance, DateTime Date, bool LookBelowBalance, bool LookBeforeDate, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(UserDisplayName) && !String.IsNullOrEmpty(UserEmail) && !String.IsNullOrEmpty(UserPhone)
               && Balance >= 0 && Date != null && LookBelowBalance != null && LookBeforeDate != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "GetUserStrict";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@displayname", UserDisplayName));
                parameters.Add(new SqlParameter("@email", UserEmail));
                parameters.Add(new SqlParameter("@phone", UserPhone));
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@look_before_date", LookBeforeDate));
                parameters.Add(new SqlParameter("@look_below_balance", LookBelowBalance));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByAddress(string Address, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Address) && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByAddress";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@address", Address));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByBalance(int Balance, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByBalance";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByEmail(string Email, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByEmail";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByID(int ID, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByAddress";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByName(string Name, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateClientByPhone(string Phone, string NewName, string NewEmail, string NewPhone, string NewAddress, int NewBalance, Bitmap NewProfilePic, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Phone) && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPhone) &&
                !String.IsNullOrEmpty(NewAddress) && NewBalance >= 0 && NewProfilePic != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateClientByPhone";
                List<SqlParameter> parameters = new List<SqlParameter>();
                byte[] imagebinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_address", NewAddress));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imagebinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDeliveryServiceByID(int ID, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDeliveryServiceByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDeliveryServiceByName(string Name, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDeliveryServiceByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDeliveryServiceByPrice(int Price, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDeliveryServiceByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDiagnosticTypeByID(int ID, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDiagnosticTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDiagnosticTypeByName(string Name, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDiagnosticTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateDiagnosticTypeByPrice(int Price, string NewName, int NewPrice, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && !String.IsNullOrEmpty(NewName) && NewPrice >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateDiagnosticTypeByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByClientID(int ClientID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (ClientID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByClientID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientid", ClientID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByClientName(string ClientName, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ClientName) && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByClientName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@clientname", ClientName));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByDate(DateTime Date, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (Date != null && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByDesiredQuantity(int Quantity, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByDesiredQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByDiagnosticTypeID(int DiagnosticTypeID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (DiagnosticTypeID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByDiagnosticTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@diagnostictypeid", DiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByID(int ID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByPrice(int Price, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByProductID(int ProductID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (ProductID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByProductID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productid", ProductID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByProductName(string ProductName, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductName) && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@productname", ProductName));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByReplacementProductID(int ReplacementProductID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (ReplacementProductID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByReplacementProductID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@replacementproductid", ReplacementProductID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByReplacementProductName(string ReplacementProductName, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ReplacementProductName) && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByReplacementProductName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@replacementproductname", ReplacementProductName));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByStatus(int Status, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByStatus";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByTypeID(int OrderTypeID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (OrderTypeID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByTypeID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@typeid", OrderTypeID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByUserID(int UserID, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (UserID >= 0 && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByUserID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userid", UserID));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderByUserDisplayName(string UserDisplayName, int NewProductID, int NewOrderTypeID, int NewReplacementProductID, int NewDiagnosticTypeID, int NewDesiredQuantity, int NewOrderPrice, int NewClientID, int NewUserID, int NewOrderStatus, bool SetNewPriceAsTotal, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserDisplayName) && NewProductID >= 0 && NewOrderTypeID >= 0 && NewReplacementProductID >= 0 && NewDiagnosticTypeID >= 0 &&
               NewDesiredQuantity >= 0 && NewOrderPrice >= 0 && NewClientID >= 0 && NewUserID >= 0 && SetNewPriceAsTotal != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderByUserDisplayName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", UserDisplayName));
                parameters.Add(new SqlParameter("@new_product_id", NewProductID));
                parameters.Add(new SqlParameter("@new_order_type", NewOrderTypeID));
                parameters.Add(new SqlParameter("@new_replacement_product_id", NewReplacementProductID));
                parameters.Add(new SqlParameter("@new_diagnostic_type", NewDiagnosticTypeID));
                parameters.Add(new SqlParameter("@new_desired_quantity", NewDesiredQuantity));
                parameters.Add(new SqlParameter("@new_order_price", NewOrderPrice));
                parameters.Add(new SqlParameter("@new_client_id", NewClientID));
                parameters.Add(new SqlParameter("@new_user_id", NewUserID));
                parameters.Add(new SqlParameter("@new_order_status", NewOrderStatus));
                parameters.Add(new SqlParameter("@new_price_is_total", SetNewPriceAsTotal));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByDate(DateTime Date, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (Date != null && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByDeliveryCargoID(string CargoID, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(CargoID) && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByDeliveryCargoID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@deliverycargoid", CargoID));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByDeliveryServiceID(int ServiceID, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (ServiceID >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByDeliveryServiceID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@deliveryserviceid", ServiceID));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByDeliveryServiceName(string ServiceName, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ServiceName) && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByDeliveryServiceName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@deliveryservicename", ServiceName));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByID(int ID, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByOrderID(int OrderID, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (OrderID >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByDeliveryOrderID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@orderid", OrderID));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByPaymentMethodID(int MethodID, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (MethodID >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByPaymentMethodID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodid", MethodID));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByPaymentMethodName(string MethodName, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(MethodName) && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByPaymentMethodName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@paymentmethodname", MethodName));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByPrice(int Price, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderDeliveryByStatus(int Status, int NewOrderID, int NewServiceID, string NewCargoID, int NewPaymentMethodID, int NewStatus, string Table, DataGrid targetgrid)
        {
            if (Status >= 0 && NewOrderID >= 0 && NewServiceID >= 0 && !String.IsNullOrEmpty(NewCargoID) && NewPaymentMethodID >= 0 && NewStatus >= 0 && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderDeliveryByStatus";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@status", Status));
                parameters.Add(new SqlParameter("@new_order", NewOrderID));
                parameters.Add(new SqlParameter("@new_service", NewServiceID));
                parameters.Add(new SqlParameter("@new_delivery_cargo_id", NewCargoID));
                parameters.Add(new SqlParameter("@new_payment_method", NewPaymentMethodID));
                parameters.Add(new SqlParameter("@new_status", NewStatus));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderTypeByID(int OrderTypeID, string NewTypeName, string Table, DataGrid targetgrid)
        {
            if (OrderTypeID >= 0 && !String.IsNullOrEmpty(NewTypeName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderTypeByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", OrderTypeID));
                parameters.Add(new SqlParameter("@new_type_name", NewTypeName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateOrderTypeByName(string OrderTypeName, string NewTypeName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(OrderTypeName) && !String.IsNullOrEmpty(NewTypeName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateOrderTypeByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", OrderTypeName));
                parameters.Add(new SqlParameter("@new_type_name", NewTypeName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdatePaymentMethodByID(int PaymentMethodID, string NewMethodName, string Table, DataGrid targetgrid)
        {
            if (PaymentMethodID >= 0 && !String.IsNullOrEmpty(NewMethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdatePaymentMethodByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", PaymentMethodID));
                parameters.Add(new SqlParameter("@new_method_name", NewMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdatePaymentMethodByName(string PaymentMethodName, string NewMethodName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(PaymentMethodName) && !String.IsNullOrEmpty(NewMethodName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdatePaymentMethodByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", PaymentMethodName));
                parameters.Add(new SqlParameter("@new_method_name", NewMethodName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductBrandByID(int ProductBrandID, string NewBrandName, string Table, DataGrid targetgrid)
        {
            if (ProductBrandID >= 0 && !String.IsNullOrEmpty(NewBrandName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductBrandByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ProductBrandID));
                parameters.Add(new SqlParameter("@new_brand_name", NewBrandName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductBrandByName(string ProductBrandName, string NewBrandName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductBrandName) && !String.IsNullOrEmpty(NewBrandName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductBrandByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", ProductBrandName));
                parameters.Add(new SqlParameter("@new_brand_name", NewBrandName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductCategoryByID(int ProductCategoryID, string NewCategoryName, string Table, DataGrid targetgrid)
        {
            if (ProductCategoryID >= 0 && !String.IsNullOrEmpty(NewCategoryName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductCategoryByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ProductCategoryID));
                parameters.Add(new SqlParameter("@new_category_name", NewCategoryName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductCategoryByName(string ProductCategoryName, string NewCategoryName, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ProductCategoryName) && !String.IsNullOrEmpty(NewCategoryName) && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductCategoryByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", ProductCategoryName));
                parameters.Add(new SqlParameter("@new_category_name", NewCategoryName));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByArtID(string ArtID, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ArtID) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByArtID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@artid", ArtID));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByBrand(string Brand, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Brand) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByBrand";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@brandname", Brand));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByCategory(string Category, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Category) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByCategory";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@categoryname", Category));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByDate(DateTime Date, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (Date != null && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByDate";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByDescription(string Description, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Description) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByDescription";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@description", Description));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByID(int ID, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByID";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByName(string Name, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Name) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByName";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@name", Name));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByPrice(int Price, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (Price >= 0 && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByPrice";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@price", Price));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByQuantity(int Quantity, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (Quantity >= 0 && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByQuantity";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@quantity", Quantity));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductBySerialNumber(string SerialNumber, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(SerialNumber) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductBySerialNumber";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@serialnumber", SerialNumber));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductByStorageLocation(string StorageLocation, int NewCategoryID, int NewBrandID, string NewName, string NewDescription, int NewQuantity, int NewPrice, string NewArtID, string NewSerialNumber, string NewStorageLocation, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(StorageLocation) && NewCategoryID >= 0 && NewBrandID >= 0 && !String.IsNullOrEmpty(NewName) && !String.IsNullOrEmpty(NewDescription) &&
                NewQuantity >= 0 && NewPrice >= 0 && !String.IsNullOrEmpty(NewArtID) && !String.IsNullOrEmpty(NewSerialNumber) && !String.IsNullOrEmpty(NewStorageLocation) &&
                !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductByStorageLocation";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@location", StorageLocation));
                parameters.Add(new SqlParameter("@new_category", NewCategoryID));
                parameters.Add(new SqlParameter("@new_brand", NewBrandID));
                parameters.Add(new SqlParameter("@new_name", NewName));
                parameters.Add(new SqlParameter("@new_description", NewDescription));
                parameters.Add(new SqlParameter("@new_quantity", NewQuantity));
                parameters.Add(new SqlParameter("@new_price", NewPrice));
                parameters.Add(new SqlParameter("@new_art_id", NewArtID));
                parameters.Add(new SqlParameter("@new_serial_number", NewSerialNumber));
                parameters.Add(new SqlParameter("@new_storage_location", NewStorageLocation));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateProductImage(string ImageName, string NewImageName, Bitmap UpdatedImage, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(ImageName) && !String.IsNullOrEmpty(NewImageName) && UpdatedImage != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateProductImage";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(UpdatedImage);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@image_name", ImageName));
                parameters.Add(new SqlParameter("@new_image_name", NewImageName));
                parameters.Add(new SqlParameter("@upated_image", imgbinary));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByBalance(int Balance, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (Balance >= 0 && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByBalance";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@balance", Balance));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }


        public void UpdateUserByDate(DateTime Date, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (Date != null && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByDate";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@date", Date));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByDisplayName(string DisplayName, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(DisplayName) && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByDisplayName";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@userdisplayname", DisplayName));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByEmail(string Email, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Email) && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByEmail";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@email", Email));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByID(int ID, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (ID >= 0 && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByID";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@id", ID));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByPhone(string Phone, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(Phone) && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByPhone";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@phone", Phone));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }

        public void UpdateUserByUserName(string UserName, string NewUserName, string NewDisplayName, string NewEmail, string NewPassword, string NewPhone, int NewBalance, Bitmap NewProfilePic, bool NewIsAdmin, bool NewIsWorker, bool NewIsClient, string Table, DataGrid targetgrid)
        {
            if (!String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(NewUserName) && !String.IsNullOrEmpty(NewDisplayName) && !String.IsNullOrEmpty(NewEmail) && !String.IsNullOrEmpty(NewPassword) &&
                !String.IsNullOrEmpty(NewPhone) && NewBalance >= 0 && NewProfilePic != null && NewIsAdmin != null && NewIsWorker != null && NewIsClient != null && !String.IsNullOrEmpty(Table) && targetgrid != null)
            {
                string ProcedureName = "UpdateUserByUserName";
                byte[] imgbinary = ImageDecoderEncoder.EncodeImageToByte(NewProfilePic);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@username", UserName));
                parameters.Add(new SqlParameter("@new_user_name", NewUserName));
                parameters.Add(new SqlParameter("@new_display_name", NewDisplayName));
                parameters.Add(new SqlParameter("@new_email", NewEmail));
                parameters.Add(new SqlParameter("@new_password", NewPassword));
                parameters.Add(new SqlParameter("@new_phone", NewPhone));
                parameters.Add(new SqlParameter("@new_balance", NewBalance));
                parameters.Add(new SqlParameter("@new_profile_pic", imgbinary));
                parameters.Add(new SqlParameter("@new_is_admin", NewIsAdmin));
                parameters.Add(new SqlParameter("@new_is_worker", NewIsWorker));
                parameters.Add(new SqlParameter("@new_is_client", NewIsClient));
                ExecuteStoredProcedure(ProcedureName, Table, parameters, targetgrid);
            }
        }


        public void PerformBulkAddOperation(string Table, DataGrid targetgrid)
        {
            try
            {
                if (Table == "Users")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string UserName = row.ItemArray[1].ToString();
                        string DisplayName = row.ItemArray[2].ToString();
                        string Email = row.ItemArray[3].ToString();
                        string Password = row.ItemArray[4].ToString();
                        string Phone = row.ItemArray[5].ToString();
                        string Address = "sample address";
                        Bitmap ProfilePic;
                        if (row.ItemArray[8] != null && row.ItemArray[8].GetType() != typeof(DBNull))
                        {
                            ProfilePic = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[8]);
                        }
                        else
                        {
                            ProfilePic = new Bitmap(16, 16);
                        }
                        bool IsAdmin = Convert.ToBoolean(row.ItemArray[9].ToString());
                        bool IsWorker = Convert.ToBoolean(row.ItemArray[10].ToString());
                        bool IsClient = Convert.ToBoolean(row.ItemArray[11].ToString());
                        if (IsAdmin && !IsWorker && !IsClient)
                        {
                            RegisterAdmin(UserName, DisplayName, Email, Password, Phone, ProfilePic, Table, targetgrid);
                        }
                        else if (IsWorker && !IsAdmin && !IsClient)
                        {
                            RegisterWorker(UserName, DisplayName, Email, Password, Phone, ProfilePic, Table, targetgrid);
                        }
                        else if (IsClient && !IsAdmin && !IsWorker)
                        {
                            RegisterClient(UserName, DisplayName, Email, Password, Phone, Address, ProfilePic, Table, targetgrid);
                        }
                        else
                        {
                            RegisterUser(UserName, DisplayName, Email, Password, Phone, ProfilePic, IsAdmin, IsWorker, IsClient, Table, targetgrid);
                        }
                        GetAllUsers("Users", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Clients")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string UserName = "USER_" + row[0].ToString();
                        string DisplayName = row.ItemArray[2].ToString();
                        string Email = row.ItemArray[3].ToString();
                        string Password = $"{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()}";
                        string Phone = row.ItemArray[4].ToString();
                        string Address = row.ItemArray[5].ToString();
                        Bitmap ProfilePic;
                        if (row.ItemArray[7] != null && row.ItemArray[7].GetType() != typeof(DBNull))
                        {
                            ProfilePic = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[7]);
                        }
                        else
                        {
                            ProfilePic = new Bitmap(16, 16);
                        }
                        if (System.Windows.MessageBox.Show("Do you want to register this client?", "Prompt", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            RegisterClient(UserName, DisplayName, Email, Password, Phone, Address, ProfilePic, Table, targetgrid);
                        }
                        else
                        {
                            AddClientWithoutRegistering(DisplayName, Email, Phone, Address, ProfilePic, Table, targetgrid);
                        }
                        GetAllClients("Clients", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DeliveryServices")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string ServiceName = row.ItemArray[1].ToString();
                        decimal ServicePrice = Convert.ToDecimal(row.ItemArray[2]);
                        int ServicePriceInt = Convert.ToInt32(ServicePrice);
                        AddDeliveryService(ServiceName, ServicePriceInt, Table, targetgrid);
                        GetAllDeliveryServices("DeliveryServices", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DiagnosticTypes")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string TypeName = row.ItemArray[1].ToString();
                        decimal TypePrice = Convert.ToDecimal(row.ItemArray[2]);
                        int TypePriceInt = Convert.ToInt32(TypePrice);
                        AddDiagnosticType(TypeName, TypePriceInt, Table, targetgrid);
                        GetAllDiagnosticTypes("DiagnosticTypes", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderTypes")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string TypeName = row.ItemArray[1].ToString();
                        AddOrderType(TypeName, Table, targetgrid);
                        GetAllOrderTypes("OrderTypes", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "PaymentMethods")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string MethodName = row.ItemArray[1].ToString();
                        AddPaymentMethod(MethodName, Table, targetgrid);
                        GetAllPaymentMethods("PaymentMethods", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductCategories")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string CategoryName = row.ItemArray[1].ToString();
                        AddProductCategory(CategoryName, Table, targetgrid);
                        GetAllProductCategories("ProductCategories", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductBrands")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        string BrandName = row.ItemArray[1].ToString();
                        AddProductBrand(BrandName, Table, targetgrid);
                        GetAllProductBrands("ProductBrands", targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductImages")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {
                        int currentRowIndex = targetgrid.Items.IndexOf(targetgrid.SelectedItem);

                        int TargetProductID = Convert.ToInt32(row.ItemArray[0].ToString());
                        string ImageName = $"PRODUCT_{TargetProductID}_IMAGE_{currentRowIndex + 1}";
                        Bitmap product_image;
                        if (row.ItemArray[2] != null && row.ItemArray[2].GetType() != typeof(DBNull))
                        {
                            product_image = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[2]);
                        }
                        else
                        {
                            product_image = new Bitmap(16, 16);
                        }
                        AddProductImage(TargetProductID, ImageName, product_image, Table, targetgrid);
                        GetAllProductImages(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Products")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        int TargetCategoryID = Convert.ToInt32(row.ItemArray[1].ToString());
                        int TargetBrandID = Convert.ToInt32(row.ItemArray[2].ToString());
                        string ProductName = row.ItemArray[3].ToString();
                        string ProductDescription = row.ItemArray[4].ToString();
                        int Quantity = Convert.ToInt32(row.ItemArray[5].ToString());
                        int Price = Convert.ToInt32(row.ItemArray[6].ToString());
                        string ArticuleNumber = row.ItemArray[7].ToString();
                        string SerialNumber = row.ItemArray[8].ToString();
                        string StorageLocation = row.ItemArray[9].ToString();
                        AddProduct(TargetCategoryID, TargetBrandID, ProductName, ProductDescription, Quantity, Price, ArticuleNumber, SerialNumber, StorageLocation, Table, targetgrid);
                        GetAllProducts(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductOrders")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {
                        int TargetProductID = Convert.ToInt32(row.ItemArray[1].ToString());
                        int TargetOrderTypeID = Convert.ToInt32(row.ItemArray[2].ToString());
                        int TargetReplacementProductID = Convert.ToInt32(row.ItemArray[3].ToString());
                        int TargetDiagnosticTypeID = Convert.ToInt32(row.ItemArray[4].ToString());
                        int DesiredQuantity = Convert.ToInt32(row.ItemArray[5].ToString());
                        int OrderPrice = Convert.ToInt32(row.ItemArray[6].ToString());
                        int TargetClientID = Convert.ToInt32(row.ItemArray[7].ToString());
                        int TargetUserID = Convert.ToInt32(row.ItemArray[8].ToString());
                        bool SetPriceAsTotal = false;
                        if (System.Windows.MessageBox.Show("Did you set the price for selected order total?", "Total Price or Not", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            SetPriceAsTotal = true;
                        }
                        else
                        {
                            SetPriceAsTotal = false;
                        }
                        AddOrder(TargetProductID, TargetOrderTypeID, TargetReplacementProductID, TargetDiagnosticTypeID, DesiredQuantity, OrderPrice, TargetClientID, TargetUserID, SetPriceAsTotal, Table, targetgrid);
                        GetAllOrders(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderDeliveries")
                {
                    foreach (DataRow row in BulkAddList[Table])
                    {

                        int TargetOrderID = Convert.ToInt32(row.ItemArray[1].ToString());
                        int TargetDeliveryServiceID = Convert.ToInt32(row.ItemArray[2].ToString());
                        string DeliveryCargoID = row.ItemArray[3].ToString();
                        int TargetPaymentMethodID = Convert.ToInt32(row.ItemArray[5].ToString());
                        AddOrderDelivery(TargetOrderID, TargetDeliveryServiceID, DeliveryCargoID, TargetPaymentMethodID, Table, targetgrid);
                        GetAllOrderDeliveries(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                BulkAddList[Table].Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void PerformBulkUpdateOperation(string Table, DataGrid targetgrid)
        {
            try
            {
                if (Table == "Users")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewUserName = "";
                        string NewDisplayName = "";
                        string NewEmail = "";
                        string NewPassword = "";
                        string NewPhone = "";
                        int NewBalance = 0;
                        Bitmap NewProfilePic = null;
                        byte[] imgbinary = null;
                        bool NewIsAdmin = false;
                        bool NewIsWorker = false;
                        bool NewIsClient = false;
                        NewUserName = row.ItemArray[1].ToString();
                        NewDisplayName = row.ItemArray[2].ToString();
                        NewEmail = row.ItemArray[3].ToString();
                        NewPassword = row.ItemArray[4].ToString();
                        NewPhone = row.ItemArray[5].ToString();
                        Int32.TryParse(row.ItemArray[6].ToString(), out NewBalance);
                        if (row.ItemArray[8].GetType() != typeof(DBNull))
                        {
                            NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[8]);
                            if (NewProfilePic == null) ;
                            {
                                NewProfilePic = new Bitmap(16, 16);
                            }
                        }
                        Boolean.TryParse(row.ItemArray[9].ToString(), out NewIsAdmin);
                        Boolean.TryParse(row.ItemArray[10].ToString(), out NewIsWorker);
                        Boolean.TryParse(row.ItemArray[11].ToString(), out NewIsClient);
                        UpdateUserByID(IntTerm, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, targetgrid);
                        GetAllUsers(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Clients")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        string NewEmail = "";
                        string NewPhone = "";
                        string NewAddress = "";
                        int NewBalance = 0;
                        Bitmap NewProfilePic = null;
                        NewName = row.ItemArray[2].ToString();
                        NewEmail = row.ItemArray[3].ToString();
                        NewPhone = row.ItemArray[4].ToString();
                        NewAddress = row.ItemArray[5].ToString();
                        Int32.TryParse(row.ItemArray[6].ToString(), out NewBalance);
                        if (row.ItemArray[7].GetType() != typeof(DBNull))
                        {
                            NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[7]);
                            if (NewProfilePic == null)
                            {
                                NewProfilePic = new Bitmap(16, 16);
                            }
                        }
                        UpdateClientByID(IntTerm, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, targetgrid);
                        GetAllClients(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DeliveryServices")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        int NewPrice = 0;
                        NewName = row.ItemArray[1].ToString();
                        Int32.TryParse(row.ItemArray[2].ToString(), out NewPrice);
                        UpdateDeliveryServiceByID(IntTerm, NewName, NewPrice, Table, targetgrid);
                        GetAllDeliveryServices(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DiagnosticTypes")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        int NewPrice = 0;
                        NewName = row.ItemArray[1].ToString();
                        Int32.TryParse(row.ItemArray[2].ToString(), out NewPrice);
                        UpdateDiagnosticTypeByID(IntTerm, NewName, NewPrice, Table, targetgrid);
                        GetAllDiagnosticTypes(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderTypes")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        NewName = row.ItemArray[1].ToString();
                        UpdateOrderTypeByID(IntTerm, NewName, Table, targetgrid);
                        GetAllOrderTypes(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "PaymentMethods")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        NewName = row.ItemArray[1].ToString();
                        UpdatePaymentMethodByID(IntTerm, NewName, Table, targetgrid);
                        GetAllPaymentMethods(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductCategories")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        NewName = row.ItemArray[1].ToString();
                        UpdateProductCategoryByID(IntTerm, NewName, Table, targetgrid);
                        GetAllProductCategories(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductBrands")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        string NewName = "";
                        NewName = row.ItemArray[1].ToString();
                        UpdateProductBrandByID(IntTerm, NewName, Table, targetgrid);
                        GetAllProductBrands(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductImages")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = $"PRODUCT_{Int32.Parse(row[0].ToString())}_Image_{BulkUpdateList[Table].IndexOf(row)}";
                        string NewImageName = "";
                        Bitmap NewProductImage = null;
                        NewImageName = row.ItemArray[1].ToString();
                        if (row.ItemArray[2].GetType() != typeof(DBNull))
                        {
                            NewProductImage = ImageDecoderEncoder.DecodeImage((byte[])row.ItemArray[2]);
                            if (NewProductImage == null)
                            {
                                NewProductImage = new Bitmap(16, 16);
                            }
                        }
                        UpdateProductImage(Term, NewImageName, NewProductImage, Table, targetgrid);
                        GetAllProductImages(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Products")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        int NewCategoryID = 0;
                        int NewBrandID = 0;
                        string NewProductName = "";
                        string NewProductDescription = "";
                        int NewProductQuantity = 0;
                        int NewProductPrice = 0;
                        string NewProductArtID = "";
                        string NewProductSerialNumber = "";
                        string NewProductStorageLocation = "";
                        Int32.TryParse(row.ItemArray[1].ToString(), out NewCategoryID);
                        Int32.TryParse(row.ItemArray[2].ToString(), out NewBrandID);
                        NewProductName = row.ItemArray[3].ToString();
                        NewProductDescription = row.ItemArray[4].ToString();
                        Int32.TryParse(row.ItemArray[5].ToString(), out NewProductQuantity);
                        Int32.TryParse(row.ItemArray[6].ToString(), out NewProductPrice);
                        NewProductArtID = row.ItemArray[7].ToString();
                        NewProductSerialNumber = row.ItemArray[8].ToString();
                        NewProductStorageLocation = row.ItemArray[9].ToString();
                        UpdateProductByID(IntTerm, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, targetgrid);
                        GetAllProducts(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductOrders")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        int NewProductID = 0;
                        int NewOrderTypeID = 0;
                        int NewReplacementProductID = 0;
                        int NewDiagnosticTypeID = 0;
                        int NewDesiredQuantity = 0;
                        int NewOrderPrice = 0;
                        int NewClientID = 0;
                        int NewUserID = 0;
                        int NewStatus = 0;
                        bool SetNewPriceAsTotal = false;
                        Int32.TryParse(row.ItemArray[1].ToString(), out NewProductID);
                        Int32.TryParse(row.ItemArray[2].ToString(), out NewOrderTypeID);
                        Int32.TryParse(row.ItemArray[3].ToString(), out NewReplacementProductID);
                        Int32.TryParse(row.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                        Int32.TryParse(row.ItemArray[5].ToString(), out NewDesiredQuantity);
                        Int32.TryParse(row.ItemArray[6].ToString(), out NewOrderPrice);
                        Int32.TryParse(row.ItemArray[7].ToString(), out NewClientID);
                        Int32.TryParse(row.ItemArray[8].ToString(), out NewUserID);
                        Int32.TryParse(row.ItemArray[11].ToString(), out NewStatus);
                        if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            SetNewPriceAsTotal = true;
                        }
                        else
                        {
                            SetNewPriceAsTotal = false;
                        }
                        UpdateOrderByID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, targetgrid);
                        GetAllOrders(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderDeliveries")
                {
                    foreach (DataRow row in BulkUpdateList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        int NewOrderID = 0;
                        int NewDeliveryServiceID = 0;
                        string NewDeliveryCargoID = "";
                        int NewPaymentMethodID = 0;
                        int NewStatus = 0;
                        Int32.TryParse(row.ItemArray[1].ToString(), out NewOrderID);
                        Int32.TryParse(row.ItemArray[2].ToString(), out NewDeliveryServiceID);
                        NewDeliveryCargoID = row.ItemArray[3].ToString();
                        Int32.TryParse(row.ItemArray[5].ToString(), out NewPaymentMethodID);
                        Int32.TryParse(row.ItemArray[8].ToString(), out NewStatus);
                        UpdateOrderDeliveryByID(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, targetgrid);
                        GetAllOrderDeliveries(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                BulkUpdateList[Table].Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void PerformBulkDeleteOperation(string Table, DataGrid targetgrid)
        {
            try
            {
                if (Table == "Users")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        DeleteUserByID(IntTerm, Table, targetgrid);
                        GetAllUsers(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Clients")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        DeleteClientByID(IntTerm,Table, targetgrid);
                        GetAllClients(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DeliveryServices")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        DeleteDeliveryServiceByID(IntTerm, Table, targetgrid);
                        GetAllDeliveryServices(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DiagnosticTypes")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteDiagnosticTypeByID(IntTerm,Table, targetgrid);
                        GetAllDiagnosticTypes(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderTypes")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteOrderTypeByID(IntTerm, Table, targetgrid);
                        GetAllOrderTypes(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "PaymentMethods")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeletePaymentMethodByID(IntTerm, Table, targetgrid);
                        GetAllPaymentMethods(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductCategories")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteProductCategoryByID(IntTerm, Table, targetgrid);
                        GetAllProductCategories(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductBrands")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteProductBrandByID(IntTerm, Table, targetgrid);
                        GetAllProductBrands(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductImages")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = $"PRODUCT_{Int32.Parse(row[0].ToString())}_Image_{BulkDeleteList[Table].IndexOf(row)}";
                        DeleteProductImage(Term, Table, targetgrid);
                        GetAllProductImages(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Products")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteProductByID(IntTerm, Table, targetgrid);
                        GetAllProducts(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductOrders")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteProductOrderByID(IntTerm, Table, targetgrid);
                        GetAllOrders(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderDeliveries")
                {
                    foreach (DataRow row in BulkDeleteList[Table])
                    {
                        string Term = row[0].ToString();
                        int IntTerm = 0;
                        Int32.TryParse(Term, out IntTerm);
                        DeleteOrderDeliveryByID(IntTerm, Table, targetgrid);
                        GetAllOrderDeliveries(Table, targetgrid);
                        if (Tables[Table].Tables.Count > 0)
                        {
                            FillDG(targetgrid, Tables[Table].Tables[0]);
                        }
                    }
                }
                BulkDeleteList[Table].Clear();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public void FillDG(DataGrid grid, DataTable table)
        {
            grid.Columns.Clear();
            List<string> columnnames = new List<string>();
            foreach (DataColumn col in table.Columns)
            {
                columnnames.Add(col.ColumnName);
                if(col.DataType == typeof(string))
                {
                    DataGridTextColumn newcol = new DataGridTextColumn();
                    newcol.Header = col.ColumnName;
                    newcol.Binding = new System.Windows.Data.Binding(string.Format("[{0}]", col.ColumnName));
                    grid.Columns.Add(newcol);
                }
                else if(col.DataType == typeof(int) && !(col.ColumnName == "OrderStatus" || col.ColumnName == "DeliveryStatus"))
                {
                    DataGridTextColumn newcol = new DataGridTextColumn();
                    newcol.Header = col.ColumnName;
                    newcol.Binding = new System.Windows.Data.Binding(string.Format("[{0}]", col.ColumnName));
                    grid.Columns.Add(newcol);
                }
                else if(col.DataType == typeof(decimal))
                {
                    DataGridTextColumn newcol = new DataGridTextColumn();
                    newcol.Header = col.ColumnName;
                    newcol.Binding = new System.Windows.Data.Binding(string.Format("[{0}]", col.ColumnName));
                    grid.Columns.Add(newcol);
                }
                else if(col.DataType == typeof(int[]))
                {
                    DataGridComboBoxColumn newcol = new DataGridComboBoxColumn();
                    newcol.Header = col.ColumnName;
                    grid.Columns.Add(newcol);
                }
                else if(col.DataType == typeof(DateTime))
                {
                    FrameworkElementFactory datePickerFactory = new FrameworkElementFactory(typeof(DatePicker));
                    datePickerFactory.SetBinding(DatePicker.SelectedDateProperty, new System.Windows.Data.Binding(string.Format("[{0}]",col.ColumnName)));
                    datePickerFactory.SetBinding(DatePicker.DisplayDateProperty, new System.Windows.Data.Binding(string.Format("[{0}]", col.ColumnName)));
                    datePickerFactory.SetBinding(DatePicker.IsEnabledProperty, new System.Windows.Data.Binding("Status"));
                    datePickerFactory.AddHandler(DatePicker.SelectedDateChangedEvent, new EventHandler<SelectionChangedEventArgs>((o, e) => {
                        if (grid.CurrentCell.Column != null)
                        {
                            int index = grid.CurrentCell.Column.DisplayIndex;
                            DataRowView currentRow = (DataRowView)grid.SelectedItem;
                            int rowID = 0;
                            object rowIDvalue = new object();
                            object cellValue = new object();
                            if (currentRow != null)
                            {
                                rowIDvalue = currentRow.Row.ItemArray[index];
                                cellValue = currentRow.Row.ItemArray[index];
                            }
                            Int32.TryParse(rowIDvalue.ToString(), out rowID);
                            DateTime newDateTime = DateTime.Now;
                            DateTime.TryParse(cellValue.ToString(), out newDateTime);
                            cellValue = newDateTime.Date.ToShortDateString();
                        }
                    }));
                    DataGridTemplateColumn newcol = new DataGridTemplateColumn()
                    {
                        Header = col.ColumnName,
                        CellTemplate = new DataTemplate() { VisualTree = datePickerFactory }
                    };
                    grid.Columns.Add(newcol);
                }
                else if(col.DataType == typeof(bool))
                {
                    DataGridCheckBoxColumn newcol = new DataGridCheckBoxColumn();
                    newcol.Header = col.ColumnName;
                    newcol.Binding = new System.Windows.Data.Binding(string.Format("[{0}]", col.ColumnName));
                    grid.Columns.Add(newcol);
                }    
                else if(col.DataType == typeof(byte[]))
                {
                    FrameworkElementFactory buttonFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Button));
                    buttonFactory.SetBinding(System.Windows.Controls.Button.ContentProperty, new System.Windows.Data.Binding("Select an image"));
                    buttonFactory.SetBinding(System.Windows.Controls.Button.IsEnabledProperty, new System.Windows.Data.Binding("Status"));
                    buttonFactory.AddHandler(System.Windows.Controls.Button.ClickEvent, new RoutedEventHandler((o,e) => {
                        if (grid.CurrentCell.Column != null)
                        {
                            int index = grid.CurrentCell.Column.DisplayIndex;
                            DataRowView currentRow = (DataRowView)grid.SelectedItem;
                            int rowID = 0;
                            object rowIDvalue = new object();
                            object cellValue = new object();
                            if (currentRow != null)
                            {
                                rowIDvalue = currentRow.Row.ItemArray[index];
                                cellValue = currentRow.Row.ItemArray[index];
                            };
                            Int32.TryParse(rowIDvalue.ToString(), out rowID);
                            MemoryStream ms = new MemoryStream();
                            Bitmap selectedbmp;
                            byte[] buffer;
                            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                            dlg.Title = "Select an image...";
                            dlg.Filter = "Bitmap Files |*.bmp*;*.png;*.jpg*;*.jpeg*;*.tiff*;*.gif*;*.jfif*";
                            dlg.Multiselect = false;
                            bool? res = dlg.ShowDialog();
                            if (res == true)
                            {
                                selectedbmp = new Bitmap(new Bitmap(dlg.FileName), 16, 16);
                                selectedbmp.Save(ms, ImageFormat.Png);
                                buffer = ms.GetBuffer();
                                cellValue = buffer;
                            }
                        }
                    }));
                    DataGridTemplateColumn newcol = new DataGridTemplateColumn()
                    {
                        Header = col.ColumnName,
                        CellTemplate = new DataTemplate() { VisualTree = buttonFactory }
                    };
                    grid.Columns.Add(newcol);
                }
                else if (col.DataType == typeof(int) && (col.ColumnName == "OrderStatus" || col.ColumnName == "DeliveryStatus"))
                {
                    DataGridComboBoxColumn newcol = new DataGridComboBoxColumn();
                    newcol.Header = col.ColumnName;
                    newcol.DisplayIndex = columnnames.IndexOf(col.ColumnName);       
                    if (col.ColumnName == "OrderStatus")
                    {
                        newcol.ItemsSource = ProductOrderStatusList.ToArray();
                    }
                    else if(col.ColumnName == "DeliveryStatus")
                    {
                        newcol.ItemsSource = OrderDeliveryStatusList.ToArray();
                    }
                    newcol.SelectedValueBinding = new System.Windows.Data.Binding(String.Format("[{0}]", col.ColumnName));
                    newcol.DisplayMemberPath = "Key";
                    newcol.SelectedValuePath = "Value";
                    grid.Columns.Add(newcol);
                }
            }
            grid.AutoGenerateColumns = false;
            grid.ItemsSource = table.DefaultView;
           
        }



        public void TestConnection()
        {
            try
            {
                SqlConnection conn = new SqlConnection( _ConnString );
                conn.Open();
                if(conn.State == ConnectionState.Open)
                {
                    System.Windows.MessageBox.Show($"Connection successful. Connected to: {conn.ConnectionString}", "Hurrah!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                conn.Close();
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{e.Message}\n{e.StackTrace}", "Critical Error. You can thank the programmer for that",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
       
    }

    public partial class windowMain : Window
    {
        DBManager manager;
        public windowMain()
        {
            manager = new DBManager();
            InitializeComponent();
            DataContext = this;
        }

        private void frmMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void frmMain_Load(object sender, RoutedEventArgs e)
        {
            DispatcherTimer tmrCheckUser = new DispatcherTimer();
            tmrCheckUser.IsEnabled = true;
            tmrCheckUser.Interval = new TimeSpan(1);
            tmrCheckUser.Tick += tmrCheckUser_Tick;
            this.Title = $"Damage Inc. SSMS [{manager.CompanyName}]";
            foreach (ResourceDictionary dictionary in this.Resources.MergedDictionaries)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowRestoreIcon"];
                    break;
                case WindowState.Normal:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowMaximiseIcon"];
                    break;
            }
            this.Icon = (BitmapSource)Resources["AppIcon"];
            manager.TestConnection();
            manager.LoadReportDefinitions();
            manager.GetAllUsers("Users", dgContents);
            manager.GetAllClients("Clients", dgContents);
            manager.GetAllProductCategories("ProductCategories", dgContents);
            manager.GetAllProductBrands("ProductBrands", dgContents);
            manager.GetAllOrderTypes("OrderTypes", dgContents);
            manager.GetAllPaymentMethods("PaymentMethods", dgContents);
            manager.GetAllDiagnosticTypes("DiagnosticTypes", dgContents);
            manager.GetAllDeliveryServices("DeliveryServices", dgContents);
            manager.GetAllProducts("Products", dgContents);
            manager.GetAllProductImages("ProductImages", dgContents);
            manager.GetAllOrders("ProductOrders", dgContents);
            manager.GetAllOrderDeliveries("OrderDeliveries", dgContents);
            dgContents.ItemsSource = null;
            cbSelectTable.ItemsSource = manager.Tables.Keys;
            cbSelectTable.SelectedIndex = 0;
            cbSelectCriteria.ItemsSource = manager.Criterias.Keys;
            cbSelectCriteria.SelectedIndex = 0;
            cbSelectBulkOperation.SelectedIndex = 0;
            if (manager.Tables.ContainsKey(cbSelectTable.Text))
            {
                if (manager.Criterias.ContainsKey(cbSelectTable.Text))
                {
                    cbSelectCriteria.ItemsSource = manager.Criterias[cbSelectTable.Text];
                    cbSelectCriteria.SelectedIndex = 0;
                }
                if (manager.Tables[cbSelectTable.Text].Tables.Count > 0)
                {
                    manager.FillDG(dgContents, manager.Tables[cbSelectTable.Text].Tables[0]);
                }
            }
        }

        private void tmrCheckUser_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (manager.CurrentUser == null)
                {
                    dgContents.IsEnabled = false;
                    btnLogin.IsEnabled = true;
                    btnLogout.IsEnabled = false;
                    btnSearch.IsEnabled = false;
                    txtSearch.IsEnabled = false;
                    btnAdd.IsEnabled = false;
                    btnUpdate.IsEnabled = false;
                    btnDelete.IsEnabled = false;
                    btnAddToBulkList.IsEnabled = false;
                    btnRemoveFromBulkList.IsEnabled = false;
                    btnGenerateReport.IsEnabled = false;
                    btnCommitOperation.IsEnabled = false;
                    cbSelectTable.IsEnabled = false;
                    cbSelectCriteria.IsEnabled = false;
                    cbSelectBulkOperation.IsEnabled = false;
                    cbLookBelow.IsEnabled = false;
                    cbSelectReportType.IsEnabled = false;
                    lstLogs.IsEnabled = false;
                    rvViewReport.Enabled = false;
                }
                else
                {
                    dgContents.IsEnabled = true;
                    btnLogin.IsEnabled = false;
                    btnLogout.IsEnabled = true;
                    btnSearch.IsEnabled = true;
                    txtSearch.IsEnabled = true;
                    btnAdd.IsEnabled = true;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    btnAddToBulkList.IsEnabled = false;
                    btnRemoveFromBulkList.IsEnabled = true;
                    btnGenerateReport.IsEnabled = true;
                    btnCommitOperation.IsEnabled = true;
                    cbSelectTable.IsEnabled = true;
                    cbSelectCriteria.IsEnabled = true;
                    cbSelectBulkOperation.IsEnabled = true;
                    cbLookBelow.IsEnabled = true;
                    cbSelectReportType.IsEnabled = true;
                    lstLogs.IsEnabled = true;
                    rvViewReport.Enabled = true;
                    string SelectedTable = cbSelectTable.Text;
                    string SelectedBulkProcedure = cbSelectBulkOperation.Text;
                    User currentUser = (User)manager.CurrentUser;
                    if(currentUser.IsClient)
                    {
                        System.Windows.MessageBox.Show("This application isn't for clients. A web application for client/consumer use may be created later. Thank you for your patience and we are sorry for the inconvenience.", "This is an administration application", MessageBoxButton.OK, MessageBoxImage.Hand);
                        System.Windows.Application.Current.Shutdown();
                    }
                    if (SelectedTable == "Users")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if(SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if(SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if(currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "Clients")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "ProductCategories")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "ProductBrands")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "OrderTypes")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "DiagnosticTypes")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "DeliveryServices")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "PaymentMethods")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "Products")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "ProductImages")
                    {
                        if (currentUser.IsAdmin)
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                        else if (currentUser.IsWorker)
                        {
                            dgContents.IsReadOnly = true;
                            btnAdd.IsEnabled = false;
                            btnDelete.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                            btnAddToBulkList.IsEnabled = false;
                            lstBulkOperations.IsEnabled = false;
                        }
                    }
                    if (SelectedTable == "ProductOrders")
                    {
                        if (currentUser.IsAdmin || currentUser.IsWorker || (currentUser.IsAdmin && currentUser.IsWorker))
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                    }
                    if (SelectedTable == "OrderDeliveries")
                    {
                        if (currentUser.IsAdmin || currentUser.IsWorker || (currentUser.IsAdmin && currentUser.IsWorker))
                        {
                            dgContents.IsReadOnly = false;
                            btnAdd.IsEnabled = true;
                            btnDelete.IsEnabled = true;
                            lstBulkOperations.IsEnabled = true;
                            if (SelectedBulkProcedure == "Add")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkInsert;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkInsert;
                                lstBulkOperations.IsEnabled = manager.EnableBulkInsert;
                            }
                            else if (SelectedBulkProcedure == "Update")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkUpdate;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkUpdate;
                                lstBulkOperations.IsEnabled = manager.EnableBulkUpdate;
                            }
                            else if (SelectedBulkProcedure == "Delete")
                            {
                                btnAddToBulkList.IsEnabled = manager.EnableBulkDelete;
                                btnRemoveFromBulkList.IsEnabled = manager.EnableBulkDelete;
                                lstBulkOperations.IsEnabled = manager.EnableBulkDelete;
                            }
                        }
                    }
                    if (currentUser.IsAdmin)
                    {
                        lstLogs.IsEnabled = true;
                    }
                    else
                    {
                        lstLogs.IsEnabled = false;
                    }
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnMaximiseRestore_Click(object sender, RoutedEventArgs e)
        {

            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowMaximiseIcon"];
                    this.WindowState = WindowState.Normal;
                    break;
                case WindowState.Normal:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowRestoreIcon"];
                    this.WindowState = WindowState.Maximized;
                    break;
            }

        }

        private void frmMain_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowRestoreIcon"];
                    break;
                case WindowState.Normal:
                    MaximRestoreButtonIcon.Source = (ImageSource)Resources["WindowMaximiseIcon"];
                    break;
            }
        }

        private void btnMinimise_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void cbSelectTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selectedTableName = e.AddedItems[0].ToString();
                if (manager.Tables.ContainsKey(selectedTableName))
                {
                    if (selectedTableName == "Users") manager.GetAllUsers(selectedTableName, dgContents);
                    else if (selectedTableName == "Clients") manager.GetAllClients(selectedTableName, dgContents);
                    else if (selectedTableName == "ProductCategories") manager.GetAllProductCategories(selectedTableName, dgContents);
                    else if (selectedTableName == "ProductBrands") manager.GetAllProductBrands(selectedTableName, dgContents);
                    else if (selectedTableName == "DeliveryServices") manager.GetAllDeliveryServices(selectedTableName, dgContents);
                    else if (selectedTableName == "OrderTypes") manager.GetAllOrderTypes(selectedTableName, dgContents);
                    else if (selectedTableName == "DiagnosticTypes") manager.GetAllDiagnosticTypes(selectedTableName, dgContents);
                    else if (selectedTableName == "PaymentMethods") manager.GetAllPaymentMethods(selectedTableName, dgContents);
                    else if (selectedTableName == "Products") manager.GetAllProducts(selectedTableName, dgContents);
                    else if (selectedTableName == "ProductImages") manager.GetAllProductImages(selectedTableName, dgContents);
                    else if (selectedTableName == "Orders") manager.GetAllOrders(selectedTableName, dgContents);
                    else if (selectedTableName == "OrderDeliveries") manager.GetAllOrderDeliveries(selectedTableName, dgContents);
                    if (manager.Criterias.ContainsKey(selectedTableName))
                    {
                        cbSelectCriteria.ItemsSource = manager.Criterias[selectedTableName];
                        cbSelectCriteria.SelectedIndex = 0;
                    }
                    if (manager.Tables[selectedTableName].Tables.Count > 0)
                    {
                        manager.FillDG(dgContents, manager.Tables[selectedTableName].Tables[0]);
                    }
                    if (selectedTableName == "ProductOrders" || selectedTableName == "OrderDeliveries")
                    {
                        if (selectedTableName == "ProductOrders")
                        {
                            cbSelectReportType.ItemsSource = manager.OrderReports;
                        }
                        else if (selectedTableName == "OrderDeliveries")
                        {
                            cbSelectReportType.ItemsSource = manager.DeliveryReports;
                        }
                        cbSelectReportType.IsEnabled = true;
                        btnGenerateReport.IsEnabled = true;
                        cbSelectReportType.SelectedIndex = 0;
                    }
                    else
                    {
                        cbSelectReportType.IsEnabled = false;
                        btnGenerateReport.IsEnabled = false;
                    }
                }
                txtSearch.Text = "";
                cbSelectBulkOperation.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Term = "";
                string Table = "";
                string Criteria = "";
                Table = cbSelectTable.Text;
                Criteria = cbSelectCriteria.Text;
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Term = txtSearch.Text;
                    if (Table == "Users")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetUserByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserName")
                        {
                            manager.GetUserByUserName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DisplayName")
                        {
                            manager.GetUserByDisplayName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.GetUserByEmail(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.GetUserByPhone(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            manager.GetUserByBalance(IntTerm, LookBelowBalance, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetUserByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Clients")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetClientByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "UserName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetClientByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.GetClientByEmail(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.GetClientByPhone(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Address")
                        {
                            manager.GetClientByAddress(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            manager.GetClientByBalance(IntTerm, LookBelowBalance, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DeliveryServices")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetDeliveryServiceByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetDeliveryServiceByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetDeliveryServiceByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DiagnosticTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetDiagnosticTypeByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetDiagnosticTypeByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetDiagnosticTypeByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderTypeByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetOrderTypeByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "PaymentMethods")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetPaymentMethodByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetPaymentMethodByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductCategories")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductCategoryByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductCategoryByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductBrands")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductBrandByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductBrandByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductImages")
                    {
                        if (Criteria == "ImageName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductImagesByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.GetProductImagesByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDescription")
                        {
                            manager.GetProductImagesByDescription(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductQuantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductPrice")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductArtID")
                        {
                            manager.GetProductImagesByArtID(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductSerialNumber")
                        {
                            manager.GetProductImagesBySerialNumber(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductStorageLocation")
                        {
                            manager.GetProductImagesByLocation(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDate")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Products")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "CategoryID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "CategoryName")
                        {
                            manager.GetProductByCategory(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "BrandID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "BrandName")
                        {
                            manager.GetProductByBrand(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Description")
                        {
                            manager.GetProductByDescription(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ArtID")
                        {
                            manager.GetProductByArtID(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "SerialNumber")
                        {
                            manager.GetProductBySerialNumber(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "StorageLocation")
                        {
                            manager.GetProductByStorageLocation(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductOrders")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ProductID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.GetOrderByProductName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByTypeID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ReplacementProductID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ReplacementProductName")
                        {
                            manager.GetOrderByReplacementProductName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DiagnosticTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByDiagnosticTypeID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            manager.GetOrderByClientName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "UserDisplayName")
                        {
                            manager.GetOrderByUserDisplayName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByStatus(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderDeliveries")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderDeliveryByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        if (Criteria == "DeliveryServiceID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "DeliveryServiceName")
                        {
                            manager.GetOrderDeliveryByServiceName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryCargoID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        if (Criteria == "PaymentMethodID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "PaymentMethodProductName")
                        {
                            manager.GetOrderDeliveryByPaymentMethodName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            manager.GetOrderDeliveryByClientName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "OrderDate")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByOrderDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderDeliveryByStatus(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    txtSearch.Text = ""; ;
                }
                else
                {
                    if (!String.IsNullOrEmpty(Table))
                    {
                        if (manager.Tables.ContainsKey(Table))
                        {
                            if (Table == "Users") manager.GetAllUsers(Table, dgContents);
                            else if (Table == "Clients") manager.GetAllClients(Table, dgContents);
                            else if (Table == "ProductCategories") manager.GetAllProductCategories(Table, dgContents);
                            else if (Table == "ProductBrands") manager.GetAllProductBrands(Table, dgContents);
                            else if (Table == "DeliveryServices") manager.GetAllDeliveryServices(Table, dgContents);
                            else if (Table == "OrderTypes") manager.GetAllOrderTypes(Table, dgContents);
                            else if (Table == "DiagnosticTypes") manager.GetAllDiagnosticTypes(Table, dgContents);
                            else if (Table == "PaymentMethods") manager.GetAllPaymentMethods(Table, dgContents);
                            else if (Table == "Products") manager.GetAllProducts(Table, dgContents);
                            else if (Table == "ProductImages") manager.GetAllProductImages(Table, dgContents);
                            else if (Table == "Orders") manager.GetAllOrders(Table, dgContents);
                            else if (Table == "OrderDeliveries") manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Criterias.ContainsKey(Table))
                            {
                                cbSelectCriteria.ItemsSource = manager.Criterias[Table];
                                cbSelectCriteria.SelectedIndex = 0;
                            }
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[cbSelectTable.Text].Tables[0]);
                            }
                        }
                    }
                    txtSearch.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Term = "";
                string Table = "";
                string Criteria = "";
                Table = cbSelectTable.Text;
                Criteria = cbSelectCriteria.Text;
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Term = txtSearch.Text;
                    if (Table == "Users")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByID(IntTerm, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserName")
                        {
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByUserName(Term, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DisplayName")
                        {
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByDisplayName(Term, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByEmail(Term, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByPhone(Term, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByBalance(IntTerm, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            DataRow currentRow;
                            string NewUserName = "";
                            string NewDisplayName = "";
                            string NewEmail = "";
                            string NewPassword = "";
                            string NewPhone = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            byte[] imgbinary = null;
                            bool NewIsAdmin = false;
                            bool NewIsWorker = false;
                            bool NewIsClient = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewUserName = currentRow.ItemArray[1].ToString();
                                NewDisplayName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPassword = currentRow.ItemArray[4].ToString();
                                NewPhone = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[8].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[8]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                                Boolean.TryParse(currentRow.ItemArray[9].ToString(), out NewIsAdmin);
                                Boolean.TryParse(currentRow.ItemArray[10].ToString(), out NewIsWorker);
                                Boolean.TryParse(currentRow.ItemArray[11].ToString(), out NewIsClient);
                            }
                            manager.UpdateUserByDate(TargetDateTime, NewUserName, NewDisplayName, NewEmail, NewPassword, NewPhone, NewBalance, NewProfilePic, NewIsAdmin, NewIsWorker, NewIsClient, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Clients")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByID(IntTerm, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "UserName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByName(Term, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByEmail(Term, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByPhone(Term, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Address")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByAddress(Term, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            string NewEmail = "";
                            string NewPhone = "";
                            string NewAddress = "";
                            int NewBalance = 0;
                            Bitmap NewProfilePic = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[2].ToString();
                                NewEmail = currentRow.ItemArray[3].ToString();
                                NewPhone = currentRow.ItemArray[4].ToString();
                                NewAddress = currentRow.ItemArray[5].ToString();
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewBalance);
                                if (currentRow.ItemArray[7].GetType() != typeof(DBNull))
                                {
                                    NewProfilePic = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[7]);
                                    if (NewProfilePic == null)
                                    {
                                        NewProfilePic = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateClientByBalance(IntTerm, NewName, NewEmail, NewPhone, NewAddress, NewBalance, NewProfilePic, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DeliveryServices")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDeliveryServiceByID(IntTerm, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDeliveryServiceByName(Term, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDeliveryServiceByPrice(IntTerm, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DiagnosticTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDiagnosticTypeByID(IntTerm, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDiagnosticTypeByName(Term, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            int NewPrice = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewPrice);
                            }
                            manager.UpdateDiagnosticTypeByPrice(IntTerm, NewName, NewPrice, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateOrderTypeByID(IntTerm, NewName, Table, dgContents);
                            manager.GetAllOrderTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateOrderTypeByName(Term, NewName, Table, dgContents);
                            manager.GetAllOrderTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "PaymentMethods")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdatePaymentMethodByID(IntTerm, NewName, Table, dgContents);
                            manager.GetAllPaymentMethods(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdatePaymentMethodByName(Term, NewName, Table, dgContents);
                            manager.GetAllPaymentMethods(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductCategories")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateProductCategoryByID(IntTerm, NewName, Table, dgContents);
                            manager.GetAllProductCategories(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateProductCategoryByName(Term, NewName, Table, dgContents);
                            manager.GetAllProductCategories(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductBrands")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateProductBrandByID(IntTerm, NewName, Table, dgContents);
                            manager.GetAllProductBrands(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            string NewName = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewName = currentRow.ItemArray[1].ToString();
                            }
                            manager.UpdateProductBrandByName(Term, NewName, Table, dgContents);
                            manager.GetAllProductBrands(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductImages")
                    {
                        if (Criteria == "ImageName")
                        {
                            DataRow currentRow;
                            string NewImageName = "";
                            Bitmap NewProductImage = null;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                NewImageName = currentRow.ItemArray[1].ToString();
                                if (currentRow.ItemArray[2].GetType() != typeof(DBNull))
                                {
                                    NewProductImage = ImageDecoderEncoder.DecodeImage((byte[])currentRow.ItemArray[2]);
                                    if (NewProductImage == null)
                                    {
                                        NewProductImage = new Bitmap(16, 16);
                                    }
                                }
                            }
                            manager.UpdateProductImage(Term, NewImageName, NewProductImage, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductDescription")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductQuantity")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductPrice")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductArtID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductSerialNumber")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductStorageLocation")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductDate")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                    }
                    else if (Table == "Products")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByID(IntTerm, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "CategoryID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "CategoryName")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByCategory(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "BrandID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "BrandName")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByBrand(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByName(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Description")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByDescription(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByQuantity(IntTerm, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByPrice(IntTerm, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ArtID")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByArtID(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "SerialNumber")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductBySerialNumber(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "StorageLocation")
                        {
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByDescription(Term, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            DataRow currentRow;
                            int NewCategoryID = 0;
                            int NewBrandID = 0;
                            string NewProductName = "";
                            string NewProductDescription = "";
                            int NewProductQuantity = 0;
                            int NewProductPrice = 0;
                            string NewProductArtID = "";
                            string NewProductSerialNumber = "";
                            string NewProductStorageLocation = "";
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewCategoryID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewBrandID);
                                NewProductName = currentRow.ItemArray[3].ToString();
                                NewProductDescription = currentRow.ItemArray[4].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewProductQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewProductPrice);
                                NewProductArtID = currentRow.ItemArray[7].ToString();
                                NewProductSerialNumber = currentRow.ItemArray[8].ToString();
                                NewProductStorageLocation = currentRow.ItemArray[9].ToString();
                            }
                            manager.UpdateProductByDate(TargetDateTime, NewCategoryID, NewBrandID, NewProductName, NewProductDescription, NewProductQuantity, NewProductPrice, NewProductArtID, NewProductSerialNumber, NewProductStorageLocation, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductOrders")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByProductID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductName")
                        {
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByProductName(Term, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByTypeID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ReplacementProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByReplacementProductID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ReplacementProductName")
                        {
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByReplacementProductName(Term, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DiagnosticTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByDiagnosticTypeID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByDesiredQuantity(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByPrice(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByClientID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ClientName")
                        {
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByClientName(Term, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByUserID(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserDisplayName")
                        {
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByUserDisplayName(Term, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByDate(TargetDateTime, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewProductID = 0;
                            int NewOrderTypeID = 0;
                            int NewReplacementProductID = 0;
                            int NewDiagnosticTypeID = 0;
                            int NewDesiredQuantity = 0;
                            int NewOrderPrice = 0;
                            int NewClientID = 0;
                            int NewUserID = 0;
                            int NewStatus = 0;
                            bool SetNewPriceAsTotal = false;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewProductID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewOrderTypeID);
                                Int32.TryParse(currentRow.ItemArray[3].ToString(), out NewReplacementProductID);
                                Int32.TryParse(currentRow.ItemArray[4].ToString(), out NewDiagnosticTypeID);
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewDesiredQuantity);
                                Int32.TryParse(currentRow.ItemArray[6].ToString(), out NewOrderPrice);
                                Int32.TryParse(currentRow.ItemArray[7].ToString(), out NewClientID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewUserID);
                                Int32.TryParse(currentRow.ItemArray[11].ToString(), out NewStatus);
                            }
                            if (System.Windows.MessageBox.Show("Do you want to set the new price as a total price?", "Product Order update", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                SetNewPriceAsTotal = true;
                            }
                            else
                            {
                                SetNewPriceAsTotal = false;
                            }
                            manager.UpdateOrderByStatus(IntTerm, NewProductID, NewOrderTypeID, NewReplacementProductID, NewDiagnosticTypeID, NewDesiredQuantity, NewOrderPrice, NewClientID, NewUserID, NewStatus, SetNewPriceAsTotal, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderDeliveries")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByID(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByOrderID(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryServiceID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByDeliveryServiceID(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DeliveryServiceName")
                        {
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByDeliveryServiceName(Term, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryCargoID")
                        {
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByDeliveryCargoID(Term, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "PaymentMethodID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByPaymentMethodID(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "PaymentMethodProductName")
                        {
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByPaymentMethodName(Term, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByPrice(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "OrderDate")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByDate(TargetDateTime, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            DataRow currentRow;
                            int NewOrderID = 0;
                            int NewDeliveryServiceID = 0;
                            string NewDeliveryCargoID = "";
                            int NewPaymentMethodID = 0;
                            int NewStatus = 0;
                            if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                            {
                                DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                                currentRow = selectedRowView.Row;
                                Int32.TryParse(currentRow.ItemArray[1].ToString(), out NewOrderID);
                                Int32.TryParse(currentRow.ItemArray[2].ToString(), out NewDeliveryServiceID);
                                NewDeliveryCargoID = currentRow.ItemArray[3].ToString();
                                Int32.TryParse(currentRow.ItemArray[5].ToString(), out NewPaymentMethodID);
                                Int32.TryParse(currentRow.ItemArray[8].ToString(), out NewStatus);
                            }
                            manager.UpdateOrderDeliveryByStatus(IntTerm, NewOrderID, NewDeliveryServiceID, NewDeliveryCargoID, NewPaymentMethodID, NewStatus, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    txtSearch.Text = "";
                }
                else
                {
                    if (manager.Tables.ContainsKey(Table))
                    {
                        if (Table == "Users") manager.GetAllUsers(Table, dgContents);
                        else if (Table == "Clients") manager.GetAllClients(Table, dgContents);
                        else if (Table == "ProductCategories") manager.GetAllProductCategories(Table, dgContents);
                        else if (Table == "ProductBrands") manager.GetAllProductBrands(Table, dgContents);
                        else if (Table == "DeliveryServices") manager.GetAllDeliveryServices(Table, dgContents);
                        else if (Table == "OrderTypes") manager.GetAllOrderTypes(Table, dgContents);
                        else if (Table == "DiagnosticTypes") manager.GetAllDiagnosticTypes(Table, dgContents);
                        else if (Table == "PaymentMethods") manager.GetAllPaymentMethods(Table, dgContents);
                        else if (Table == "Products") manager.GetAllProducts(Table, dgContents);
                        else if (Table == "ProductImages") manager.GetAllProductImages(Table, dgContents);
                        else if (Table == "Orders") manager.GetAllOrders(Table, dgContents);
                        else if (Table == "OrderDeliveries") manager.GetAllOrderDeliveries(Table, dgContents);
                        if (manager.Criterias.ContainsKey(Table))
                        {
                            cbSelectCriteria.ItemsSource = manager.Criterias[Table];
                            cbSelectCriteria.SelectedIndex = 0;
                        }
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                    txtSearch.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Term = "";
                string Table = "";
                string Criteria = "";
                Table = cbSelectTable.Text;
                Criteria = cbSelectCriteria.Text;
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Term = txtSearch.Text;
                    if (Table == "Users")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteUserByID(IntTerm, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserName")
                        {
                            manager.DeleteUserByUserName(Term, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DisplayName")
                        {
                            manager.DeleteUserByDisplayName(Term, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.DeleteUserByEmail(Term, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.DeleteUserByPhone(Term, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            bool LookAboveBalance = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteUserByBalance(IntTerm, LookBelowBalance, LookAboveBalance, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            bool LookAfterDate = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteUserByDate(TargetDateTime, LookBeforeDate, LookBeforeDate, Table, dgContents);
                            manager.GetAllUsers(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Clients")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteClientByID(IntTerm, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteClientByUserID(IntTerm, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserName")
                        {
                            bool AlsoDeleteUser = false;
                            if (System.Windows.MessageBox.Show("Do you want to delete the user data of this client?", "Client deletion", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                            {
                                AlsoDeleteUser = true;
                            }
                            else
                            {
                                AlsoDeleteUser = false;
                            }
                            manager.DeleteClientByUserName(Term, AlsoDeleteUser, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteClientByName(Term, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.DeleteClientByEmail(Term, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.DeleteClientByPhone(Term, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Address")
                        {
                            manager.DeleteClientByAddress(Term, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            bool LookAboveBalance = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteClientByBalance(IntTerm, LookBelowBalance, LookAboveBalance, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DeliveryServices")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteDeliveryServiceByID(IntTerm, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteDeliveryServiceByName(Term, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteDeliveryServiceByPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllDeliveryServices(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DiagnosticTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteDiagnosticTypeByID(IntTerm, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteDiagnosticTypeByName(Term, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteDiagnosticTypeByPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllDiagnosticTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderTypeByID(IntTerm, Table, dgContents);
                            manager.GetAllOrderTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteOrderTypeByName(Term, Table, dgContents);
                            manager.GetAllOrderTypes(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "PaymentMethods")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeletePaymentMethodByID(IntTerm, Table, dgContents);
                            manager.GetAllPaymentMethods(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeletePaymentMethodByName(Term, Table, dgContents);
                            manager.GetAllPaymentMethods(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductCategories")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductCategoryByID(IntTerm, Table, dgContents);
                            manager.GetAllProductCategories(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteProductCategoryByName(Term, Table, dgContents);
                            manager.GetAllProductCategories(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductBrands")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductBrandByID(IntTerm, Table, dgContents);
                            manager.GetAllProductBrands(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteProductBrandByName(Term, Table, dgContents);
                            manager.GetAllProductBrands(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductImages")
                    {
                        if (Criteria == "ImageName")
                        {
                            manager.DeleteProductImage(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductImageByProductID(IntTerm, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.DeleteProductImageByProductName(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDescription")
                        {
                            manager.DeleteProductImageByProductDescription(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductQuantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            bool LookAboveQuantity = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductImageByProductQuantity(IntTerm, LookBelowQuantity, LookAboveQuantity, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductPrice")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductImageByProductPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductArtID")
                        {
                            manager.DeleteProductImageByProductArtID(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductSerialNumber")
                        {
                            manager.DeleteProductImageBySerialNumber(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductStorageLocation")
                        {
                            manager.DeleteProductImageByProductStorageLocation(Term, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDate")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            ; bool LookAfterDate = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductImageByProductDate(TargetDateTime, LookBeforeDate, LookAfterDate, Table, dgContents);
                            manager.GetAllProductImages(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Products")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductByID(IntTerm, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "CategoryID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductByCategoryID(IntTerm, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "CategoryName")
                        {
                            manager.DeleteProductByCategoryName(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "BrandID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductByBrandID(IntTerm, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "BrandName")
                        {
                            manager.DeleteProductByBrandName(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.DeleteProductByName(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Description")
                        {
                            manager.DeleteProductByDescription(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            bool LookAboveQuantity = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductByQuantity(IntTerm, LookBelowQuantity, LookAboveQuantity, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductByPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ArtID")
                        {
                            manager.DeleteProductByArtID(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "SerialNumber")
                        {
                            manager.DeleteProductBySerialNumber(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "StorageLocation")
                        {
                            manager.DeleteProductByStorageLocation(Term, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            bool LookAfterDate = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductByDate(TargetDateTime, LookBeforeDate, LookAfterDate, Table, dgContents);
                            manager.GetAllProducts(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductOrders")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByProductID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.DeleteProductOrderByProductName(Term, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByTypeID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ReplacementProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByReplacementProductID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ReplacementProductName")
                        {
                            manager.DeleteProductOrderByReplacementProductName(Term, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DiagnosticTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByDiagnosticTypeID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            bool LookAboveQuantity = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductOrderByQuantity(IntTerm, LookBelowQuantity, LookAboveQuantity, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductOrderByPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByClientID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ClientName")
                        {
                            manager.DeleteProductOrderByClientName(Term, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByUserID(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserDisplayName")
                        {
                            manager.DeleteProductOrderByUserDisplayName(Term, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            bool LookAfterDate = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteProductOrderByDate(TargetDateTime, LookBeforeDate, LookAfterDate, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteProductOrderByStatus(IntTerm, Table, dgContents);
                            manager.GetAllOrders(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderDeliveries")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderDeliveryByID(IntTerm, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderDeliveryByOrderID(IntTerm, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryServiceID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderDeliveryByDeliveryServiceID(IntTerm, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DeliveryServiceName")
                        {
                            manager.DeleteOrderDeliveryByDeliveryServiceName(Term, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryCargoID")
                        {
                            manager.DeleteOrderDeliveryByCargoID(Term, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "PaymentMethodID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderDeliveryByPaymentMethodID(IntTerm, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "PaymentMethodProductName")
                        {
                            manager.DeleteOrderDeliveryByPaymentMethodName(Term, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            bool LookAbovePrice = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteOrderDeliveryByPrice(IntTerm, LookBelowPrice, LookAbovePrice, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "OrderDate")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            bool LookAfterDate = (cbLookBelow.SelectedIndex == 1);
                            manager.DeleteOrderDeliveryByDate(TargetDateTime, LookBeforeDate, LookAfterDate, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.DeleteOrderDeliveryByStatus(IntTerm, Table, dgContents);
                            manager.GetAllOrderDeliveries(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    txtSearch.Text = "";
                }
                else
                {
                    if (manager.Tables.ContainsKey(Table))
                    {
                        if (Table == "Users") manager.GetAllUsers(Table, dgContents);
                        else if (Table == "Clients") manager.GetAllClients(Table, dgContents);
                        else if (Table == "ProductCategories") manager.GetAllProductCategories(Table, dgContents);
                        else if (Table == "ProductBrands") manager.GetAllProductBrands(Table, dgContents);
                        else if (Table == "DeliveryServices") manager.GetAllDeliveryServices(Table, dgContents);
                        else if (Table == "OrderTypes") manager.GetAllOrderTypes(Table, dgContents);
                        else if (Table == "DiagnosticTypes") manager.GetAllDiagnosticTypes(Table, dgContents);
                        else if (Table == "PaymentMethods") manager.GetAllPaymentMethods(Table, dgContents);
                        else if (Table == "Products") manager.GetAllProducts(Table, dgContents);
                        else if (Table == "ProductImages") manager.GetAllProductImages(Table, dgContents);
                        else if (Table == "Orders") manager.GetAllOrders(Table, dgContents);
                        else if (Table == "OrderDeliveries") manager.GetAllOrderDeliveries(Table, dgContents);
                        if (manager.Criterias.ContainsKey(Table))
                        {
                            cbSelectCriteria.ItemsSource = manager.Criterias[Table];
                            cbSelectCriteria.SelectedIndex = 0;
                        }
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                    txtSearch.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Table = "";
                Table = cbSelectTable.Text;
                if (Table == "Users")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string UserName = selectedRow.Row.ItemArray[1].ToString();
                        string DisplayName = selectedRow.Row.ItemArray[2].ToString();
                        string Email = selectedRow.Row.ItemArray[3].ToString();
                        string Password = selectedRow.Row.ItemArray[4].ToString();
                        string Phone = selectedRow.Row.ItemArray[5].ToString();
                        string Address = "sample address";
                        Bitmap ProfilePic;
                        if (selectedRow.Row.ItemArray[8] != null && selectedRow.Row.ItemArray[8].GetType() != typeof(DBNull))
                        {
                            ProfilePic = ImageDecoderEncoder.DecodeImage((byte[])selectedRow.Row.ItemArray[8]);
                        }
                        else
                        {
                            ProfilePic = new Bitmap(16, 16);
                        }
                        bool IsAdmin = Convert.ToBoolean(selectedRow.Row.ItemArray[9].ToString());
                        bool IsWorker = Convert.ToBoolean(selectedRow.Row.ItemArray[10].ToString());
                        bool IsClient = Convert.ToBoolean(selectedRow.Row.ItemArray[11].ToString());
                        if (IsAdmin && !IsWorker && !IsClient)
                        {
                            manager.RegisterAdmin(UserName, DisplayName, Email, Password, Phone, ProfilePic, Table, dgContents);
                        }
                        else if (IsWorker && !IsAdmin && !IsClient)
                        {
                            manager.RegisterWorker(UserName, DisplayName, Email, Password, Phone, ProfilePic, Table, dgContents);
                        }
                        else if (IsClient && !IsAdmin && !IsWorker)
                        {
                            manager.RegisterClient(UserName, DisplayName, Email, Password, Phone, Address, ProfilePic, Table, dgContents);
                        }
                        else
                        {
                            manager.RegisterUser(UserName, DisplayName, Email, Password, Phone, ProfilePic, IsAdmin, IsWorker, IsClient, Table, dgContents);
                        }
                        manager.GetAllUsers("Users", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Clients")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string UserName = "USER_" + selectedRow.Row[0].ToString();
                        string DisplayName = selectedRow.Row.ItemArray[2].ToString();
                        string Email = selectedRow.Row.ItemArray[3].ToString();
                        string Password = $"{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()},{new Random().NextInt64()}";
                        string Phone = selectedRow.Row.ItemArray[4].ToString();
                        string Address = selectedRow.Row.ItemArray[5].ToString();
                        Bitmap ProfilePic;
                        if (selectedRow.Row.ItemArray[7] != null && selectedRow.Row.ItemArray[7].GetType() != typeof(DBNull))
                        {
                            ProfilePic = ImageDecoderEncoder.DecodeImage((byte[])selectedRow.Row.ItemArray[7]);
                        }
                        else
                        {
                            ProfilePic = new Bitmap(16, 16);
                        }
                        if (System.Windows.MessageBox.Show("Do you want to register this client?", "Prompt", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            manager.RegisterClient(UserName, DisplayName, Email, Password, Phone, Address, ProfilePic, Table, dgContents);
                        }
                        else
                        {
                            manager.AddClientWithoutRegistering(DisplayName, Email, Phone, Address, ProfilePic, Table, dgContents);
                        }
                        manager.GetAllClients("Clients", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DeliveryServices")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string ServiceName = selectedRow.Row.ItemArray[1].ToString();
                        decimal ServicePrice = Convert.ToDecimal(selectedRow.Row.ItemArray[2]);
                        int ServicePriceInt = Convert.ToInt32(ServicePrice);
                        manager.AddDeliveryService(ServiceName, ServicePriceInt, Table, dgContents);
                        manager.GetAllDeliveryServices("DeliveryServices", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "DiagnosticTypes")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string TypeName = selectedRow.Row.ItemArray[1].ToString();
                        decimal TypePrice = Convert.ToDecimal(selectedRow.Row.ItemArray[2]);
                        int TypePriceInt = Convert.ToInt32(TypePrice);
                        manager.AddDiagnosticType(TypeName, TypePriceInt, Table, dgContents);
                        manager.GetAllDiagnosticTypes("DiagnosticTypes", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderTypes")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string TypeName = selectedRow.Row.ItemArray[1].ToString();
                        manager.AddOrderType(TypeName, Table, dgContents);
                        manager.GetAllOrderTypes("OrderTypes", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "PaymentMethods")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string MethodName = selectedRow.Row.ItemArray[1].ToString();
                        manager.AddPaymentMethod(MethodName, Table, dgContents);
                        manager.GetAllPaymentMethods("PaymentMethods", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductCategories")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string CategoryName = selectedRow.Row.ItemArray[1].ToString();
                        manager.AddProductCategory(CategoryName, Table, dgContents);
                        manager.GetAllProductCategories("ProductCategories", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductBrands")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        string BrandName = selectedRow.Row.ItemArray[1].ToString();
                        manager.AddProductBrand(BrandName, Table, dgContents);
                        manager.GetAllProductBrands("ProductBrands", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductImages")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        int currentRowIndex = dgContents.Items.IndexOf(dgContents.SelectedItem);
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        int TargetProductID = Convert.ToInt32(selectedRow.Row.ItemArray[0].ToString());
                        string ImageName = $"PRODUCT_{TargetProductID}_IMAGE_{currentRowIndex + 1}";
                        Bitmap product_image;
                        if (selectedRow.Row.ItemArray[2] != null && selectedRow.Row.ItemArray[2].GetType() != typeof(DBNull))
                        {
                            product_image = ImageDecoderEncoder.DecodeImage((byte[])selectedRow.Row.ItemArray[2]);
                        }
                        else
                        {
                            product_image = new Bitmap(16, 16);
                        }
                        manager.AddProductImage(TargetProductID, ImageName, product_image, Table, dgContents);
                        manager.GetAllProductImages("ProductImages", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "Products")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        int TargetCategoryID = Convert.ToInt32(selectedRow.Row.ItemArray[1].ToString());
                        int TargetBrandID = Convert.ToInt32(selectedRow.Row.ItemArray[2].ToString());
                        string ProductName = selectedRow.Row.ItemArray[3].ToString();
                        string ProductDescription = selectedRow.Row.ItemArray[4].ToString();
                        int Quantity = Convert.ToInt32(selectedRow.Row.ItemArray[5].ToString());
                        int Price = Convert.ToInt32(selectedRow.Row.ItemArray[6].ToString());
                        string ArticuleNumber = selectedRow.Row.ItemArray[7].ToString();
                        string SerialNumber = selectedRow.Row.ItemArray[8].ToString();
                        string StorageLocation = selectedRow.Row.ItemArray[9].ToString();
                        manager.AddProduct(TargetCategoryID, TargetBrandID, ProductName, ProductDescription, Quantity, Price, ArticuleNumber, SerialNumber, StorageLocation, Table, dgContents);
                        manager.GetAllProducts("Products", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "ProductOrders")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        int TargetProductID = Convert.ToInt32(selectedRow.Row.ItemArray[1].ToString());
                        int TargetOrderTypeID = Convert.ToInt32(selectedRow.Row.ItemArray[2].ToString());
                        int TargetReplacementProductID = Convert.ToInt32(selectedRow.Row.ItemArray[3].ToString());
                        int TargetDiagnosticTypeID = Convert.ToInt32(selectedRow.Row.ItemArray[4].ToString());
                        int DesiredQuantity = Convert.ToInt32(selectedRow.Row.ItemArray[5].ToString());
                        int OrderPrice = Convert.ToInt32(selectedRow.Row.ItemArray[6].ToString());
                        int TargetClientID = Convert.ToInt32(selectedRow.Row.ItemArray[7].ToString());
                        int TargetUserID = Convert.ToInt32(selectedRow.Row.ItemArray[8].ToString());
                        bool SetPriceAsTotal = false;
                        if (System.Windows.MessageBox.Show("Did you set the price for selected order total?", "Total Price or Not", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
                        {
                            SetPriceAsTotal = true;
                        }
                        else
                        {
                            SetPriceAsTotal = false;
                        }
                        manager.AddOrder(TargetProductID, TargetOrderTypeID, TargetReplacementProductID, TargetDiagnosticTypeID, DesiredQuantity, OrderPrice, TargetClientID, TargetUserID, SetPriceAsTotal, Table, dgContents);
                        manager.GetAllOrders("ProductOrders", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                else if (Table == "OrderDeliveries")
                {
                    if (dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView selectedRow = (DataRowView)dgContents.SelectedItem;
                        int TargetOrderID = Convert.ToInt32(selectedRow.Row.ItemArray[1].ToString());
                        int TargetDeliveryServiceID = Convert.ToInt32(selectedRow.Row.ItemArray[2].ToString());
                        string DeliveryCargoID = selectedRow.Row.ItemArray[3].ToString();
                        int TargetPaymentMethodID = Convert.ToInt32(selectedRow.Row.ItemArray[5].ToString());
                        manager.AddOrderDelivery(TargetOrderID, TargetDeliveryServiceID, DeliveryCargoID, TargetPaymentMethodID, Table, dgContents);
                        manager.GetAllOrderDeliveries("OrderDeliveries", dgContents);
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
                txtSearch.Text = "";
                if (manager.Tables.ContainsKey(Table))
                {
                    if (Table == "Users") manager.GetAllUsers(Table, dgContents);
                    else if (Table == "Clients") manager.GetAllClients(Table, dgContents);
                    else if (Table == "ProductCategories") manager.GetAllProductCategories(Table, dgContents);
                    else if (Table == "ProductBrands") manager.GetAllProductBrands(Table, dgContents);
                    else if (Table == "DeliveryServices") manager.GetAllDeliveryServices(Table, dgContents);
                    else if (Table == "OrderTypes") manager.GetAllOrderTypes(Table, dgContents);
                    else if (Table == "DiagnosticTypes") manager.GetAllDiagnosticTypes(Table, dgContents);
                    else if (Table == "PaymentMethods") manager.GetAllPaymentMethods(Table, dgContents);
                    else if (Table == "Products") manager.GetAllProducts(Table, dgContents);
                    else if (Table == "ProductImages") manager.GetAllProductImages(Table, dgContents);
                    else if (Table == "Orders") manager.GetAllOrders(Table, dgContents);
                    else if (Table == "OrderDeliveries") manager.GetAllOrderDeliveries(Table, dgContents);
                    if (manager.Criterias.ContainsKey(Table))
                    {
                        cbSelectCriteria.ItemsSource = manager.Criterias[Table];
                        cbSelectCriteria.SelectedIndex = 0;
                    }
                    if (manager.Tables[Table].Tables.Count > 0)
                    {
                        manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                    }
                    txtSearch.Text = "";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void dgContents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgContents.SelectedItem != null && dgContents.SelectedItem.GetType() == typeof(DataRowView))
                {
                    int index = 0;
                    if (dgContents.CurrentCell != null && dgContents.CurrentCell.Column != null)
                    {
                        index = dgContents.CurrentCell.Column.DisplayIndex;
                        DataRowView row = (DataRowView)dgContents.SelectedItem;
                        var value = row.Row.ItemArray[index];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Term = "";
                string Table = "";
                string Criteria = "";
                Table = cbSelectTable.Text;
                Criteria = cbSelectCriteria.Text;
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Term = txtSearch.Text;
                    if (Table == "Users")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetUserByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "UserName")
                        {
                            manager.GetUserByUserName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "DisplayName")
                        {
                            manager.GetUserByDisplayName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.GetUserByEmail(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.GetUserByPhone(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            manager.GetUserByBalance(IntTerm, LookBelowBalance, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetUserByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Clients")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetClientByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "UserName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetClientByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Email")
                        {
                            manager.GetClientByEmail(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Phone")
                        {
                            manager.GetClientByPhone(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Address")
                        {
                            manager.GetClientByAddress(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Balance")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowBalance = (cbLookBelow.SelectedIndex == 0);
                            manager.GetClientByBalance(IntTerm, LookBelowBalance, Table, dgContents);
                            manager.GetAllClients(Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DeliveryServices")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetDeliveryServiceByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetDeliveryServiceByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetDeliveryServiceByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "DiagnosticTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetDiagnosticTypeByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetDiagnosticTypeByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetDiagnosticTypeByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderTypes")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderTypeByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetOrderTypeByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "PaymentMethods")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetPaymentMethodByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetPaymentMethodByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductCategories")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductCategoryByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductCategoryByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductBrands")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductBrandByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductBrandByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductImages")
                    {
                        if (Criteria == "ImageName")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductImagesByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.GetProductImagesByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDescription")
                        {
                            manager.GetProductImagesByDescription(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductQuantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductPrice")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductArtID")
                        {
                            manager.GetProductImagesByArtID(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductSerialNumber")
                        {
                            manager.GetProductImagesBySerialNumber(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductStorageLocation")
                        {
                            manager.GetProductImagesByLocation(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ProductDate")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductImagesByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "Products")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetProductByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "CategoryID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "CategoryName")
                        {
                            manager.GetProductByCategory(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "BrandID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "BrandName")
                        {
                            manager.GetProductByBrand(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Name")
                        {
                            manager.GetProductByName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Description")
                        {
                            manager.GetProductByDescription(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "ArtID")
                        {
                            manager.GetProductByArtID(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "SerialNumber")
                        {
                            manager.GetProductBySerialNumber(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "StorageLocation")
                        {
                            manager.GetProductByStorageLocation(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetProductByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "ProductOrders")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ProductID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ProductName")
                        {
                            manager.GetOrderByProductName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByTypeID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ReplacementProductID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ReplacementProductName")
                        {
                            manager.GetOrderByReplacementProductName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DiagnosticTypeID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByDiagnosticTypeID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Quantity")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowQuantity = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByQuantity(IntTerm, LookBelowQuantity, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            manager.GetOrderByClientName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "UserID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "UserDisplayName")
                        {
                            manager.GetOrderByUserDisplayName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderByStatus(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                    else if (Table == "OrderDeliveries")
                    {
                        if (Criteria == "ID")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderDeliveryByID(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "OrderID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        if (Criteria == "DeliveryServiceID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "DeliveryServiceName")
                        {
                            manager.GetOrderDeliveryByServiceName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "DeliveryCargoID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        if (Criteria == "PaymentMethodID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "PaymentMethodProductName")
                        {
                            manager.GetOrderDeliveryByPaymentMethodName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Price")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            bool LookBelowPrice = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByPrice(IntTerm, LookBelowPrice, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "ClientID")
                        {
                            System.Windows.MessageBox.Show("This criteria is not supported for the requested operation", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        else if (Criteria == "ClientName")
                        {
                            manager.GetOrderDeliveryByClientName(Term, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "OrderDate")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByOrderDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        else if (Criteria == "Date")
                        {
                            DateTime TargetDateTime = DateTime.Now;
                            DateTime.TryParse(Term, out TargetDateTime);
                            bool LookBeforeDate = (cbLookBelow.SelectedIndex == 0);
                            manager.GetOrderDeliveryByDate(TargetDateTime, LookBeforeDate, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                        if (Criteria == "Status")
                        {
                            int IntTerm = 0;
                            Int32.TryParse(Term, out IntTerm);
                            manager.GetOrderDeliveryByStatus(IntTerm, Table, dgContents);
                            if (manager.Tables[Table].Tables.Count > 0)
                            {
                                manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                            }
                        }
                    }
                }
                else
                {
                    if (manager.Tables.ContainsKey(Table))
                    {
                        if (Table == "Users") manager.GetAllUsers(Table, dgContents);
                        else if (Table == "Clients") manager.GetAllClients(Table, dgContents);
                        else if (Table == "ProductCategories") manager.GetAllProductCategories(Table, dgContents);
                        else if (Table == "ProductBrands") manager.GetAllProductBrands(Table, dgContents);
                        else if (Table == "DeliveryServices") manager.GetAllDeliveryServices(Table, dgContents);
                        else if (Table == "OrderTypes") manager.GetAllOrderTypes(Table, dgContents);
                        else if (Table == "DiagnosticTypes") manager.GetAllDiagnosticTypes(Table, dgContents);
                        else if (Table == "PaymentMethods") manager.GetAllPaymentMethods(Table, dgContents);
                        else if (Table == "Products") manager.GetAllProducts(Table, dgContents);
                        else if (Table == "ProductImages") manager.GetAllProductImages(Table, dgContents);
                        else if (Table == "Orders") manager.GetAllOrders(Table, dgContents);
                        else if (Table == "OrderDeliveries") manager.GetAllOrderDeliveries(Table, dgContents);
                        if (manager.Criterias.ContainsKey(Table))
                        {
                            cbSelectCriteria.ItemsSource = manager.Criterias[Table];
                            cbSelectCriteria.SelectedIndex = 0;
                        }
                        if (manager.Tables[Table].Tables.Count > 0)
                        {
                            manager.FillDG(dgContents, manager.Tables[Table].Tables[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Term = "";
                string Table = "";
                string Criteria = "";
                DataSet reportds = new DataSet();
                string ReportSource = "";
                List<ReportParameter> report_params = new List<ReportParameter>();
                DataRow selectedRow = null;
                int targetID = 0;
                bool LookBelowCertainCriterias = false;
                Table = cbSelectTable.Text;
                Criteria = cbSelectCriteria.Text;
                ReportSource = $"{System.IO.Directory.GetCurrentDirectory()}/{manager.ReportDefinitionPath}/{cbSelectReportType.Text}.rdlc";
                LookBelowCertainCriterias = (cbLookBelow.SelectedIndex == 0);
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Term = txtSearch.Text;
                    manager.GenerateReportDataSet(Term, LookBelowCertainCriterias, Table, Criteria, out reportds);
                    if (dgContents.SelectedItem != null && dgContents.SelectedItem.GetType() == typeof(DataRowView))
                    {
                        DataRowView currentView = (DataRowView)dgContents.SelectedItem;
                        selectedRow = currentView.Row;
                        Int32.TryParse(selectedRow[0].ToString(), out targetID);
                        manager.GenerateReportDataSet(Term, LookBelowCertainCriterias, Table, Criteria, out reportds);
                        ReportDataSource reportdata = new ReportDataSource();
                        rvViewReport.LocalReport.DataSources.Clear();
                        rvViewReport.LocalReport.ReportPath = ReportSource;
                        rvViewReport.LocalReport.DataSources.Add(reportdata);
                        ReportParameter idparam = new ReportParameter();
                        if (Table == "ProductOrders")
                        {
                            OrderReportDS ds = new OrderReportDS();
                            ds.Tables.Clear();
                            ds.Tables.Add(reportds.Tables[0].Copy());
                            reportdata.Name = "OrderReportDS";
                            reportdata.Value = ds.Tables[0];
                            idparam = new ReportParameter("orderid");
                            idparam.Values.Add(targetID.ToString());
                        }
                        else if(Table == "OrderDeliveries")
                        {
                            DeliveryReportDS ds = new DeliveryReportDS();
                            ds.Tables.Clear();
                            ds.Tables.Add(reportds.Tables[0].Copy());
                            reportdata.Name = "DeliveryReportDS";
                            reportdata.Value = ds.Tables[0];
                            idparam = new ReportParameter("deliveryid");
                            idparam.Values.Add(targetID.ToString());
                        }
                        ReportParameter companynameparam = new ReportParameter("companyname");
                        companynameparam.Values.Add(manager.CompanyName);
                        report_params.Add(idparam);
                        report_params.Add(companynameparam);
                        rvViewReport.LocalReport.SetParameters(report_params);
                        rvViewReport.RefreshReport();
                    }
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void cbSelectBulkOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = cbSelectBulkOperation.SelectedIndex;
            string selectedTableName = cbSelectTable.Text;
            try
            {
                switch (selectedIndex)
                {
                    case 0:
                        if (manager.BulkAddList.ContainsKey(selectedTableName)) ;
                        {
                            lstBulkOperations.ItemsSource = manager.BulkAddList[selectedTableName];
                        }
                        break;
                    case 1:
                        if (manager.BulkUpdateList.ContainsKey(selectedTableName)) ;
                        {
                            lstBulkOperations.ItemsSource = manager.BulkUpdateList[selectedTableName];
                        }
                        break;
                    case 2:
                        if (manager.BulkDeleteList.ContainsKey(selectedTableName)) ;
                        {
                            lstBulkOperations.ItemsSource = manager.BulkDeleteList[selectedTableName];
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnAddToBulkList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow selectedRow = null;
                string TableName = "";
                TableName = cbSelectTable.Text;
                if (dgContents.SelectedItem != null && dgContents.SelectedItem.GetType() == typeof(DataRowView))
                {
                    DataRowView selectedRowView = (DataRowView)dgContents.SelectedItem;
                    selectedRow = selectedRowView.Row;
                }
                if (selectedRow != null)
                {
                    switch (cbSelectBulkOperation.SelectedIndex)
                    {
                        case 0:
                            manager.BulkAddList[TableName].Add(selectedRow);
                            break;
                        case 1:
                            manager.BulkUpdateList[TableName].Add(selectedRow);
                            break;
                        case 2:
                            manager.BulkDeleteList[TableName].Add(selectedRow);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnRemoveFromBulkList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<DataRow> currentBulkList = new ObservableCollection<DataRow>();
                DataRow selectedRow = null;
                if (lstBulkOperations.ItemsSource != null)
                {
                    currentBulkList = (ObservableCollection<DataRow>)lstBulkOperations.ItemsSource;
                }
                if (lstBulkOperations.SelectedItem != null && lstBulkOperations.SelectedItem.GetType() == typeof(DataRow))
                {
                    selectedRow = (DataRow)lstBulkOperations.SelectedItem;
                }
                if (currentBulkList != null && selectedRow != null)
                {
                    currentBulkList.Remove(selectedRow);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An exception occured.\nDetails:{ex.Message}\n{ex.StackTrace}", "Critical Error. You can thank the programmer for that", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void btnCommitOperation_Click(object sender, RoutedEventArgs e)
        {
            string Table = "";
            int selectedIndex = 0;
            Table = cbSelectTable.Text;
            selectedIndex = cbSelectBulkOperation.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    manager.PerformBulkAddOperation(Table, dgContents);
                    break;
                case 1:
                    manager.PerformBulkUpdateOperation(Table, dgContents);
                    break;
                case 2:
                    manager.PerformBulkDeleteOperation(Table, dgContents);
                    break;
            }
        }

        private void exShowOptions_Expanded(object sender, RoutedEventArgs e)
        {
            Expander thisExpander = (Expander)sender;
            if (thisExpander.IsExpanded)
            {
                txtServerAddress.Text = ConfigurationManager.AppSettings["DB_ADDRESS"];
                txtServerPort.Text = ConfigurationManager.AppSettings["DB_PORT"];
                txtDatabaseName.Text = ConfigurationManager.AppSettings["DB_NAME"];
                txtDatabaseRootUserName.Text = ConfigurationManager.AppSettings["DB_USER"];
                txtDatabaseRootPassword.Text = ConfigurationManager.AppSettings["DB_PASSWORD"];
                checkEnableBulkInsert.IsChecked = manager.EnableBulkInsert;
                checkEnableBulkUpdate.IsChecked = manager.EnableBulkUpdate;
                checkEnableBulkDelete.IsChecked = manager.EnableBulkDelete;
            }
        }
    }


}

