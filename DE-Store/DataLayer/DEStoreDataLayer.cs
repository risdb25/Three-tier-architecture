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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace DE_Store.DataLayer
{
    class DEStoreDataLayer
    {
        #region VARIABLES
        private SqlConnection sqlConnection;
        #endregion

        #region CONSTRUCTOR
        public DEStoreDataLayer()
        {
            makeDatabaseConnection();
        }
        #endregion

        #region METHODS
        public void makeDatabaseConnection()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DE_Store.Properties.Settings.DE_StoreConnectionString"].ConnectionString;
                sqlConnection = new SqlConnection(connectionString);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }

        public DataTable retrieveProductDetails(string requestedProductID)
        {
            /*This method returns a DataTable containing details stored in the database about the product that has been passed as a parameter
             * to the method.
             */

            DataTable productTable = new DataTable();

            try
            {
                string query = "SELECT productName AS \"Product name\" , productPrice as \"Price\", productStockCount AS \"Stock remaining\" " +
                    "FROM Product WHERE Product.ID = @productID";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlCommand.Parameters.AddWithValue("@productID", requestedProductID);

                    sqlDataAdapter.Fill(productTable);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return productTable;
        }

        public void assignProductOffer(int offerID, string productID)
        {
            /*This method writes an offer ID in the offerID attribute of the product whose ID has been passed as a parameter.
             * Each of the possible product offers have an ID of 1, 2 or 3. Depending on which offer the user selected in the
             * presentation layer, and thus has been passed as a parameter to this method as the offerID, will determine which
             * offer is recorded in the offerID attribute of the given product. The two parameters passed to the method are used
             * as arguments in the SQL query.
             */

            try
            {
                string query = "UPDATE Product SET offerID = @offerID WHERE Product.ID = @productID";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@offerID", offerID);
                sqlCommand.Parameters.AddWithValue("@productID", productID);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }      

        }

        public void productPriceUpdate(decimal newPrice, string productID)
        {
            /*This method is used to update the price of a product. The new price and product ID to update are passed as parameters
             * to the method and are used as arguments in the SQL query
             */

            try
            {
                string query = "UPDATE Product SET productPrice = @newPrice WHERE Product.ID = @productID";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@newPrice", newPrice);
                sqlCommand.Parameters.AddWithValue("@productID", productID);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        public DataTable productsWithLowStock()
        {
            /*This method returns a DataTable containing products whose stock count attribute value in the database is less than 10.
             * Any products that satify this condition are retrieved from the Product table and added to the C# DataTable variable.
             */

            DataTable lowStockProducts = new DataTable();
            try
            {
                string query = "SELECT Product.productName AS \"Product name\", Product.productStockCount AS \"Stock count\" " +
                    "FROM Product WHERE Product.productStockCount < 10";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlDataAdapter.Fill(lowStockProducts);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return lowStockProducts;
        }

        public void applyCustomerSpecialOffer(string customerID)
        {
            /*This method checks whether a customer is suitable to receieve a special offer through the loyalty card. In order to qualify
             * for the loyalty card, the number of purchases the customer has made must be at least 5. The number of purchases the customer has made
             * is stored in the Customer database as numPurchasesMade attribute. So this method creates a method to check the value for the customer
             * whose ID has been passed as a parameter. This parameter is used as an argument in the query.
             */

            try
            {
                string query = "UPDATE Customer SET hasLoyaltyCard = CASE WHEN (numPurchasesMade < 5) THEN 0 ELSE 1 END WHERE Customer.ID = @customerID";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("@customerID", customerID);
                sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        public int retrieveCustomerSpecialOfferStatus(string customerID)
        {
            /*This method is called from the business layer directly after the call to the applyCustomerSpecialOffer data layer method. The job
             * of this method is to check whether the customer has been suitable to receive the special offer or not, depending on whether they have 
             * made at least 5 purchases or not. This method returns a '0' if they were not suitable, and a '1' if they were suitable, as this is they
             * value reflected in the hasLoyaltyCard attribute of the Customer database to show whether they have the special offer applied or not. The
             * customer ID passed as a parameter to this method is used as an argument in the SQL query.
             */

            int result = 0;

            try
            {
                string query = "SELECT Customer.hasLoyaltyCard FROM Customer WHERE Customer.ID = @customerID";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("customerID", customerID);
                result = (int)sqlCommand.ExecuteScalar();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sqlConnection.Close();
            }

            return result;
        }

        public DataTable retrieveSales()
        {
            /*This method returns a DataTable of the last 5 sales recorded in the Sales database. The Sales database is a relationship table
             * consisting of foreign keys to IDs of Products and Customers. The most recent 5 sales added to the table are retrieved and added
             * to the C# DataTable variable. This variable is then returned from the method.
             */

            DataTable salesTable = new DataTable();

            try
            {
                string query = "SELECT TOP 5 productID AS \"Product ID\", customerID AS \"Customer ID\" FROM Sale ORDER BY Sale.ID DESC";
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter)
                {
                    sqlDataAdapter.Fill(salesTable);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return salesTable;
        }
        #endregion
    }
}
