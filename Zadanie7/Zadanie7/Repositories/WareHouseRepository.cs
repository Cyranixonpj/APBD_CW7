using Zadanie7.DTOs;
using System.Data.SqlClient;
using System.Net.Http.Headers;

namespace Zadanie7.Repositories;

public class WareHouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;

    public WareHouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    
    public async Task<int> Add(WareHouseDTO wareHouseDto)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await sqlConnection.OpenAsync();
        await using var transact = sqlConnection.BeginTransaction();

        try
        {
            if (!await DoesWareHouseWithIDExist(wareHouseDto.IdWaregouse,sqlConnection,transact)||
                !await DoesProductWithIDExist(wareHouseDto.IdProduct,sqlConnection,transact)||
                !CheckAmountGreaterThanZero(wareHouseDto.Amount))
            {
                return 0;
            }

            var orderId = await FetchOrderId(wareHouseDto.IdProduct, wareHouseDto.Amount, wareHouseDto.CreatedAt,
                sqlConnection, transact);
            if (orderId==null||!await CheckOrderCreationDate(orderId.Value,wareHouseDto.CreatedAt,sqlConnection,transact))
            {
                return 0;
            }

            if (!await CheckIfCompletedOrder(orderId.Value,sqlConnection,transact))
            {
                UpdateOrderFullfilledAt(orderId.Value,sqlConnection,transact);
                return await InsertIntoDb(wareHouseDto, orderId, sqlConnection, transact);
            }

            return 0;

        }
        catch (Exception e)
        {
            await transact.RollbackAsync();
            throw;
        }
    }


    public async Task<bool> DoesProductWithIDExist(int id,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command = new SqlCommand("SELECT COUNT(*)FROM Warehouse WHERE IdWarehouse=@Id", sqlConnection, transaction);
        command.Parameters.AddWithValue("@Id", id);
        var res = (int)await command.ExecuteScalarAsync();
        return res > 0;
    }

    public async Task<bool> DoesWareHouseWithIDExist(int id,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse=@Id", sqlConnection,
            transaction);
        command.Parameters.AddWithValue("@Id", id);
        var res = (int)await command.ExecuteScalarAsync();
        return res > 0;
    }

    private bool CheckAmountGreaterThanZero(int amount)
    {
        return amount > 0;
    }

    public async Task<bool> CheckOrderCreationDate(int orderID,DateTime creationDate,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command = new SqlCommand("SELECT COUNT(*) FROM [Order] WHERE IdOrder = @Id AND CreatedAt < @CreatedAt",
            sqlConnection, transaction);
        command.Parameters.AddWithValue("@Id",orderID);
        command.Parameters.AddWithValue("@CreatedAt", creationDate);

        var res = (int)await command.ExecuteScalarAsync();
        return res > 0;
        
    }

    public async Task<int?> FetchOrderId(int idProduct, int amount,DateTime creationDate, SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command =
            new SqlCommand(
                "SELECT IdORder FROM [Order] WHERE IdProduct = @Id AND Amount=@Amount AND CreatedAt < @CreatedAt",
                sqlConnection, transaction);
        command.Parameters.AddWithValue("@Id", idProduct);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", creationDate);
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetInt32(0);
        }

        return null;
    }

    public async Task<bool> CheckIfCompletedOrder(int orderId,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command = new SqlCommand("SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @Id", sqlConnection,
            transaction);
        command.Parameters.AddWithValue("@Id", orderId);
        var res = (int)await command.ExecuteScalarAsync();
        return res > 0;
    }

    public async void UpdateOrderFullfilledAt(int orderId,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var nowDate = DateTime.Now;
        var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder=@Id", sqlConnection,
            transaction);
        command.Parameters.AddWithValue("@Id", orderId);
        command.Parameters.AddWithValue("@FulfilledAt", nowDate);
        var res = (int)await command.ExecuteScalarAsync();
        
    }

    public async Task<decimal> FetchProductPrice(int idProduct,SqlConnection sqlConnection,SqlTransaction transaction)
    {
        var command = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @Id",
            sqlConnection, transaction);
        command.Parameters.AddWithValue("@Id", idProduct);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetDecimal(0);
        }
        
        throw new InvalidOperationException();
    }

    public async Task<int> InsertIntoDb(WareHouseDTO wareHouseDto,int? orderId,SqlConnection sqlConnection, SqlTransaction transaction)
    {
        var price = await FetchProductPrice(wareHouseDto.IdProduct, sqlConnection, transaction);
        
        var nowDate = DateTime.Now;

        var command = new SqlCommand(
            "INSERT INTO Product_Warehouse(IdWarehouse,IdProduct,IdOrder,Amount,Price,CreatedAt)VALUES " +
            "(@Warehouse,@Product,@Order,@Amount,@Price,@CreatedAt);SELECT SCOPE_IDENTITY();", sqlConnection,
            transaction);

        command.Parameters.AddWithValue("@Warehouse", wareHouseDto.IdWaregouse);
        command.Parameters.AddWithValue("Product", wareHouseDto.IdProduct);
        command.Parameters.AddWithValue("@Order", orderId);
        command.Parameters.AddWithValue("@Amount", wareHouseDto.Amount);
        command.Parameters.AddWithValue("@Price", wareHouseDto.Amount * price);
        command.Parameters.AddWithValue("@CreatedAt", nowDate);

        var res = await command.ExecuteScalarAsync();

        await transaction.CommitAsync();

        return Convert.ToInt32(res);
        
        
    }
}