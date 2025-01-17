using System;
using System.Configuration;
using System.Collections.Generic;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.Util;
using TradeManager.Models;
using System.Web;
using myTokens;

namespace TradeManager.Functions
{
    /// <summary>
    /// A simple item adding sample,
    /// show basic flow to list an item to eBay Site using eBay SDK.
    /// </summary>
    class AddItem
    {
        private static ApiContext apiContext = null;

        public static void AddMyItem(string Token, ItemType item)
        {

            try
            {
                //[Step 1] Initialize eBay ApiContext object
                ApiContext apiContext = GetApiContext(Token);

                //[Step 2] Create a new ItemType object
                //ItemType item = BuildItem();


                //[Step 3] Create Call object and execute the Call
                AddItemCall apiCall = new AddItemCall(apiContext);
                //Console.WriteLine("Begin to call eBay API, please wait ...");
                item.ShippingDetails = BuildShippingDetails();
                FeeTypeCollection fees = apiCall.AddItem(item);
                //Console.WriteLine("End to call eBay API, show call result ...");

                //[Step 4] Handle the result returned
                //Console.WriteLine("The item was listed successfully!");
                //double listingFee = 0.0;
                //foreach (FeeType fee in fees)
                //{
                //    if (fee.Name == "ListingFee")
                //    {
                //        listingFee = fee.Fee.Value;
                //    }
                //}
                // Console.WriteLine(String.Format("Listing fee is: {0}", listingFee));
                // Console.WriteLine(String.Format("Listed Item ID: {0}", item.ItemID));
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Fail to list the item : " + ex.Message);
            }
        }

        /// <summary>
        /// Populate eBay SDK ApiContext object with data from application configuration file
        /// </summary>
        /// <returns>ApiContext object</returns>
        public static ApiContext GetApiContext(string Token)
        {
            //apiContext is a singleton,
            //to avoid duplicate configuration reading
            if (apiContext != null)
            {
                return apiContext;
            }
            else
            {
                apiContext = new ApiContext();

                //set Api Server Url
                apiContext.SoapApiServerUrl =
                    ConfigurationManager.AppSettings["Environment.ApiServerUrl"];
                //set Api Token to access eBay Api Server
                ApiCredential apiCredential = new ApiCredential();
                apiCredential.eBayToken = Token;
                apiContext.ApiCredential = apiCredential;
                //set eBay Site target to US
                apiContext.Site = SiteCodeType.US;

                //set Api logging
                apiContext.ApiLogManager = new ApiLogManager();
                apiContext.ApiLogManager.ApiLoggerList.Add(
                    new FileLogger("listing_log.txt", true, true, true)
                    );
                apiContext.ApiLogManager.EnableLogging = true;


                return apiContext;
            }
        }

        /// <summary>
        /// Build a sample item
        /// </summary>
        /// <returns>ItemType object</returns>
        static ItemType BuildItem()
        {
            ItemType item = new ItemType();

            // item title
            item.Title = "Test Create Item";
            // item description
            item.Description = "eBay SDK sample test item";

            // listing type
            item.ListingType = ListingTypeCodeType.Chinese;
            // listing price
            item.Currency = CurrencyCodeType.USD;
            item.StartPrice = new AmountType();
            item.StartPrice.Value = 0.1;
            item.StartPrice.currencyID = CurrencyCodeType.USD;

            // listing duration
            item.ListingDuration = "Days_3";

            // item location and country
            item.Location = "San Jose";
            item.Country = CountryCodeType.US;

            // listing category, 
            CategoryType category = new CategoryType();
            category.CategoryID = "11104"; //CategoryID = 11104 (CookBooks) , Parent CategoryID=267(Books)
            item.PrimaryCategory = category;

            // item quality
            item.Quantity = 1;

            // item condition, New
            item.ConditionID = 1000;

            // item specifics
            item.ItemSpecifics = buildItemSpecifics();

            //Console.WriteLine("Do you want to use Business policy profiles to list this item? y/n");
            //String input = Console.ReadLine();
            //if (input.ToLower().Equals("y"))
            //{
            //    item.SellerProfiles = BuildSellerProfiles();
            //}
            //else
            //{
                // payment methods
                item.PaymentMethods = new BuyerPaymentMethodCodeTypeCollection();
                item.PaymentMethods.AddRange(
                    new BuyerPaymentMethodCodeType[] { BuyerPaymentMethodCodeType.PayPal }
                    );
                // email is required if paypal is used as payment method
                item.PayPalEmailAddress = "me@ebay.com";

                // handling time is required
                item.DispatchTimeMax = 1;
                // shipping details
                item.ShippingDetails = BuildShippingDetails();

                // return policy
                item.ReturnPolicy = new ReturnPolicyType();
                item.ReturnPolicy.ReturnsAcceptedOption = "ReturnsAccepted";
            //}
            //item Start Price
            AmountType amount = new AmountType();
            amount.Value = 2.8;
            amount.currencyID = CurrencyCodeType.USD;
            item.StartPrice = amount;


            return item;
        }

