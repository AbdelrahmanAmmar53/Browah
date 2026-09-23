using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Berwah.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
