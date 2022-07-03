using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace KendoSelect
{
    class Program
    {
        //Kendo Select
        static void Main(string[] args)
        {
            var users = new List<User>()
            {
                new User() { FirstName = "Andela", LastName = "Colak", Age = 29, DateOfBirth = new DateTime(1993, 7, 7) },
                new User() { FirstName = "Andela2", LastName = "Colak2", Age = 43, DateOfBirth = new DateTime(1980, 4, 23) },
                new User() { FirstName = "234", LastName = "5333", Age = 43, DateOfBirth = new DateTime(1980, 4, 23)},
                new User() { FirstName = "aaaa", LastName = "aaaa", Age = 43, DateOfBirth = new DateTime(2022, 4, 23) }
            };

            WhereKendoSelect(users);
        }

        public static IQueryable WhereKendoSelect(List<User> users)
        {
            var selectedColumns = new List<string>() { "FirstName", "Age" };
            var query = users.Select(x => new UserView() 
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = x.Age
            }).AsQueryable();

            //users.Select(x => new UserView() { FirstName = x.FirstName, Age = x.Age});

            //In this case user but this would work with any type
            var param = Expression.Parameter(query.ElementType, "x");
            var memberBindings = new List<MemberAssignment>();

            foreach (var column in selectedColumns)
            {
                var property = query.ElementType.GetProperty(column);
                //This does FirstName = x.FirstName. Thats why we need getProperty to assign property name to the property value itself
                var memberbinding = Expression.Bind(property, Expression.Property(param, property));
                memberBindings.Add(memberbinding);
            }
            //Initialize member with current midings. In practice it means new UserView() { FirstName = x.FirstName, Age = x.Age }
            var member = Expression.MemberInit(Expression.New(query.ElementType), memberBindings);
            //Creates lambda from initialized member. In this case x => new UserView() { FirstName = x.FirstName, Age = x.Age }
            var lambda = Expression.Lambda(member, param);

            //This will not work because in order for Select to work we need to know select input and output beforehand which we do not 
            //We need to call select statement manually and let it know what it works with
            //query.Select(lambda);

            //This is because iqueryable doesnt know what it is working with. We need to specify it expicitly.
            var call = Expression.Call(typeof(Queryable),
                "Select",
                //This is so IQueryable knows what it is working with. In this case IQueryable<UserView, UserView>
                new Type[] { query.ElementType, query.ElementType },
                //This is what we are working with
                //query.Expression - users.Select(x => new UserView() { FirstName = x.FirstName, LastName = x.LastName, Age = x.Age }).AsQueryable();
                //lambda - part that we have added
                new[] { query.Expression, lambda }
                );

            var result = query.Provider.CreateQuery(call);

            return result;
        }

        public static IEnumerable<User> WhereMixedTypes(List<User> users)
        {
            var rules = new List<ComplexRule>()
            { 
                new ComplexRule()
                {
                    PropertyName = "FirstName",
                    Value = "a"
                },
                new ComplexRule()
                {
                    PropertyName = "Age",
                    Value = 43
                },
                //new ComplexRule()
                //{
                //    PropertyName = "DateOfBirth",
                //    Value = "2022-4-23"
                //}
            };

            var param = Expression.Parameter(typeof(User));
            var globalOperation = (Expression)Expression.Constant(true);

            foreach (var rule in rules)
            {
                var property = Expression.Property(param, rule.PropertyName);
                var rightSide = Expression.Constant(rule.Value);
                Expression operation;

                switch ((new User()).GetType().GetProperty(property.Member.Name).PropertyType.Name)
                {
                    case "String":
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        operation = Expression.Call(property, method, rightSide);
                        break;
                    default:
                        operation = Expression.Equal(property, rightSide);
                        break;
                }

                globalOperation = Expression.AndAlso(globalOperation, operation);
            }

            var lambda = Expression.Lambda<Func<User, bool>>(globalOperation, param);
            var result = users.Where(lambda.Compile());

            return result;
        }

        public static IEnumerable<User> WhereStringContains(List<User> users)
        {
            var SimpleRules = new List<SimpleRule>()
            {
                new SimpleRule() { PropertyName = "FirstName", Value = "a" }
            };

            var param = Expression.Parameter(typeof(User));
            var globalOperation = (Expression)Expression.Constant(true);
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            foreach (var SimpleRule in SimpleRules)
            {
                var property = Expression.Property(param, SimpleRule.PropertyName);
                var rightSide = Expression.Constant(SimpleRule.Value);
                var operation = Expression.Call(property, method, rightSide);

                globalOperation = Expression.AndAlso(globalOperation, operation);
            }

            var lambda = Expression.Lambda<Func<User, bool>>(globalOperation, param);
            var result = users.Where(lambda.Compile());

            return result;
        }

        public static IEnumerable<User> WherePropertyEqualsSomething(List<User> users)
        {
            var SimpleRules = new List<SimpleRule>()
            {
                new SimpleRule() { PropertyName = "FirstName", Value = "Andela2" }
            };

            var param = Expression.Parameter(typeof(User));
            var globalOperation = (Expression)Expression.Constant(true);

            foreach (var SimpleRule in SimpleRules)
            {
                var property = Expression.Property(param, SimpleRule.PropertyName);
                var rightSide = Expression.Constant(SimpleRule.Value);
                var operation = Expression.Equal(property, rightSide);

                globalOperation = Expression.AndAlso(globalOperation, operation);
            }

            var lambda = Expression.Lambda<Func<User, bool>>(globalOperation, param);

            var result = users.Where(lambda.Compile());

            return result;
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class UserView
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class SimpleRule
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }

    public class ComplexRule
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}
