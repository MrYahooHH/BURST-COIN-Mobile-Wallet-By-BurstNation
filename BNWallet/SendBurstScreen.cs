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
using ZXing.Mobile;
using ZXing;
using ZXing.Common;

namespace BNWallet
{
    [Activity(Theme = "@style/MyTheme.BNWallet")]
    public class SendBurstScreen : Activity
    {
        EditText RecipientBurstAddress;
        EditText Message;
        EditText Amount;
        EditText Fee;
        EditText PassPhrase;
        BNWalletAPI BNWAPI;
        Toast toast;
        MobileBarcodeScanner scanner;
        View scanOverlay;
        Button buttonFlash;
        Button buttonCancelScan;
        Button CreateQRCode;
        CheckBox cbEncrypt;
        string burstAddress;
        string amount;
        string fee;
        string balance;
        UserAccountRuntime UAR;
        UserAccountRuntimeDB UARDB;
        TextView BurstBalance;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SendBurstScreen);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            MobileBarcodeScanner.Initialize(Application);
            scanner = new MobileBarcodeScanner();

            RuntimeVar RT = new RuntimeVar();
            RuntimeVarDB RTDB = new RuntimeVarDB();
            RT = RTDB.Get();


            burstAddress = Intent.GetStringExtra("BurstAddress");
            balance = Intent.GetStringExtra("WalletBalance");


            RecipientBurstAddress = FindViewById<EditText>(Resource.Id.sendBurstAddress);
            Message = FindViewById<EditText>(Resource.Id.sendMessage);
            Amount = FindViewById<EditText>(Resource.Id.sendAmount);
            Fee = FindViewById<EditText>(Resource.Id.sendFee);
            cbEncrypt = FindViewById<CheckBox>(Resource.Id.cbEncryptMessage);
            
            
            BurstBalance = FindViewById<TextView>(Resource.Id.txtSendBalance);

            UARDB = new UserAccountRuntimeDB();
            UAR = UARDB.Get();
            string password = UAR.Password;
            string SecretPhrase = StringCipher.Decrypt(RT.CurrentPassphrase, password);
            BurstBalance.Text = balance;
    

            Button btnsend = FindViewById<Button>(Resource.Id.btnSend);
            btnsend.Click += delegate
            {
                AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
                alertDialog.SetTitle("Confirmation");
                alertDialog.SetMessage("Are you sure all the details are correct?");
                alertDialog.SetPositiveButton("Yes", delegate
                {
                    double amntdbl = Convert.ToDouble(Amount.Text);
                    amntdbl = amntdbl * 100000000;
                    amount = amntdbl.ToString();

                    double amntdblconf = Convert.ToDouble(amount);
                    amntdblconf = amntdblconf / 100000000;
                    Amount.Text = amntdblconf.ToString("#,0.00000000");

                    double feeamnt = Convert.ToDouble(Fee.Text);
                    feeamnt = feeamnt * 100000000;
                    fee = feeamnt.ToString();

                    double feeamntconf = Convert.ToDouble(fee);
                    feeamntconf = feeamntconf / 100000000;
                    Fee.Text = feeamntconf.ToString("#,0.00000000");

                    BNWAPI = new BNWalletAPI();
                    GetsendMoneyResult gsmr = BNWAPI.sendMoney(RecipientBurstAddress.Text, amount, fee, SecretPhrase, Message.Text,cbEncrypt.Checked);
                    if (gsmr.success)
                    {
                        GetTransactionResult gtr = BNWAPI.getTransaction(gsmr.transaction);
                        if (gtr.success)
                        {

                            AlertDialog.Builder ConfirmationDetailsDialog = new AlertDialog.Builder(this);
                            ConfirmationDetailsDialog.SetTitle("Confirmation Details");
                             ConfirmationDetailsDialog.SetMessage("Sender Address: "+ gtr.senderRS +"\n" + "Amount of Burst sent: " + Amount.Text + "\n"+ "Fee: "+ Fee.Text + "\n" + "Recipient Address: " + gtr.recipientRS + "\n" + "Signature Hash: " + gsmr.signatureHash
                             + "\n" + "Transaction ID: " + gsmr.transaction + "\n" + "Block Height: " + gtr.ecBlockHeight);

                            ConfirmationDetailsDialog.SetPositiveButton("OK", delegate
                            {
                                GetAccountResult gar = BNWAPI.getAccount(gtr.senderRS);
                                if (gar.success)
                                {
                                    Intent intent = new Intent(this, typeof(InfoScreen));
                                    intent.SetFlags(ActivityFlags.SingleTop);
                                    intent.PutExtra("SendAmount", Amount.Text);
                                    intent.PutExtra("FeeAmount", Fee.Text);
                                    intent.PutExtra("BurstBalance", BurstBalance.Text);
                                    StartActivity(intent);
                                    Finish();
                                }
                            });
                            ConfirmationDetailsDialog.Show();
                        }
                    }
                    else
                    {
                        toast = Toast.MakeText(this, "Received Error: " + gsmr.errorMsg, ToastLength.Long);
                        toast.Show();
                    }
                });
                alertDialog.SetNegativeButton("No", delegate
                {
                    alertDialog.Dispose();
                });
                alertDialog.Show();
            };

            Button btnBack = FindViewById<Button>(Resource.Id.btnBackButton);
            btnBack.Click += delegate
            {
                Intent intent = new Intent(this, typeof(InfoScreen));
                intent.SetFlags(ActivityFlags.SingleTop);
                intent.PutExtra("BurstBalance", balance);
                intent.PutExtra("BackYes", "Yes");
                StartActivity(intent);
                Finish();
            };



            Button ScanQRCode = FindViewById<Button>(Resource.Id.btnSendQRCode);
            ScanQRCode.Click += async delegate
            {
                await ScanFunction();
            };




            // Create your application here
        }


        private async System.Threading.Tasks.Task ScanFunction()
        {
            //Tell our scanner we want to use a custom overlay instead of the default
            scanner.UseCustomOverlay = true;

            //Inflate our custom overlay from a resource layout
            scanOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ScanLayout, null);

            //Find the button from our resource layout and wire up the click event
            buttonFlash = scanOverlay.FindViewById<Button>(Resource.Id.buttonFlash);
            buttonFlash.Click += (sender, e) =>
            {
                scanner.ToggleTorch();
            };

            buttonCancelScan = scanOverlay.FindViewById<Button>(Resource.Id.buttonCancel);
            buttonCancelScan.Click += (sender, e) => scanner.Cancel();

            //Set our custom overlay
            scanner.CustomOverlay = scanOverlay;

            //Set scanner possible modes
            var options = new MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<BarcodeFormat>() {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.PDF_417,
                    BarcodeFormat.CODE_39
                 };

            //Start scanning!
            var result = await scanner.Scan(options);


            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                msg = result.Text;
                if (result.BarcodeFormat == BarcodeFormat.QR_CODE)
                {
                    RecipientBurstAddress.Text = msg;
                }
            }
            else
            {
                msg = "Scanning Cancelled!";
                this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Long).Show());
            }
        }

    }

}