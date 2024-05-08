using Zadanie7.DTOs;

namespace Zadanie7.Repositories;


using System.Data.SqlClient;
public interface IWarehouseRepository
{
    public Task<int> Add(WareHouseDTO wareHouseDto);

    Task<bool> DoesProductWithIDExist(int id,SqlConnection sqlConnection,SqlTransaction sqlTransaction);
    Task<bool> DoesWareHouseWithIDExist(int idWareHouse,SqlConnection sqlConnection,SqlTransaction sqlTransaction);
    

    Task<bool> CheckOrderCreationDate(int orderID,DateTime creationDate,SqlConnection sqlConnection,SqlTransaction transaction);

    Task<int?> FetchOrderId(int idProduct, int amount,DateTime creationDate, SqlConnection sqlConnection,SqlTransaction transaction);

    Task<bool> CheckIfCompletedOrder(int orderId,SqlConnection sqlConnection,SqlTransaction transaction);

    public void UpdateOrderFullfilledAt(int orderId,SqlConnection sqlConnection,SqlTransaction transaction);

    Task<Decimal> FetchProductPrice(int idProduct,SqlConnection sqlConnection,SqlTransaction transaction);

    Task<int> InsertIntoDb(WareHouseDTO wareHouseDto,int? orderId,SqlConnection sqlConnection,SqlTransaction transaction);



}