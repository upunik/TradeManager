﻿using System;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Util;
using eBay.Service.Core.Soap;
using System.Configuration;
using TradeManager.Models;
using System.Web;
using Microsoft.AspNet.Identity;
using myTokens;
using System.Web.Mvc;

namespace TradeManager.Functions
{
    /// <summary>
    /// A simple fixed price item adding sample,
    /// show basic flow to list a fixed price item to eBay Site using eBay SDK.
    /// </summary>

    class AddFixedPriceItem
    {
        private static ApiContext apiContext = null;

        public static void AddMyFixedPriceItem(string Token, ItemType item)
        {

            try
            {

                //[Step 1] Initialize eBay ApiContext object
                ApiContext apiContext = GetApiContext(Token);

                //[Step 2] Create a new ItemType object
                //ItemType item = BuildItem();


                //[Step 3] Create Call object and execute the Call
                AddFixedPriceItemCall apiCall = new AddFixedPriceItemCall(apiContext);
                FeeTypeCollection fees = apiCall.AddFixedPriceItem(item);

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
        static ApiContext GetApiContext(string Token)
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
                //apiCredential.eBayToken = ConfigurationManager.AppSettings["UserAccount.ApiToken"];
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
            item.Title = "Sample Cookbook";
            // item description
            item.Description = "eBay SDK sample Cookbook fixed price item";

            // listing type
            item.ListingType = ListingTypeCodeType.FixedPriceItem;
            // listing price
            item.Currency = CurrencyCodeType.USD;

            // listing duration
            item.ListingDuration = "Days_3";

            // item location and country
            item.Location = "San Jose";
            item.Country = CountryCodeType.US;

            // item condition, New
            item.ConditionID = 1000;

            // listing category, 
            CategoryType category = new CategoryType();
            category.CategoryID = "11104"; //CategoryID = 11104 (CookBooks) , Parent CategoryID=267(Books)
            item.PrimaryCategory = category;


            // payment methods
            item.PaymentMethods = new BuyerPaymentMethodCodeTypeCollection();
            item.PaymentMethods.AddRange(
                new BuyerPaymentMethodCodeType[] { BuyerPaymentMethodCodeType.PayPal }
                );
            // email is required if paypal is used as payment method
            item.PayPalEmailAddress = "me@ebay.com";

            // shipping details
            item.ShippingDetails = BuildShippingDetails();

            // return policy
            item.ReturnPolicy = new ReturnPolicyType();
            item.ReturnPolicy.ReturnsAcceptedOption = "ReturnsAccepted";

            // handling time is required
            item.DispatchTimeMax = 1;


            //item Start Price
            AmountType amount = new AmountType();
            amount.Value = 1;
            amount.currencyID = CurrencyCodeType.USD;
            item.StartPrice = amount;



            // item variations- Variations not needed for Cookbook
            //item.Variations = buildVariationsType();
            return item;
        }

        /// <summary>
        /// Build sample SellerProfile details
        /// </summary>
        /// <returns></returns>
        static SellerProfilesType BuildSellerProfiles()
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

            Console.WriteLine("Enter Return policy profile Id:");
            sellerProfile.SellerReturnProfile = new SellerReturnProfileType();
            sellerProfile.SellerReturnProfile.ReturnProfileID = Int64.Parse(Console.ReadLine());

            Console.WriteLine("Enter Shipping profile Id:");
            sellerProfile.SellerShippingProfile = new SellerShippingProfileType();
            sellerProfile.SellerShippingProfile.ShippingProfileID = Int64.Parse(Console.ReadLine());

            Console.WriteLine("Enter Payment profile Id:");
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
            amount.Value = 1;
            amount.currencyID = CurrencyCodeType.USD;
            sd.PaymentInstructions = "eBay .Net SDK test instruction.";

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
            amount.Value = 1; //Refer getCategoryFeatures() for maximum shipping amounts.
            amount.currencyID = CurrencyCodeType.USD;
            shippingOptions.ShippingServiceCost = amount;
            shippingOptions.ShippingServicePriority = 1;
            amount = new AmountType();
            amount.Value = 1.0;
            amount.currencyID = CurrencyCodeType.USD;
            shippingOptions.ShippingInsuranceCost = amount;

            sd.ShippingServiceOptions = new ShippingServiceOptionsTypeCollection(
                new ShippingServiceOptionsType[] { shippingOptions }
                );

            return sd;
        }

