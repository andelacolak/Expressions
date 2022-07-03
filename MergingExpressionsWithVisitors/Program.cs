using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MergingExpressionsWithVisitors
{
    class Program
    {
        static void Main(string[] args)
        {
            var documents = GetDocuments().AsEnumerable();

            Expression<Func<Document, bool>> expression1 = x => !x.IsDeleted;
            Expression<Func<Document, bool>> expression2 = y => y.IsPublished;

            //Expression<Func<Document, bool>> finalExpression = x => !x.IsDeleted && x.IsPublished;

            var param = Expression.Parameter(typeof(Document), "z");
            var combined = Expression.AndAlso(expression1.Body, expression2.Body);
            var withLeveledParameter = new Visitor(param).Visit(combined);
            var lambda = Expression.Lambda(withLeveledParameter, param);

            var result = lambda.Compile();
        }

        public static List<Document> GetDocuments()
        {
            return new List<Document>() 
            {
                new Document()
                {
                    FileName = "Unpublished",
                    IsDeleted = false,
                    IsPublished = false
                },
                new Document()
                {
                    FileName = "Deleted",
                    IsDeleted = true,
                    IsPublished = false
                },
                new Document()
                {
                    FileName = "Published",
                    IsDeleted = false,
                    IsPublished = true
                }
            };
        }
    }



    public class Visitor : ExpressionVisitor
    {
        private readonly ParameterExpression newParam;

        public Visitor(ParameterExpression newParam)
        {
            this.newParam = newParam;
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return newParam;
        }
    }


    public class Document
    {
        public string FileName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPublished { get; set; }
    }
}
