using AutoMapper;
using OWTournamentsHistory.Api.Services.Contract.Exceptions;
using OWTournamentsHistory.DataAccess.Contract;
using OWTournamentsHistory.DataAccess.Model;

namespace OWTournamentsHistory.Api.Services
{
    public abstract class ControllerServiceBase<TContract, TEntity>
        where TEntity : MongoCollectionEntry
    {
        protected readonly IMapper _mapper;
        protected readonly IRepository<TEntity> _repository;

        protected ControllerServiceBase(IMapper mapper, IRepository<TEntity> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public virtual async Task<IReadOnlyCollection<TContract>> GetMany(int? skip = null, int? limit = null, CancellationToken cancellationToken = default)
        {
            if (skip < 0 || limit < 0)
            {
                throw new InvalidParametersException();
            }
            var results = await _repository.GetSortedAsync(p => p.ExternalId, skip: skip, limit: limit, cancellationToken: cancellationToken);

            return results.Select(_mapper.Map<TContract>).ToArray();
        }

        public virtual async Task<TContract> Get(long id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                throw new NotFoundException($"{typeof(TContract).Name} (id:{id}) was not found");
            }
            var result = await _repository.GetAsync(id, cancellationToken);
            return _mapper.Map<TContract>(result);
        }

        public virtual async Task<long> Add(TContract entity, CancellationToken cancellationToken)
        {
            var generatedId = await _repository.AddAsync(_mapper.Map<TEntity>(entity), cancellationToken);
            return generatedId;
        }

        public virtual async Task Delete(long id, CancellationToken cancellationToken)
        {
            if (id < 0)
            {
                throw new NotFoundException($"{typeof(TContract).Name} (id:{id}) was not found");
            }
            await _repository.RemoveAsync(id, cancellationToken);
        }

        public abstract Task ImportFromHtml(string html, CancellationToken cancellationToken);
    }
}