        /// <summary>
        /// Build sample item Variations
        /// </summary>
        /// <returns>Variations object</returns>
        static VariationsType buildVariationsType()
        {
            // listing variations
            VariationsType variations = new VariationsType();

            // first variation
            VariationType variation1 = new VariationType();
            // create the content of VariationSpecifics
            NameValueListTypeCollection nvcol1 = new NameValueListTypeCollection();
            NameValueListType nv11 = new NameValueListType();
            nv11.Name = "Color";
            StringCollection nv1StrCol = new StringCollection();
            String[] strArr1 = new string[] { "RED" };
            nv1StrCol.AddRange(strArr1);
            nv11.Value = nv1StrCol;
            NameValueListType nv12 = new NameValueListType();
            nv12.Name = "Size";
            StringCollection nv2StrCol = new StringCollection();
            String[] strArr2 = new string[] { "M" };
            nv2StrCol.AddRange(strArr2);
            nv12.Value = nv2StrCol;
            nvcol1.Add(nv11);
            nvcol1.Add(nv12);
            // set variation-level specifics for first variation
            variation1.VariationSpecifics = nvcol1;
            // set start price
            AmountType amount1 = new AmountType();
            amount1.Value = 100;
            amount1.currencyID = CurrencyCodeType.USD;
            variation1.StartPrice = amount1;
            // set quantity
            variation1.Quantity = 10;
            // set variation name
            variation1.VariationTitle = "RED,M";

            // second variation
            VariationType variation2 = new VariationType();
            // create the content of specifics for each variation
            NameValueListTypeCollection nvcol2 = new NameValueListTypeCollection();
            NameValueListType nv21 = new NameValueListType();
            nv21.Name = "Color";
            StringCollection nv21StrCol = new StringCollection();
            String[] strArr21 = new string[] { "BLACK" };
            nv21StrCol.AddRange(strArr21);
            nv21.Value = nv21StrCol;
            NameValueListType nv22 = new NameValueListType();
            nv22.Name = "Size";
            StringCollection nv22StrCol = new StringCollection();
            String[] strArr22 = new string[] { "L" };
            nv22StrCol.AddRange(strArr22);
            nv22.Value = nv22StrCol;
            nvcol2.Add(nv21);
            nvcol2.Add(nv22);
            // set variation-level specifics for first variation
            variation2.VariationSpecifics = nvcol2;
            // set start price
            AmountType amount2 = new AmountType();
            amount2.Value = 110;
            amount2.currencyID = CurrencyCodeType.USD;
            variation2.StartPrice = amount2;
            // set quantity
            variation2.Quantity = 20;
            // set variation name
            variation2.VariationTitle = "BLACK,L";

            // set variation
            VariationTypeCollection varCol = new VariationTypeCollection();
            varCol.Add(variation1);
            varCol.Add(variation2);
            variations.Variation = varCol;

            // create the content of specifics for variations
            NameValueListTypeCollection nvcol3 = new NameValueListTypeCollection();
            NameValueListType nv31 = new NameValueListType();
            nv31.Name = "Color";
            StringCollection nv31StrCol = new StringCollection();
            String[] strArr31 = new string[] { "BLACK", "RED" };
            nv31StrCol.AddRange(strArr31);
            nv31.Value = nv31StrCol;
            NameValueListType nv32 = new NameValueListType();
            nv32.Name = "Size";
            StringCollection nv32StrCol = new StringCollection();
            String[] strArr32 = new string[] { "M", "L" };
            nv32StrCol.AddRange(strArr32);
            nv32.Value = nv32StrCol;
            nvcol3.Add(nv31);
            nvcol3.Add(nv32);
            // set variationSpecifics
            variations.VariationSpecificsSet = nvcol3;
            return variations;
            
        }
    }
}
