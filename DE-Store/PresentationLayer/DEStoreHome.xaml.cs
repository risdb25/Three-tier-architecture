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
using DE_Store.BusinessLayer;

namespace DE_Store.PresentationLayer
{
    /// <summary>
    /// Interaction logic for DEStoreHome.xaml
    /// </summary>
    public partial class DEStoreHome : Page
    {
        #region VARIABLES
        private static DEStoreBusinessLayer businessLayer = new DEStoreBusinessLayer();
        #endregion

        #region CONSTRUCTOR
        public DEStoreHome()
        {
            InitializeComponent();

            businessLayer.PresentationLayer = this;
        }
        #endregion

        #region METHODS

        #region Button events
        private void btnRetrieveProductDetails_Click(object sender, RoutedEventArgs e)
        {
            string productID = txtProductID.Text;
            businessLayer.productDetailsRequest(productID);
        }
        
        private void btnProductPriceChange_Click(object sender, RoutedEventArgs e)
        {
            businessLayer.productPriceUpdate();
        }

        private void btnEmailLowStockReport_Click(object sender, RoutedEventArgs e)
        {
            businessLayer.emailLowStockWarning();
        }

        private void btnApplyLoyaltyOffer_Click(object sender, RoutedEventArgs e)
        {
            businessLayer.applyCustomerSpecialOffer();
        }

        private void btnFinanceApprovalPortal_Click(object sender, RoutedEventArgs e)
        {
            businessLayer.financeApprovalPortal();
        }

        private void btnSalesReport_Click(object sender, RoutedEventArgs e)
        {
            businessLayer.salesReport();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Title = "Exit Confirmation", "Application is closing", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }
        #endregion

        #region Product offers radio buttons
        /*These methods relate to the user clicking any of the radio buttons
         * used to apply offers to a product they have searched for - the offer is
         * applied to the product which the details of are shown in the data grid.
         * The offerID is passed as a paramarer as this is the way which the offer is
         * identified in the database of the data layer.
         */

        private void radbtnBuyOneGetOneFree_Checked(object sender, RoutedEventArgs e)
        {
            businessLayer.applyOfferToProduct(1); //offerID 1
        }

        private void radbtnThreeForTwo_Checked(object sender, RoutedEventArgs e)
        {
            businessLayer.applyOfferToProduct(2); //offerID 2
        }

        private void radbtnFreeDelivery_Checked(object sender, RoutedEventArgs e)
        {
            businessLayer.applyOfferToProduct(3); //offerID 3
        }
        
        #endregion

        #endregion
    }
}
