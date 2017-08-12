using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BNWallet
{
    [Activity(Theme = "@style/MyTheme.BNWallet")]
    public class LoginScreen : Activity
    {
        BNWalletAPI BNWAPI;
        EditText SecretPhrase;
        Toast toast;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LoginScreen);

            RuntimeVar RT = new RuntimeVar();
            RuntimeVarDB RTDB = new RuntimeVarDB();
            RT = RTDB.Get();

            SecretPhrase = FindViewById<EditText>(Resource.Id.PassPhrase);
           // if(RT.CurrentPassphrase != "")
           // {
            SecretPhrase.Text = RT.CurrentPassphrase;
            //}
           // else
           // {
            //    SecretPhrase.Text = "";
           // }
                

            Button btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            btnLogin.Click += delegate
            {
                BNWAPI = new BNWalletAPI();
                GetAccountIDResult gair = BNWAPI.getAccountID(SecretPhrase.Text,"");
                if(gair.success)
                {
                    GetAccountResult gar = BNWAPI.getAccount(gair.accountRS);
                    if(gar.success)
                    {
                        Intent intent = new Intent(this, typeof(InfoScreen));
                        intent.PutExtra("WalletAddress", gar.accountRS);
                        intent.PutExtra("WalletName", gar.name);
                        intent.PutExtra("WalletBalance", gar.balanceNQT);
                        intent.SetFlags(ActivityFlags.SingleTop);
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        toast = Toast.MakeText(this, "Received API Error: " + gar.errorMsg, ToastLength.Long);
                        toast.Show();
                    }
                }
                else
                {
                    toast = Toast.MakeText(this, "Received API Error: " + gair.errorMsg, ToastLength.Long);
                    toast.Show();
                }
                
            };

            

            Button btnLoadWallet = FindViewById<Button>(Resource.Id.btnLoadWallet);
            btnLoadWallet.Click += delegate
            {
                Intent intent = new Intent(this, typeof(WalletSelector));
                intent.SetFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
                Finish();
            };



            // Create your application here
        }
    }
}