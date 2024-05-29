using System;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IdentityModel.Client;
using IdentityModel.OidcClient.Results;
using RestSharp;
using IRestResponse = RestSharp.Portable.IRestResponse;

namespace WPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Auth0Client client;

        readonly string[] _connectionNames = new string[]
        {
            "Username-Password-Authentication",
            "google-oauth2",
            "twitter",
            "facebook",
            "github",
            "windowslive"
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                
                Scope = "openid  email offline_access"
            });

            var extraParameters = new Dictionary<string, string>();
            var refreshToken = File.ReadAllText(@"D:\VAOS\RefreshToken.txt");

            extraParameters = new Dictionary<string, string>()
            {
                {"prompt","none"},
                {"refresh_token",refreshToken}
                // <- According to the article, this should prevent the pop up.
            };


            DisplayResult(await client.LoginAsync(extraParameters));
        }


        private void DisplayResult(LoginResult loginResult)
        {
            if (loginResult.IsError)
            {
                GetToken();

            }
            else
            {

                //Display result
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Tokens");
                sb.AppendLine("------");
                sb.AppendLine($"id_token: {loginResult.IdentityToken}");
                sb.AppendLine($"access_token: {loginResult.AccessToken}");
                sb.AppendLine($"refresh_token: {loginResult.RefreshToken}");
                sb.AppendLine();

                sb.AppendLine("Claims");
                sb.AppendLine("------");
                foreach (var claim in loginResult.User.Claims)
                {
                    sb.AppendLine($"{claim.Type}: {claim.Value}");
                }

                resultTextBox.Text = sb.ToString();
            }

            //Display error
            //if (loginResult.IsError)
            //{
            //    resultTextBox.Text = loginResult.Error;
            //    return;
            //}

            logoutButton.Visibility = Visibility.Visible;
            loginButton.Visibility = Visibility.Collapsed;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionNameComboBox.ItemsSource = _connectionNames;
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];
            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                Scope = "profile api_access openid offline_access"
            });

            BrowserResultType browserResult = await client.LogoutAsync();

            var refreshToken = File.ReadAllText(@"D:\VAOS\RefreshToken.txt");
            var re =
                await client.RefreshTokenAsync(
                    refreshToken);

            var restclient = new RestClient($"https://{domain}/oauth/revoke");
            var request = new RestRequest(String.Empty,Method.Post);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{ \"client_id\": \"{OERRkQhXgZUHWxyGBEqNaUolaQZsadnF}\", \"client_secret\": \"{F-CFh6JLrnJkwoEc8QJBwK6tK1KqbMLgWhL-DOvBv-Iq3ORnrz-Da-nKZuLCxanJ}\", \"token\": \"{v1.MYeOiIXqSZOTMQkTM3G3hwBfFe13YxNx_zaBSNLGuH4otZh4y7kruuklXnUDDWQrnPA7dLsBfOcFnfmYKHDy1ds}\" }", ParameterType.RequestBody);
            var response = restclient.Execute(request);

            if (browserResult != BrowserResultType.Success)
            {
                resultTextBox.Text = browserResult.ToString();
                return;
            }

            logoutButton.Visibility = Visibility.Collapsed;
            loginButton.Visibility = Visibility.Visible;

            audienceTextBox.Text = "";
            resultTextBox.Text = "";
            connectionNameComboBox.ItemsSource = _connectionNames;
        }

        private async void GetToken()
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];


            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                Scope = "profile api_access openid offline_access"
            });

            var extraParameters = new Dictionary<string, string>();

            extraParameters = new Dictionary<string, string>()
            {
                // <- According to the article, this should prevent the pop up.
            };

            var refreshToken = File.ReadAllText(@"D:\VAOS\RefreshToken.txt");
            var re =
                await client.RefreshTokenAsync(
                    refreshToken);
            Thread.Sleep(5000);
            GetUserInfo(re.AccessToken);

        }

        private async void GetUserInfo(string accesstoken)
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                Scope = "openid profile email offline_access"
            });
            var user =
                await client.GetUserInfoAsync(accesstoken);
            Thread.Sleep(5000);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("User Info");
            sb.AppendLine();

            sb.AppendLine("Claims");
            sb.AppendLine("------");
            foreach (var claim in user.Claims)
            {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            resultTextBox.Text = sb.ToString();
        }
    }
}