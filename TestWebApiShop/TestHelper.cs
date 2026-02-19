using AutoMapper;
using Services;

namespace TestWebApiShop
{
    public static class TestHelper
    {
        private static IMapper? _mapper;

        public static IMapper CreateMapper()
        {
            if (_mapper != null) return _mapper;
            
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MyMapper>());
            _mapper = config.CreateMapper();
            return _mapper;
        }
    }
}
