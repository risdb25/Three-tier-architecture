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
using DE_Store.DataLayer;
using DE_Store.PresentationLayer;
using System.Data;
using MimeKit;

namespace DE_Store.BusinessLayer
{
    class DEStoreBusinessLayer
    {
        #region VARIABLES
        private static DEStoreDataLayer dataLayer = new DEStoreDataLayer();
        private static DEStoreHome presentationLayer;

        //PROPERTY
        public DEStoreHome PresentationLayer { get => presentationLayer; set => presentationLayer = value; }
        #endregion

        #region CONSTRUCTOR
        public DEStoreBusinessLayer()
        {
            
        }
        #endregion

        #region METHODS
        private void populateListBoxWithProducts(ref DataTable dt)
        {
            /*This method is called by the Data layer after it has returned the details
             * for the product that the user has requested the details of. This method populates
             * the Data Grid in the Presentation layer.
             */


            //Check if listbox is null or does not contain any rows of data
            if(dt != null)
            {
                if(dt.Rows.Count > 0)
                {
                    PresentationLayer.gridProduct.ItemsSource = dt.DefaultView;
                    PresentationLayer.gridProduct.DataContext = dt.DefaultView;
                }
                else
                {
                    MessageBox.Show("No product with that ID found in the database");

                    /*There may be information left in the data grid from a previous search
                     * made using a valid product ID, so the following code will clear that
                     * information following the failed attempt to retrieve details for this
                     * invalid product ID.
                     * 
                     * The DataTable must be set to null or else an exception is thrown when
                     * the attempt to clear the data grid is made.
                     */
                    PresentationLayer.gridProduct.ItemsSource = null;

                    //clear data grid
                    PresentationLayer.gridProduct.Items.Clear();

                    //clear textbox of ID that does not exist
                    PresentationLayer.txtProductID.Clear();
                }
            }
            
        }

        public void productDetailsRequest(string productID)
        {
            /*This method is called by the Presentation layer on the button used to
             * retrieve the details of a product that the user has entered the product ID for
             */

            DataTable dt = dataLayer.retrieveProductDetails(productID);
            populateListBoxWithProducts(ref dt);
        }

        public void applyOfferToProduct(int offerID)
        {
            /*First need to check that there is a product showing in the data grid that the user has searched for.
             * This will be the product that the offer is applied to.
             */

            if(PresentationLayer.gridProduct.Items.Count == 0)
            {
                MessageBox.Show("No product found. Please enter the Product ID for the product you wish to apply the offer to");
                uncheckRadioButtons();
            }
            else
            {
                //apply offer - call data layer to update database
                //data layer method call(product ID, offerID);
                dataLayer.assignProductOffer(offerID, presentationLayer.txtProductID.Text);
                MessageBox.Show("Sale applied to product");
                //Uncheck button
                uncheckRadioButtons();
            }
            
        }

        private void uncheckRadioButtons()
        {
            /*This method is required to ensure when the user selects a radio button, there is not a
             * mark left in the button as this could cause confusion as to whether the sale has been
             * applied or not.
             */

            PresentationLayer.radbtnBuyOneGetOneFree.IsChecked = false;
            PresentationLayer.radbtnThreeForTwo.IsChecked = false;
            PresentationLayer.radbtnFreeDelivery.IsChecked = false;
        }

        public void productPriceUpdate()
        {
            /*This method is called from the presentation layer when the user wishes to change the price of a product.
             * Validation first ensures three issues have not taken place
             * 1 - the product ID box for the product to change the price of is not empty
             * 2 - the text box where the user must enter the new price of the product is not empty
             * 3 - the user has entered a valid amount which is possible to be converted from a string to a decimal
             * After these checks are done, if they all pass, the call to the data layer then takes place to update the
             * product's price in the database.
             */

            decimal testNewPrice; //used for TryParse method call

            if(string.IsNullOrEmpty(presentationLayer.txtPriceChangeProductID.Text))
            {
                MessageBox.Show("No Product ID entered to change the price of");
            }
            else
            {
                if(string.IsNullOrEmpty(presentationLayer.txtNewProductPrice.Text))
                {
                    MessageBox.Show("No price entered");
                }
                else
                {
                    if(decimal.TryParse(presentationLayer.txtNewProductPrice.Text, out testNewPrice) == false)
                    {
                        MessageBox.Show("You have not entered a valid number");
                    }
                    else
                    {
                        decimal newPrice = Convert.ToDecimal(presentationLayer.txtNewProductPrice.Text); //convert new price from string to float
                        decimal.Round(newPrice);
                        string productID = presentationLayer.txtPriceChangeProductID.Text;

                        dataLayer.productPriceUpdate(newPrice, productID);

                        MessageBox.Show("Price updated successfully");
                    }
                }
            }            
        }

