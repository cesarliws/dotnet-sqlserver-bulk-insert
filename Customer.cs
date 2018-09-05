using System;
using System.Collections.Generic;

/*
    use Insumos;

    create table Customer (
        Id uniqueidentifier primary key,
        FirstName varchar(100),
        LastName varchar(100),
        DateOfBirth date,
        CreatedAt  datetime2 default getdate()
    );

    select * from customer;
    delete from Customer;
*/

namespace BulkOperations
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }

        public static IEnumerable<Customer> Generate(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = "FirstName" + i,
                    LastName = "LastName" + i,
                    DateOfBirth = DateTime.Now
                };
            }
        }
    }
}