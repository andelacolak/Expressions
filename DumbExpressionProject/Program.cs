using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DumbExpressionProject
{
    //Rule builder
    class Program
    {
        static void Main(string[] args)
        {
            var users = new List<User>()
            {
                new User() { FirstName = "Andela", LastName = "Colak", Age = 29 },
                new User() { FirstName = "Andela2", LastName = "Colak2", Age = 43 }
            };
        }

        public static IEnumerable<User> WhereAgeGreaterThan40(List<User> users)
        {
            //var result = users.Where(x => x.Age > 40);
            var param = Expression.Parameter(typeof(User));
            var property = Expression.Property(param, "Age");
            var rightSide = Expression.Constant(40);
            var operation = Expression.GreaterThan(property, rightSide);
            var lambda = Expression.Lambda<Func<User, bool>>(operation, param);

            var result = users.Where(lambda.Compile());
            return result;
        }

        public static IEnumerable<User> WhereFirstnameIsAndela(List<User> users)
        {
            //var result = list.Where(x => x.FirstName == "Andela");
            var param = Expression.Parameter(typeof(User));
            var property = Expression.Property(param, "FirstName");
            var rightSide = Expression.Constant("Andela");
            var operation = Expression.Equal(property, rightSide);
            var lambda = Expression.Lambda<Func<User, bool>>(operation, param);

            var result = users.Where(lambda.Compile());

            return result;
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
