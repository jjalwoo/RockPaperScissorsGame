﻿using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using LoginServer.Model;
using LoginServer.ErrorCodeEnum;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;

namespace LoginServer.DB
{
    public class MySQLRepository : IDBRepository
    {
        private IDbConnection _dbConnection;

        public MySQLRepository()
        {
            Open();
        }

        private void Open()
        {
            _dbConnection = new MySqlConnection("Server=localhost;Database=account_db;UserId=root;Password=0000");
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            var result = ((MySqlConnection)_dbConnection).Ping();

            if (result)
            {
                Console.WriteLine("successful!");
            }
        }

        public string SHA256Hash(string password)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }

            return stringBuilder.ToString();
        }

        public async Task<ErrorCode> CheckLogin(string userID, string password)
        {
            try
            {
                var query = "SELECT * FROM account WHERE user_id = @userId";  // query param , sql injection                                                                               

                var result = await _dbConnection.QueryFirstOrDefaultAsync<Account>(query, new { userId = userID });

                if (result is null) // result == null과 동일
                {
                    Console.WriteLine("없음");
                    return ErrorCode.Fail;
                }

                return ErrorCode.Suceess;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                return ErrorCode.Fail;
            }
        }

        // 에러코드 반환
        public async Task<ErrorCode> CheckDuplicationID(string userID)
        {
            // ID 중복 체크
            var query1 = "SELECT * FROM account WHERE user_id = @userId";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<Account>(query1, new { userId = userID });

            if (result is null)
            {
                return ErrorCode.NotDuplication;
            }
            else
            {
                return ErrorCode.Duplication;
            }
        }

        public async Task CreateAccount(string userID, string password)
        {
            password = SHA256Hash(password);
            // password security 
            // sha, aes, !  nuget
            // password  db             

            var query2 = "INSERT INTO account (user_id, user_pw) VALUES (@user_id, @user_pw)";

            var rowsAffected = await _dbConnection.ExecuteAsync(query2, new { user_id = userID, user_pw = password });
            Console.WriteLine($"{rowsAffected} row(s) inserted.");
        }
    }
}