        public void emailLowStockWarning()
        {
            /*This method calls a method in the data layer to retrieve all products that have a stock count of
             * less than 10. Products whose stock count meets this condition are returned from this method in the data later
             * Once the method has obtained the products with a stock count of below 10, this information, stored in the DataTable,
             * is passed to a private method to send the email to the admin. 
             */

            DataTable dt = dataLayer.productsWithLowStock();
            sendEmail(ref dt);
        }

        private void sendEmail(ref DataTable dt)
        {
            /* This private method is called from within the business layer and is used to send the email containing products with low stock count.
             * In the real system, this information would be emailed to the DE-Store admin. For the testing/prototype demonstration, 
             * it is emailed to a test email address. The first step in this method is to convert the DataTable data to a string so that
             * it is possible to put the data in a format that is accepted in the message body.
             */

            string messageBody = string.Join(Environment.NewLine, dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray))); //convert from DataTable to string

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("kp26fs2n6goalzor@ethereal.email"));
            message.To.Add(MailboxAddress.Parse("kp26fs2n6goalzor@ethereal.email"));
            message.Subject = "Test low stock alert";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = messageBody };

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.Connect("smtp.ethereal.email", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("kp26fs2n6goalzor@ethereal.email", "dS6g7WWU6BDrEys3VM");
                smtp.Send(message);
                smtp.Disconnect(true);
            }
        }

        public void applyCustomerSpecialOffer()
        {
            /*In this method, a method in the data layer is called to try to apply a special offer to a customer. The method in the data layer
             * will either return a 0 or a 1. A '0' means the customer is not suitable for the loyalty card because they do not have 5 or more purchases.
             * If the method returns a '1', this means the customer has at least five purchases made and the special offer has been applied
             */

            int result;

            if(string.IsNullOrEmpty(presentationLayer.txtLoyaltyCardCustomerID.Text)) //validate text box is not empty
            {
                MessageBox.Show("Please ensure you have entered a customer ID to apply the special offer to");
            }
            else
            {
                dataLayer.applyCustomerSpecialOffer(presentationLayer.txtLoyaltyCardCustomerID.Text);
                result = dataLayer.retrieveCustomerSpecialOfferStatus(presentationLayer.txtLoyaltyCardCustomerID.Text);

                if(result == 0) //customer not suitable for special offer loyalty card
                {
                    MessageBox.Show("It was not possible to apply the special offer to the customer -\ncustomer has less than 5 purchases");
                }
                else //customer suitable, special offer has been applied
                {
                    MessageBox.Show("Special offer applied!");
                }
            }
        }

        public void financeApprovalPortal()
        {
            /*This method will allow for users of the DE-Store application to access the finance approval system that
             * the company's customers use to apply for finance when making purchases. The button will take users
             * to this portal. For the purposes of the prototype demonstration, the user is taken to a sample
             * site
             */

            System.Diagnostics.Process.Start("https://www.moneysupermarket.com/loans/eligibility-checker/");
        }

        public void salesReport()
        {
            /*This method calls the data layer to retrieve the last five sales that customers have made. This method in the data layer
             * returns a DataTable. The information in this DataTable is then displayed in the respective Data Grid on the presentation layer.
             */

            DataTable dt = dataLayer.retrieveSales();

            presentationLayer.gridProductSales.ItemsSource = dt.DefaultView;
            presentationLayer.gridProductSales.DataContext = dt.DefaultView;
        }
        #endregion
    }
}
