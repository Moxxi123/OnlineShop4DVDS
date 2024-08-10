using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IContentTypeRepository ContentTypeRepository { get; }

        IAlbumRepository AlbumRepository { get; }

        IAlbumCategoryRepository AlbumCategoryRepository { get; }

        IArtistRepository ArtistRepository { get; }

        IAlbumProducerRepository AlbumProducerRepository { get; }

        IGameRepository GameRepository { get; }

        IGameCategoryRepository GameCategoryRepository { get; }

        IGameProducerRepository GameProducerRepository { get; }

        IMovieRepository MovieRepository { get; }

        IMovieCategoryRepository MovieCategoryRepository { get; }

        IMovieProducerRepository MovieProducerRepository { get; }

        IPromotionRepository PromotionRepository { get; }

        INewsRepository NewsRepository { get; }

        IOrderDetailRepository OrderDetailRepository { get; }

        IOrderItemRepository OrderItemRepository { get; }

        IApplicationUserRepository ApplicationUserRepository { get; }

        IMainBannerRepository MainBannerRepository { get; }

        IRecommendProductSliderRepository RecommendProductSliderRepository { get; }

        IContactRepository ContactRepository { get; }

        IReviewRepository ReviewRepository { get; }

        Task Save();
    }
}
