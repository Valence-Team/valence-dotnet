namespace Valence.Observatorio.Application.Dtos
{
    public class PageResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int Deslocamento { get; set; }
        public int RegistroRetornado { get; set; }
        public int TotalRegistros { get; set; }
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