        /// <summary>
        /// Build sample SellerProfile details
        /// </summary>
        /// <returns></returns>
        public static SellerProfilesType BuildSellerProfiles()
        {
            /*
             * Beginning with release 763, some of the item fields from
             * the AddItem/ReviseItem/VerifyItem family of calls have been
             * moved to the Business Policies API. 
             * See http://developer.ebay.com/Devzone/business-policies/Concepts/BusinessPoliciesAPIGuide.html for more
             * 
             * This example uses profiles that were previously created using this api.
             */

            SellerProfilesType sellerProfile = new SellerProfilesType();

            //Console.WriteLine("Enter Return policy profile Id:");
            sellerProfile.SellerReturnProfile = new SellerReturnProfileType();
            sellerProfile.SellerReturnProfile.ReturnProfileID = Int64.Parse(Console.ReadLine());

            //Console.WriteLine("Enter Shipping profile Id:");
            sellerProfile.SellerShippingProfile = new SellerShippingProfileType();
            sellerProfile.SellerShippingProfile.ShippingProfileID = Int64.Parse(Console.ReadLine());

            //Console.WriteLine("Enter Payment profile Id:");
            sellerProfile.SellerPaymentProfile = new SellerPaymentProfileType();
            sellerProfile.SellerPaymentProfile.PaymentProfileID = Int64.Parse(Console.ReadLine());

            return sellerProfile;
        }

        /// <summary>
        /// Build sample shipping details
        /// </summary>
        /// <returns>ShippingDetailsType object</returns>
        public static ShippingDetailsType BuildShippingDetails()
        {
            // Shipping details
            ShippingDetailsType sd = new ShippingDetailsType();

            sd.ApplyShippingDiscount = true;
            AmountType amount = new AmountType();
            //amount.Value = 2.8;
            //amount.currencyID = CurrencyCodeType.USD;
            //sd.PaymentInstructions = "eBay .Net SDK test instruction.";

            // Shipping type and shipping service options
            sd.ShippingType = ShippingTypeCodeType.Flat;
            ShippingServiceOptionsType shippingOptions = new ShippingServiceOptionsType();
            shippingOptions.ShippingService =
                ShippingServiceCodeType.ShippingMethodStandard.ToString();
            amount = new AmountType();
            amount.Value = 1;
            amount.currencyID = CurrencyCodeType.USD;
            shippingOptions.ShippingServiceAdditionalCost = amount;
            amount = new AmountType();
            amount.Value = 1;
            amount.currencyID = CurrencyCodeType.USD;
            shippingOptions.ShippingServiceCost = amount;
            shippingOptions.ShippingServicePriority = 1;
            

            sd.ShippingServiceOptions = new ShippingServiceOptionsTypeCollection(
                new ShippingServiceOptionsType[] { shippingOptions }
                );

            return sd;
        }

        /// <summary>
        /// Build sample item specifics
        /// </summary>
        /// <returns>ItemSpecifics object</returns>
        public static NameValueListTypeCollection buildItemSpecifics()
        {
            //create the content of item specifics
            NameValueListTypeCollection nvCollection = new NameValueListTypeCollection();
            NameValueListType nv1 = new NameValueListType();
            nv1.Name = "Platform";
            StringCollection nv1Col = new StringCollection();
            String[] strArr1 = new string[] { "Microsoft Xbox 360" };
            nv1Col.AddRange(strArr1);
            nv1.Value = nv1Col;
            NameValueListType nv2 = new NameValueListType();
            nv2.Name = "Genre";
            StringCollection nv2Col = new StringCollection();
            String[] strArr2 = new string[] { "Simulation" };
            nv2Col.AddRange(strArr2);
            nv2.Value = nv2Col;
            nvCollection.Add(nv1);
            nvCollection.Add(nv2);
            return nvCollection;
        }
    }
}
