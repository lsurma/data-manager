using System.Linq.Expressions;

namespace DataManager.Application.Core.Common;

public interface IBasicSpecification<TEntity>
{
    Expression<Func<TEntity, bool>> ToExpression();
}
