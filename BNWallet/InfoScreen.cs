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
    public class InfoScreen : Activity
    {

        BNWalletAPI BNWAPI;
        TextView WalletName;
        TextView BurstAddress;
        TextView BurstBalance;
        Button btnSendBurst;
        Button btnReceiveBurst;
        Button BurstRadio;
        string burstAddress;
        string walletName;
        string balance;
        ImageView ImgLogo;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.InfoScreen);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            burstAddress = Intent.GetStringExtra("WalletAddress");
            walletName = Intent.GetStringExtra("WalletName");
            balance = Intent.GetStringExtra("WalletBalance");
            RuntimeVar RT = new RuntimeVar();
            RuntimeVarDB RTDB = new RuntimeVarDB();
            RT = RTDB.Get();
            UserAccounts UA;
            UserAccountsDB UADB = new UserAccountsDB();
            UA = UADB.Get(RT.CurrentWalletName);

            UserAccountRuntimeDB UARDB = new UserAccountRuntimeDB();
            UserAccountRuntime UAR = UARDB.Get();
            string password = UAR.Password;
            string SecretPhrase = StringCipher.Decrypt(RT.CurrentPassphrase, password);

            string SentAmount = Intent.GetStringExtra("SendAmount");
            string FeeAmount = Intent.GetStringExtra("FeeAmount");
            string NewBB = Intent.GetStringExtra("BurstBalance");
            string BackYes = Intent.GetStringExtra("BackYes");
           

            double amntdblconf = Convert.ToDouble(SentAmount);
            double FeeAmnt = Convert.ToDouble(FeeAmount);
            double NewBBA = Convert.ToDouble(NewBB); 
         


            BNWAPI = new BNWalletAPI();
            GetAccountIDResult gair = BNWAPI.getAccountID(SecretPhrase, "");
            if (gair.success)
            {

                GetAccountResult gar = BNWAPI.getAccount(gair.accountRS);
                if (gar.success)
                {


                    WalletName = FindViewById<TextView>(Resource.Id.txtWalletName);
                    BurstAddress = FindViewById<TextView>(Resource.Id.txtBurstAddress);
                    BurstBalance = FindViewById<TextView>(Resource.Id.txtBalance);


                    BurstAddress.Text = gar.accountRS;
                    WalletName.Text = gar.name;
                    string BB;
                    BB = gar.balanceNQT;
                    double burstdbl = Convert.ToDouble(BB);
                    burstdbl = burstdbl / 100000000;
                    if (amntdblconf != 0)
                    {
                        if(NewBBA != 0)
                        {
                            double DisplayAmount;
                            DisplayAmount = (NewBBA - (amntdblconf + FeeAmnt));
                            BurstBalance.Text = DisplayAmount.ToString("#,0.00000000");
                        }
                        else
                        {
                            double DisplayAmount;
                            DisplayAmount = (burstdbl - (amntdblconf + FeeAmnt));
                            BurstBalance.Text = DisplayAmount.ToString("#,0.00000000");
                        }
                        
                    }else if(BackYes == "Yes")
                    {
                        BurstBalance.Text = NewBB;
                    }
                    else
                    {
                        BurstBalance.Text = burstdbl.ToString("#,0.00000000");
                    }
                        
                            
                        
                }
            }
            btnSendBurst = FindViewById<Button>(Resource.Id.btnSendBurst);
            btnSendBurst.Click += delegate
            {
                Intent intent = new Intent(this, typeof(SendBurstScreen));
                intent.SetFlags(ActivityFlags.SingleTop);
                intent.PutExtra("BurstAddress", BurstAddress.Text);
                intent.PutExtra("WalletBalance", BurstBalance.Text);
                StartActivity(intent);
                Finish();
                

            };
            btnReceiveBurst = FindViewById<Button>(Resource.Id.btnReceiveBurst);
            btnReceiveBurst.Click += delegate
            {
                Intent intent = new Intent(this, typeof(QRCodeView));
                intent.SetFlags(ActivityFlags.SingleTop);
                StartActivity(intent);


            };
            BurstRadio = FindViewById<Button>(Resource.Id.btnBurstRadio);
            BurstRadio.Click += delegate
            {
                
                var uri = Android.Net.Uri.Parse("https://www.burstnation.com/listen.html");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };

            ImgLogo = FindViewById<ImageView>(Resource.Id.imageView1);
            ImgLogo.Click += delegate
            {
                Intent intent = new Intent(this, typeof(WalletSelector));
                intent.SetFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
            };


            // Create your application here
        }
    }
}