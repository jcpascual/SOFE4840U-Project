using System.Data;
using System.Data.Common;
using Conference.Models;
using MySqlConnector;

namespace Conference.Services;

public class DatabaseService
{
    private const string CreateTableDataScript = "CreateTable.sql";
    
    private readonly MySqlDataSource _dataSource;
    
    public DatabaseService(MySqlDataSource dataSource)
    {
        _dataSource = dataSource;

        // Create the tables if necessary.
        
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();

        command.CommandText = File.ReadAllText(CreateTableDataScript);
        
        command.ExecuteNonQuery();
    }

    public void InsertUser(string username, string password)
    {
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();

        command.CommandText = @"INSERT INTO users(username, password) VALUES(@username, @password);";
        
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", password);
        
        command.ExecuteNonQuery();
    }
    
    private ConferenceUser? ReadUser(DbDataReader reader)
    {
        if (!reader.Read())
        {
            return null;
        }

        return new ConferenceUser()
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Password = reader.GetString(2)
        };
    }

    public ConferenceUser? GetUser(string username)
    {
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();
        
        command.CommandText = @"SELECT id, username, password FROM users WHERE username = @username;";
        command.Parameters.AddWithValue("@username", username);
        
        using MySqlDataReader reader = command.ExecuteReader();

        return ReadUser(reader);
    }
    
    public ConferenceUser? GetUser(int id)
    {
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();
        
        command.CommandText = @"SELECT id, username, password FROM users WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);
        
        using MySqlDataReader reader = command.ExecuteReader();

        return ReadUser(reader);
    }
    
    public List<ConferenceContact> GetContactsForUser(ConferenceUser user)
    {
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();
        
        command.CommandText = @"SELECT id, ownerId, targetId FROM contacts WHERE ownerId = @id;";
        command.Parameters.AddWithValue("@id", user.Id);
        
        using MySqlDataReader reader = command.ExecuteReader();

        List<ConferenceContact> contacts = new List<ConferenceContact>();

        while (reader.Read())
        {
            contacts.Add(new ConferenceContact()
            {
                Id = reader.GetInt32(0),
                OwnerId = reader.GetInt32(1),
                TargetId = reader.GetInt32(2)
            });
        }

        return contacts;
    }
    
    public void InsertContact(ConferenceUser ownerUser, ConferenceUser targetUser)
    {
        using MySqlConnection connection = _dataSource.OpenConnection();
        using MySqlCommand command = connection.CreateCommand();

        command.CommandText = @"INSERT INTO contacts(ownerId, targetId) VALUES(@ownerId, @targetId);";
        
        command.Parameters.AddWithValue("@ownerId", ownerUser.Id);
        command.Parameters.AddWithValue("@targetId", targetUser.Id);
        
        command.ExecuteNonQuery();
    }
}