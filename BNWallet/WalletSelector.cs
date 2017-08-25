using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace BNWallet
{
    [Activity(Theme = "@style/MyTheme.BNWallet")]
    public class WalletSelector : Activity
    {
        ListView UserAccountView;
        List<UserAccounts> items;
        BNWalletAPI BNWAPI;
        UserAccountRuntime UAR;
        UserAccountRuntimeDB UARDB;
        UserAccounts UA;
        UserAccountsDB UADB;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WalletSelector);
            UserAccountView = FindViewById<ListView>(Resource.Id.lstUserAccounts);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            UserAccountsDB userAccountDB = new UserAccountsDB();
           
            UserAccounts[] userAccount = userAccountDB.GetAccountList();

            items = userAccount.ToList<UserAccounts>();

            UserAccountsViewAdapter adapter = new UserAccountsViewAdapter(this, items);
            UserAccountView.Adapter = adapter;
            UserAccountView.ItemClick += delegate(object sender, AdapterView.ItemClickEventArgs e)
            {
                RuntimeVarDB RTDB = new RuntimeVarDB();
                RuntimeVar RT = new RuntimeVar();
                RT.CurrentWalletName = items[e.Position].AccountName;
                UserAccounts UA = new UserAccounts();
                UA = userAccountDB.Get(items[e.Position].AccountName);
                RT.CurrentPassphrase = UA.PassPhrase;               
                RTDB.Save(RT);

                UARDB = new UserAccountRuntimeDB();
                UAR = UARDB.Get();
                string password = UAR.Password;
                string SecretPhrase = StringCipher.Decrypt(RT.CurrentPassphrase, password);

                
                BNWAPI = new BNWalletAPI();
                GetAccountIDResult gair = BNWAPI.getAccountID(SecretPhrase, "");
                if (gair.success)
                {
                    GetAccountResult gar = BNWAPI.getAccount(gair.accountRS);
                    if (gar.success)
                    {
                        Intent intentSuccess = new Intent(this, typeof(InfoScreen));
                        intentSuccess.PutExtra("WalletAddress", gar.accountRS);
                        if(gar.name == "")
                            intentSuccess.PutExtra("WalletName", "No Name Set");
                        else 
                            intentSuccess.PutExtra("WalletName", gar.name);
                        intentSuccess.PutExtra("WalletBalance", gar.balanceNQT);
                        intentSuccess.SetFlags(ActivityFlags.SingleTop);
                        StartActivity(intentSuccess);

                    }
                    else
                    {

                        UADB = new UserAccountsDB();
                        UA = UADB.Get(RT.CurrentWalletName);
                        Intent intentNew = new Intent(this, typeof(InfoScreen));
                        intentNew.PutExtra("WalletAddress", UA.BurstAddress);
                        intentNew.PutExtra("WalletName", UA.AccountName);
                        intentNew.PutExtra("WalletBalance", "0.00000000");
                        intentNew.SetFlags(ActivityFlags.SingleTop);
                        StartActivity(intentNew);
                    }
                }
                else
                {
                    UADB = new UserAccountsDB();
                    UA = UADB.Get(RT.CurrentWalletName);
                    Intent intentNew2 = new Intent(this, typeof(InfoScreen));
                    intentNew2.PutExtra("WalletAddress", UA.BurstAddress);
                    intentNew2.PutExtra("WalletName", UA.AccountName);
                    intentNew2.PutExtra("WalletBalance", "0.00000000");
                    intentNew2.SetFlags(ActivityFlags.SingleTop);
                    StartActivity(intentNew2);
                }

                

                
                
            };

            Button AddWallet = FindViewById<Button>(Resource.Id.btnAddNewWallet);
            AddWallet.Click += delegate
            {
                Intent intent = new Intent(this, typeof(AddNewWallet));
                intent.SetFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
                Finish();
            };

            Button btnNewUser = FindViewById<Button>(Resource.Id.btnNewUser);
            btnNewUser.Click += delegate
            {
                Intent intent = new Intent(this, typeof(AddNewPassphrase));
                intent.SetFlags(ActivityFlags.SingleTop);
                StartActivity(intent);
                Finish();
            };

            // Create your application here
        }
    }
}