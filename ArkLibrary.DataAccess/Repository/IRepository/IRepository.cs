using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArkWeb.DataAccess.Repository.IRepository
{
	public interface IRepository<T> where T : class
	{
		//T - Category or any other generic model on which we perform crud or interact with dbcontex

		IEnumerable<T> GetAll(string? includeProperties = null);
		T Get(Expression<Func<T,bool>> filter, string? includeProperties = null);

		void Add(T entity);
		void Delete(T entity);
		void DeleteRange(IEnumerable<T> entity);

	}
}
