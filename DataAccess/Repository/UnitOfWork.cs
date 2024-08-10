using DataAccess.Data;
using DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _dbContext;

        public IContentTypeRepository ContentTypeRepository { get; set; }

        public IAlbumRepository AlbumRepository { get; private set; }

        public IAlbumCategoryRepository AlbumCategoryRepository { get; private set; }

        public IArtistRepository ArtistRepository { get; private set; }

        public IAlbumProducerRepository AlbumProducerRepository { get; private set; }

        public IGameRepository GameRepository { get; private set; }

        public IGameCategoryRepository GameCategoryRepository { get; private set; }

        public IGameProducerRepository GameProducerRepository { get; private set; }

        public IMovieRepository MovieRepository { get; private set; }

        public IMovieCategoryRepository MovieCategoryRepository { get; private set; }

        public IMovieProducerRepository MovieProducerRepository { get; private set; }

        public IPromotionRepository PromotionRepository { get; private set; }

        public INewsRepository NewsRepository { get; private set; }

        public IOrderDetailRepository OrderDetailRepository { get; private set; }

        public IOrderItemRepository OrderItemRepository { get; private set; }

        public IApplicationUserRepository ApplicationUserRepository { get; private set; }

        public IMainBannerRepository MainBannerRepository { get; private set; }

        public IRecommendProductSliderRepository RecommendProductSliderRepository { get; private set; }

        public IContactRepository ContactRepository { get; private set; }

        public IReviewRepository ReviewRepository { get; private set; }
        public UnitOfWork(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
            ContentTypeRepository = new ContentTypeRepository(_dbContext);
            AlbumRepository = new AlbumRepository(_dbContext);
            AlbumCategoryRepository = new AlbumCategoryRepository(_dbContext);
            ArtistRepository = new ArtistRepository(_dbContext);
            AlbumProducerRepository = new AlbumProducerRepository(_dbContext);
            GameRepository = new GameRepository(_dbContext);
            GameCategoryRepository = new GameCategoryRepository(_dbContext);
            GameProducerRepository = new GameProducerRepository(_dbContext);
            MovieRepository = new MovieRepository(_dbContext);
            MovieCategoryRepository = new MovieCategoryRepository(_dbContext);
            MovieProducerRepository = new MovieProducerRepository(_dbContext);
            PromotionRepository = new PromotionRepository(_dbContext);
            NewsRepository = new NewsRepository(_dbContext);
            OrderDetailRepository = new OrderDetailRepository(_dbContext);
            OrderItemRepository = new OrderItemRepository(_dbContext);
            ApplicationUserRepository = new ApplicationUserRepository(_dbContext);
            MainBannerRepository = new MainBannerRepository(_dbContext);
            RecommendProductSliderRepository = new RecommendProductSliderRepository(_dbContext);
            ContactRepository = new ContactRepository(_dbContext);
            ReviewRepository = new ReviewRepository(_dbContext);
        }

        public async Task Save()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
