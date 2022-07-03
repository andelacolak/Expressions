using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GenericSecurityService
{
    class Program
    {
        //GenericSecurityService
        //Only Active users visible
        static void Main(string[] args)
        {
            var query = GetDocuments().AsQueryable();

            if (!typeof(IBelongToUser).IsAssignableFrom(query.ElementType))
                return;

            var param = Expression.Parameter(query.ElementType);
            var prop = Expression.Property(Expression.Property(param, nameof(IBelongToUser.User)), nameof(IBelongToUser.User.IsActive));
            var rightSide = Expression.Constant(true);
            var operation = Expression.Equal(prop, rightSide);
            var lambda = Expression.Lambda(operation, param);

            var call = Expression.Call(typeof(Queryable),
                "Where",
                new Type[] { query.ElementType },
                new[] { query.Expression, lambda });

            var result = query.Provider.CreateQuery(call);
        }

        public static List<Document> GetDocuments() 
        {
            return new List<Document>()
            {
                new Document()
                {
                    FileName = "Document 1",
                    Path = "c:/someting/something",
                    User = new User()
                    {
                        FirstName = "Andela",
                        LastName = "Colak",
                        IsActive = true
                    }
                },
                new Document()
                {
                    FileName = "Document 2",
                    Path = "C:/some/some",
                    User = new User()
                    {
                        FirstName = "Manoj",
                        LastName = "Rojas",
                        IsActive = false
                    }
                }
            };
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
    }

    public class Document : IBelongToUser
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public User User { get; set; }
    }

    public interface IBelongToUser
    {
        public User User { get; set; }
    }
}
