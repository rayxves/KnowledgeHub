using Api.Dtos;
using Api.Models;

namespace Api.Mappers
{
    public static class MediaMapper
    {
        public static MediaDto ToMediaDto(this Media media)
        {
            return new MediaDto
            {
                Url = media.Url,
                Type = media.Type.ToString(),
                Description = media.Description
            };
        }

        public static MediaDto ToMediaVersionDto(this MediaVersion media)
        {
            return new MediaDto
            {
                Url = media.Url,
                Type = media.Type.ToString(),
                Description = media.Description
            };
        }
    }
}